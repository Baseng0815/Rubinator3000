/* 
* Stepper.cpp
*
* Created: 09.12.2019 17:23:55
* Author: Philipp Geil
*/


#include "Stepper.h"

Stepper::Stepper(uint8_t pins[4]) {
	this->pins = pins;
} //Stepper

// default destructor
Stepper::~Stepper() {
} //~Stepper

void Stepper::init() {
	for(int i = 0; i < 4; i++) {
		
	}
}