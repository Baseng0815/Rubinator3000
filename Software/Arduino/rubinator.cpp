#include "rubinator.h"

rubinator::rubinator() {
    for(int s = 0; s < 6; s++) {
        steppers[s] = stepper(stepperPins[s]);
    }

    connected = false;    
}

void rubinator::handleRequest() {
    byte request = Serial.read();

    if(request == 0x01){
        movesCount = Serial.read();        
    }
    if(request >= 0x04 && request <= 0x1B) {
        handleMoveRequest(request);
    }
    else if(request <= 0x5A) {
        handleMultiMoveRequest(request - 0x1B);
    }
    else if(request <= 0x5F) {
        handleStatusRequest(request - 0x5A);
    }
}

void rubinator::handleMoveRequest(byte moveByte) {
    if(connected){
        lcdDisplay.displayMove(moveByte);
        doMove(moveByte);
        movesDone++;        
        lcdDisplay.displayProgress((float)movesDone / (float)movesCount);
    }
    else {
        // TODO: add error response
    }
}

void rubinator::doMove(byte moveByte) {
    int face = ((moveByte & 0x1C) >> 2) - 1;
    int count = moveByte & 0x02 ? 2 : 1;

    // inverted move
    if(moveByte & 0x01){
        count *= -1;
    }

    steppers[face].doMove(count * 50, 5);
}

void rubinator::handleMultiMoveRequest(byte moveByte) {
    if(connected) {
        lcdDisplay.displayMultiMove(moveByte);
        doMultiMove(moveByte);
        movesDone++;
        lcdDisplay.displayProgress((float)movesDone / (float)movesCount);
    }
    else {

    }
}

void rubinator::doMultiMove(byte moveByte) {
    int left = ((moveByte & 0x30) >> 4) - 1;
    int right = axisMapping[left];

    int leftCount = (moveByte & 0x04 ? 2 : 1) * 50; 
    int rightCount = (moveByte & 0x08 ? 2 : 1) * 50;

    int leftDir = moveByte & 0x01 ? -1 : 1;        
    int rightDir = moveByte & 0x02 ? -1 : 1;        

    for(int i = 0; i < max(leftCount, rightCount); i++) {
        steppers[left].doStep(leftDir, 0);
        steppers[right].doStep(rightDir, 5);
    }
}

void rubinator::handleStatusRequest(byte status) {
    switch(status){
        case STATE_DISCONNECT:
            connected = false;
            break;
        case STATE_CONNECT:
            connected = true;
            break;
        case STATE_SHUFFLE:
            if(connected)
                solved = false;
            else {}
            break;
        case STATE_SOLVE:
            if(connected)
                solved = true;
            else {}
            break;
    }
}