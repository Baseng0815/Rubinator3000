#include "pin.h"
#include "Serial.h"
#include <util/delay.h>

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
