/*
  KOYA Stream Deck - Arduino Pro Micro (Atmega32U4)
  WERSJA: Absolute Potentiometer mapping (0-255)
*/

#include "HID-Project.h"

// KONFIGURACJA PINÓW
const int buttonPins[] = {5, 4, 2, 3, 6, 7, 8, 9, 10, 16, 14, 15}; 
const int potPins[] = {A1, A2}; // Górny na A1, Dolny na A2

// STAN URZĄDZENIA
bool lastButtonState[12];
int lastSentPotValue[2] = {-1, -1}; // Wymuś wysłanie przy starcie
const int potJitterThreshold = 3; // Ignoruj minimalne wahania ADC (0-1023)
byte rawhidBuffer[64];

void setup() {
  // Inicjalizacja przycisków
  for (int i = 0; i < 12; i++) {
    pinMode(buttonPins[i], INPUT_PULLUP);
    lastButtonState[i] = HIGH;
  }

  RawHID.begin(rawhidBuffer, sizeof(rawhidBuffer));
}

void loop() {
  // 1. OBSŁUGA PRZYCISKÓW (Debounced)
  for (int i = 0; i < 12; i++) {
    bool currentState = digitalRead(buttonPins[i]);
    if (currentState == LOW && lastButtonState[i] == HIGH) {
      sendKoyaReport(1, i, 1);
      delay(10); 
    } else if (currentState == HIGH && lastButtonState[i] == LOW) {
      sendKoyaReport(1, i, 0); // Opcjonalnie: puszczenie przycisku
    }
    lastButtonState[i] = currentState;
  }

  // 2. OBSŁUGA POTENCJOMETRÓW (Mapowanie Absolutne 0-255)
  for (int i = 0; i < 2; i++) {
    int rawVal = analogRead(potPins[i]);
    
    // Jeśli zmiana jest znacząca (filtrowanie szumu/jittera)
    if (abs(rawVal - lastSentPotValue[i]) > potJitterThreshold) {
      // Mapujemy 0-1023 -> 0-255 (1 bajt dla RawHID)
      byte mappedVal = map(rawVal, 0, 1023, 0, 255);
      
      sendKoyaReport(2, 12 + i, mappedVal);
      lastSentPotValue[i] = rawVal;
    }
  }

  delay(5); // Stabilizacja pętli
}

void sendKoyaReport(byte type, byte index, byte val) {
  memset(rawhidBuffer, 0, sizeof(rawhidBuffer));
  rawhidBuffer[0] = type;
  rawhidBuffer[1] = index;
  rawhidBuffer[2] = val;
  RawHID.write(rawhidBuffer, sizeof(rawhidBuffer));
}
