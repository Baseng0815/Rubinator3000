/* 
* Stepper.cpp
*
* Created: 09.12.2019 19:15:26
* Author: Philipp
*/

#include "Stepper.h"
#include "pins.h"
#include <stdlib.h>

// default constructor
Stepper::Stepper() {
} //Stepper

// default destructor
Stepper::~Stepper() {
} //~Stepper

Stepper::Stepper(int pins[4]) {
	count = 100;
	
	for(int i = 0; i < 4; i++) {
		pinMode(pins[i], OUTPUT);
		
		this->pins[i] = pins[i];
	}
}

void Stepper::move(int steps) {
	int dir = steps < 0 ? -1 : 1;	
	
	for(int i = 0; i < abs(steps); i++) {
		count += dir;
		
		switch(count % 4) {
			case 0:
				writeState(1, 0, 0, 1);
				break;
			case 1:
				writeState(0, 1, 0, 1);
				break;
			case 2:
				writeState(0, 1, 1, 0);
				break;
			case 3: 
				writeState(1, 0, 1, 0);
				break;
		}
		
		if(count > 100) 
			count %= 4;
			
		if(count < 0)
			count += 100;
	}
}

void Stepper::writeState(int pin1, int pin2, int pin3, int pin4) const {
	digitalWrite(pins[0], pin1);
	digitalWrite(pins[1], pin2);
	digitalWrite(pins[2], pin3);
	digitalWrite(pins[3], pin4);
}
