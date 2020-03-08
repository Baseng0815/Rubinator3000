#include "display.h"
#include "math.h"

display::display(uint8_t rs, uint8_t enable, uint8_t d0, uint8_t d1, uint8_t d2, uint8_t d3) {
    lcd = LiquidCrystal(rs, enable, d0, d1, d2, d3);

    lcd.begin(2, 16);    
}

void display::updateLCD() {
    lcd.clear();

    for(char* it = moveStr.begin(); it != moveStr.end(); it++) {
        lcd.write((*it));
    }

    lcd.setCursor(1, 0);

    for(int i = 0; i < progressBarCount; i++) {
        lcd.write('#');
    }
}

void display::displayMove(uint8_t moveByte) {
    moveStr = "";
    
    int face = ((moveByte & 0x1C) >> 2) - 1;    
    moveStr += faceChars[face];
    
    if(moveByte & 0x02)
        moveStr += '2';

    if(moveByte & 0x01)
        moveStr += 'i';

    updateLCD();
}

void display::displayMultiMove(uint8_t moveByte) {
    moveStr = "";

    int left = ((moveByte & 0x30) >> 4) - 1;
    int right = axisMapping[left];

    moveStr += faceChars[left];
    
    if(moveByte & 0x04)
        moveStr += '2';

    if(moveByte & 0x01)
        moveStr += 'i';

    moveStr += faceChars[right];

    if(moveByte & 0x08)
        moveStr += '2';

    if(moveByte & 0x02)
        moveStr += 'i';

    updateLCD();
}

void display::displayProgress(float percent) {
    int count = (int)floor(16.0f * percent);    

    progressBarCount = count;

    updateLCD();
}