/* 
* stepper.cpp
*
* Created: 02.12.2019 18:19:49
* Author: Philipp Geil
*/


#include "stepper.h"
#include <avr/io.h>

Stepper::Stepper(Pin pins[]) {
	this->pins = pins;
	
	// config output pins
	for (int i = 0; i < 4; i++) {
		this->pins[i].pinMode(OUTPUT);
	}
}

void Stepper::move(int steps, bool reverse) {
	if(reverse) {
		
	}
	else {
		
	}
}
