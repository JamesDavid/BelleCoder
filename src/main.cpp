// BelleCoder — main entry. Wires the HAL, screens, PAL, and the serial debug loop.
#include <Arduino.h>
#include "hal/Display.h"
#include "hal/Touch.h"
#include "core/App.h"
#include "pal/pal.h"
#include "services/Storage.h"
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
  pal::begin();

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
  app.loop();
  delay(5);
}
