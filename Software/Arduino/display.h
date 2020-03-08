#include <LiquidCrystal.h>

class display {
private:
    LiquidCrystal lcd = LiquidCrystal(0, 0, 0, 0, 0, 0);

    String moveStr = "";
    int progressBarCount = 0;

    void updateLCD();
    const char faceChars[6] = { 'L', 'U', 'F', 'D', 'R', 'B' };
    const int axisMapping[3] = { 4, 3, 5 }; 

public:
    display(uint8_t rs, uint8_t enable, uint8_t d0, uint8_t d1, uint8_t d2, uint8_t d3);

    void displayMove(uint8_t moveByte);
    void displayMultiMove(uint8_t moveByte);
    void displayProgress(float percent);
};