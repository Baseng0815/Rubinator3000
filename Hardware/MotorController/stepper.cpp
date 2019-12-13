#include "stepper.h"
#include "pin.h"
#include <util/delay.h>

#define abs(i) i < 0 ? -i : i;

Stepper::Stepper(uint8_t *pins) {
    count = 100;

    for(int i = 0; i < 4; i++) {
        this->pins[i] = pins[i];
        pinMode(pins[i], OUTPUT);
    }
}

void Stepper::writeState(uint8_t state) const {
    for(int i = 0; i < 4; i++) {
        digitalWrite(pins[i], (state & (1 << i)) >> i);
    }
}

void Stepper::move(int steps) {
    int dir = steps < 0 ? -1 : 1;
    steps = abs(steps);

    for (int i = 0; i < steps; i++) {
        count += dir;

        switch (count % 4) {
            case 0: writeState(STEPPER_STATE_0); break;
            case 1: writeState(STEPPER_STATE_1); break;
            case 2: writeState(STEPPER_STATE_2); break;
            case 3: writeState(STEPPER_STATE_3); break;
        }

        if(count > 500)
            count %= 4;

        if(count < 10)
            count += 1000;

        _delay_ms(10);
    }
    
    writeState(0x00);
}