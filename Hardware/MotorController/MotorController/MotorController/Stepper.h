/* 
* Stepper.h
*
* Created: 09.12.2019 17:23:56
* Author: Philipp Geil
*/


#ifndef __STEPPER_H__
#define __STEPPER_H__


class Stepper {
//variables
public:
protected:
private:
	uint8_t pins[4];
//functions
public:
	Stepper(uint8_t pins[4]);
	~Stepper();
protected:
private:
	Stepper( const Stepper &c );
	Stepper& operator=( const Stepper &c );

	void init();
}; //Stepper

#endif //__STEPPER_H__
