#pragma once
#define pinState0 0x09  // 1001
#define pinState1 0x0A  // 1010
#define pinState2 0x07  // 0110
#define pinState3 0x05  // 0101


class Stepper{
private:
    unsigned int pins[4];
    unsigned int count;    

    void writePins(unsigned int pinState) const;
public:
    Stepper(const unsigned int pins[4]);

    void move(unsigned int steps, bool reverse);
};