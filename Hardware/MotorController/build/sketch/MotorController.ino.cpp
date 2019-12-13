#include <Arduino.h>
#line 1 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
#include "Serial.h"
#include <util/delay.h>
#include "stepper.h"

#define STATE_DISCONNECT  0x00
#define STATE_CONNECT     0x01
#define STATE_SOLVED      0x02

uint8_t stepperPins[6][4] = {
  { 8, 9, 10, 11 },
  { 12, 13, 14, 15 },
  { 31, 16, 17, 18 },
  { 19, 20, 21, 22 },
  { 23, 7, 6, 5 },
  { 4, 3, 2, 1 }
};

Stepper steppers[6] = {
  Stepper(stepperPins[0]),
  Stepper(stepperPins[1]),
  Stepper(stepperPins[2]),
  Stepper(stepperPins[3]),
  Stepper(stepperPins[4]),
  Stepper(stepperPins[5])
};


int state;
uint8_t ledGreen = 29;
uint8_t ledRed = 28; 


#line 33 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void setup();
#line 40 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void loop();
#line 63 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void setState(uint8_t newState);
#line 91 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void handleMove(uint8_t move);
#line 33 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void setup() {
  // put your setup code here, to run once:
  serial_init();  

  state = STATE_DISCONNECT;
}

void loop() {
  // put your main code here, to run repeatedly:      

  if(serial_available()) {
    uint8_t data = serial_receive();

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

  serial_transmit(0xF0 + state);
}

void handleMove(uint8_t move) {
  if(state != STATE_CONNECT) {
    serial_transmit(0xB1);  // send not connected
    return;
  }

  uint8_t face = (move >> 1) - 1;
  bool isPrime = move & 0x01;

  if(face < 6) {
    int steps = (1 + (-2 * isPrime)) * 50;
    steppers[face].move(steps);

    serial_transmit(0x10 + move);
  }
  else {
    serial_transmit(0xB2);  // send move not valid
  }
}

