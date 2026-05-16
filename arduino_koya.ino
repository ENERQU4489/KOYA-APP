/*
  KOYA Stream Deck - STRICT MAPPING VERSION
  Arduino Pro Micro (Atmega32U4)
*/

#include "HID-Project.h"

// MAPOWANIE PINÓW (Fizyczny Pin -> Indeks w aplikacji)
// Indeksy 0-3: Rząd 1 | 4-7: Rząd 2 | 8-11: Rząd 3
const int buttonPins[] = {
  6,  3,  2,  4,   // Rząd 1: Pin 6, 3, 2, 4
  16, 10, 5,  9,   // Rząd 2: Pin 16, 10, 5, 9
  15, 14, 8,  2    // Rząd 3: Pin 15, 14, 8, [PIN 2 ZDUBLOWANY]
}; 

const int potPins[] = {A0, A1}; // Górna gałka na A0, Dolna na A1

// STAN
bool lastButtonState[12];
int lastSentPotValue[2] = {-1, -1};
const int potJitterThreshold = 5; 
byte rawhidBuffer[64];

void setup() {
  for (int i = 0; i < 12; i++) {
    pinMode(buttonPins[i], INPUT_PULLUP);
    lastButtonState[i] = HIGH;
  }
  RawHID.begin(rawhidBuffer, sizeof(rawhidBuffer));
}

void loop() {
  // 1. PRZYCISKI
  for (int i = 0; i < 12; i++) {
    bool currentState = digitalRead(buttonPins[i]);
    if (currentState == LOW && lastButtonState[i] == HIGH) {
      sendReport(1, i, 1); // Wciśnięty
      delay(20); // Debounce
    } else if (currentState == HIGH && lastButtonState[i] == LOW) {
      sendReport(1, i, 0); // Puszczony
    }
    lastButtonState[i] = currentState;
  }

  // 2. OBSŁUGA POTENCJOMETRÓW (Mapowanie Absolutne 0-255)
  for (int i = 0; i < 2; i++) {
    int rawVal = analogRead(potPins[i]);
    if (abs(rawVal - lastSentPotValue[i]) > potJitterThreshold) {
      // Kalibracja: 8-1015 zamiast 0-1023 dla pewności osiągnięcia 0 i 100%
      int constrainedVal = constrain(rawVal, 8, 1015);
      byte mappedVal = map(constrainedVal, 8, 1015, 0, 255);
      
      sendReport(2, 12 + i, mappedVal);
      lastSentPotValue[i] = rawVal;
    }
  }
  delay(1);
}

void sendReport(byte type, byte index, byte val) {
  memset(rawhidBuffer, 0, sizeof(rawhidBuffer));
  rawhidBuffer[0] = type;
  rawhidBuffer[1] = index;
  rawhidBuffer[2] = val;
  RawHID.write(rawhidBuffer, sizeof(rawhidBuffer));
}
