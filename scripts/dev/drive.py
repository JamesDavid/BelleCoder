#!/usr/bin/env python
"""BelleCoder serial UI driver (the PIO_DEBUG / GridBot loop).

Opens the CYD once, resets to a known boot state, then runs a semicolon-separated command
script and captures screenshots — so the whole UI is verifiable with no human at the board.

Console commands (firmware src/debug/SerialConsole.cpp):
  nav <name>   -> 'G <name>'  (home|editor|palette|run|settings|dance)
  tap <x> <y>  -> 'T x y'
  wait <s>
  shot <path>  -> 'S' then decode the RGB565 stream to PNG

  python scripts/dev/drive.py COM5 "wait 3; shot docs/img/home.png; nav editor; shot docs/img/editor.png"
"""
import sys, time
import serial            # pyserial
from PIL import Image


def read_exact(ser, n):
    buf = bytearray(); deadline = time.time() + 30
    while len(buf) < n and time.time() < deadline:
        c = ser.read(n - len(buf))
        if c: buf += c
    return bytes(buf)


def shot(ser, path):
    ser.reset_input_buffer()
    ser.write(b"S\n"); ser.flush()
    hdr = None; deadline = time.time() + 10
    while time.time() < deadline:
        line = ser.readline()
        if line.startswith(b"GBSHOT"):
            hdr = line.strip().split(); break
    if not hdr:
        print("  no GBSHOT header"); return
    w, h = int(hdr[1]), int(hdr[2])
    raw = read_exact(ser, w * h * 2)
    img = Image.new("RGB", (w, h))
    px = img.load()
    for i in range(w * h):
        # CYD ILI9341 readback mapping, derived empirically via scripts/dev/calibrate.py
        # (cyd.md warns this is panel-specific): big-endian, then hi5=Blue, mid6=Red, lo5=Green.
        v = (raw[i*2] << 8) | raw[i*2+1]
        b = (v >> 11) & 0x1f            # high 5 bits  -> Blue
        r = (v >> 5)  & 0x3f            # middle 6 bits -> Red
        g =  v        & 0x1f            # low 5 bits   -> Green
        px[i % w, i // w] = ((r << 2) | (r >> 4), (g << 3) | (g >> 2), (b << 3) | (b >> 2))
    import os
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    img.save(path)
    print(f"  wrote {path} ({w}x{h})")


def main():
    port = sys.argv[1] if len(sys.argv) > 1 else "COM5"
    cmds = sys.argv[2] if len(sys.argv) > 2 else sys.stdin.read()
    ser = serial.Serial()
    ser.port = port; ser.baudrate = 115200; ser.timeout = 2
    ser.dtr = False; ser.rts = False
    ser.open()
    # NOTE: do NOT pulse RTS to hard-reset — with NimBLE running that re-enumerates USB and
    # invalidates the write handle (Access denied). Opening with DTR/RTS low leaves the board
    # running; we reach a known state by sending 'H' (Home) instead.
    time.sleep(0.4)
    ser.reset_input_buffer()
    ser.write(b"H\n"); ser.flush(); time.sleep(0.3)

    for raw in cmds.split(";"):
        c = raw.strip()
        if not c: continue
        p = c.split(); op = p[0]
        print(c)
        if   op == "nav":  ser.write(f"G {p[1]}\n".encode()); ser.flush(); time.sleep(0.35)
        elif op == "tap":  ser.write(f"T {p[1]} {p[2]}\n".encode()); ser.flush(); time.sleep(0.35)
        elif op == "wait": time.sleep(float(p[1]))
        elif op == "shot": shot(ser, p[1])
        elif op == "key":  ser.write((" ".join(p[1:]) + "\n").encode()); ser.flush()
    ser.close()


if __name__ == "__main__":
    main()
