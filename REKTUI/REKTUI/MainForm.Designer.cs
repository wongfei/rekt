namespace REKTUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.portSelector = new System.Windows.Forms.ComboBox();
			this.openPort = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.portPanel = new System.Windows.Forms.Panel();
			this.casOffsetValue = new System.Windows.Forms.TextBox();
			this.altitudeValue = new System.Windows.Forms.TextBox();
			this.correctCheck = new System.Windows.Forms.CheckBox();
			this.analyzeData = new System.Windows.Forms.Button();
			this.loadData = new System.Windows.Forms.Button();
			this.resetData = new System.Windows.Forms.Button();
			this.saveData = new System.Windows.Forms.Button();
			this.panXCheck = new System.Windows.Forms.CheckBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statComName = new System.Windows.Forms.ToolStripStatusLabel();
			this.statComID = new System.Windows.Forms.ToolStripStatusLabel();
			this.statComRate = new System.Windows.Forms.ToolStripStatusLabel();
			this.statComErr = new System.Windows.Forms.ToolStripStatusLabel();
			this.statTimestamp = new System.Windows.Forms.ToolStripStatusLabel();
			this.oxyView = new OxyPlot.WindowsForms.PlotView();
			this.statComRxPos = new System.Windows.Forms.ToolStripStatusLabel();
			this.portPanel.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// portSelector
			// 
			this.portSelector.FormattingEnabled = true;
			this.portSelector.Location = new System.Drawing.Point(2, 3);
			this.portSelector.Name = "portSelector";
			this.portSelector.Size = new System.Drawing.Size(121, 21);
			this.portSelector.TabIndex = 0;
			this.portSelector.DropDown += new System.EventHandler(this.portSelector_DropDown);
			// 
			// openPort
			// 
			this.openPort.Location = new System.Drawing.Point(125, 2);
			this.openPort.Name = "openPort";
			this.openPort.Size = new System.Drawing.Size(75, 23);
			this.openPort.TabIndex = 1;
			this.openPort.Text = "Connect";
			this.openPort.UseVisualStyleBackColor = true;
			this.openPort.Click += new System.EventHandler(this.openPort_Click);
			// 
			// timer
			// 
			this.timer.Interval = 33;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// portPanel
			// 
			this.portPanel.AutoSize = true;
			this.portPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.portPanel.BackColor = System.Drawing.SystemColors.Control;
			this.portPanel.Controls.Add(this.casOffsetValue);
			this.portPanel.Controls.Add(this.altitudeValue);
			this.portPanel.Controls.Add(this.correctCheck);
			this.portPanel.Controls.Add(this.analyzeData);
			this.portPanel.Controls.Add(this.loadData);
			this.portPanel.Controls.Add(this.resetData);
			this.portPanel.Controls.Add(this.saveData);
			this.portPanel.Controls.Add(this.panXCheck);
			this.portPanel.Controls.Add(this.portSelector);
			this.portPanel.Controls.Add(this.openPort);
			this.portPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.portPanel.Location = new System.Drawing.Point(0, 0);
			this.portPanel.Name = "portPanel";
			this.portPanel.Size = new System.Drawing.Size(992, 28);
			this.portPanel.TabIndex = 4;
			// 
			// casOffsetValue
			// 
			this.casOffsetValue.Location = new System.Drawing.Point(426, 4);
			this.casOffsetValue.Name = "casOffsetValue";
			this.casOffsetValue.Size = new System.Drawing.Size(40, 20);
			this.casOffsetValue.TabIndex = 9;
			this.casOffsetValue.Text = "96";
			// 
			// altitudeValue
			// 
			this.altitudeValue.Location = new System.Drawing.Point(562, 4);
			this.altitudeValue.Name = "altitudeValue";
			this.altitudeValue.Size = new System.Drawing.Size(40, 20);
			this.altitudeValue.TabIndex = 8;
			this.altitudeValue.Text = "512";
			// 
			// correctCheck
			// 
			this.correctCheck.AutoSize = true;
			this.correctCheck.Location = new System.Drawing.Point(604, 6);
			this.correctCheck.Name = "correctCheck";
			this.correctCheck.Size = new System.Drawing.Size(60, 17);
			this.correctCheck.TabIndex = 6;
			this.correctCheck.Text = "Correct";
			this.correctCheck.UseVisualStyleBackColor = true;
			// 
			// analyzeData
			// 
			this.analyzeData.Location = new System.Drawing.Point(485, 2);
			this.analyzeData.Name = "analyzeData";
			this.analyzeData.Size = new System.Drawing.Size(75, 23);
			this.analyzeData.TabIndex = 7;
			this.analyzeData.Text = "Analyze";
			this.analyzeData.UseVisualStyleBackColor = true;
			this.analyzeData.Click += new System.EventHandler(this.analyzeData_Click);
			// 
			// loadData
			// 
			this.loadData.Location = new System.Drawing.Point(350, 2);
			this.loadData.Name = "loadData";
			this.loadData.Size = new System.Drawing.Size(75, 23);
			this.loadData.TabIndex = 6;
			this.loadData.Text = "Load";
			this.loadData.UseVisualStyleBackColor = true;
			this.loadData.Click += new System.EventHandler(this.loadData_Click);
			// 
			// resetData
			// 
			this.resetData.Location = new System.Drawing.Point(200, 2);
			this.resetData.Name = "resetData";
			this.resetData.Size = new System.Drawing.Size(75, 23);
			this.resetData.TabIndex = 5;
			this.resetData.Text = "Reset";
			this.resetData.UseVisualStyleBackColor = true;
			this.resetData.Click += new System.EventHandler(this.resetData_Click);
			// 
			// saveData
			// 
			this.saveData.Location = new System.Drawing.Point(275, 2);
			this.saveData.Name = "saveData";
			this.saveData.Size = new System.Drawing.Size(75, 23);
			this.saveData.TabIndex = 4;
			this.saveData.Text = "Save";
			this.saveData.UseVisualStyleBackColor = true;
			this.saveData.Click += new System.EventHandler(this.saveData_Click);
			// 
			// panXCheck
			// 
			this.panXCheck.AutoSize = true;
			this.panXCheck.Checked = true;
			this.panXCheck.CheckState = System.Windows.Forms.CheckState.Checked;
			this.panXCheck.Location = new System.Drawing.Point(680, 6);
			this.panXCheck.Name = "panXCheck";
			this.panXCheck.Size = new System.Drawing.Size(52, 17);
			this.panXCheck.TabIndex = 3;
			this.panXCheck.Text = "PanX";
			this.panXCheck.UseVisualStyleBackColor = true;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statComName,
            this.statComErr,
            this.statComID,
            this.statTimestamp,
            this.statComRate,
            this.statComRxPos});
			this.statusStrip1.Location = new System.Drawing.Point(0, 667);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.statusStrip1.Size = new System.Drawing.Size(992, 22);
			this.statusStrip1.TabIndex = 7;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// statComName
			// 
			this.statComName.Name = "statComName";
			this.statComName.Size = new System.Drawing.Size(84, 17);
			this.statComName.Text = "statComName";
			// 
			// statComID
			// 
			this.statComID.Name = "statComID";
			this.statComID.Size = new System.Drawing.Size(63, 17);
			this.statComID.Text = "statComID";
			// 
			// statComRate
			// 
			this.statComRate.Name = "statComRate";
			this.statComRate.Size = new System.Drawing.Size(75, 17);
			this.statComRate.Text = "statComRate";
			// 
			// statComErr
			// 
			this.statComErr.Name = "statComErr";
			this.statComErr.Size = new System.Drawing.Size(66, 17);
			this.statComErr.Text = "statComErr";
			// 
			// statTimestamp
			// 
			this.statTimestamp.Name = "statTimestamp";
			this.statTimestamp.Size = new System.Drawing.Size(86, 17);
			this.statTimestamp.Text = "statTimestamp";
			// 
			// oxyView
			// 
			this.oxyView.BackColor = System.Drawing.SystemColors.Control;
			this.oxyView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.oxyView.Location = new System.Drawing.Point(0, 28);
			this.oxyView.Name = "oxyView";
			this.oxyView.PanCursor = System.Windows.Forms.Cursors.Hand;
			this.oxyView.Size = new System.Drawing.Size(992, 639);
			this.oxyView.TabIndex = 6;
			this.oxyView.Text = "plotView1";
			this.oxyView.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
			this.oxyView.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
			this.oxyView.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
			// 
			// statComRxPos
			// 
			this.statComRxPos.Name = "statComRxPos";
			this.statComRxPos.Size = new System.Drawing.Size(83, 17);
			this.statComRxPos.Text = "statComRxPos";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Info;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(992, 689);
			this.Controls.Add(this.oxyView);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.portPanel);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "R.E.K.T";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.portPanel.ResumeLayout(false);
			this.portPanel.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox portSelector;
        private System.Windows.Forms.Button openPort;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Panel portPanel;
        private OxyPlot.WindowsForms.PlotView oxyView;
        private System.Windows.Forms.CheckBox panXCheck;
        private System.Windows.Forms.Button saveData;
        private System.Windows.Forms.Button resetData;
        private System.Windows.Forms.Button loadData;
        private System.Windows.Forms.Button analyzeData;
        private System.Windows.Forms.CheckBox correctCheck;
        private System.Windows.Forms.TextBox altitudeValue;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statComName;
		private System.Windows.Forms.ToolStripStatusLabel statComID;
		private System.Windows.Forms.ToolStripStatusLabel statComErr;
		private System.Windows.Forms.ToolStripStatusLabel statComRate;
		private System.Windows.Forms.TextBox casOffsetValue;
		private System.Windows.Forms.ToolStripStatusLabel statTimestamp;
		private System.Windows.Forms.ToolStripStatusLabel statComRxPos;
	}
}

