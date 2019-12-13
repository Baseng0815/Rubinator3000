# 1 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
# 2 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino" 2
# 3 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino" 2
# 4 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino" 2


# 5 "d:\\Repos\\Rubinator3000\\Hardware\\MotorController\\MotorController.ino"
void setup() {
  // put your setup code here, to run once:
  pinMode(8, 0x01);

  serial_init();
}

void loop() {
  // put your main code here, to run repeatedly:    

  char character = serial_receive();

  if(character == 0x12) {
    digitalWrite(8, 0x01);
  }
  else {
    digitalWrite(8, 0x00);
  }
}
