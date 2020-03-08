#include "rubinator.h"

rubinator hardware;

void setup() {
	hardware = rubinator();  
}

void loop() {
  // put your main code here, to run repeatedly:
	if(Serial.available() > 0) {
    hardware.handlePacket();    
  }  
}