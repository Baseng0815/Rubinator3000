# 1 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
# 2 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino" 2
# 3 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino" 2
# 4 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino" 2





uint8_t stepperPins[6][4] = {
  { 8, 9, 10, 11 },
  { 12, 13, 14, 15 },
  { 31, 16, 17, 18 },
  { 19, 20, 21, 22 },
  { 23, 7, 6, 5 },
  { 4, 3, 2, 1 }
};

Stepper steppers[6] = {
  Stepper(stepperPins[0]),
  Stepper(stepperPins[1]),
  Stepper(stepperPins[2]),
  Stepper(stepperPins[3]),
  Stepper(stepperPins[4]),
  Stepper(stepperPins[5])
};


int state;
uint8_t ledGreen = 29;
uint8_t ledRed = 28;


void setup() {
  // put your setup code here, to run once:
  serial_init();

  state = 0x00;
}

void loop() {
  // put your main code here, to run repeatedly:      

  if(serial_available()) {
    uint8_t data = serial_receive();

    switch (data) {
      case 0xA1: // connect
        setState(0x01);
        break;
      case 0xA0: // disconnect
        setState(0x00);
        break;
      case 0xA2: // solved event
        setState(0x02);
        break;
      default: // move
        handleMove(data);
        break;
    }
  }
}

void setState(uint8_t newState) {
  if(state != newState) {

    switch(newState) {
      case 0x00:
        state = 0x00;

        digitalWrite(ledGreen, 0x0);
        digitalWrite(ledRed, 0x1);
        break;
      case 0x01:
        state = 0x01;

        digitalWrite(ledGreen, 0x1);
        digitalWrite(ledRed, 0x1);
        break;
      case 0x02:
        state = 0x02;

        digitalWrite(ledGreen, 0x1);
        digitalWrite(ledRed, 0x0);
        break;
    }
  }

  serial_transmit(0xF0 + state);
}

void handleMove(uint8_t move) {
  if(state != 0x01) {
    serial_transmit(0xB1); // send not connected
    return;
  }

  uint8_t face = (move >> 1) - 1;
  bool isPrime = move & 0x01;

  if(face < 6) {
    int steps = (1 + (-2 * isPrime)) * 50;
    steppers[face].move(steps);

    serial_transmit(0x10 + move);
  }
  else {
    serial_transmit(0xB2); // send move not valid
  }
}
