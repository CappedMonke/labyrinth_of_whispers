#include <Adafruit_BNO055.h>
#include <Adafruit_Sensor.h>
#include <BluetoothSerial.h>
#include <Wire.h>

#define PIN_IMU_SDA 32
#define PIN_IMU_SCL 33
#define PIN_VIBRATION_MOTOR 13
#define PIN_BUTTON_CALIBRATE 23
#define PIN_BUTTON_RESET 22
#define PIN_BUTTON_ACCEPT 21

Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28, &Wire);
BluetoothSerial SerialBT;

unsigned long lastIMUTime = 0;
const unsigned long imuInterval = 50;
String input = "";
bool hasVibratedForCalibration = false;
bool hasSentCalibrateMessage = false;
unsigned long startTime = 0;
bool hasWaited = false;

bool lastCalibrateState = HIGH;
bool lastResetState = HIGH;
bool lastAcceptState = HIGH;

void setup() {
  Serial.begin(115200);
  SerialBT.begin("BlindCane");

  Wire.begin(PIN_IMU_SDA, PIN_IMU_SCL);

  if (!bno.begin()) {
    Serial.println("BNO055 not found!");
    while (1)
      ;
  }

  bno.setExtCrystalUse(true);

  ledcSetup(0, 1000, 8);
  ledcAttachPin(PIN_VIBRATION_MOTOR, 0);
  ledcWrite(0, 0);

  pinMode(PIN_BUTTON_CALIBRATE, INPUT_PULLUP);
  pinMode(PIN_BUTTON_RESET, INPUT_PULLUP);
  pinMode(PIN_BUTTON_ACCEPT, INPUT_PULLUP);
}

void loop() {
  if (!hasWaited) {
    if (startTime == 0) {
      startTime = millis();
    }
    if (millis() - startTime < 1000) {
      return; // Exit loop until 1 second has passed
    }
    hasWaited = true;
  }

  // Read current states
  bool currCalibrate = digitalRead(PIN_BUTTON_CALIBRATE);
  bool currReset = digitalRead(PIN_BUTTON_RESET);
  bool currAccept = digitalRead(PIN_BUTTON_ACCEPT);

  // Calibrate button
  if (lastCalibrateState == HIGH && currCalibrate == LOW) {
    Serial.println("Calibrate button pressed");
    SerialBT.println("calibrate");
  }

  // Reset button
  if (lastResetState == HIGH && currReset == LOW) {
    Serial.println("Reset button pressed");
    SerialBT.println("reset");
  }

  // Accept button
  if (lastAcceptState == HIGH && currAccept == LOW) {
    Serial.println("Accept button pressed");
    SerialBT.println("accept");
  }

  // Update states
  lastCalibrateState = currCalibrate;
  lastResetState = currReset;
  lastAcceptState = currAccept;

  uint8_t sys, gyro, accel, mag;
  bno.getCalibration(&sys, &gyro, &accel, &mag);

  if (gyro == 3 && !hasVibratedForCalibration) {
    for (int i = 0; i < 3; i++) {
      ledcWrite(0, 50);
      delay(100);
      ledcWrite(0, 0);
      delay(100);
    }

    hasVibratedForCalibration = true;
  }

  // Send IMU data repeatedly after calibration feedback
  if (gyro == 3 && hasVibratedForCalibration) {
    unsigned long now = millis();
    if (now - lastIMUTime > imuInterval) {
      sensors_event_t event;
      bno.getEvent(&event);

      float x = event.orientation.x;
      float y = event.orientation.y;
      float z = event.orientation.z;

      String imuData = String(x) + "," + String(y) + "," + String(z);
      SerialBT.println(imuData);

      lastIMUTime = now;

      if (!hasSentCalibrateMessage) {
        SerialBT.println("calibrate");
        hasSentCalibrateMessage = true;
      }
    }
  }

  // Handle vibration intensity messages
  while (SerialBT.available()) {
    char c = SerialBT.read();

    if (c == '\n') {
      input.trim();
      int strength = input.toInt();
      strength = constrain(strength, 0, 255);
      ledcWrite(0, strength);
      input = "";
    } else {
      input += c;
    }
  }
}
