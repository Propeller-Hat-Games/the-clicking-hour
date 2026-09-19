# The Clicking Hour — AGENTS.md

Godot 4.7 GDScript arcade clicker ("Club 404"). There is **no test framework**; verification = GDScript lint/format + running the game in the Godot editor (F5).

## Dev environment

- One-command setup: `python setup_dev.py` (Python 3.10+). Creates `.venv/` with pinned `gdtoolkit==4.5.0` and `pre-commit==4.5.1`, installs git hooks. `--reset` rebuilds the venv, `--hooks` reinstalls hooks.
- `gdlint`/`gdformat` only exist in `.venv/bin`. Activate with `source .venv/bin/activate`; do not install or call system-wide copies.

## Verification (before committing)

CI runs exactly these on every push/PR (`.github/workflows/gdscript-checks.yml`):

```bash
source .venv/bin/activate
gdformat --check scripts
gdlint scripts
pre-commit run --all-files   # adds trailing-whitespace / EOF-newline / YAML checks
```

- Only `scripts/` is linted/formatted. `addons/` is excluded via `.pre-commit-config.yaml` — it vendors `discord-rpc-gd` (GDExtension with prebuilt per-platform binaries under `addons/discord-rpc-gd/bin`); don't modify it.
- `gdformat` rewrites files, `gdformat --check` only verifies. GDScript is tab-indented (`.editorconfig`).
- Keep `gdtoolkit` versions in sync: `requirements-dev.txt` ↔ `.pre-commit-config.yaml` `rev`.

## Architecture

- **Autoloads** (registered in `project.godot[autoload]`): `SettingsManager`, `MusicManager`, `SfxManager`, `GameEvents` — the global signal bus that decouples systems. When adding a signal to `GameEvents.gd`, it needs `@warning_ignore("unused_signal")` to pass gdlint. They all live under `scripts/system/`, but not everything there is an autoload: `DiscordRPCManager` (wraps the vendored `discord-rpc-gd` GDExtension; `GameManager` instantiates it at runtime) and `SplashScreen` are plain nodes.
- **Game flow / wiring**: `run/main_scene` is `scenes/splash_screen.tscn` (`SplashScreen`, logo video), which does `change_scene_to_file` to `scenes/game/game_manager.tscn`. That scene is the hub: it wires every `@export` on `GameManager` (`door`, `main_menu`, `trash`, `board`, `heart_container`, `background`, `spawn_area`, …) and holds `MainMenu` as a child, which signals `GameManager` via `play_button_pressed` to start the game.
- **Managers**: per-game managers in `scripts/game/managers/` subclass `GameManagerInterface` (holds `game: GameManager`, `init(p_game)`). They are created in `GameManager._setup_managers()` via `new()` → `add_child()` → `init(self)`. A new manager must be added to both that method and its typed `Array[GameManagerInterface]`.
- **Entities**: base `scripts/game/entity/Entity.gd` (`CharacterBody2D`). Variants `SimpleEntity`, `HidingEntity`, `MultiClickEntity`, `TeleportEntity` override hooks `initialize_entity()`, `process_entity(delta)`, `_on_clicked()`. A new entity type is **not** auto-discovered: create the script + scene under `scripts/game/entity/` / `scenes/game/entity/`, then register it explicitly in `EntitiesManager.load_entities()` via `_load_entity_scene(...)`. Spawning is weighted in `EntitiesManager.get_random_entity()` (indices 0–3; advanced types only appear after wave 5) — a type appended to `entity_scenes` stays unspawnable until that table is updated.
- **Glasses**: base `scripts/game/GlassInterface.gd` (`Sprite2D`); concrete types in `scripts/game/glass/` (`Blue`, `Green`, `Yellow`, `Red`, `Heal`) override `dead_effect`, `door_entered_effect`, `bin_entered_effect`, `clicked_effect`.
- **Audio**: SFX are loaded individually in `SfxManager.gd` as `@onready var _x := load("res://assets/sounds/X.mp3")` and exposed via a `play_*()` function — a new sound must be registered there by hand. Music is playlist-driven on `MusicManager` (menu / normal / night = `assets/musics/darkMode`). Audio buses and effects live in `assets/themes/default_bus_layout.tres`; the "pause" low-pass effect is created in code in `MusicManager._setup_music_bus()`, not in the editor.
- Code conventions: every script uses `class_name` + `extends`, full static typing (`-> void`, typed vars), StringName literals (`&"..."`), `@onready` scene refs. Match this in new code.

## Scenes, uid refs, generated files

- `project.godot` and `.tscn` files reference resources/scripts by `uid://`; `.gd.uid` files are committed. Prefer the Godot editor for scene-structure changes and script renames — hand-editing uid refs breaks loading.
- `.import` and `.translation` files are machine-generated and gitignored; never hand-edit them.

## Translations

- Source of truth is `assets/translations/translations.csv` (`key,en,fr`). Godot regenerates the gitignored `.translation` files from it on import. Add/change strings there in **both** locales, then re-import in the editor. Code looks up keys (`UI_PLAY`, …) via `tr()`.

## Export & deploy

- Local release builds: `python export.py` (needs `godot` in PATH) → `releases/` (gitignored). Presets come from `export_presets.cfg`, version from `project.godot`. Flags: `--preset`, `--godot`, `--dry-run`.
- GitHub Pages ships only from pushes to `main` (`.github/workflows/deploy.yml`: web export + injects `coi-serviceworker.js`). Active dev happens on `develop`; merge to `main` to ship.
- Commit history uses gitmoji-style conventional commits (`:sparkles: feat:`, `:bug: fix:`); follow it.

## Notes

- Wave difficulty is tunable via the `Curve` exports on `GameManager` (`delay_curve`, `glass_count_curve`, `entity_speed_curve`) set in `scenes/game/game_manager.tscn`, not constants.
- Settings persist to `user://settings.cfg` via `SettingsManager` (a `ConfigFile`).
- `DebugPanel` (`scripts/ui/menus/DebugPanel.gd`) exposes wave/spawn/manager debug controls — the usual way to exercise gameplay without playing full waves.
