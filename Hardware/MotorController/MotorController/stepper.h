/* 
* stepper.h
*
* Created: 02.12.2019 18:19:50
* Author: Philipp Geil
*/

#include "pin.h"

class Stepper {
private:
	Pin pins[4];
	int count = 500;
	
public:
	Stepper(Pin pins[]);
	
	void move(int steps, bool reverse) const;
};
