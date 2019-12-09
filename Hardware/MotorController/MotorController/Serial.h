/* 
* Serial.h
*
* Created: 09.12.2019 19:37:16
* Author: Philipp Geil
*/


#ifndef __SERIAL_H__
#define __SERIAL_H__


class Serial {
private:
	//static int baudRate;
public:
	static void begin(unsigned long baudRate);
	static void send(char byte);
	static char receive();
	static int available();
}; 

#endif //__SERIAL_H__
