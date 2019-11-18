#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#define SSID "Rubinator3000"
#define PASS "4059_9045_9208_3409"

unsigned int localPort = 8000;

char packetBuffer[UDP_TX_PACKET_MAX_SIZE + 1];
char replyBuffer[UDP_TX_PACKET_MAX_SIZE + 1];

WiFiUDP Udp;

void setup() {
    Serial.begin(9600);
    WiFi.mode(WIFI_AP);
    WiFi.begin(SSID, PASS);

    Udp.begin(localPort);
}

void loop() {
    int packetSize = Udp.parsePacket();
    if(packetSize > 0) {
        int n = Udp.read(packetBuffer, UDP_TX_PACKET_MAX_SIZE);
        packetBuffer[n] = 0;
        Serial.println(packetBuffer);

        while(Serial.available() < 1) 
            delay(100);

        int rn = Serial.readBytes(replyBuffer, UDP_TX_PACKET_MAX_SIZE);
        replyBuffer[rn] = 0;

        Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
        Udp.write(replyBuffer);
        Udp.endPacket();
    }
}