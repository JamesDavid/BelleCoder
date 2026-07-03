#include "SerialConsole.h"
#include "../hal/Display.h"
#include "../hal/Touch.h"
#include "../core/App.h"
#include "../imu/Imu.h"
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
  if (!strcmp(n,"load"))     return ScreenId::Load;
  if (!strcmp(n,"play"))     return ScreenId::Play;
  if (!strcmp(n,"draw"))     return ScreenId::Draw;
  if (!strcmp(n,"songs"))    return ScreenId::Songs;
  if (!strcmp(n,"mirror"))   return ScreenId::Mirror;
  if (!strcmp(n,"games"))    return ScreenId::Games;
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
    case 'D': case 'd': {                 // draw a stroke point: "D x y" (or "D x y u" = pen up)
      int x=0,y=0; char up=0;
      if (sscanf(line+1, "%d %d %c", &x, &y, &up)>=2) touch.injectDraw(x,y, up=='u'||up=='U');
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
      Serial.printf("[info] screen=%d heap=%u imu=%s fw=%s board=%s\n",
                    (int)app.current(), (unsigned)ESP.getFreeHeap(), app.st.imuLabel,
                    BELLE_FW_VERSION, BOARD_NAME);
      break;
    case 'A': case 'a': {                 // stream accel/gyro for a serial plot (M5)
      ImuSample s; Serial.println("ax ay az gx gy gz");
      for (int i=0;i<120;i++){ if (imu::read(s)) Serial.printf("%.3f %.3f %.3f %.2f %.2f %.2f\n",
                               s.ax,s.ay,s.az,s.gx,s.gy,s.gz); delay(10); }
      break;
    }
    case 'F': case 'f': {                 // toggle a synthetic IMU so the capture UI is demoable
      bool on = !imu::isSynthetic();
      imu::forceSynthetic(on);
      app.st.imuPresent = on; strncpy(app.st.imuLabel, imu::label(), sizeof app.st.imuLabel-1);
      Serial.printf("[imu] synthetic %s\n", on?"ON":"OFF");
      app.repaint();
      break;
    }
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
