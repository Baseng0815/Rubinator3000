#include <Arduino.h>
#line 1 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
#include "pin.h"
#include "Serial.h"
#include <util/delay.h>

#line 5 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void setup();
#line 12 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void loop();
#line 5 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void setup() {
  // put your setup code here, to run once:
  pinMode(8, OUTPUT);  

  serial_init();
}

void loop() {
  // put your main code here, to run repeatedly:    

  char character = serial_receive();
  
  if(character == 0x12) {
    digitalWrite(8, HIGH);
  }
  else {
    digitalWrite(8, LOW);
  }
}

