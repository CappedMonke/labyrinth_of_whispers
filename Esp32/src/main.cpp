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

unsigned long lastIMUTime = 0;
const unsigned long imuInterval = 50;

struct Button {
  uint8_t pin;
  int lastState;
  uint8_t vibrationStrength;

  Button(uint8_t p, uint8_t strength)
      : pin(p), lastState(HIGH), vibrationStrength(strength) {}

  void setup() { pinMode(pin, INPUT_PULLUP); }

  void monitor(const char *buttonName) {
    int currentState = digitalRead(pin);
    delay(10);
    int stableState = digitalRead(pin);

    if (currentState == stableState && currentState != lastState) {
      lastState = currentState;

      if (currentState == LOW) {
        Serial.print(buttonName);
        Serial.println(" pressed.");
        ledcWrite(0, vibrationStrength);
      } else {
        Serial.print(buttonName);
        Serial.println(" released.");
        ledcWrite(0, 0);
      }
    }
  }
};

Button button1(PIN_BUTTON_1, 64);  // Weak vibration
Button button2(PIN_BUTTON_2, 128); // Medium vibration
Button button3(PIN_BUTTON_3, 192); // Strong vibration
Button button4(PIN_BUTTON_4, 255); // Maximum vibration

void setup() {
  Serial.begin(115200);
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
  button1.monitor("Button 1");
  button2.monitor("Button 2");
  button3.monitor("Button 3");
  button4.monitor("Button 4");

  unsigned long now = millis();
  if (now - lastIMUTime > imuInterval) {
    sensors_event_t event;
    bno.getEvent(&event);

    float x = event.orientation.x;
    float y = event.orientation.y;
    float z = event.orientation.z;

    String imuData = String(x) + "," + String(y) + "," + String(z);
    Serial.println("IMU Data: " + imuData);

    lastIMUTime = now;
  }
}
