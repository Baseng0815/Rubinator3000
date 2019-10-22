int steppers[6][4] = {
  { 25, 24, 23, 22 }, //l
  { 31, 30, 29, 28 }, //u
  { 37, 36, 35, 34 }, //f
  { 40, 41, 42, 43 }, //d
  { 46, 47, 48, 49 }, //r
  { 4, 3, 53, 52, }    //b
};

int led_green = 10;
int led_red = 11;
int led_yellow = 12;

bool connected = false;

int steppersCount[] = { 500, 500, 500, 500, 500, 500 };

void setup() {
	// put your setup code here, to run once:
	Serial.begin(9600);
  
  // init status leds
  pinMode(led_green, OUTPUT);
	pinMode(led_yellow, OUTPUT);
	pinMode(led_red, OUTPUT);

	digitalWrite(led_red, HIGH);
	digitalWrite(led_yellow, LOW);
	digitalWrite(led_green, LOW);

	// init stepper Pins
	for (int i = 0; i < 6; i++)
	{
		for (int j = 0; j < 4; j++)
		{
			pinMode(steppers[i][j], OUTPUT);
		}
		digitalWrite(steppers[i][0], LOW);
		digitalWrite(steppers[i][1], LOW);
		digitalWrite(steppers[i][2], LOW);
		digitalWrite(steppers[i][3], LOW);
	}
}

void move(int, bool);

void loop() {
	// put your main code here, to run repeatedly:
	
  // wait for command
	while(Serial.available() <= 0)
		delay(50);

  // parse command
	String s = Serial.readStringUntil('\n');
    s.toLowerCase();	

    if(s.equals("connect")) {
        Serial.println("Connected");
        Serial.println("Funduino Mega");
		digitalWrite(led_red, LOW);
		digitalWrite(led_yellow, HIGH);	
		connected = true;
	}
	else if(s.equals("disconnect")) {
		Serial.println("Disconnected");
		digitalWrite(led_red, HIGH);
		digitalWrite(led_yellow, LOW);
		connected = false;
	}
	else if (s.equals("solved"))
	{
		digitalWrite(led_red, LOW);
		digitalWrite(led_yellow, LOW);
		digitalWrite(led_green, HIGH);

		Serial.println("Work done");
	}	
	else {
		if(connected){
        	bool isPrime = false;
        	if (s.length() > 1)
          		if (s[1] == 'i') isPrime = true;

        	switch (s[0]) {
          		case 'l': move(0, isPrime); break;
          		case 'u': move(1, isPrime); break;
          		case 'f': move(2, isPrime); break;
          		case 'd': move(3, isPrime); break;
          		case 'r': move(4, isPrime); break;
          		case 'b': move(5, isPrime); break;
        	}
		}
	}	
}

void move(int stepper, bool isPrime = false) {
  int dir = isPrime ? 1 : -1;

	for (int i = 0; i < 50; i++) {
		steppersCount[stepper] += dir;
		switch (steppersCount[stepper] % 4)
		{
		case 0:
			digitalWrite(steppers[stepper][0], HIGH);
			digitalWrite(steppers[stepper][1], LOW);
			digitalWrite(steppers[stepper][2], LOW);
			digitalWrite(steppers[stepper][3], HIGH);
			break;
		case 1:
			digitalWrite(steppers[stepper][0], LOW);
			digitalWrite(steppers[stepper][1], HIGH);
			digitalWrite(steppers[stepper][2], LOW);
			digitalWrite(steppers[stepper][3], HIGH);
			break;
		case 2:
			digitalWrite(steppers[stepper][0], LOW);
			digitalWrite(steppers[stepper][1], HIGH);
			digitalWrite(steppers[stepper][2], HIGH);
			digitalWrite(steppers[stepper][3], LOW);
			break;
		case 3:
			digitalWrite(steppers[stepper][0], HIGH);
			digitalWrite(steppers[stepper][1], LOW);
			digitalWrite(steppers[stepper][2], HIGH);
			digitalWrite(steppers[stepper][3], LOW);
			break;
		}
   
		delay(15);
		
		if(i % 10 == 0){
			digitalWrite(led_yellow, HIGH);
		}
		else if(i % 10 == 5){
			digitalWrite(led_yellow, LOW);
    }


	}	

  if(steppersCount[stepper] > 500){
		steppersCount[stepper] %= 4;
	}

	if(steppersCount[stepper] < 50) {
		steppersCount[stepper] += 100;
	}	
	
	digitalWrite(led_yellow, HIGH);

	delay(50);

	digitalWrite(steppers[stepper][0], LOW);
	digitalWrite(steppers[stepper][1], LOW);
 	digitalWrite(steppers[stepper][2], LOW);
 	digitalWrite(steppers[stepper][3], LOW);
	

  Serial.println("Move done");
}
