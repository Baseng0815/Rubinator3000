#include "stepper.h"
#include <arduino.h>

Stepper::Stepper(const unsigned int pins[4]) {
    for(int i = 0; i < 4; i++) {
        pinMode(pins[i], OUTPUT);
        digitalWrite(pins[i], LOW);
        this->pins[i] = pins[i];
    }

    count = 500;
}

void Stepper::move(unsigned int steps, bool reverse) {
    int dir = reverse ? -1 : 1;

    for(int i = 0; i < steps; i++) {
        count += dir;
        switch (count % 4) {
            case 0:
                writePins(pinState0);
                break;        
            case 1:
                writePins(pinState1);
                break;
            case 2:
                writePins(pinState2);
                break;
            case 3:
                writePins(pinState3);        
                break;
        }

        if (count < 50)
            count += 100;

        delay(15);
    } 

    if(count > 500)   
        count %= 4;
}

void Stepper::writePins(unsigned int pinState) const {
    for(int i = 0; i < 4; i++){
        digitalWrite(pins[i], (pinState & (1 << i)));
    }
}