#include "stepper.h"
#include <avr/io.h>

Stepper::Stepper(int pin1, int pin2, int pin3, int pin4) {
    pins[0] = pin1;
    pins[1] = pin2;
    pins[2] = pin3;
    pins[3] = pin4;
}

void Stepper::Move(int steps, bool reverse) {
    int dir = reverse ? -1 : 1;    

    for(int i = 0; i < steps; i++) {
        count += dir;
        switch (count) {
            case 0:

        }
    }
}