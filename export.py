#!/usr/bin/env python3
"""
Export all Godot presets for The Clicking Hour.

Reads the project version from project.godot, then runs
`godot --headless --export-release` for every preset defined
in export_presets.cfg.

If the Android preset is included and the release keystore is missing,
you will be prompted to generate one on the spot.

Usage:
    python export.py                  # Export all presets
    python export.py --preset Linux   # Export a single preset by name
    python export.py --godot /path/to/godot  # Override Godot binary
    python export.py --dry-run        # Print commands without running them
"""

from __future__ import annotations

import argparse
import getpass
import os
import re
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

# ── Paths ────────────────────────────────────────────────────────────────────

PROJECT_DIR = Path(__file__).parent.resolve()
PROJECT_GODOT = PROJECT_DIR / "project.godot"
EXPORT_PRESETS = PROJECT_DIR / "export_presets.cfg"
# Godot stores credentials (keystore passwords) in .godot/export_credentials.cfg
EXPORT_CREDENTIALS = PROJECT_DIR / ".godot" / "export_credentials.cfg"

# ── Keystore defaults ─────────────────────────────────────────────────────────

KEYSTORE_PATH = PROJECT_DIR / "releases" / "keystore" / "release.keystore"
KEYSTORE_ALIAS = "the_clicking_hour"
KEYSTORE_DNAME = "CN=Propeller Hat Games, O=Propeller Hat Games, C=FR"

# ── Colours ───────────────────────────────────────────────────────────────────

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


# Box inner width (between the two ║ borders).
_BOX_WIDTH = 54


def box_row(content: str, emoji_count: int = 0, color: str = "") -> None:
    """
    Print a box row padded to _BOX_WIDTH.
    emoji_count: number of emoji in content (each is visually 2-wide but len() == 1).
    """
    padding = " " * (_BOX_WIDTH - len(content) - emoji_count)
    row = f"║{content}{padding}║"
    print(c(row, color) if color else row)


# ── Helpers ───────────────────────────────────────────────────────────────────


def find_keytool() -> str:
    """Return the keytool binary path, or exit with an error."""
    kt = shutil.which("keytool")
    if not kt:
        err(
            "keytool not found in PATH.\n"
            "  Install a JDK (e.g. openjdk-17-jdk) and make sure it is in PATH."
        )
        sys.exit(1)
    return kt


def prompt_passwords() -> tuple[str, str]:
    """
    Interactively prompt for a keystore password (min 6 chars, confirmed) and an
    optional key password.  Returns (ks_pass, key_pass).
    Shared by gen_keystore() and the credentials-regeneration branch in
    ensure_android_keystore() so both paths enforce identical validation.
    """
    ks_pass = getpass.getpass(c("  Keystore password (min 6 chars): ", "36"))
    if len(ks_pass) < 6:
        err("Password must be at least 6 characters.")
        sys.exit(1)
    ks_pass2 = getpass.getpass(c("  Confirm keystore password:       ", "36"))
    if ks_pass != ks_pass2:
        err("Passwords do not match.")
        sys.exit(1)

    key_pass = getpass.getpass(c("  Key password (leave blank to reuse keystore password): ", "36"))
    if not key_pass:
        key_pass = ks_pass
    return ks_pass, key_pass


def gen_keystore() -> None:
    """Interactively generate the Android release keystore and write .godot/export_credentials.cfg."""
    step("Android keystore generation")

    keytool = find_keytool()
    info(f"keytool : {keytool}")
    info(f"Output  : {KEYSTORE_PATH}")
    info(f"Alias   : {KEYSTORE_ALIAS}")
    print()

    # Prompt for passwords securely — never echoed, never in shell history.
    ks_pass, key_pass = prompt_passwords()

    KEYSTORE_PATH.parent.mkdir(parents=True, exist_ok=True)

    # Pass passwords via environment variables so they are never visible in
    # process listings (ps, /proc/<pid>/cmdline, audit logs, etc.).
    env = os.environ.copy()
    env["KS_PASS"] = ks_pass
    env["KEY_PASS"] = key_pass
    cmd = [
        keytool, "-genkey", "-v",
        "-keystore", str(KEYSTORE_PATH),
        "-alias", KEYSTORE_ALIAS,
        "-keyalg", "RSA",
        "-keysize", "2048",
        "-validity", "10000",
        "-dname", KEYSTORE_DNAME,
        "-storepass:env", "KS_PASS",
        "-keypass:env", "KEY_PASS",
    ]
    result = subprocess.run(cmd, env=env)
    if result.returncode != 0:
        err("keytool failed — keystore was not created.")
        sys.exit(1)

    ok(f"Keystore created at {KEYSTORE_PATH}")
    write_godot_credentials(str(KEYSTORE_PATH), KEYSTORE_ALIAS, ks_pass, key_pass)


def write_godot_credentials(
    keystore_path: str, alias: str, ks_pass: str, key_pass: str
) -> None:
    """
    Patch the keystore path, alias, and passwords into .godot/export_credentials.cfg
    (the file Godot actually reads at export time).
    """
    text = EXPORT_CREDENTIALS.read_text(encoding="utf-8")

    replacements = {
        'keystore/release=""': f'keystore/release="{keystore_path}"',
        'keystore/release_user=""': f'keystore/release_user="{alias}"',
        'keystore/release_password=""': f'keystore/release_password="{ks_pass}"',
    }
    for old, new in replacements.items():
        text = text.replace(old, new)

    EXPORT_CREDENTIALS.write_text(text, encoding="utf-8")
    ok(f"Credentials written to .godot/export_credentials.cfg (gitignored — never committed).")


def credentials_are_set() -> bool:
    """Return True if .godot/export_credentials.cfg already has a release keystore path."""
    if not EXPORT_CREDENTIALS.exists():
        return False
    text = EXPORT_CREDENTIALS.read_text(encoding="utf-8")
    match = re.search(r'^keystore/release="([^"]+)"', text, re.MULTILINE)
    return bool(match and match.group(1))


def ensure_android_keystore() -> None:
    """
    Called automatically before an Android export.
    - If the keystore file is missing, generate it from scratch (prompts for password).
    - If the keystore exists but .godot/export_credentials.cfg has no path set,
      prompt for the password and write credentials without regenerating the keystore.
    """
    if not KEYSTORE_PATH.exists():
        warn("Android keystore not found — generating now.")
        gen_keystore()
        return

    if not credentials_are_set():
        warn(".godot/export_credentials.cfg has no keystore set — filling in now.")
        info(f"Keystore : {KEYSTORE_PATH}")
        print()
        # Reuse the shared prompt so validation (min length, confirmation) is
        # identical to the gen_keystore() flow.
        ks_pass, key_pass = prompt_passwords()
        write_godot_credentials(str(KEYSTORE_PATH), KEYSTORE_ALIAS, ks_pass, key_pass)


def patch_keystore_path(absolute: bool) -> None:
    """
    Godot headless requires an absolute keystore path.
    Toggle between absolute (before export) and relative (after export, for git portability).
    """
    text = EXPORT_PRESETS.read_text(encoding="utf-8")
    abs_path = str(KEYSTORE_PATH)
    # Derive the relative form from the canonical KEYSTORE_PATH constant so
    # this stays in sync automatically if the keystore location ever changes.
    rel_path = str(KEYSTORE_PATH.relative_to(PROJECT_DIR))

    if absolute:
        patched = text.replace(
            f'keystore/release="{rel_path}"',
            f'keystore/release="{abs_path}"',
        )
    else:
        patched = text.replace(
            f'keystore/release="{abs_path}"',
            f'keystore/release="{rel_path}"',
        )

    if patched != text:
        EXPORT_PRESETS.write_text(patched, encoding="utf-8")


def find_godot() -> str:
    """Return the name / path of the Godot 4 headless binary."""
    candidates = ["godot4", "godot", "Godot_v4", "godot-headless"]
    for name in candidates:
        if shutil.which(name):
            return name
    err(
        "Could not find a Godot 4 binary in PATH.\n"
        "  Install Godot 4 and make sure it is in PATH, or pass --godot /path/to/godot."
    )
    sys.exit(1)


def read_version() -> str:
    """Extract config/version from project.godot."""
    text = PROJECT_GODOT.read_text(encoding="utf-8")
    match = re.search(r'^config/version\s*=\s*"([^"]+)"', text, re.MULTILINE)
    if match:
        return match.group(1)
    warn("Could not read version from project.godot — using 'unknown'.")
    return "unknown"


def read_presets() -> list[dict[str, str]]:
    """
    Parse export_presets.cfg and return a list of preset dicts with
    keys 'name' and 'export_path'.
    """
    text = EXPORT_PRESETS.read_text(encoding="utf-8")
    presets: list[dict[str, str]] = []
    # Split on preset section headers, e.g. [preset.0]
    blocks = re.split(r"^\[preset\.\d+\]", text, flags=re.MULTILINE)
    for block in blocks:
        name_m = re.search(r'^name\s*=\s*"([^"]+)"', block, re.MULTILINE)
        path_m = re.search(r'^export_path\s*=\s*"([^"]+)"', block, re.MULTILINE)
        if name_m and path_m:
            presets.append(
                {
                    "name": name_m.group(1),
                    "export_path": path_m.group(1),
                }
            )
    return presets


def ensure_output_dir(export_path: str) -> None:
    out_dir = (PROJECT_DIR / export_path).parent
    out_dir.mkdir(parents=True, exist_ok=True)


# Output path template for each preset — version is injected at runtime.
# The paths in export_presets.cfg may have a stale version baked in; these take precedence.
EXPORT_PATH_TEMPLATES: dict[str, str] = {
    "Android": "releases/the_clicking_hour_{v}_android.apk",
    "Linux":   "releases/the_clicking_hour_{v}_linux.x86_64",
    "Windows": "releases/the_clicking_hour_{v}_windows.exe",
    "Web":     "releases/web/index.html",
}


def versioned_export_path(preset_name: str, version: str, fallback: str) -> str:
    """Return the correct export path for a preset, with the live version injected."""
    template = EXPORT_PATH_TEMPLATES.get(preset_name)
    if template:
        return template.format(v=version.replace(".", "_"))
    return fallback  # unknown preset — use whatever is in export_presets.cfg


def zip_web_export(version: str, dry_run: bool) -> None:
    """Zip the releases/web/ directory into releases/the_clicking_hour_{version}_web.zip."""
    web_dir = PROJECT_DIR / "releases" / "web"
    version_slug = version.replace(".", "_")
    zip_path = PROJECT_DIR / "releases" / f"the_clicking_hour_{version_slug}_web.zip"

    if dry_run:
        warn(f"Dry-run: would zip {web_dir} → {zip_path.name}")
        return

    if not web_dir.exists():
        warn(f"Web export directory not found: {web_dir}")
        return

    info(f"Zipping {web_dir.name}/ → {zip_path.name} …")
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
        for file in web_dir.rglob("*"):
            if file.is_file():
                zf.write(file, file.relative_to(web_dir))
    ok(f"Web archive: {zip_path.name}")
    # Remove the raw web folder — the zip is the release artifact.
    shutil.rmtree(web_dir)
    info(f"Removed {web_dir.name}/")


def zip_desktop_export(preset_name: str, version: str, export_path: str, dry_run: bool) -> None:
    """
    Bundle a desktop binary with its Discord SDK native libraries into a versioned zip.

    Linux: binary + lib*.so  → the_clicking_hour_{version}_linux.zip
    Windows: binary + *.dll  → the_clicking_hour_{version}_windows.zip
    """
    releases_dir = PROJECT_DIR / "releases"
    binary = PROJECT_DIR / export_path
    version_slug = version.replace(".", "_")

    if preset_name == "Linux":
        lib_pattern = "*.so"
        zip_name = f"the_clicking_hour_{version_slug}_linux.zip"
    elif preset_name == "Windows":
        lib_pattern = "*.dll"
        zip_name = f"the_clicking_hour_{version_slug}_windows.zip"
    else:
        return

    libs = list(releases_dir.glob(lib_pattern))
    files_to_zip = [binary, *libs]

    if dry_run:
        names = ", ".join(f.name for f in files_to_zip)
        warn(f"Dry-run: would zip [{names}] → {zip_name}")
        return

    # Validate that every expected file is present before touching the archive.
    missing = [f for f in files_to_zip if not f.exists()]
    if not binary.exists():
        err(f"{preset_name} export binary not found: {binary}")
        return
    if missing:
        warn(
            f"{preset_name} desktop export is incomplete — "
            f"missing file(s): {', '.join(m.name for m in missing)}"
        )
        return

    zip_path = releases_dir / zip_name
    info(f"Zipping {preset_name} + {len(libs)} lib(s) → {zip_name} …")
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
        for f in files_to_zip:
            zf.write(f, f.name)

    # Remove the individual files — the zip is the release artifact.
    for f in files_to_zip:
        f.unlink()
        info(f"Removed {f.name}")
    ok(f"{preset_name} archive: {zip_name}")


def export_preset(
    godot: str, preset_name: str, export_path: str, dry_run: bool
) -> bool:
    """
    Run `godot --headless --export-release <preset_name> <export_path>`.
    Returns True on success.
    """
    abs_path = str(PROJECT_DIR / export_path)
    ensure_output_dir(export_path)
    cmd = [godot, "--headless", "--export-release", preset_name, abs_path]
    info(f"Command: {' '.join(cmd)}")
    if dry_run:
        warn("Dry-run mode — skipping execution.")
        return True
    result = subprocess.run(cmd, cwd=str(PROJECT_DIR))
    if result.returncode == 0:
        # Remove stray .idsig files produced by Android signing.
        for idsig in (PROJECT_DIR / "releases").glob("*.idsig"):
            idsig.unlink()
    return result.returncode == 0


def clean_releases() -> None:
    """
    Wipe the releases/ directory before a full export, preserving only keystore/.
    This ensures no stale files (old builds, SDK libs, leftover web folder) remain.
    """
    releases_dir = PROJECT_DIR / "releases"
    if not releases_dir.exists():
        return
    step("Cleaning releases/")
    for item in releases_dir.iterdir():
        if item.name == "keystore":
            continue  # keep the signing keystore
        if item.is_dir():
            shutil.rmtree(item)
            info(f"Removed {item.name}/")
        else:
            item.unlink()
            info(f"Removed {item.name}")
    ok("releases/ is clean.")


# ── Main ──────────────────────────────────────────────────────────────────────


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Export all Godot presets for The Clicking Hour."
    )
    parser.add_argument(
        "--preset",
        metavar="NAME",
        help="Export only the preset with this name (case-sensitive).",
    )
    parser.add_argument(
        "--godot",
        metavar="PATH",
        help="Path or name of the Godot 4 binary (default: auto-detect).",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print the export commands without executing them.",
    )
    args = parser.parse_args()

    print(c("\n🕰️  The Clicking Hour — Export\n", "1;35"))

    godot = args.godot or find_godot()
    version = read_version()
    info(f"Project version : {version}")
    info(f"Godot binary    : {godot}")

    presets = read_presets()
    if not presets:
        err(f"No presets found in {EXPORT_PRESETS}.")
        sys.exit(1)

    # Filter by --preset if provided
    if args.preset:
        presets = [p for p in presets if p["name"] == args.preset]
        if not presets:
            err(f"No preset named '{args.preset}' found in export_presets.cfg.")
            sys.exit(1)

    results: list[tuple[str, bool]] = []

    # Clean the releases/ folder before a full export (not when targeting a single preset).
    if not args.preset and not args.dry_run:
        clean_releases()

    for preset in presets:
        name = preset["name"]
        # Override the path from export_presets.cfg with one built from the live version.
        export_path = versioned_export_path(name, version, fallback=preset["export_path"])
        # Ensure the Android keystore exists before attempting the export.
        if name == "Android" and not args.dry_run:
            ensure_android_keystore()
            # Godot headless needs an absolute keystore path; restore relative path after.
            patch_keystore_path(absolute=True)
            try:
                step(f"Exporting: {name}  →  {export_path}")
                success = export_preset(godot, name, export_path, dry_run=args.dry_run)
            finally:
                patch_keystore_path(absolute=False)
        else:
            step(f"Exporting: {name}  →  {export_path}")
            success = export_preset(godot, name, export_path, dry_run=args.dry_run)
            if success:
                # Zip the web export into a versioned archive.
                if name == "Web":
                    zip_web_export(version, dry_run=args.dry_run)
                # Bundle desktop binaries with their Discord SDK native libs.
                elif name in ("Linux", "Windows"):
                    zip_desktop_export(name, version, export_path, dry_run=args.dry_run)

        results.append((name, success))
        if success:
            ok(f"{name} export complete.")
        else:
            err(f"{name} export FAILED.")

    # ── Summary ───────────────────────────────────────────────────────────────
    total = len(results)
    passed = sum(1 for _, s in results if s)
    failed = total - passed

    color = "1;32" if failed == 0 else "1;31"
    print(c(
        "\n╔══════════════════════════════════════════════════════╗\n"
        "║   Export summary                                     ║\n"
        "╠══════════════════════════════════════════════════════╣",
        color,
    ))
    for name, success in results:
        icon = "✅" if success else "❌"
        status = "OK" if success else "FAILED"
        row_color = "1;32" if success else "1;31"
        box_row(f"  {icon} {name:<20} {status}", emoji_count=1, color=row_color)
    print(c(
        "╠══════════════════════════════════════════════════════╣",
        color,
    ))
    summary = f"  {passed}/{total} preset(s) exported successfully."
    box_row(summary, color=color)
    print(c(
        "╚══════════════════════════════════════════════════════╝",
        color,
    ))

    sys.exit(0 if failed == 0 else 1)


if __name__ == "__main__":
    main()
