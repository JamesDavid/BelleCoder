#!/usr/bin/env python
"""Derive the CYD screenshot readback mapping. Sends 'P' (8 known-colour bands), reads the raw
GBSHOT stream, samples each band centre, and reports how the panel returns each known colour so
the decoder in drive.py can be set correctly (cyd.md: readback mapping is panel-specific)."""
import sys, time, serial

KNOWN = [("red",0xF800),("green",0x07E0),("blue",0x001F),("yellow",0xFFE0),
         ("cyan",0x07FF),("magenta",0xF81F),("white",0xFFFF),("gray",0x8410)]

def read_exact(ser,n):
    b=bytearray(); dl=time.time()+30
    while len(b)<n and time.time()<dl:
        c=ser.read(n-len(b))
        if c: b+=c
    return bytes(b)

def main():
    port=sys.argv[1] if len(sys.argv)>1 else "COM5"
    ser=serial.Serial(); ser.port=port; ser.baudrate=115200; ser.timeout=2
    ser.dtr=False; ser.rts=False; ser.open()
    ser.setRTS(True); time.sleep(0.12); ser.setRTS(False); time.sleep(2.8)
    ser.reset_input_buffer()
    ser.write(b"P\n"); ser.flush(); time.sleep(0.4)
    ser.reset_input_buffer()
    ser.write(b"S\n"); ser.flush()
    hdr=None; dl=time.time()+10
    while time.time()<dl:
        l=ser.readline()
        if l.startswith(b"GBSHOT"): hdr=l.split(); break
    w,h=int(hdr[1]),int(hdr[2])
    raw=read_exact(ser,w*h*2)
    ser.close()
    bw=w//8; y=h//2
    print(f"{'band':8} {'logical':>8}  {'raw_LE':>7} {'raw_BE':>7}   decoded(BE: hi5,mid6,lo5)")
    for i,(name,logical) in enumerate(KNOWN):
        x=i*bw+bw//2; idx=y*w+x
        b0=raw[idx*2]; b1=raw[idx*2+1]
        le=b0|(b1<<8); be=(b0<<8)|b1
        hi=(be>>11)&0x1f; mid=(be>>5)&0x3f; lo=be&0x1f
        print(f"{name:8} {logical:#06x}  {le:#06x} {be:#06x}   hi={hi:2d} mid={mid:2d} lo={lo:2d}")

if __name__=="__main__":
    main()
