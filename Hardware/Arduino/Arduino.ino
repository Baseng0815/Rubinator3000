#include "stepper.h"

#define ARDUINO_UNO false

#define STATE_DISCONNECT 0x00
#define STATE_CONNECT 0x01
#define STATE_SOLVED 0x02

#if ARDUINO_UNO
int stepperPins[6][4] = {
  { 2, 3, 4, 5 },
  { 2, 3, 4, 5 },
  { 2, 3, 4, 5 },
  { 2, 3, 4, 5 },
  { 2, 3, 4, 5 },
  { 2, 3, 4, 5 }
};
#else
int stepperPins[6][4] = {
    { 26, 28, 22, 24 },
    { 23, 25, 27, 29 },
    { 36, 38, 32, 34 },
    { 33, 35, 37, 39 },
    { 42, 44, 46, 48 },
    { 47, 49, 43, 45 }
};
#endif

Stepper steppers[6];

int state;
#if ARDUINO_UNO
int ledGreen = 6;
int ledRed = 7;
#else
int ledGreen = 2;
int ledRed = 3;
#endif

void setup() {
	for(int i = 0; i < 6; i++) {
        steppers[i] = Stepper(stepperPins[i]);
    }

    pinMode(ledGreen, OUTPUT);
    pinMode(ledRed, OUTPUT);

    Serial.begin(9600);

	// 2 minutes timeout
	Serial.setTimeout(120000);
	digitalWrite(ledRed, HIGH);
}

void loop() {
  // put your main code here, to run repeatedly:

	if (Serial.available()) {
	uint8_t data = Serial.read();

	// disconnected
	if (data == -1) {
		setState(STATE_DISCONNECT);
		return;
	}

    switch (data) {
      case 0xA1:  // connect
        setState(STATE_CONNECT);
        break;
      case 0xA0:  // disconnect
        setState(STATE_DISCONNECT);
        break;
      case 0xA2:  // solved event
        setState(STATE_SOLVED);
        break;
      default:    // move
        handleMove(data);
        break;
    }
  }
}

void setState(uint8_t newState) {
  if(state != newState) {

    switch(newState) {
      case STATE_DISCONNECT:
        state = STATE_DISCONNECT;

        digitalWrite(ledGreen, LOW);
        digitalWrite(ledRed, HIGH);
        break;
      case STATE_CONNECT:
        state = STATE_CONNECT;

        digitalWrite(ledGreen, HIGH);
        digitalWrite(ledRed, HIGH);
        break;
      case STATE_SOLVED:
        state = STATE_SOLVED;

        digitalWrite(ledGreen, HIGH);
        digitalWrite(ledRed, LOW);
        break;
    }
  }

  Serial.write(0xF0 + state);
}

void handleMove(uint8_t move) {  
  if(state != STATE_CONNECT) {
    Serial.write(0xB1);  // send not connected
    return;
  }

  //handle parallel move
  if((move & 0x10) == 0x10) {
    handleParallelMove(move);
    return;
  }

  uint8_t face = (move >> 1) - 1;
  bool isPrime = move & 0x01;

  if(face < 6) {
    int steps = (1 + (-2 * isPrime)) * 50;
    steppers[face].doMove(steps, 10);

    Serial.write(0x20 + move);
  }
  else {
    Serial.write(0xB2);  // send move not valid
  }
}

void handleParallelMove(uint8_t parallelMove) {
    int axis = parallelMove & (1 << 3 | 1 << 2);

    int leftDir = parallelMove & (1 << 1);
    int rightDir = parallelMove & (1 << 10);

    doParallelMove(axis, leftDir, rightDir, 10);
}

void doParallelMove(int axis, int leftDir, int rightDir, int stepDelay) {    
  for(int i = 0; i < 50; i++) {
    steppers[axis].doStep(leftDir, 0);
    steppers[axis+3].doStep(rightDir, 0);
    delay(stepDelay);
  }
}