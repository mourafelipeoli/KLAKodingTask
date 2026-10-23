#!/usr/bin/env python3
"""Rejects mojibake and misplaced byte-order marks in tracked files.

Mojibake here means UTF-8 text that was read as CP1252 and saved again as UTF-8:
"português" turns into "portuguÃªs". Detection does not use a list of suspicious
sequences; it tests the round trip. A line is flagged only when

    line.encode("cp1252").decode("utf-8")

succeeds AND changes the line. Correct accented text fails that round trip, because
a single accented letter becomes one byte above 0x7F, which is not valid UTF-8 alone.

BOM policy: .cs files carry a BOM, nothing else does (see .editorconfig for why).

Usage: python3 scripts/checks/check-encoding.py   (run from anywhere inside the repo)
"""
import subprocess
import sys

BOM = b"\xef\xbb\xbf"
BOM_REQUIRED = (".cs",)
BOM_TOLERATED = (".csproj", ".sln", ".slnx", ".resx")
SKIP = (
    ".png", ".jpg", ".jpeg", ".gif", ".ico", ".pdf", ".eps", ".zip",
    ".dll", ".exe", ".woff", ".woff2", ".ttf",
)


def tracked_files():
    out = subprocess.run(["git", "ls-files", "-z"], capture_output=True, check=True).stdout
    return [p.decode("utf-8") for p in out.split(b"\0") if p]


def mojibake_lines(text):
    hits = []
    for number, line in enumerate(text.split("\n"), 1):
        if all(ord(c) < 128 for c in line):
            continue
        try:
            repaired = line.encode("cp1252").decode("utf-8")
        except (UnicodeEncodeError, UnicodeDecodeError):
            continue
        if repaired != line:
            hits.append((number, line.strip(), repaired.strip()))
    return hits


def main():
    problems = []
    for path in tracked_files():
        if path.lower().endswith(SKIP):
            continue
        try:
            with open(path, "rb") as handle:
                raw = handle.read()
        except (FileNotFoundError, IsADirectoryError):
            continue
        if b"\0" in raw[:8000]:
            continue
        has_bom = raw.startswith(BOM)
        try:
            text = raw[3:].decode("utf-8") if has_bom else raw.decode("utf-8")
        except UnicodeDecodeError as err:
            problems.append((path, "not valid UTF-8: %s" % err))
            continue

        for number, bad, good in mojibake_lines(text):
            problems.append((path, "line %d mojibake: %s  ->  %s" % (number, bad, good)))

        if path.endswith(BOM_REQUIRED) and not has_bom:
            problems.append((path, ".cs file without BOM (the policy requires a BOM in .cs)"))
        if not path.endswith(BOM_REQUIRED + BOM_TOLERATED) and has_bom:
            problems.append((path, "BOM in a non-.cs file (JSON and shell parsers break)"))

    if problems:
        print("Encoding problems (%d):" % len(problems), file=sys.stderr)
        for path, detail in problems:
            print("  %s: %s" % (path, detail), file=sys.stderr)
        return 1
    print("Encoding OK in every tracked file.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
