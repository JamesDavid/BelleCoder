// BelleCoder — main entry. Wires the HAL, screens, PAL, and the serial debug loop.
#include <Arduino.h>
#include "hal/Display.h"
#include "hal/Touch.h"
#include "core/App.h"
#include "pal/pal.h"
#include "services/Storage.h"
#include "services/Ble.h"
#include "services/Battery.h"
#include "imu/Imu.h"
#include "debug/SerialConsole.h"

#include "screens/HomeScreen.h"
#include "screens/EditorScreen.h"
#include "screens/PaletteScreen.h"
#include "screens/RunScreen.h"
#include "screens/SettingsScreen.h"
#include "screens/DanceScreen.h"
#include "screens/LoadScreen.h"

void setup() {
  Serial.begin(115200);
  delay(120);
  Serial.println("\n[BelleCoder] " BELLE_FW_VERSION "  " BOARD_NAME);

  display.begin();
  touch.begin();
  storage::begin();
  ble::begin();
  battery::begin();
  pal::begin();

  // Optional IMU: probe I2C, verify chip-ID; gate the capture feature on the result (SPEC §9).
  imu::detect();
  if (imu::begin()) { app.st.imuPresent = true;  strncpy(app.st.imuLabel, imu::label(), sizeof app.st.imuLabel-1); }
  else              { app.st.imuPresent = false; strncpy(app.st.imuLabel, "none", sizeof app.st.imuLabel-1); }

  app.registerScreen(ScreenId::Home,     &homeScreen);
  app.registerScreen(ScreenId::Editor,   &editorScreen);
  app.registerScreen(ScreenId::Palette,  &paletteScreen);
  app.registerScreen(ScreenId::Run,      &runScreen);
  app.registerScreen(ScreenId::Settings, &settingsScreen);
  app.registerScreen(ScreenId::Dance,    &danceScreen);
  app.registerScreen(ScreenId::Load,     &loadScreen);

  app.begin();
  console.begin();
  app.go(ScreenId::Home);
}

void loop() {
  console.poll();
  ble::tick();
  pal::tick();
  battery::tick();
  app.loop();
  delay(5);
}
