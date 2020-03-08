#include <LiquidCrystal.h>
#include "stepper.h"
#include "rubinatorPins.h"

class rubinator {
private:        
    void doMove(byte moveByte);
    void doMultiMove(byte multiMoveByte);

    void handleMoveRequest(byte moveByte);
    void handleMultiMoveRequest(byte moveByte);
    void handleLedRequest(byte ledFlag, byte brightness);
    void handleStatusRequest(byte state);        

    void sendResponse(byte* data, int length);    

    stepper steppers[6];
    LiquidCrystal lcd = LiquidCrystal(lcdRS, lcdEN, lcdPins[0], lcdPins[1], lcdPins[2], lcdPins[3]);

    bool connected;
    bool solved;

    int movesCount = 0;
    int movesDone = 0;

    const int axisMapping[3] = { 4, 3, 5 };    

public:
    rubinator();    

    void handleRequest();
};