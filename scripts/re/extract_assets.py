#!/usr/bin/env python
"""SPEC §11 asset extraction — pull the 10 preset dance choreographies and the phrase
duration table out of the Dance Code APK.

The dances are Unity TextAssets under Resources/connect/<name>/{dance,steps}; the
duration table is a switch in the (already decompiled) BLEToyAudio.cs.

Requires: UnityPy  (pip install UnityPy)
Usage:    python scripts/re/extract_assets.py re/dancecode.apk re/decompiled/Assembly-CSharp
Outputs:  assets/presets/dances/<name>_{dance,steps}.json , data/phrase_durations.csv
"""
import sys, os, re, glob, csv, json


def extract_dances(apk, outdir):
    import UnityPy
    # 1) unzip the Unity Data dir and join the 1 MB split asset files
    import zipfile
    data = "re/apk_data/assets/bin/Data"
    if not os.path.isdir(data):
        with zipfile.ZipFile(apk) as z:
            for n in z.namelist():
                if n.startswith("assets/bin/Data/"):
                    z.extract(n, "re/apk_data")
    groups = {}
    for f in glob.glob(data + "/*.split*"):
        groups.setdefault(re.sub(r'\.split\d+$', '', f), []).append(f)
    for base, parts in groups.items():
        if os.path.exists(base):
            continue
        parts.sort(key=lambda p: int(re.search(r'split(\d+)$', p).group(1)))
        with open(base, "wb") as o:
            for p in parts:
                o.write(open(p, "rb").read())

    # 2) resolve connect/<name>/{dance,steps} via the ResourceManager container map
    env = UnityPy.load(data)
    rm = next(o.read() for o in env.objects if o.type.name == "ResourceManager")
    want = re.compile(r'connect/(\w+)/(dance|steps)$', re.I)
    os.makedirs(outdir, exist_ok=True)
    n = 0
    for key, pptr in rm.m_Container:
        m = want.search(key)
        if not m:
            continue
        name, kind = m.group(1).lower(), m.group(2).lower()
        d = pptr.read()
        s = d.m_Script if hasattr(d, "m_Script") else getattr(d, "text", "")
        if isinstance(s, (bytes, bytearray)):
            s = s.decode("utf-8", "replace")
        open(f"{outdir}/{name}_{kind}.json", "w", encoding="utf-8").write(s)
        n += 1
    print(f"wrote {n} dance files -> {outdir}")


def extract_durations(decompiled, outcsv):
    src = open(os.path.join(decompiled, "BLEToyAudio.cs"), encoding="utf-8-sig").read()
    pairs = re.findall(r'case\s+BLEToyAudioPhrase\.(eP\d+):\s*return\s+([0-9.]+)f?;', src)
    os.makedirs(os.path.dirname(outcsv), exist_ok=True)
    with open(outcsv, "w", newline="") as f:
        w = csv.writer(f); w.writerow(["phrase", "index", "seconds"])
        for name, sec in pairs:
            w.writerow([name, int(name[2:]), f"{float(sec):.3f}"])
    print(f"wrote {len(pairs)} durations -> {outcsv}")


if __name__ == "__main__":
    apk = sys.argv[1] if len(sys.argv) > 1 else "re/dancecode.apk"
    decompiled = sys.argv[2] if len(sys.argv) > 2 else "re/decompiled/Assembly-CSharp"
    extract_dances(apk, "assets/presets/dances")
    extract_durations(decompiled, "data/phrase_durations.csv")
