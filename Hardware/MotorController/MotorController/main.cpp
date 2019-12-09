/*
 * MotorController.cpp
 *
 * Created: 09.12.2019 17:19:05
 * Author : Philipp Geil
 */ 

#include "Serial.h"
#include "pins.h"
#include "Stepper.h"

int stepperPins[6][4] = {
	{ PIN_B0, PIN_B1, PIN_B2, PIN_B3 },
	{ PIN_B4, PIN_B5, PIN_B6, PIN_B7 },
	{ PIN_D7, PIN_C0, PIN_C1, PIN_C2 },
	{ PIN_C3, PIN_C4, PIN_C5, PIN_C6 },
	{ PIN_C7, PIN_A7, PIN_A6, PIN_A5 },
	{ PIN_A4, PIN_A3, PIN_A2, PIN_A1 }
};

Stepper steppers[6];
unsigned char status;

int main(void)
{
	status = 0x00;	// set unconnected status
	Serial::begin(9600);	// init serial
    
	for(int i = 0; i < 6; i++) { // init steppers
		int* pins = stepperPins[i];
		steppers[i] = Stepper(pins);
	}
	
    while (1) {
		
    }
}

