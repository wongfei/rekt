#include "crc.h"

static unsigned char crc7_poly = 0x89;
static unsigned char crc8_table[256];

void crc8_init()
{
    uint16_t i, j;
	for (i = 0; i < 256; i++)
    {
        crc8_table[i] = (i & 0x80) ? i ^ crc7_poly : i;
        for (j = 1; j < 8; j++)
        {
            crc8_table[i] <<= 1;
            if (crc8_table[i] & 0x80)
                crc8_table[i] ^= crc7_poly;
        }
    }
	return;
}

uint8_t crc8_compute(uint8_t* data, uint8_t length)
{
    uint8_t crc = 0;
    while (length--)
	{
		crc = crc8_table[(crc << 1) ^ *data++];
	}
    return crc;
}
