#ifndef __CRC_H__
#define __CRC_H__

#include <stdint.h>

void crc8_init();
uint8_t crc8_compute(uint8_t* data, uint8_t length);

#endif
