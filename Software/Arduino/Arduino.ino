#include "stepper.h"

#define ARDUINO_UNO false

#define STATE_DISCONNECT 0x00
#define STATE_CONNECT 0x01
#define STATE_SOLVED 0x02

int stepperPins[6][4] = {
    { 26, 28, 22, 24 },
    { 23, 25, 27, 29 },
    { 36, 38, 32, 34 },
    { 33, 35, 37, 39 },
    { 42, 44, 46, 48 },
    { 47, 49, 43, 45 }
};


Stepper steppers[6];

int state;
int ledGreen = 2;
int ledRed = 3;

int ledsUp = 4;
int ledsDown = 5;
int ledStripes = 6;

void setup() {
	for(int i = 0; i < 6; i++) {
    steppers[i] = Stepper(stepperPins[i]);
  }

  pinMode(ledGreen, OUTPUT);
  pinMode(ledRed, OUTPUT);
  pinMode(ledsUp, OUTPUT);
  pinMode(ledsDown, OUTPUT);
  pinMode(ledStripes, OUTPUT);

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
		  stateCommand(STATE_DISCONNECT);
		  return;
	  }

    switch (data & 0xF0) {
      case 0xA0:    // states
        stateCommand(data & 0xF);
        break;
      case 0x00:    // move
        handleMove(data);
        break;
      case 0x10:    // multiTurn
        handleMultiTurnMove(data);
        break;
      case  0x40:   // leds
        while (!Serial.available()) ;
        uint8_t brightness = Serial.read();
        ledCommand(data & 0x0F, brightness);
        break;
    }
  }
}

void stateCommand(uint8_t newState) {
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
  
  Serial.write(0xF0 | state);
}

void handleMove(uint8_t move) {  
  if(state != STATE_CONNECT) {
    Serial.write(0xB1);  // send not connected
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
    Serial.write(0xB2);  // send not valid
  }
}

void handleMultiTurnMove(uint8_t multiTurnMove) {
  if(state != STATE_CONNECT) {
    Serial.write(0xB1);  // send not connected
    return;
  }

  int axis = (multiTurnMove & (1 << 3 | 1 << 2)) >> 2;

  if(axis < 3) {
    Serial.write(axis);

    int leftDir = (multiTurnMove & (1 << 1)) >> 1;
    int rightDir = multiTurnMove & (1 << 0);

    leftDir = leftDir == 0 ? 1 : -1;
    rightDir = rightDir == 0 ? 1 : -1;

    Serial.write(leftDir);
    Serial.write(rightDir);

    doMultiTurnMove(axis, leftDir, rightDir, 10);

    Serial.write(0x20 + multiTurnMove);
  }
  else {
    Serial.write(0xB2);  // send not valid
  }
}

void doMultiTurnMove(int axis, int leftDir, int rightDir, const int stepDelay) {
  int rightSteppers[] = {4, 3, 5};
  int rightStepper = rightSteppers[axis];
  int leftStepper = axis;

  for(int i = 0; i < 50; i++) {
    steppers[leftStepper].doStep(leftDir, 0);
    steppers[rightStepper].doStep(rightDir, 0);

    delay(stepDelay);
  }

  steppers[leftStepper].writeState(0, 0, 0, 0);
  steppers[rightStepper].writeState(0, 0, 0, 0);
}

void ledCommand(uint8_t command, uint8_t brightness) {    
  if(command & 0x01 == 0x01) {
    // leds down
    analogWrite(ledsDown, brightness);    
  }

  if(command & 0x02 == 0x02) {
    // leds up
    analogWrite(ledsUp, brightness);
  }

  if(command & 0x04 == 0x04) {
    // led stripes
    digitalWrite(ledStripes, brightness > 0 ? HIGH : LOW);
  }

  Serial.write(command);
  Serial.write(brightness);
}