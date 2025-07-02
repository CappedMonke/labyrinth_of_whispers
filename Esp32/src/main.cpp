#include <Adafruit_BNO055.h>
#include <Adafruit_Sensor.h>
#include <BluetoothSerial.h>
#include <Wire.h>

#define PIN_IMU_SDA 16
#define PIN_IMU_SCL 17
#define PIN_VIBRATION_MOTOR 25
#define PIN_BUTTON_1 22
#define PIN_BUTTON_2 23
#define PIN_BUTTON_3 32
#define PIN_BUTTON_4 33

Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28, &Wire);
BluetoothSerial SerialBT;
String bluetoothInput = "";

unsigned long lastIMUTime = 0;
const unsigned long imuInterval = 50;

struct Button {
  uint8_t pin;
  int lastState;
  String name;

  Button(uint8_t p, String name) : pin(p), lastState(HIGH), name(name) {}

  void setup() { pinMode(pin, INPUT_PULLUP); }

  void monitor() {
    int currentState = digitalRead(pin);
    delay(10);
    int stableState = digitalRead(pin);

    if (currentState == stableState && currentState != lastState) {
      lastState = currentState;

      String message = name;
      if (currentState == LOW) {
        message += "_pressed";
      } else {
        message += "_released";
      }
      Serial.println(message);
      SerialBT.println(message);
    }
  }
};

Button button1(PIN_BUTTON_1, "button_1");
Button button2(PIN_BUTTON_2, "button_2");
Button button3(PIN_BUTTON_3, "button_3");
Button button4(PIN_BUTTON_4, "button_4");

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

  button1.setup();
  button2.setup();
  button3.setup();
  button4.setup();
}

void loop() {
  button1.monitor();
  button2.monitor();
  button3.monitor();
  button4.monitor();

  unsigned long now = millis();
  if (now - lastIMUTime > imuInterval) {
    sensors_event_t event;
    bno.getEvent(&event);

    float x = event.orientation.x;
    float y = event.orientation.y;
    float z = event.orientation.z;

    String imuData = "imu_" + String(x) + "_" + String(y) + "_" + String(z);
    Serial.println(imuData);
    SerialBT.println(imuData);

    lastIMUTime = now;

    while (SerialBT.available()) {
      char c = SerialBT.read();

      if (c == '\n') {
        bluetoothInput.trim();
        int strength = bluetoothInput.toInt();
        strength = constrain(strength, 0, 255);
        ledcWrite(0, strength);
        bluetoothInput = "";
      } else {
        bluetoothInput += c;
      }
    }
  }
}
