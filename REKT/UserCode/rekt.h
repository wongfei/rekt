#ifndef __REKT_H__
#define __REKT_H__

#include "stm32f1xx_hal.h"
#include "main.h"

#define LED_Toggle(Name) HAL_GPIO_TogglePin(Name##_GPIO_Port, Name##_Pin)
#define LED_Write(Name, Value) HAL_GPIO_WritePin(Name##_GPIO_Port, Name##_Pin, Value)

void rekt_init();
void rekt_loop();
void rekt_fault();

#endif
