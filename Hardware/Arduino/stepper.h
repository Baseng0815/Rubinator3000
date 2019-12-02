#pragma once

class Stepper {
private: 
    int count = 500;
    int pins[4];

public:
    Stepper(int pin1, int pin2, int pin3, int pin4);
    void Move(int steps, bool reverse);
};