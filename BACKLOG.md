# BelleCoder — Backlog

Ideas not yet built, grouped by what they need. The shipped features (LED choreography, Belle
battery + volume, Draw-a-Dance, Dance-Along, Live Mirror, Simon Says) are in the README.

## Needs the real doll (§11.4 live pass)
These depend on notify payloads / timing we could only see *statically* in the decompile.
- **Autonomous queue playback** — preload a whole routine into the doll's sequence queue
  (`EnqueueSequenceCommand` + `CheckSequenceQueueFreeSpace`) and let Belle dance it solo, tightly
  synced to onboard audio, so the wand can be set down. Needs the freespace-notify format.
- **Necklace-driven games** — confirm the input-state notify (`RequestInputState` / necklace
  button) so Simon Says (and new reaction games) can use Belle's button as real input, not just a
  start trigger.
- **Belle battery** — confirm the `RequestLVDValue` reply mapping so the % readout is accurate.
- **Motion/audio timing calibration** — real wheel power/on-time for "one step" and "90°/180°
  twirl", the app-mode keep-alive cadence, and per-move durations (feeds `durationOf()`).

## Doable now (SIMULATE-testable)
- **Freeze Dance** — music plays, stops at random; Belle freezes (settle); last one moving loses.
- **Beat / tempo tools** — a BPM metronome and "snap steps to the beat" so built or danced routines
  land on a beat (the meme-dance beat generator).
- **Perform mode** — full-screen stage/curtain animation with a big countdown while Belle runs the
  routine and announces the dance name (`eHLS_DanceName` clips).
- **Personality / idle** — trigger Belle's idle chatter (`eHLS_IdlePhrase`, ~20 clips) when the
  child's been away, to invite play.
- **LED keyframes / fades** — `FadeLED` over a duration and multi-colour patterns, beyond the
  single `SetLED` colour move.
- **Nested REPEAT / mini-AST** — multi-level loops and grouped spans (v0.1 is single-level).
- **Save / share** — export a dance as a QR code or a short code to re-enter on another wand.

## Content / tooling
- **Phrase catalog transcription** — record the doll playing each of the ~421 audio indices and
  transcribe them into `data/audio_catalog.csv`, upgrading the functional labels ("Cue: Spin!") to
  actual words.
- **Richer phrase palette** — surface the labelled `PLAY_PHRASE` catalog in the Sound tab (grouped
  by Cues / Praise / Greetings) instead of raw indices.

## Platform / hardware
- **CrowPanel S3 5.0-HMI bring-up** — the env + HAL branch exist; finish the parallel-RGB / GT911
  path so the big-screen variant runs (BLE half is identical).
- **Battery-tap calibration** — trim the wand's LiPo divider math against a multimeter; low-battery
  warning + auto-dim.
- **Enclosure iterations** — print/fit passes on `hardware/wand.scad` for the chosen IMU + cell.
