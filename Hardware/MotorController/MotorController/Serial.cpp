/* 
* Serial.cpp
*
* Created: 09.12.2019 19:37:16
* Author: Philipp
*/


#include "Serial.h"
#include <stdlib.h>
#include <stdio.h>
#include <avr/io.h>

#define F_CPU 8000000UL

void Serial::begin(unsigned long baudRate) {
	uint16_t ubbr = (F_CPU / (16 * baudRate) - 1);
	
	// write baud rate registers
	UBRRH = ubbr >> 8;
	UBRRL = ubbr;
	
	// write enable registers
	UCSRB = (1 << TXEN) | (1 << RXEN);
	
	// write character size
	UCSRC = (1 << UCSZ0) | (1 << UCSZ1);
}

void Serial::send(char byte) {
	UDR = byte;
}

int Serial::available() {
	return UCSRA & (1 << RXC);
}

char Serial::receive() {
	while(!(UCSRA & (1 << RXC))); // wait for data
	
	return UDR;
}
