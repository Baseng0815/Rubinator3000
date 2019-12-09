/*
 * pins.c
 *
 * Created: 09.12.2019 17:45:37
 *  Author: Philipp Geil
 */ 

#include "avr/io.h"

#ifdef __cplusplus
extern "C" {
#endif

#define OUTPUT 0x00
#define INPUT 0x01

#define LOW 0x00
#define HIGH 0x01

#define PIN_A0 0x00
#define PIN_A1 0x01
#define PIN_A2 0x02
#define PIN_A3 0x03
#define PIN_A4 0x04
#define PIN_A5 0x05
#define PIN_A6 0x06
#define PIN_A7 0x07

#define PIN_B0 0x10
#define PIN_B1 0x11
#define PIN_B2 0x12
#define PIN_B3 0x13
#define PIN_B4 0x14
#define PIN_B5 0x15
#define PIN_B6 0x16
#define PIN_B7 0x17

#define PIN_C0 0x20
#define PIN_C1 0x21
#define PIN_C2 0x22
#define PIN_C3 0x23
#define PIN_C4 0x24
#define PIN_C5 0x25
#define PIN_C6 0x26
#define PIN_C7 0x27

#define PIN_D0 0x30
#define PIN_D1 0x31
#define PIN_D2 0x32
#define PIN_D3 0x33
#define PIN_D4 0x34
#define PIN_D5 0x35
#define PIN_D6 0x36
#define PIN_D7 0x37

const uint8_t* dataDirRegs[4] = {
	&DDRA,
	&DDRB,
	&DDRC,
	&DDRD
};

const uint8_t* portOutRegs[4] = {
	&PORTA,
	&PORTB,	
	&PORTC,
	&PORTD
};

const uint8_t* portInRegs[4] = {
	&PINA,
	&PINB,
	&PINC,
	&PIND
};

void pinMode(uint8_t pin, uint8_t mode) {
	uint8_t port = pin & (0x10);
	uint8_t bit =  pin & (0x0F);
	
	volatile uint8_t* dataDirReg = dataDirRegs[port];	
	
	if(mode == OUTPUT) {
		*dataDirReg |= (1 << bit);
	}
	else if (mode == INPUT) {
		*dataDirReg &= ~(1 << bit);
	}
}

void digitalWrite(uint8_t pin, uint8_t value) {
	uint8_t port = pin & (0x10);
	uint8_t bit =  pin & (0x0F);
	
	volatile uint8_t *out;
	
	out = portOutRegs[port];
	
	if(value == LOW) {
		*out &= ~(1 << bit);
	}
	else {
		*out |= (1 << bit);
	}
}

int digitalRead(uint8_t pin) {
	uint8_t port = pin & (0x10);
	uint8_t bit =  pin & (0x0F);
	
	if(portInRegs[port] & (1 << bit)) return HIGH;
	else return LOW;
}

#if __cplusplus
};
#endif