#include "SerialConsole.h"
#include "../hal/Display.h"
#include "../hal/Touch.h"
#include "../core/App.h"
#include <Arduino.h>
#include <string.h>
#include <stdlib.h>

SerialConsole console;

void SerialConsole::begin() {
  Serial.println("[console] ready: S | T x y | H | G <name> | I");
}

static ScreenId screenByName(const char* n) {
  if (!strcmp(n,"home"))     return ScreenId::Home;
  if (!strcmp(n,"editor"))   return ScreenId::Editor;
  if (!strcmp(n,"palette"))  return ScreenId::Palette;
  if (!strcmp(n,"run"))      return ScreenId::Run;
  if (!strcmp(n,"settings")) return ScreenId::Settings;
  if (!strcmp(n,"dance"))    return ScreenId::Dance;
  return ScreenId::Home;
}

void SerialConsole::handle(char* line) {
  // trim
  while (*line==' ') line++;
  if (!*line) return;
  char op = line[0];
  switch (op) {
    case 'S': case 's':
      display.streamScreenshot();
      break;
    case 'T': case 't': {
      int x=0,y=0; if (sscanf(line+1, "%d %d", &x, &y)==2) { touch.injectTap(x,y); Serial.printf("[tap] %d,%d\n",x,y); }
      break;
    }
    case 'H': case 'h':
      app.go(ScreenId::Home); Serial.println("[nav] home");
      break;
    case 'G': case 'g': {
      char name[16]={0}; if (sscanf(line+1, "%15s", name)==1) { app.go(screenByName(name)); Serial.printf("[nav] %s\n", name); }
      break;
    }
    case 'P': case 'p': {
      // colour-calibration pattern: 8 vertical bands of known logical RGB565.
      static const uint16_t bands[8] = {
        0xF800/*red*/,0x07E0/*green*/,0x001F/*blue*/,0xFFE0/*yellow*/,
        0x07FF/*cyan*/,0xF81F/*magenta*/,0xFFFF/*white*/,0x8410/*gray*/ };
      int bw = SCREEN_W/8;
      for (int i=0;i<8;i++) display.gfx().fillRect(i*bw,0,bw,SCREEN_H,bands[i]);
      Serial.println("[cal] pattern R G B Y C M W gray");
      break;
    }
    case 'I': case 'i':
      Serial.printf("[info] screen=%d heap=%u fw=%s board=%s\n",
                    (int)app.current(), (unsigned)ESP.getFreeHeap(), BELLE_FW_VERSION, BOARD_NAME);
      break;
    default:
      Serial.printf("[console] ? %c\n", op);
  }
}

void SerialConsole::poll() {
  while (Serial.available()) {
    char c = (char)Serial.read();
    if (c=='\n' || c=='\r') {
      if (_len) { _buf[_len]=0; handle(_buf); _len=0; }
    } else if (_len < sizeof(_buf)-1) {
      _buf[_len++] = c;
    }
  }
}
