#!/usr/bin/env python3
"""
Cross-platform development environment setup for The Clicking Hour.
Works on Linux, macOS, and Windows (Python 3.10+).

Usage:
    python setup_dev.py          # Full setup (venv + deps + git hooks)
    python setup_dev.py --reset  # Delete and recreate the venv from scratch
    python setup_dev.py --hooks  # Only (re)install the pre-commit git hooks
"""

from __future__ import annotations

import argparse
import os
import shutil
import subprocess
import sys
import venv
from pathlib import Path

MIN_PYTHON = (3, 10)
VENV_DIR = Path(".venv")
REQUIREMENTS = Path("requirements-dev.txt")

# ── Colours (disabled on Windows without ANSI support) ──────────────────────
IS_WIN = sys.platform == "win32"
ANSI = not IS_WIN or os.environ.get("TERM") == "xterm"


def c(text: str, code: str) -> str:
    return f"\033[{code}m{text}\033[0m" if ANSI else text


def info(msg: str) -> None:
    print(c(f"  {msg}", "36"))


def ok(msg: str) -> None:
    print(c(f"✅ {msg}", "32"))


def warn(msg: str) -> None:
    print(c(f"⚠️  {msg}", "33"))


def err(msg: str) -> None:
    print(c(f"❌ {msg}", "31"), file=sys.stderr)


def step(msg: str) -> None:
    print(c(f"\n▶ {msg}", "1;34"))


# ── Helpers ──────────────────────────────────────────────────────────────────


def check_python_version() -> None:
    if sys.version_info < MIN_PYTHON:
        err(
            f"Python {'.'.join(str(v) for v in MIN_PYTHON)}+ required "
            f"(you have {sys.version.split()[0]})."
        )
        sys.exit(1)


def bin_path(name: str) -> Path:
    """Return the path to an executable inside the venv."""
    if IS_WIN:
        return VENV_DIR / "Scripts" / f"{name}.exe"
    return VENV_DIR / "bin" / name


def run(*args: str | Path, **kwargs) -> None:
    """Run a command, raising on failure."""
    subprocess.run([str(a) for a in args], check=True, **kwargs)


# ── Main steps ───────────────────────────────────────────────────────────────


def create_venv(reset: bool = False) -> None:
    step("Virtual environment")
    if reset and VENV_DIR.exists():
        info(f"Removing existing {VENV_DIR}/ …")
        shutil.rmtree(VENV_DIR)

    if VENV_DIR.exists():
        info(f"{VENV_DIR}/ already exists — skipping creation.")
    else:
        info(f"Creating {VENV_DIR}/ …")
        venv.create(VENV_DIR, with_pip=True, clear=True)
        ok("Virtual environment created.")


def install_deps() -> None:
    step("Python dependencies")
    if not REQUIREMENTS.exists():
        warn(f"{REQUIREMENTS} not found — skipping pip install.")
        return
    info(f"Installing from {REQUIREMENTS} …")
    run(bin_path("pip"), "install", "--quiet", "--upgrade", "pip")
    run(bin_path("pip"), "install", "--quiet", "-r", str(REQUIREMENTS))
    ok("Dependencies installed.")


def install_hooks() -> None:
    step("Pre-commit hooks")
    pre_commit = bin_path("pre-commit")
    if not pre_commit.exists():
        err("pre-commit not found in venv — run full setup first.")
        sys.exit(1)
    info("Installing git hooks …")
    run(pre_commit, "install", "--install-hooks")
    ok("Git hooks installed.")


def print_summary() -> None:
    activate = (
        r".venv\Scripts\activate"
        if IS_WIN
        else "source .venv/bin/activate"
    )
    print(
        c(
            f"""
╔══════════════════════════════════════════════════════╗
║   Development environment ready!                     ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Activate the venv (optional, for manual CLI use):   ║
║    {activate:<48}║
║                                                      ║
║  Useful commands (inside the venv):                  ║
║    gdlint scripts/       → lint GDScript files       ║
║    gdformat scripts/     → format GDScript files     ║
║    pre-commit run --all-files  → run all hooks       ║
║                                                      ║
║  Git hooks run automatically on every commit. 🎉     ║
╚══════════════════════════════════════════════════════╝""",
            "1;32",
        )
    )


# ── Entry point ──────────────────────────────────────────────────────────────


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Set up the dev environment for The Clicking Hour."
    )
    parser.add_argument(
        "--reset",
        action="store_true",
        help="Delete and recreate the venv from scratch.",
    )
    parser.add_argument(
        "--hooks",
        action="store_true",
        help="Only (re)install the pre-commit git hooks.",
    )
    args = parser.parse_args()

    print(c("\n🕰️  The Clicking Hour — Dev Setup\n", "1;35"))
    check_python_version()

    if args.hooks:
        install_hooks()
    else:
        create_venv(reset=args.reset)
        install_deps()
        install_hooks()
        print_summary()


if __name__ == "__main__":
    main()
