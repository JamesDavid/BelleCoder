#pragma once
// debug/SerialConsole.h — the autonomous debug loop (GridBot / PIO_DEBUG). Line commands:
//   S            stream a screenshot (framed RGB565 -> host PNG)
//   T <x> <y>    inject a tap at screen px (drives the UI headless)
//   H            go Home
//   G <name>     go to a screen: home|editor|palette|run|settings|dance
//   I            print info (active screen, heap, fw)
// So the whole app is buildable/verifiable with no human holding the board.
#include <stdint.h>

class SerialConsole {
public:
  void begin();
  void poll();     // call every loop()
private:
  void handle(char* line);
  char _buf[64];
  uint8_t _len = 0;
};
extern SerialConsole console;
