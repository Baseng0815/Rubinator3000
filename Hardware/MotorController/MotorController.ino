#include "stepper.h"
#include <Arduino.h>
#include "math.h"
#include "avr/iom16.h"

const unsigned int stepperPins[6][4] = {
  { 1, 2, 3, 4 }, 
  { }
};
bool connected = false;

Stepper steppers[6];

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);

  for(int i = 0; i < 6; i++){
    steppers[i] = Stepper(stepperPins[i]);
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  if(Serial.available() > 0){
    char data = Serial.read();
    char responseData;

    switch (data)
    {
      case 0xA0:    // disconnect
        responseData = 0xF0;
        connected = false;
        break;
      case 0xA1:    // connect
        responseData = 0xF1;  // connected
        connected = true;
        break;      
      default:
        if(connected){
          int face = (int)log2(data >> 1);
          bool isPrime = data & 0x01;

          steppers[face].move(50, isPrime);

          responseData = data | 0x80;        
        }
        else{
          responseData = 0xB1;
        }
        break;
    }

    Serial.write(responseData);
  }
}