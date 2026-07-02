#pragma once
// hal/LGFX_Config.h — concrete LovyanGFX device for the active board variant.
// The ONE place that knows the panel/bus/touch wiring. Everything above the HAL draws
// against core/Canvas, never this type. (Config carried from ../BladeKey-Overhead.)

#define LGFX_USE_V1
#include <LovyanGFX.hpp>
#include "Board.h"

// ===========================================================================
#if defined(BOARD_CYD28_ILI9341)
// ===========================================================================
class LGFX : public lgfx::LGFX_Device {
  lgfx::Panel_ILI9341  _panel;
  lgfx::Bus_SPI        _bus;
  lgfx::Light_PWM      _light;
  lgfx::Touch_XPT2046  _touch;

public:
  LGFX() {
    { // Display SPI bus (HSPI: 12/13/14)
      auto cfg = _bus.config();
      cfg.spi_host    = SPI2_HOST;
      cfg.spi_mode    = 0;
      cfg.freq_write  = 40000000;
      cfg.freq_read   = 16000000;
      cfg.spi_3wire   = false;
      cfg.use_lock    = true;
      cfg.dma_channel = SPI_DMA_CH_AUTO;
      cfg.pin_sclk    = PIN_TFT_SCLK;
      cfg.pin_mosi    = PIN_TFT_MOSI;
      cfg.pin_miso    = PIN_TFT_MISO;
      cfg.pin_dc      = PIN_TFT_DC;
      _bus.config(cfg);
      _panel.setBus(&_bus);
    }
    { // Panel (ILI9341) — landscape-native 320x240 at MV=0 (see Board.h)
      auto cfg = _panel.config();
      cfg.pin_cs           = PIN_TFT_CS;
      cfg.pin_rst          = PIN_TFT_RST;
      cfg.pin_busy         = -1;
      cfg.panel_width      = TFT_PANEL_WIDTH;
      cfg.panel_height     = TFT_PANEL_HEIGHT;
      cfg.memory_width     = TFT_PANEL_WIDTH;
      cfg.memory_height    = TFT_PANEL_HEIGHT;
      cfg.offset_x         = 0;
      cfg.offset_y         = 0;
      cfg.offset_rotation  = 0;
      cfg.dummy_read_pixel = 8;
      cfg.dummy_read_bits  = 1;
      cfg.readable         = true;      // needed for screenshot readback
      cfg.invert           = (CYD_INVERT_DISPLAY != 0);
      cfg.rgb_order        = (CYD_PANEL_RGB_ORDER != 0);
      cfg.dlen_16bit       = false;
      cfg.bus_shared       = false;     // touch is on its own bus
      _panel.config(cfg);
    }
    { // Backlight
      auto cfg = _light.config();
      cfg.pin_bl      = PIN_TFT_BL;
      cfg.invert      = (BACKLIGHT_ACTIVE_HIGH == 0);
      cfg.freq        = BACKLIGHT_PWM_FREQ;
      cfg.pwm_channel = 7;
      _light.config(cfg);
      _panel.setLight(&_light);
    }
    { // Touch (XPT2046 on its own VSPI bus)
      auto cfg = _touch.config();
      cfg.x_min          = 300;
      cfg.x_max          = 3900;
      cfg.y_min          = 200;
      cfg.y_max          = 3700;
      cfg.pin_int        = -1;          // poll pressure; do NOT gate on IRQ (cyd.md)
      cfg.bus_shared     = false;
      cfg.offset_rotation = 0;
      cfg.spi_host       = SPI3_HOST;   // VSPI, separate from display
      cfg.freq           = 1000000;
      cfg.pin_sclk       = PIN_TOUCH_SCLK;
      cfg.pin_mosi       = PIN_TOUCH_MOSI;
      cfg.pin_miso       = PIN_TOUCH_MISO;
      cfg.pin_cs         = PIN_TOUCH_CS;
      _touch.config(cfg);
      _panel.setTouch(&_touch);
    }
    setPanel(&_panel);
  }
};

// ===========================================================================
#elif defined(BOARD_CROWPANEL_S3_5HMI)
// ===========================================================================
// Placeholder device so the env compiles; full RGB/GT911 bring-up deferred (SPEC §2).
class LGFX : public lgfx::LGFX_Device {
public:
  LGFX() {}
};

#endif
