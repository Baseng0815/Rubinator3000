#ifndef SERIAL_H_
#define SERIAL_H_

extern "C" {

#include <stdint.h>
#include <avr/io.h>

void serial_init() {
	// set baud rate			

	unsigned long ubrr = 0x33;

	UBRRH = (uint8_t)(ubrr >> 8);	
	UBRRL = (uint8_t)ubrr;

	// enable RX and TX
	UCSRB = (1 << RXEN) | (1 << TXEN);

	UCSRC = (1 << URSEL) | (3 << UCSZ0);	
}

void serial_transmit(unsigned char data) {
	while((UCSRA & (1 << TXC)) == 1) ;
	
	UDR = data;

	while((UCSRA & (1 << TXC)) == 1) ;
}

unsigned char serial_receive() {
	while((UCSRA & (1 << RXC)) == 0);
		
	return (UDR);
}

int serial_available() {
	return (UCSRA & (1 << RXC));
}

};

#endif /* SERIAL_H_ */