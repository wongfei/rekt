#include "rekt.h"
#include "crc.h"

#define UPDATE_HZ 1000
#define UPDATE_MILLIS (1000 / UPDATE_HZ)

#define ADC_VREF 3.23f // -> 4095
#define ADC_CHANNELS 3
#define ADC_PRESSURE 0
#define ADC_CAS24 1
#define ADC_CAS2 2

#define PK_PSICAS_ID 0xFA
typedef struct
{
	uint8_t id;
	uint16_t tseconds; // 1unit = second
	uint16_t tfract; // 1unit = 100 microsecond
	uint16_t psi;
	uint16_t cas2;
} __attribute__((packed)) PkPsiCas_t;

extern TIM_HandleTypeDef htim1;
extern UART_HandleTypeDef huart1;
extern ADC_HandleTypeDef hadc1;
extern DMA_HandleTypeDef hdma_adc1;

__IO __attribute__((aligned(4))) uint32_t AdcRaw[ADC_CHANNELS + 2] = { 0 };
__IO __attribute__((aligned(4))) float AdcV[ADC_CHANNELS] = { 0.0f };

__IO uint16_t TimeSeconds = 0;
__IO uint16_t TimeFract = 0;
uint32_t TxRate = 0;
uint32_t TxCount = 0;
uint32_t TxErrCount = 0;

static uint16_t PackFloat16(float value, float min, float max)
{
	float packed = 65535.0f * (value - min) / (max - min);
	return (uint16_t)(packed < 0.0f ? 0.0f : (packed > 65535.0f ? 65535.0f : packed));
}

void rekt_init()
{
	//UART 115200 256000 460800

	LED_Write(LED_RED, 1);
	LED_Write(LED_GREEN, 1);

	crc8_init();

	AdcRaw[ADC_CHANNELS + 0] = 6666;
	AdcRaw[ADC_CHANNELS + 1] = 9999;

	HAL_ADCEx_Calibration_Start(&hadc1);
	HAL_ADC_Start_DMA(&hadc1, (uint32_t*)AdcRaw, ADC_CHANNELS);

	HAL_TIM_Base_Start_IT(&htim1);

	LED_Write(LED_RED, 0);
	LED_Write(LED_GREEN, 0);
}

void rekt_loop()
{
	PkPsiCas_t pk;
	float vrefScale = ADC_VREF / 4095.0f;
	float volt;
	uint32_t curTick = 0, statusTick = 0;
	uint16_t raw;
	uint8_t i;

	while (1)
	{
		for (i = 0; i < ADC_CHANNELS; ++i)
		{
			AdcV[i] = AdcRaw[i] * vrefScale;
		}

		pk.id = PK_PSICAS_ID;
		pk.tseconds = TimeSeconds;
		pk.tfract = TimeFract;
		pk.psi = PackFloat16(AdcV[ADC_PRESSURE], 0.0f, 5.0f);
		pk.cas2 = PackFloat16(AdcV[ADC_CAS2], 0.0f, 5.0f);
		//pk.crc = crc8_compute((uint8_t*)&pk, sizeof(pk) - 1);

		if (HAL_UART_Transmit(&huart1, (uint8_t*)&pk, sizeof(pk), 10) == HAL_OK) {
			TxCount++;
		} else {
			TxErrCount++;
		}

		curTick = HAL_GetTick();
		if (statusTick + 1000 <= curTick)
		{
			statusTick = curTick;

			TxRate = TxCount;
			TxCount = 0;

			LED_Write(LED_RED, ((TxRate >= UPDATE_HZ) ? 0 : 1));
			LED_Toggle(LED_GREEN);
		}
	}
}

void rekt_fault()
{
	LED_Write(LED_RED, 1);
	LED_Write(LED_GREEN, 0);

	while (1) 
	{
		HAL_Delay(100);
		LED_Toggle(LED_RED);
		LED_Toggle(LED_GREEN);
	}
}
