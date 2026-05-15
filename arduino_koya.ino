/*
  KOYA Stream Deck - Arduino Pro Micro (Atmega32U4)
  WERSJA: Analog Potentiometer + Buttons
*/

#include "HID-Project.h"

// KONFIGURACJA PINÓW
const int buttonPins[] = {5, 4, 3, 2, 6, 7, 8, 9, 10, 16, 14, 15}; 
const int potPins[] = {A0, A2}; // Górny na A0, Dolny na A2

// STAN URZĄDZENIA
bool lastButtonState[12];
int lastPotValue[2];
const int potThreshold = 20; // Zwiększona martwa strefa (mniejszy jitter)
byte rawhidBuffer[64];

void setup() {
  // Inicjalizacja przycisków
  for (int i = 0; i < 12; i++) {
    pinMode(buttonPins[i], INPUT_PULLUP);
    lastButtonState[i] = HIGH;
  }

  // Inicjalizacja potencjometrów
  for (int i = 0; i < 2; i++) {
    lastPotValue[i] = analogRead(potPins[i]);
  }

  RawHID.begin(rawhidBuffer, sizeof(rawhidBuffer));
}

void loop() {
  // 1. OBSŁUGA PRZYCISKÓW (Bez zmian)
  for (int i = 0; i < 12; i++) {
    bool currentState = digitalRead(buttonPins[i]);
    if (currentState == LOW && lastButtonState[i] == HIGH) {
      sendKoyaReport(1, i, 1);
      delay(20); 
    }
    lastButtonState[i] = currentState;
  }

  // 2. OBSŁUGA POTENCJOMETRÓW ANALOGOWYCH
  for (int i = 0; i < 2; i++) {
    // ADC Stabilization: Double read
    analogRead(potPins[i]); 
    delay(1); 
    int currentVal = analogRead(potPins[i]);
    
    int diff = currentVal - lastPotValue[i];

    // Jeśli zmiana jest większa niż próg (martwa strefa)
    if (abs(diff) >= potThreshold) {
      bool physicalRight = (diff > 0); 
      bool actionDirection = !physicalRight; 
      
      sendKoyaReport(2, 12 + i, actionDirection ? 1 : 0);
      
      lastPotValue[i] += (physicalRight ? potThreshold : -potThreshold);
      delay(10); 
    }
  }
}

void sendKoyaReport(byte type, byte index, byte val) {
  memset(rawhidBuffer, 0, sizeof(rawhidBuffer));
  rawhidBuffer[0] = type;
  rawhidBuffer[1] = index;
  rawhidBuffer[2] = val;
  RawHID.write(rawhidBuffer, sizeof(rawhidBuffer));
}
