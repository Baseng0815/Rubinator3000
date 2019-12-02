/* 
* pin.h
*
* Created: 02.12.2019 18:43:59
* Author: Philipp Geil
*/
#pragma once
#ifndef OUTPUT 
#define OUTPUT = 1
#endif // OUTPUT
#ifdef INPUT
#define INPUT = 0
#endif // INPUT

struct Pin{
private:	
	int port;
	int bit;
	
public:
	Pin(int port, int bit);
	
	void pinMode(int mode);
};
