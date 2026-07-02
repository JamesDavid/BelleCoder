#include "App.h"
#include "../hal/Touch.h"
#include <Arduino.h>

App app;

void App::go(ScreenId id) {
  if (_screens[(int)id] == nullptr) return;
  _cur = id;
  _screens[(int)id]->enter();
}

void App::repaint() {
  if (_screens[(int)_cur]) _screens[(int)_cur]->enter();
}

void App::dispatchTap(int x, int y) {
  if (_screens[(int)_cur]) _screens[(int)_cur]->onTap(x, y);
}

void App::begin() {
  st.scratch.clear();
  st.scratch.setName("My Dance");
}

void App::loop() {
  TouchPoint tp;
  if (touch.poll(tp) && tp.pressed) {
    dispatchTap(tp.x, tp.y);
  }
  if (_screens[(int)_cur]) _screens[(int)_cur]->tick(millis());
}
