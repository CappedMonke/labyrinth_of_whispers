#include <Arduino.h>

#define PIN_TEST_POWER 26
#define PIN_TEST_MONITOR 27

void setupTestPin() {
  pinMode(PIN_TEST_POWER, OUTPUT);
  digitalWrite(PIN_TEST_POWER, HIGH);

  pinMode(PIN_TEST_MONITOR, INPUT_PULLUP);
}

void monitorTestPin() {
  static int lastState = HIGH;
  int currentState = digitalRead(PIN_TEST_MONITOR);

  delay(10);
  int stableState = digitalRead(PIN_TEST_MONITOR);

  if (currentState == stableState && currentState != lastState) {
    lastState = currentState;

    if (currentState == LOW) {
      Serial.println("Button pressed.");
    } else {
      Serial.println("Button released.");
    }
  }
}

void setup() {
  Serial.begin(115200);
  setupTestPin();
}

void loop() { monitorTestPin(); }