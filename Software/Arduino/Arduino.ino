#include "Rubinator.h"

Rubinator hardware;

void setup() {
	hardware = Rubinator();
}

void loop() {
  // put your main code here, to run repeatedly:
	if(Serial.available() > 0) {
    hardware.handlePacket();
  }
}