#include "stepper.h"
#include "Arduino.h"

Stepper::Stepper() {
    
}

Stepper::Stepper(int *pins) {
    count = 500;

    for(int i = 0; i < 4; i++) {
        this->pins[i] = pins[i];

        pinMode(pins[i], OUTPUT);
        digitalWrite(pins[i], LOW);
    }
}

void Stepper::writeState(int p0, int p1, int p2, int p3) const {
    digitalWrite(pins[0], p0);
    digitalWrite(pins[1], p1);
    digitalWrite(pins[2], p2);
    digitalWrite(pins[3], p3);
}

void Stepper::doMove(int steps) {
    int dir = steps < 0 ? -1 : 1;
    steps = abs(steps);

    for(int i = 0; i < steps; i++) {
        count += dir;

        switch(count % 4) {
            case 0:     writeState(0, 1, 1, 0); break;
            case 1:     writeState(0, 1, 0, 1); break;
            case 2:     writeState(1, 0, 0, 1); break;
            case 3:     writeState(1, 0, 1, 0); break;
        }

        if(count < 10)
            count += 100;
        else if(count > 1000)
            count %= 4;

        delay(10);
    }
    writeState(0,0,0,0);
}
