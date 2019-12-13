#ifndef __Stepper_H__
#define __Stepper_H__

#include <stdint.h>

#define STEPPER_STATE_0 0b00001001
#define STEPPER_STATE_1 0b00001010
#define STEPPER_STATE_2 0b00000110
#define STEPPER_STATE_3 0b00000101

class Stepper {
private:
    int count;
    uint8_t pins[4];    

    void writeState(uint8_t state) const;
public:
    Stepper(uint8_t* pins);

    void move(int steps);
};

#endif  // __Stepper_H__