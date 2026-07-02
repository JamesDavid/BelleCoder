#pragma once
// core/Canvas.h — thin drawing abstraction over the HAL display. Screens draw against this,
// never the concrete LGFX type (keeps the UI board-agnostic, mirrors Overhead's core/Canvas).
#include <stdint.h>
#include "../hal/Board.h"      // SCREEN_W/H, BOARD_NAME, pin/cap macros
#include "../model/Move.h"

enum class Align { L, C, R };

class Canvas {
public:
  int W() const;
  int H() const;

  void clear(uint16_t c);
  void fillRect(int x, int y, int w, int h, uint16_t c);
  void rect(int x, int y, int w, int h, uint16_t c);
  void fillRound(int x, int y, int w, int h, int r, uint16_t c);
  void round(int x, int y, int w, int h, int r, uint16_t c);
  void line(int x0, int y0, int x1, int y1, uint16_t c);
  void fillCircle(int x, int y, int r, uint16_t c);
  void circle(int x, int y, int r, uint16_t c);
  void fillTriangle(int x0,int y0,int x1,int y1,int x2,int y2,uint16_t c);

  // text: size is an integer scale (1..). Returns nothing; use textWidth to measure.
  void text(const char* s, int x, int y, uint16_t c, int size = 1, Align a = Align::L);
  int  textWidth(const char* s, int size = 1);

  // higher-level widgets
  void title(const char* s);                             // top gold header bar
  void button(int x,int y,int w,int h,const char* label,uint16_t fill,uint16_t txt,bool sel=false);
  void card(int x,int y,int w,int h,uint16_t fill,bool sel=false);
  // draw a category/move glyph centred in a box (locomotion arrow, spin swirl, arms, note...)
  void moveGlyph(MoveId m, int cx, int cy, int r, uint16_t c);
  void catGlyph(MoveCat cat, int cx, int cy, int r, uint16_t c);
};

extern Canvas canvas;
