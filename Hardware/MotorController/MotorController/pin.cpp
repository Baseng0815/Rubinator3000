/* 
* pin.cpp
*
* Created: 02.12.2019 18:43:59
* Author: Philipp Geil
*/


#include "pin.h"

Pin::Pin(port, bit) {
	this->port = port;
	this->bit = bit;
}

void Pin::pinMode(int mode) {
	if(mode == INPUT){
		switch(port){
			case PORTA:
				DDRA |= (1 << bit);
				break;
			case PORTB:
				DDRB |= (1 << bit);
				break;
			case PORTC:
				DDRC |= (1 << bit);
				break;
			case PORTD:
				DDRD |= (1 << bit);
				break;
		}
	}
	else if(mode == OUTPUT){
		switch(port){
			case PORTA:
				DDRA &= (1 << bit);
				break;
			case PORTB:
				DDRB &= (1 << bit);
				break;
			case PORTC:
				DDRC &= (1 << bit);
				break;
			case PORTD:
				DDRD &= (1 << bit);
				break;
		}
	}
	
}
