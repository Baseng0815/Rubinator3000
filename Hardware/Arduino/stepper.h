#ifndef __STEPPER_H
#define __STEPPER_H

class Stepper {
private:
    int pins[4];
    int count;

    void writeState(int p0, int p1, int p2, int p3) const;
public:
    Stepper(int *pins);
    Stepper();

    void doMove(int steps, const int timeout);
    void doStep(const int direction, const int timeout);
};

#endif //__STEPPER_H