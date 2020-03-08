#pragma once

class stepper {
private:
    int pins[4];
    int count;

public:
    stepper(const int *pins);
    stepper();

    void doMove(int steps, const int timeout);
    void doStep(const int direction, const int timeout);
    void writeState(int p0, int p1, int p2, int p3) const;
};