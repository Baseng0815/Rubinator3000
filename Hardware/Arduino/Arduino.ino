#include "stepper.h"

#define STATUS_DISCONNECTED 0x00
#define STATUS_CONNNECTED	0x01
#define STATUS_QUO			0x02

void setStatus(int status);

/*int steppers[6][4] = {
  { 25, 24, 23, 22 }, //l
  { 31, 30, 29, 28 }, //u
  { 37, 36, 35, 34 }, //f
  { 40, 41, 42, 43 }, //d
  { 46, 47, 48, 49 }, //r
  { 4, 3, 53, 52, }    //b
};*/

Stepper steppers[6];
int status = STATUS_DISCONNECTED;

void setup() {
	// put your setup code here, to run once:
	Serial.begin(9600);
	
}

void loop() {
	if(Serial.available() > 0) {
		char byte = Serial.read();

		if(status != STATUS_CONNNECTED){
			// connection request
			if(byte == 0xA1){
				setStatus(STATUS_CONNNECTED);

				// send connected
				Serial.write(0xF1);
			}
			else {
				// send connection Error			

			}
		}
	}
}

void setStatus(int statusCode) {
	status = statusCode;

	switch (status) {
		case STATUS_CONNNECTED:

	}
}