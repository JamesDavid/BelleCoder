// BelleCoder wand enclosure — parametric handle + display head for the 2.8" CYD.
// SPEC §2 / §M8: a drop-tolerant, untethered wand a child holds and waves; the IMU is mounted
// RIGIDLY to the board so gyro/accel track the wand's true motion. Two printed parts:
//   part = "front" : bezel/face that frames the LCD + touch window
//   part = "back"  : shell with the handle, LiPo cavity, IMU boss, wrist-strap loop
// Render one at a time (set `part`), export STL, print in opaque black PETG (drop-tough +
// blocks LCD light bleed to the LDR, cyd.md).

/* [What to render] */
part = "back";            // "front" | "back" | "preview"

/* [CYD board] (ESP32-2432S028R, measure your unit) */
board_w      = 86.0;      // long edge
board_h      = 50.0;      // short edge
board_t      = 1.6;       // PCB thickness
comp_clear   = 14.0;      // depth for the LCD + rear components
screw_pitch_x = 78.0;     // M3 mounting-hole pitch (cyd.md: inset 4mm)
screw_pitch_y = 42.0;

/* [Window] visible active area of the 2.8" panel */
win_w        = 60.0;
win_h        = 43.0;
win_off_x    = 0.0;       // nudge if the active area isn't centred
win_off_y    = 0.0;

/* [Shell] */
wall         = 2.4;
corner_r     = 5.0;
front_lip    = 1.6;       // how far the bezel overlaps the LCD glass

/* [Handle] */
handle_len   = 95.0;      // grip length (small hands)
handle_dia   = 26.0;      // grip diameter
handle_flat  = 3.0;       // flatten sides for a non-roll, easy-grip oval

/* [Battery] 503450-class 1000mAh LiPo (adjust to your cell) */
batt_w       = 34.0;
batt_h       = 50.0;
batt_t       = 6.0;

/* [IMU] breakout (e.g. GY-521 MPU-6050 ~21x16mm, or a bare LSM6DSO board) */
imu_w        = 21.0;
imu_h        = 16.0;
imu_boss_h   = 4.0;       // standoff height; keep SHORT + rigid (no cable slack)

/* [Strap] wrist-strap slot */
strap_w      = 12.0;
strap_t      = 3.0;

$fn = 48;
eps = 0.02;

// ---- helpers -------------------------------------------------------------
module rrect(w, h, r) { hull() for (sx=[-1,1], sy=[-1,1]) translate([sx*(w/2-r), sy*(h/2-r)]) circle(r); }
module rbox(w, h, d, r) { linear_extrude(d) rrect(w, h, r); }

module screw_posts(h, hole=false) {
  for (sx=[-1,1], sy=[-1,1])
    translate([sx*screw_pitch_x/2, sy*screw_pitch_y/2, 0])
      if (hole) cylinder(h=h+eps, d=2.6);      // M3 self-tap pilot
      else difference() { cylinder(h=h, d=6.5); translate([0,0,-eps]) cylinder(h=h+eps, d=2.6); }
}

// ---- FRONT: bezel that frames the LCD + touch window ---------------------
module front() {
  head_w = board_w + 2*wall;
  head_h = board_h + 2*wall;
  difference() {
    union() {
      rbox(head_w, head_h, wall+front_lip, corner_r);
      // inner rim that sits around the LCD glass
      translate([0,0,wall]) linear_extrude(comp_clear*0.4)
        difference() { rrect(board_w+0.6, board_h+0.6, corner_r-1);
                       rrect(board_w-2,   board_h-2,   corner_r-1); }
    }
    // touch window (through the face)
    translate([win_off_x, win_off_y, -eps]) rbox(win_w, win_h, wall+front_lip+2*eps, 3);
    // LDR pinhole (offset to a corner near the short edge, cyd.md)
    translate([board_w/2-9, -board_h/2+7, -eps]) cylinder(h=wall+front_lip+2*eps, d=3.5);
  }
}

// ---- BACK: shell + handle + battery + IMU boss + strap -------------------
module back() {
  head_w = board_w + 2*wall;
  head_h = board_h + 2*wall;
  difference() {
    union() {
      // head shell (open toward the front)
      difference() {
        rbox(head_w, head_h, wall+comp_clear, corner_r);
        translate([0,0,wall]) rbox(board_w+0.4, board_h+0.4, comp_clear+eps, corner_r-1);
      }
      // board screw posts rising from the shell floor
      translate([0,0,wall]) screw_posts(comp_clear-board_t-3);
      // handle: oval grip extending off the short edge, angled slightly for the wrist
      translate([0, -head_h/2+2, wall+comp_clear/2])
        rotate([90,0,0]) handle();
      // IMU mount boss (rigid, centred on the board's inertial axis)
      translate([0,0,wall]) imu_boss();
    }
    // battery cavity in the head, behind the board area
    translate([0, head_h/2-batt_h/2-6, wall+comp_clear-batt_t-0.4])
      rbox(batt_w, batt_h, batt_t+eps, 2);
    // strap slot through the handle end
    translate([0, -head_h/2 - handle_len + 8, wall+comp_clear/2])
      cube([strap_w, strap_t, handle_dia*2], center=true);
    // USB-C / charge port access on the short edge opposite the handle
    translate([0, head_h/2-eps, wall+comp_clear-6]) rotate([90,0,0]) rbox(12, 7, wall+2*eps, 2);
    screw_posts(wall+comp_clear+eps, hole=true);
  }
}

module handle() {
  // oval cross-section, flattened sides, hollow for wiring + boost board
  difference() {
    linear_extrude(handle_len) scale([1, (handle_dia-2*handle_flat)/handle_dia]) circle(d=handle_dia);
    translate([0,0,-eps]) linear_extrude(handle_len-wall+eps)
      scale([1,(handle_dia-2*handle_flat)/handle_dia]) circle(d=handle_dia-2*wall);
  }
}

module imu_boss() {
  translate([0, 6, comp_clear-board_t-imu_boss_h-3])
    difference() {
      rbox(imu_w+4, imu_h+4, imu_boss_h, 2);
      // 2x M2 pilot holes at a typical breakout spacing
      for (sx=[-1,1]) translate([sx*(imu_w/2-2), 0, -eps]) cylinder(h=imu_boss_h+2*eps, d=1.6);
    }
}

// ---- render --------------------------------------------------------------
if (part == "front") front();
else if (part == "back") back();
else { back(); translate([0,0,40]) color("gray",0.4) front(); }
