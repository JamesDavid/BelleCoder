#include "Canvas.h"
#include "Theme.h"
#include "../hal/Display.h"
#include <math.h>
#include <string.h>

Canvas canvas;
static LGFX& G() { return display.gfx(); }

int Canvas::W() const { return SCREEN_W; }
int Canvas::H() const { return SCREEN_H; }

void Canvas::clear(uint16_t c) { G().fillScreen(c); }
void Canvas::fillRect(int x,int y,int w,int h,uint16_t c){ G().fillRect(x,y,w,h,c); }
void Canvas::rect(int x,int y,int w,int h,uint16_t c){ G().drawRect(x,y,w,h,c); }
void Canvas::fillRound(int x,int y,int w,int h,int r,uint16_t c){ G().fillRoundRect(x,y,w,h,r,c); }
void Canvas::round(int x,int y,int w,int h,int r,uint16_t c){ G().drawRoundRect(x,y,w,h,r,c); }
void Canvas::line(int x0,int y0,int x1,int y1,uint16_t c){ G().drawLine(x0,y0,x1,y1,c); }
void Canvas::fillCircle(int x,int y,int r,uint16_t c){ G().fillCircle(x,y,r,c); }
void Canvas::circle(int x,int y,int r,uint16_t c){ G().drawCircle(x,y,r,c); }
void Canvas::fillTriangle(int x0,int y0,int x1,int y1,int x2,int y2,uint16_t c){ G().fillTriangle(x0,y0,x1,y1,x2,y2,c); }

int Canvas::textWidth(const char* s, int size){ return (int)strlen(s) * 6 * size; }

void Canvas::text(const char* s, int x, int y, uint16_t c, int size, Align a) {
  G().setTextColor(c);
  G().setTextSize(size);
  G().setTextDatum(a == Align::L ? TL_DATUM : a == Align::C ? TC_DATUM : TR_DATUM);
  G().drawString(s, x, y);
}

void Canvas::title(const char* s) {
  fillRect(0, 0, W(), 26, Theme::BG_ALT);
  fillRect(0, 26, W(), 2, Theme::GOLD_DIM);
  text(s, W()/2, 6, Theme::GOLD, 2, Align::C);
}

void Canvas::card(int x,int y,int w,int h,uint16_t fill,bool sel){
  fillRound(x,y,w,h,6,fill);
  if (sel) round(x,y,w,h,6,Theme::GOLD);
}

void Canvas::button(int x,int y,int w,int h,const char* label,uint16_t fill,uint16_t txt,bool sel){
  fillRound(x,y,w,h,7,fill);
  if (sel) { round(x,y,w,h,7,Theme::GOLD); round(x+1,y+1,w-2,h-2,6,Theme::GOLD); }
  // pick the largest size that fits the button width, then shrink until it fits (no run-off)
  int sz = (h >= 40) ? 2 : 1;
  while (sz > 1 && textWidth(label, sz) > w - 8) sz--;
  if (sz == 1 && textWidth(label, 1) > w - 4) {
    // still too wide at size 1: split on the last space into two lines
    const char* sp = nullptr;
    for (const char* p = label; *p; ++p) if (*p == ' ') sp = p;
    if (sp) {
      char a[24]; int n = (int)(sp - label); if (n > 23) n = 23;
      memcpy(a, label, n); a[n] = 0;
      text(a,     x + w/2, y + h/2 - 9, txt, 1, Align::C);
      text(sp+1,  x + w/2, y + h/2 + 1, txt, 1, Align::C);
      return;
    }
  }
  text(label, x + w/2, y + (h - 8*sz)/2, txt, sz, Align::C);
}

uint16_t catColor(MoveCat c) {
  switch (c) {
    case MoveCat::Move:   return Theme::C_MOVE;
    case MoveCat::Spin:   return Theme::C_SPIN;
    case MoveCat::Arms:   return Theme::C_ARMS;
    case MoveCat::Head:   return Theme::C_HEAD;
    case MoveCat::Light:  return Theme::GOLD;
    case MoveCat::Sound:  return Theme::C_SOUND;
    case MoveCat::Dance:  return Theme::C_DANCE;
    case MoveCat::Wait:   return Theme::C_WAIT;
    case MoveCat::Repeat: return Theme::C_REPEAT;
    default:              return Theme::TEXT_DIM;
  }
}

// ---- glyphs --------------------------------------------------------------
static void arrowUp(int cx,int cy,int r,uint16_t c){ G().fillTriangle(cx,cy-r,cx-r,cy+r/2,cx+r,cy+r/2,c); G().fillRect(cx-r/3,cy,2*(r/3)+1,r,c); }
static void arrowDown(int cx,int cy,int r,uint16_t c){ G().fillTriangle(cx,cy+r,cx-r,cy-r/2,cx+r,cy-r/2,c); G().fillRect(cx-r/3,cy-r,2*(r/3)+1,r,c); }
static void arrowLeft(int cx,int cy,int r,uint16_t c){ G().fillTriangle(cx-r,cy,cx+r/2,cy-r,cx+r/2,cy+r,c); }
static void arrowRight(int cx,int cy,int r,uint16_t c){ G().fillTriangle(cx+r,cy,cx-r/2,cy-r,cx-r/2,cy+r,c); }

static void swirl(int cx,int cy,int r,uint16_t c,bool cw){
  for (int a = 20; a < 320; a += 12) {
    float t = a * 3.14159f / 180.0f;
    float rr = r * (a / 320.0f);
    int x = cx + (int)(cosf(t) * rr), y = cy + (int)(sinf(t) * rr);
    G().fillCircle(x, y, 1, c);
  }
  if (cw) arrowRight(cx + r - 2, cy - r/2, r/3, c); else arrowLeft(cx - r + 2, cy - r/2, r/3, c);
}

static void stickArms(int cx,int cy,int r,uint16_t c,int lArm,int rArm){
  // body
  G().fillCircle(cx, cy - r + 3, 3, c);            // head
  G().drawLine(cx, cy - r + 6, cx, cy + r - 3, c); // torso
  // arms: dir -1 = down, 0 = out, +1 = up
  auto arm=[&](int dir,int sx){ int ey = cy - (dir)*r/2; G().drawLine(cx, cy - r/3, cx + sx*r, ey, c); G().drawLine(cx+1, cy - r/3, cx + sx*r, ey, c); };
  arm(lArm, -1); arm(rArm, +1);
}

static void note(int cx,int cy,int r,uint16_t c){
  G().fillCircle(cx - r/2, cy + r/2, r/3, c);
  G().fillRect(cx - r/2 + r/3 - 1, cy - r/2, 2, r, c);
  G().fillRect(cx - r/2 + r/3 - 1, cy - r/2, r/2, 2, c);
}

static void hourglass(int cx,int cy,int r,uint16_t c){
  G().drawLine(cx-r,cy-r,cx+r,cy-r,c); G().drawLine(cx-r,cy+r,cx+r,cy+r,c);
  G().drawLine(cx-r,cy-r,cx+r,cy+r,c); G().drawLine(cx+r,cy-r,cx-r,cy+r,c);
}

static void loop(int cx,int cy,int r,uint16_t c){ G().drawCircle(cx,cy,r,c); G().drawCircle(cx,cy,r-1,c); arrowRight(cx+r,cy,r/3,c); }

static void star(int cx,int cy,int r,uint16_t c){
  for (int i=0;i<5;i++){ float a=(-90+i*72)*3.14159f/180.0f; int x=cx+(int)(cosf(a)*r),y=cy+(int)(sinf(a)*r); G().drawLine(cx,cy,x,y,c); G().drawLine(cx-1,cy,x,y,c);} }

static void bulb(int cx,int cy,int r,uint16_t c){
  G().fillCircle(cx, cy-r/4, (r*3)/4, c);              // glass
  G().fillRect(cx-r/3, cy+r/2, 2*(r/3)+1, r/3, c);     // base
  // sparkle rays
  for (int i=0;i<4;i++){ float a=(-90+i*90+45)*3.14159f/180.0f;
    G().drawLine(cx+(int)(cosf(a)*(r+2)), cy-r/4+(int)(sinf(a)*(r+2)),
                 cx+(int)(cosf(a)*(r+6)), cy-r/4+(int)(sinf(a)*(r+6)), c); }
}

void Canvas::catGlyph(MoveCat cat, int cx, int cy, int r, uint16_t c) {
  switch (cat) {
    case MoveCat::Move:   arrowUp(cx,cy,r,c); break;
    case MoveCat::Spin:   swirl(cx,cy,r,c,true); break;
    case MoveCat::Arms:   stickArms(cx,cy,r,c,1,1); break;
    case MoveCat::Head:   G().fillCircle(cx,cy,r-2,c); arrowRight(cx,cy,r/2,Theme::BG); break;
    case MoveCat::Light:  bulb(cx,cy,r,c); break;
    case MoveCat::Sound:  note(cx,cy,r,c); break;
    case MoveCat::Dance:  star(cx,cy,r,c); break;
    case MoveCat::Wait:   hourglass(cx,cy,r,c); break;
    case MoveCat::Repeat: loop(cx,cy,r,c); break;
    default: break;
  }
}

void Canvas::moveGlyph(MoveId m, int cx, int cy, int r, uint16_t c) {
  switch (m) {
    case STEP_FORWARD: arrowUp(cx,cy,r,c); break;
    case STEP_BACK:    arrowDown(cx,cy,r,c); break;
    case TWIRL_LEFT:   swirl(cx,cy,r,c,false); break;
    case TWIRL_RIGHT:  swirl(cx,cy,r,c,true); break;
    case ARM_L_UP:     stickArms(cx,cy,r,c,1,-1); break;
    case ARM_L_DOWN:   stickArms(cx,cy,r,c,-1,-1); break;
    case ARM_R_UP:     stickArms(cx,cy,r,c,-1,1); break;
    case ARM_R_DOWN:   stickArms(cx,cy,r,c,-1,-1); break;
    case ARMS_UP:      stickArms(cx,cy,r,c,1,1); break;
    case ARMS_DOWN:    stickArms(cx,cy,r,c,-1,-1); break;
    case HEAD_LEFT:    G().fillCircle(cx,cy,r-2,c); arrowLeft(cx,cy,r/2,Theme::BG); break;
    case HEAD_RIGHT:   G().fillCircle(cx,cy,r-2,c); arrowRight(cx,cy,r/2,Theme::BG); break;
    case HEAD_CENTER:  G().fillCircle(cx,cy,r-2,c); G().fillCircle(cx,cy,2,Theme::BG); break;
    case LED_COLOR:    bulb(cx,cy,r,c); break;
    case PLAY_SONG:    note(cx,cy,r,c); break;
    case PLAY_PHRASE:  note(cx,cy,r,c); G().fillCircle(cx+r/2,cy-r/2,2,c); break;
    case PLAY_DANCE:   star(cx,cy,r,c); break;
    case WAIT:         hourglass(cx,cy,r,c); break;
    case REPEAT:       loop(cx,cy,r,c); break;
    default: break;
  }
}
