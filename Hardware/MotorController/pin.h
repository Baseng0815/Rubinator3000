#ifndef __PIN_H__
#define __PIN_H__

#define INPUT 0x00
#define OUTPUT 0x01

#define LOW 0x00
#define HIGH 0x01

extern "C" {
	#include <stdint.h>
	#include <avr/io.h>

	#undef digitalRead(uint8_t);

	volatile uint8_t* getDDR(uint8_t port) {
		switch (port)
		{
			case 0:				
				return &DDRA;
			case 1:
				return &DDRB;
			case 2:
				return &DDRC;
			case 3:
				return &DDRD;
			default:
				return 0;
		}
	}

	volatile uint8_t* getPort(uint8_t port) {
		switch (port)
		{
			case 0:				
				return &PORTA;
			case 1:
				return &PORTB;
			case 2:
				return &PORTC;
			case 3:
				return &PORTD;
			default:
				return 0;
		}
	}

	volatile uint8_t* getPin(uint8_t pin) {
		switch (pin)
		{
			case 0:				
				return &PINA;
			case 1:
				return &PINB;
			case 2:
				return &PINC;
			case 3:
				return &PIND;
			default:
				return 0;
		}
	}

	void pinMode(uint8_t pin, uint8_t mode) {
		uint8_t port = (pin >> 3);
		uint8_t bit =  (pin & 0x07);

		volatile uint8_t* ddr = getDDR(pin >> 3);

		if(mode == OUTPUT) {
			*ddr |= (1 << bit);
		}
		else {
			*ddr &= ~(1 << bit);
		}
	}

	void digitalWrite(uint8_t pin, uint8_t value) {
		uint8_t port = (pin >> 3);
		uint8_t bit =  (pin & 0x07);

		volatile uint8_t* portReg = getPort(port);

		if(value == HIGH) {
			*portReg |= (1 << bit);
		}
		else {
			*portReg &= ~(1 << bit);
		}
	}	
};

//void digitalWrite(uint8_t pin, uint8_t mode);
//void pinMode(uint8_t pin, uint8_t mode);

#endif //__PIN_H__
