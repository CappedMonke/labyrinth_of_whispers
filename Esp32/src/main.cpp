#include <Adafruit_BNO055.h>
#include <Adafruit_DRV2605.h>
#include <Adafruit_Sensor.h>
#include <BluetoothSerial.h>
#include <Wire.h>

Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28);
Adafruit_DRV2605 drv;
BluetoothSerial SerialBT;

bool vibrationEnabled = false;
unsigned long lastIMUTime = 0;
unsigned long lastVibrationTime = 0;
const unsigned long imuInterval = 30;
const unsigned long vibrationInterval = 50;

void setup() {
  Serial.begin(115200);
  SerialBT.begin("BlindCane");
  Serial.println("Bluetooth device started, pair with 'BlindCane'");

  if (!bno.begin()) {
    Serial.println("BNO055 not found!");
    while (1)
      ;
  }
  bno.setExtCrystalUse(true);

  if (!drv.begin()) {
    Serial.println("DRV2605 not found!");
    while (1)
      ;
  }
  drv.selectLibrary(1);
  drv.setMode(DRV2605_MODE_INTTRIG);
}

void loop() {
  unsigned long now = millis();

  // Send IMU data at regular intervals
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

  // Continuously vibrate if enabled
  if (vibrationEnabled && now - lastVibrationTime > vibrationInterval) {
    drv.setWaveform(0, 75); // Buzz 3 (strongest built-in)
    drv.setWaveform(1, 75);
    drv.setWaveform(2, 75);
    drv.setWaveform(3, 0); // End
    drv.go();
    lastVibrationTime = now;
  }

  // Always check for incoming Bluetooth commands
  while (SerialBT.available()) {
    char c = SerialBT.read();
    if (c == '1') {
      vibrationEnabled = true;
    } else if (c == '0') {
      vibrationEnabled = false;
    }
  }
}
