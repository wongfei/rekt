using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.WindowsForms;

namespace REKTUI
{
	public partial class MainForm : Form
	{
		public const byte PK_PSICAS_ID = 0xFA;
		public const int PK_PSICAS_SIZE = 9;

		struct Point
		{
			public double x, y;
			public Point(double px, double py) { x = px; y = py; }
		}

		private SerialPort _port;
		private bool _comGotFirstPacket;
		private int _comErrorCount;
		private double _startTicks;

		private PlotModel _oxyPlot;
		private LinearAxis _axisTime;
		private LinearAxis _axisPsi;
		private LinearAxis _axisCas;
		private LineSeries _dataPsi;
		private LineSeries _dataCas2;

		private object _pointsMx = new object();
		private List<Point> _pointsPsi = new List<Point>();
		private List<Point> _pointsCas2 = new List<Point>();
		private List<Point> _pointsPsiX = new List<Point>();
		private List<Point> _pointsCas2X = new List<Point>();

		private byte[] _rxbuf = new byte[4096];
		private byte[] _swapbuf = new byte[4096];
		private byte[] _resetbuf = new byte[4096];
		private int _rxpos;
		private int _rxmax;

		private int _sampleHz;
		private int _sampleCounter;
		private int _sampleTimer;
		private double _sampleTimestamp;

		//=====================================================================
		// MainForm
		//=====================================================================

		public MainForm()
		{
			crc8_init();
			InitializeComponent();
			UpdateStatus();
			ResetPlot();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			ClosePort();
		}

		private void UpdateStatus()
		{
			openPort.Text = (_port != null ? "Disconnect" : "Connect");
			if (_port != null)
			{
				int nsamples = _dataPsi != null ? _dataPsi.Points.Count : 0;
				statComName.Text = "Port:" + _port.PortName;
				statComErr.Text = "Err:" + _comErrorCount.ToString();
				statComID.Text = "Samples:" + nsamples.ToString();
				statTimestamp.Text = "Timestamp: " + Str(_sampleTimestamp, "N6");
				statComRate.Text = "Rate:" + _sampleHz.ToString();
				statComRxPos.Text = "RxMax: " + _rxmax;
			}
			else
			{
				statComName.Text = "Not connected";
				statComErr.Text = "";
				statComID.Text = "";
				statTimestamp.Text = "";
				statComRate.Text = "";
				statComRxPos.Text = "";
			}
		}

		private void ResetAppState()
		{
			_comGotFirstPacket = false;
			_comErrorCount = 0;
			_startTicks = 0;
			_sampleHz = 0;
			_sampleCounter = 0;
			_sampleTimer = 0;
			_sampleTimestamp = 0;
		}

		//=====================================================================
		// COM PORT
		//=====================================================================

		private void portSelector_DropDown(object sender, EventArgs e)
		{
			portSelector.Items.Clear();
			var names = SerialPort.GetPortNames();
			foreach (var name in names)
			{
				portSelector.Items.Add(name);
			}
		}

		private void openPort_Click(object sender, EventArgs e)
		{
			if (_port != null)
			{
				ClosePort();
			}
			else
			{
				ResetAppState();
				ResetPlot();

				var name = portSelector.SelectedItem as string;
				if (!string.IsNullOrEmpty(name))
				{
					try
					{
						//UART 115200 256000 460800
						_port = new SerialPort(name, 460800, Parity.None, 8, StopBits.One);
						_port.Open();
						_port.DiscardInBuffer();
						_port.DataReceived += new SerialDataReceivedEventHandler(OnSerialPortData);
						timer.Enabled = true;
					}
					catch (Exception ex) { MessageBox.Show(ex.Message); }
				}
			}
			UpdateStatus();
		}

		private void ClosePort()
		{
			timer.Enabled = false;
			if (_port != null)
			{
				try { _port.Close(); }
				catch (Exception) { }
				_port = null;
			}
			_rxpos = 0;
		}

		private void DiscardBuffer()
		{
			if (_port != null)
			{
				try { _port.DiscardInBuffer(); }
				catch (Exception) { }
			}
			_rxpos = 0;
		}

		private void OnSerialPortData(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort port = (SerialPort)sender;
			try
			{
				while (port.BytesToRead > 0)
				{
					int ChunkSize = Math.Min(port.BytesToRead, _rxbuf.Length - _rxpos);
					if (ChunkSize > 0)
					{
						int BytesReceived = port.Read(_rxbuf, (int)_rxpos, ChunkSize);
						if (BytesReceived > 0)
						{
							ProcessBytes(port, BytesReceived);
						}
						else
						{
							Debug.WriteLine("OnSerialPortData: read failed");
							_comErrorCount++;
						}
					}
					else
					{
						Debug.WriteLine("OnSerialPortData: buffer overflow");
						DiscardBuffer();
						_comErrorCount++;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("OnSerialPortData: " + ex.Message);
				DiscardBuffer();
				_comErrorCount++;
			}
		}

		private void ProcessBytes(SerialPort port, int BytesReceived)
		{
			_rxpos += BytesReceived;
			_rxmax = Math.Max(_rxmax, _rxpos);

			const int size = PK_PSICAS_SIZE;
			int pos = 0;

			while (pos < _rxpos)
			{
				byte id = _rxbuf[pos];
				if (id != PK_PSICAS_ID)
				{
					Debug.WriteLine("invalid ID: id=" + id + " pos=" + pos + " _rxpos=" + _rxpos);
					pos++;
					_comErrorCount++;
					continue;
				}

				if (pos + size > _rxpos)
				{
					break;
				}

				ReadPacket(id, pos + 1); // skip ID
				pos += size;
			}

			if (pos > 0 && pos < _rxpos)
			{
				int count = _rxpos - pos;
				Buffer.BlockCopy(_rxbuf, pos, _swapbuf, 0, count);
				var tmp = _rxbuf;
				_rxbuf = _swapbuf;
				_swapbuf = tmp;
				_rxpos = count;
			}
			else if (pos == _rxpos)
			{
				_rxpos = 0;
			}
		}

		private void ReadPacket(byte id, int pos)
		{
			switch (id)
			{
				case PK_PSICAS_ID:
				{
					UInt16 TimeSeconds = BitConverter.ToUInt16(_rxbuf, pos); pos += 2;
					UInt16 TimesFract = BitConverter.ToUInt16(_rxbuf, pos); pos += 2;
					_sampleTimestamp = TimeSeconds + TimesFract * 0.0001;

					UInt16 PressureRaw = BitConverter.ToUInt16(_rxbuf, pos); pos += 2;
					UInt16 Cas2Raw = BitConverter.ToUInt16(_rxbuf, pos); pos += 2;

					float PressureV = UnpackFloat16(PressureRaw, 0.0f, 5.0f);
					float PressurePsi = LinearScale(PressureV, 0.5f, 4.5f, 0.0f, 200.0f);
					float PressurePsiC = Clamp(PressurePsi, 0.0f, 200.0f);

					float CasScale = 1;
					float CasOff = 0;
					float Cas2V = UnpackFloat16(Cas2Raw, 0.0f, 5.0f) * CasScale + CasOff;

					if (_startTicks < 0.1) _startTicks = _sampleTimestamp;

					Point pt;
					pt.x = (_sampleTimestamp - _startTicks);

					lock (_pointsMx)
					{
						pt.y = PressurePsiC;
						_pointsPsi.Add(pt);

						pt.y = Cas2V;
						_pointsCas2.Add(pt);
					}

					_sampleCounter++;

					if (!_comGotFirstPacket)
					{
						_comGotFirstPacket = true;
						_comErrorCount = 0;
					}

					break;
				}
			}
		}

		//=====================================================================
		// PLOT
		//=====================================================================

		private void ResetPlot()
		{
			_pointsPsi.Clear();
			_pointsCas2.Clear();

			_oxyPlot = new PlotModel { PlotType = PlotType.XY };

			_axisTime = new LinearAxis { Title = "sec", Minimum = 0, Maximum = 30, Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
			_oxyPlot.Axes.Add(_axisTime);

			_axisPsi = new LinearAxis { Title = "PSI", Minimum = -5, Maximum = 150, Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
			_oxyPlot.Axes.Add(_axisPsi);

			_axisCas = new LinearAxis { Title = "CAS", Minimum = 0, Maximum = 3.3, Position = AxisPosition.Right, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
			_oxyPlot.Axes.Add(_axisCas);

			_dataPsi = new LineSeries
			{
				Title = "",
				//MarkerType = MarkerType.Triangle,
				MarkerType = MarkerType.None,
				MarkerSize = 3,
				StrokeThickness = 2
			};
			_oxyPlot.Series.Add(_dataPsi);

			_dataCas2 = new LineSeries
			{
				Title = "",
				MarkerType = MarkerType.None,
				Color = OxyColors.DarkBlue
			};
			_oxyPlot.Series.Add(_dataCas2);

			/*
            _oxyPlot.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = 110,
                MaximumX = float.MaxValue,
                Color = OxyColors.Green,
                Text = "GOOD"
            });*/

			oxyView.Model = _oxyPlot;
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			this.Invoke(new MethodInvoker(UpdateData));
		}

		private void UpdateData()
		{
			bool refresh = false;

			lock (_pointsMx)
			{
				_pointsPsiX.AddRange(_pointsPsi);
				_pointsPsi.Clear();

				_pointsCas2X.AddRange(_pointsCas2);
				_pointsCas2.Clear();
			}

			if (_pointsPsiX.Count > 0)
			{
				foreach (var pt in _pointsPsiX)
				{
					_dataPsi.Points.Add(new DataPoint(pt.x, pt.y));
				}
				var tail = _pointsPsiX[_pointsPsiX.Count - 1];
				if ((panXCheck.Checked) && (tail.x > _axisTime.ActualMaximum))
				{
					var pan = ((tail.x - _axisTime.ActualMaximum) + (_axisTime.ActualMaximum - _axisTime.ActualMinimum) * 0.5f) * _axisTime.Scale * -1.0f;
					_axisTime.Pan(pan);
				}
				_pointsPsiX.Clear();
				refresh = true;
			}

			if (_pointsCas2X.Count > 0)
			{
				foreach (var pt in _pointsCas2X)
				{
					_dataCas2.Points.Add(new DataPoint(pt.x, pt.y));
				}
				_pointsCas2X.Clear();
				refresh = true;
			}

			if (refresh)
			{
				_oxyPlot.InvalidatePlot(false);
			}

			int curTicks = Environment.TickCount;
			int delta = curTicks - _sampleTimer;

			if (delta >= 1000)
			{
				_sampleTimer = curTicks;
				_sampleHz = _sampleCounter;
				_sampleCounter = 0;
				_rxmax = 0;
			}

			UpdateStatus();
		}

		//=====================================================================
		// PROCESS DATA
		//=====================================================================

		private void resetData_Click(object sender, EventArgs e)
		{
			ResetAppState();
			ResetPlot();
		}

		private void saveData_Click(object sender, EventArgs e)
		{
			ClosePort();
			UpdateStatus();

			try
			{
				var dialog = new SaveFileDialog();
				dialog.InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
				dialog.Filter = "bin (*.bin)|*.bin|png (*.png)|*.png";
				dialog.FilterIndex = 1;
				dialog.RestoreDirectory = true;
				dialog.CheckFileExists = false;

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					if (dialog.FileName.Contains(".png"))
					{
						var pngExporter = new PngExporter { Width = oxyView.Width, Height = oxyView.Height, Background = OxyColors.White };
						pngExporter.ExportToFile(_oxyPlot, dialog.FileName);
					}
					else if (dialog.FileName.Contains(".bin"))
					{
						using (var writer = new BinaryWriter(new FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.Write)))
						{
							writer.Write((UInt32)_dataPsi.Points.Count);
							writer.Write((UInt32)_dataCas2.Points.Count);

							foreach (var pt in _dataPsi.Points)
							{
								writer.Write((float)pt.X);
								writer.Write((float)pt.Y);
							}

							foreach (var pt in _dataCas2.Points)
							{
								writer.Write((float)pt.X);
								writer.Write((float)pt.Y);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void loadData_Click(object sender, EventArgs e)
		{
			ClosePort();
			ResetPlot();
			UpdateStatus();

			try
			{
				float casOff = 0;
				float.TryParse(casOffsetValue.Text, out casOff);

				var dialog = new OpenFileDialog();
				dialog.InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
				dialog.Filter = "bin (*.bin)|*.bin";
				dialog.FilterIndex = 1;
				dialog.RestoreDirectory = true;
				dialog.CheckFileExists = true;

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					if (dialog.FileName.Contains(".bin"))
					{
						using (var reader = new BinaryReader(new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read)))
						{
							UInt32 psiCount = reader.ReadUInt32();
							UInt32 casCount = reader.ReadUInt32();
							Point pt;

							for (var i = 0; i < psiCount; ++i)
							{
								pt.x = reader.ReadSingle();
								pt.y = reader.ReadSingle();
								_pointsPsi.Add(pt);
							}

							for (var i = 0; i < casCount; ++i)
							{
								pt.x = reader.ReadSingle();
								pt.y = reader.ReadSingle() + casOff;
								_pointsCas2.Add(pt);
							}
						}

						UpdateData();
						statComName.Text = Path.GetFileName(dialog.FileName);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void analyzeData_Click(object sender, EventArgs e)
		{
			ClosePort();

			if (_oxyPlot == null || _dataPsi == null || _dataPsi.Points == null || _dataPsi.Points.Count == 0)
			{
				MessageBox.Show("No data!");
				return;
			}

			bool correct = correctCheck.Checked;
			int altitudeFeet = 0;
			if (correct)
			{
				if (!int.TryParse(altitudeValue.Text, out altitudeFeet))
				{
					MessageBox.Show("Invalid altitude value!");
					return;
				}
			}

			//
			// PSI peaks
			//

			double riseX = -1, fallX = -1;
			double peakX = 0;
			double peakY = 0;
			double maxPsi = 0;

			List<DataPoint> peaks = new List<DataPoint>();

			foreach (var pt in _dataPsi.Points)
			{
				if (riseX < 0 && pt.Y > 70)
				{
					riseX = pt.X;
					fallX = -1;
					peakX = peakY = 0;
				}

				if (peakY < pt.Y)
				{
					peakX = pt.X;
					peakY = pt.Y;
				}

				if (riseX > 0 && fallX < 0 && pt.Y < 5)
				{
					peaks.Add(new DataPoint(peakX, peakY));
					fallX = pt.X;
					riseX = -1;
				}
			}

			if (peaks.Count < 3)
			{
				MessageBox.Show("No peaks detected!");
				return;
			}

			for (int i = 0; i < peaks.Count; ++i)
			{
				if (maxPsi < peaks[i].Y)
				{
					maxPsi = peaks[i].Y;
				}
			}

			/*
			250 RPM-> 4,17 RPS-> 1500 DegPerSec
			4.17r = 1s
			1r = 0,23980815347721822541966426858513s
			1500 deg = 1s
			5 deg = 0,00333333333333333333333333333333s
			*/
			double time = peaks[peaks.Count - 1].X - peaks[0].X;
			double crankRevDelta = time / (double)(peaks.Count - 1);
			double rpm = 60.0 / crankRevDelta;
			double degPerSec = rpm * 360 / 60;
			double rpmDiff = rpm - 250;
			double ignOff = (crankRevDelta * 5.0) / 360.0;

			//
			// CAS peaks (1.65 vref, 0.9 min, 2.4 max)
			//

			List<DataPoint> casPeaks = new List<DataPoint>();

			double casOff = 0;
			double.TryParse(casOffsetValue.Text, out casOff);

			riseX = -1;
			fallX = -1;
			peakX = 0;
			peakY = 0;

			foreach (var pt in _dataCas2.Points)
			{
				double casV = pt.Y - casOff;

				if (riseX < 0 && casV > 2.0)
				{
					riseX = pt.X;
					fallX = -1;
					peakX = peakY = 0;
				}

				if (peakY < pt.Y)
				{
					peakX = pt.X;
					peakY = pt.Y;
				}

				if (riseX > 0 && fallX < 0 && casV < 1.3)
				{
					casPeaks.Add(new DataPoint(peakX, peakY));
					fallX = pt.X;
					riseX = -1;
				}
			}

			riseX = -1;
			fallX = -1;
			peakX = 0;
			peakY = double.MaxValue;

			foreach (var pt in _dataCas2.Points)
			{
				double casV = pt.Y - casOff;

				if (fallX < 0 && casV < 1.3)
				{
					fallX = pt.X;
					riseX = -1;
					peakX = 0;
					peakY = double.MaxValue;
				}

				if (peakY > pt.Y)
				{
					peakX = pt.X;
					peakY = pt.Y;
				}

				if (fallX > 0 && riseX < 0 && casV > 2.0)
				{
					casPeaks.Add(new DataPoint(peakX, peakY));
					riseX = pt.X;
					fallX = -1;
				}
			}

			//
			// Draw PSI peaks
			//

			_oxyPlot.Annotations.Clear();

			for (int i = 0; i < peaks.Count; ++i)
			{
				var pt = peaks[i];

				_oxyPlot.Annotations.Add(new PointAnnotation
				{
					X = pt.X,
					Y = pt.Y,
					Size = 5,
					TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
					TextVerticalAlignment = VerticalAlignment.Middle,
					Fill = OxyColors.Red,
					Text = Str(pt.Y, "N1") + (i + 2 == peaks.Count ? " @ " + Str(rpm, "N1") + " RPM" : "")
				});

				_oxyPlot.Annotations.Add(new LineAnnotation
				{
					Type = LineAnnotationType.Vertical,
					X = pt.X,
					MaximumX = float.MaxValue,
					MinimumY = 0,
					MaximumY = 150,
					Color = OxyColors.Red,
					Text = ""
				});

				_oxyPlot.Annotations.Add(new LineAnnotation
				{
					Type = LineAnnotationType.Vertical,
					X = pt.X + ignOff,
					MaximumX = float.MaxValue,
					MinimumY = 0,
					MaximumY = 150,
					Color = OxyColors.Orange,
					Text = ""
				});

				if (correct)
				{
					// TODO: not sure that shit is OK
					// https://www.rx7club.com/3rd-generation-specific-1993-2002-16/compression-testing-1098715/#post12052569
					// https://www.rx7club.com/3rd-generation-specific-1993-2002-16/what-normal-deterioration-compression-13b-rew-1119515/#post12222281
					// https://drive.google.com/file/d/0B_jE2-IDoVTfbWxqTlROdDgxQ2M/view
					double cor1 = pt.Y + (rpmDiff * -0.22041);
					double cor2 = cor1 + (altitudeFeet * 0.0028);

					_oxyPlot.Annotations.Add(new PointAnnotation
					{
						X = pt.X,
						Y = cor2,
						Size = 5,
						TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
						TextVerticalAlignment = VerticalAlignment.Middle,
						Fill = OxyColors.Blue,
						Text = Str(cor2, "N1")
					});
				}
			}

			//
			// Draw CAS peaks
			//

			for (int i = 0; i < casPeaks.Count; ++i)
			{
				var pt = casPeaks[i];

				double deltaX = double.MaxValue;
				int psiId = -1;

				for (int j = 0; j < peaks.Count; ++j)
				{
					var dx = Math.Abs(pt.X - peaks[j].X);
					if (deltaX > dx)
					{
						deltaX = dx;
						psiId = j;
					}
				}

				if (psiId != -1)
				{
					double off = (pt.X - peaks[psiId].X);
					double deg = degPerSec * off;

					_oxyPlot.Annotations.Add(new PointAnnotation
					{
						X = pt.X,
						Y = pt.Y,
						Size = 4,
						TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
						TextVerticalAlignment = VerticalAlignment.Middle,
						Fill = OxyColors.BlueViolet,
						Text = Str(off * 1000, "N1") + "ms ; " + Str(deg, "N1") + "*"
					});
				}
			}

			_oxyPlot.InvalidatePlot(false);
		}

		//=====================================================================
		// UTILS
		//=====================================================================

		private static string Str(double x)
		{
			return x.ToString().Replace(',', '.');
		}

		private static string Str(double x, string format)
		{
			return x.ToString(format).Replace(',', '.');
		}

		private static float UnpackFloat16(UInt16 packed, float min, float max)
		{
			float value = min + ((float)packed / 65535.0f) * (max - min);
			return (value < min ? min : (value > max ? max : value));
		}

		private static float LinearScale(float x, float a, float b, float c, float d)
		{
			float y = ((x - a) * ((d - c) / (b - a))) + c;
			return y;
		}

		private static float Clamp(float x, float a, float b)
		{
			return (x < a ? a : (x > b ? b : x));
		}

		//=====================================================================
		// CRC
		//=====================================================================

		byte crc7_poly = 0x89;
		byte[] crc8_table = new byte[256];

		void crc8_init()
		{
			int i, j;
			for (i = 0; i < 256; i++)
			{
				byte x = (byte)i;
				crc8_table[i] = (byte)(((x & 0x80) != 0) ? (x ^ crc7_poly) : x);
				for (j = 1; j < 8; j++)
				{
					crc8_table[i] <<= 1;
					if ((crc8_table[i] & 0x80) != 0)
					{
						crc8_table[i] ^= crc7_poly;
					}
				}
			}
		}

		byte crc8_compute(byte[] data, int offset, int count)
		{
			byte crc = 0;
			for (int i = offset; i < offset + count; ++i)
			{
				crc = crc8_table[(crc << 1) ^ data[i]];
			}
			return crc;
		}
	}
}
