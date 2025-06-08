#include <Adafruit_BNO055.h>
#include <Adafruit_Sensor.h>
#include <BluetoothSerial.h>
#include <Wire.h>

#define PIN_IMU_SDA 32
#define PIN_IMU_SCL 33
#define PIN_VIBRATION_MOTOR 13

Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28, &Wire);
BluetoothSerial SerialBT;

unsigned long lastIMUTime = 0;
const unsigned long imuInterval = 30;
String input = "";

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
}

void loop() {
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
  }

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