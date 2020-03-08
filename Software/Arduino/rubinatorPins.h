#define LEDS_DOWN 0x01
#define LEDS_UP 0x02
#define LED_STRIPES 0x04

#define STATE_DISCONNECT 0x00
#define STATE_CONNECT 0x01
#define STATE_SHUFFLE 0x02
#define STATE_SOLVE 0x03

const int stepperPins[6][4] = {
    { 26, 28, 22, 24 },
    { 23, 25, 27, 29 },
    { 36, 38, 32, 34 },
    { 33, 35, 37, 39 },
    { 42, 44, 46, 48 },
    { 47, 49, 43, 45 }
};

const int ledRed = 2;
const int ledYellow = 3;
const int ledGreen = 4;

const int ledsUp = 5;
const int ledsDown = 6;
const int ledStripes = 7;

const int lcdEN = 12;
const int lcdRS = 13;
const int lcdPins[4] = { 14, 15, 16, 17}