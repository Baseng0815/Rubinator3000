#include "rubinator.h"

rubinator hardware = rubinator();

void setup() {
	
}

void loop() {
  // put your main code here, to run repeatedly:
	if(Serial.available() > 0) {
    hardware.handleRequest();    
  }  
}