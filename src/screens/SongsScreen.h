#pragma once
#include "../core/Screen.h"

// Dance-Along — a jukebox of Belle's named songs. Tap one and she performs (plays the song; on
// the real doll her built-in choreography runs with it). "Now playing" is highlighted.
class SongsScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
private:
  void drawList();
  int _scroll = 0;
  int _playing = -1;
};
extern SongsScreen songsScreen;
