/* 
* Stepper.h
*
* Created: 09.12.2019 19:15:26
* Author: Philipp Geil
*/


#ifndef __STEPPER_H__
#define __STEPPER_H__


class Stepper
{
//variables
public:
protected:
private:
	int count;
	int pins[4];

//functions
public:	
	Stepper();
	~Stepper();	
	
	Stepper(int pins[4]);
	
	void move(int steps);
protected:
private:	
	
	void writeState(int pin1, int pin2, int pin3, int pin4) const;
}; //Stepper

#endif //__STEPPER_H__
