#!/usr/bin/env python3
"""Blocks commits that contain terms the maintainer keeps out of this public repository.

The list of terms is NOT part of the repository: publishing it would defeat its purpose.
Each maintainer points the check at a local file, one term per line (# starts a comment):

    git config kodingtask.privateTermsFile "/path/outside/the/repo/private-terms.txt"

Without that setting the check does nothing, so contributors are not affected.

Matching is case-insensitive. Terms made only of letters match whole words; any other
term (identifiers, numbers, e-mail domains) matches as a plain substring.

Usage:
    check-private-terms.py --staged            staged file names and contents (pre-commit)
    check-private-terms.py --message FILE      a commit message file (commit-msg)
    check-private-terms.py --all               every tracked file and the whole history
"""
import os
import re
import subprocess
import sys


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, check=True).stdout


def terms_file():
    path = os.environ.get("KODINGTASK_PRIVATE_TERMS")
    if not path:
        try:
            path = git("config", "--get", "kodingtask.privateTermsFile").decode().strip()
        except subprocess.CalledProcessError:
            path = ""
    return path


def load_patterns(path):
    patterns = []
    with open(path, encoding="utf-8") as handle:
        for raw in handle:
            term = raw.strip()
            if not term or term.startswith("#"):
                continue
            if re.fullmatch(r"[^\W\d_]+", term):
                patterns.append((term, re.compile(r"(?<![^\W\d_])" + re.escape(term) + r"(?![^\W\d_])", re.IGNORECASE)))
            else:
                patterns.append((term, re.compile(re.escape(term), re.IGNORECASE)))
    return patterns


def scan(label, text, patterns, hits):
    for number, line in enumerate(text.splitlines(), 1):
        for term, pattern in patterns:
            if pattern.search(line):
                hits.append("%s:%d contains a private term (%s...)" % (label, number, term[:2]))


def staged(patterns, hits):
    names = git("diff", "--cached", "--name-only", "--diff-filter=ACMR", "-z").split(b"\0")
    for name in filter(None, names):
        path = name.decode("utf-8")
        scan("file name " + path, path, patterns, hits)
        blob = git("show", ":" + path)
        if b"\0" in blob[:8000]:
            continue
        scan(path, blob.decode("utf-8", errors="replace"), patterns, hits)


def everything(patterns, hits):
    for name in filter(None, git("ls-files", "-z").split(b"\0")):
        path = name.decode("utf-8")
        scan("file name " + path, path, patterns, hits)
        with open(path, "rb") as handle:
            blob = handle.read()
        if b"\0" not in blob[:8000]:
            scan(path, blob.decode("utf-8", errors="replace"), patterns, hits)
    history = git("log", "--all", "-p", "--format=commit %H%n%an <%ae>%n%B").decode("utf-8", errors="replace")
    scan("git history", history, patterns, hits)


def main():
    path = terms_file()
    if not path:
        return 0
    if not os.path.isfile(path):
        print("Private-terms file not found: %s" % path, file=sys.stderr)
        return 1
    patterns = load_patterns(path)
    hits = []
    mode = sys.argv[1] if len(sys.argv) > 1 else "--staged"
    if mode == "--staged":
        staged(patterns, hits)
    elif mode == "--message":
        with open(sys.argv[2], encoding="utf-8") as handle:
            scan("commit message", handle.read(), patterns, hits)
    elif mode == "--all":
        everything(patterns, hits)
    else:
        print(__doc__, file=sys.stderr)
        return 2
    if hits:
        print("Private terms found (%d):" % len(hits), file=sys.stderr)
        for hit in hits:
            print("  " + hit, file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
