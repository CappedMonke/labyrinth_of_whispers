#include <Adafruit_BNO055.h>
#include <Adafruit_Sensor.h>
#include <BluetoothSerial.h>
#include <Wire.h>

#define PIN_IMU_SDA 14
#define PIN_IMU_SCL 12
#define PIN_VIBRATION_MOTOR 25
#define PIN_BUTTON_1 32
#define PIN_BUTTON_2 33
#define PIN_BUTTON_3 34
#define PIN_BUTTON_4 35

Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28, &Wire);
BluetoothSerial SerialBT;

unsigned long lastIMUTime = 0;
const unsigned long imuInterval = 50;
String input = "";
bool hasVibratedForCalibration = false;
bool hasSentCalibrateMessage = false;
unsigned long startTime = 0;
bool hasWaited = false;

bool lastButton1State = HIGH;
bool lastButton2State = HIGH;
bool lastButton3State = HIGH;
bool lastButton4State = HIGH;

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

  pinMode(PIN_BUTTON_1, INPUT_PULLUP);
  pinMode(PIN_BUTTON_2, INPUT_PULLUP);
  pinMode(PIN_BUTTON_3, INPUT_PULLUP);
  pinMode(PIN_BUTTON_4, INPUT_PULLUP);
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
  bool currButton1 = digitalRead(PIN_BUTTON_1);
  bool currButton2 = digitalRead(PIN_BUTTON_2);
  bool currButton3 = digitalRead(PIN_BUTTON_3);
  bool currButton4 = digitalRead(PIN_BUTTON_4);

  // Calibrate button
  if (lastButton1State == HIGH && currButton1 == LOW) {
    Serial.println("Calibrate button pressed");
    SerialBT.println("calibrate");
  }

  // Reset button
  if (lastButton2State == HIGH && currButton2 == LOW) {
    Serial.println("Reset button pressed");
    SerialBT.println("reset");
  }

  // Accept button
  if (lastButton3State == HIGH && currButton3 == LOW) {
    Serial.println("Accept button pressed");
    SerialBT.println("accept");
  }

  // Accept button
  if (lastButton4State == HIGH && currButton3 == LOW) {
    Serial.println("Accept button pressed");
    SerialBT.println("button4");
  }

  // Update states
  lastButton1State = currButton1;
  lastButton2State = currButton2;
  lastButton3State = currButton3;
  lastButton4State = currButton4;

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
      Serial.println("IMU Data: " + imuData);
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
