#pragma once
// core/App.h — global app state + screen manager. Holds the live "scratch" sequence (the edit
// buffer, SPEC §6), the skill tier, editor selection, and capability flags. Screens read/write
// this and call go() to navigate.
#include "Screen.h"
#include "../model/Sequence.h"

enum class Tier : uint8_t { Kid, Advanced };

struct AppState {
  Sequence scratch;              // live edit buffer
  int      sel = 0;              // selected step index in the editor
  Tier     tier = Tier::Kid;
  bool     imuPresent = false;   // set at boot (M5)
  char     imuLabel[24] = "none";
  int      globalGapMs = 150;    // inter-step gap (SPEC §8)
  int      sensitivity = 5;      // capture sensitivity 1..10 (M6)
  bool     bounceToArms = true;  // map a bounce to arms (true) or a step (false)
  int      belleVolume = 3;      // doll speaker volume 0..5
};

class App {
public:
  void begin();
  void loop();

  void go(ScreenId id);          // switch screen (full repaint)
  ScreenId current() const { return _cur; }
  void repaint();                // re-enter current screen

  void registerScreen(ScreenId id, Screen* s) { _screens[(int)id] = s; }
  Screen* screen(ScreenId id) { return _screens[(int)id]; }

  void dispatchTap(int x, int y);

  AppState st;

private:
  Screen*  _screens[(int)ScreenId::COUNT] = {nullptr};
  ScreenId _cur = ScreenId::Home;
};

extern App app;
