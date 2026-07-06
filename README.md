# 🕰️ The Clicking Hour

[![Godot Engine](https://img.shields.io/badge/Godot-4.6.2-%23478cbf?logo=godot-engine&logoColor=white)](https://godotengine.org)
[![GDScript](https://img.shields.io/badge/GDScript-%23355570?logo=godot-engine&logoColor=white)](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_basics.html)
[![License: Non-Commercial](https://img.shields.io/badge/License-Non--Commercial-red.svg)](LICENSE)

**The Clicking Hour** is a fast-paced arcade clicker set in the neon-soaked "Club 404".

> *The service at "Club 404", a favorite haunt for vaporwave regulars, has just begun. The waiters are arriving with drinks in hand... but they don't always know the menu! Only let those in who have the right glass. Watch out! Some are tricky and will try to serve their order at all costs...*

🏆 **Winner of the [Code Game Jam 2026](https://cgj.bpaul.fr/)** organized by the IUT of Montpellier.

---

## 🎮 Gameplay

Your goal is simple but challenging: filter the incoming stream of waiters. The bar has specific requirements for each wave, displayed on the board. You must click on the waiters with the **incorrect** drinks to remove them before they reach the door, while letting the **correct** ones through to fulfill the bar's requirements.

### Controls
- **Mouse Left Click:** Click on waiters to remove them. That's all you need!

### Key Features
- **Dynamic Wave System:** Every wave increases in difficulty, requiring more glasses and introducing faster waiters.
- **Waiter Variety:**
    - **Simple Waiters:** The standard waiter.
    - **Teleport Waiters:** They won't stay in one place for long!
    - **Hiding Waiters:** Catch them before they disappear back into the shadows.
    - **Multi-Click Waiters:** Requires multiple hits to release their prize.
- **Night Mode:** A rare, high-intensity mode that challenges your visibility and reaction time with unique visuals and "drunk" music variations.
- **VFX & Atmosphere:** Retro-inspired CRT/VHS effects, glitch shaders, and a reactive soundtrack suitable for a vaporwave club.

---

## 🛠️ Technical Stack

- **Engine:** [Godot 4.6.2](https://godotengine.org/) (Forward Plus renderer)
- **Language:** GDScript
- **Shaders:** Custom GLSL shaders for glitch and VHS post-processing.
- **Architecture:**
    - Event-driven UI and state management.
    - Component-based waiter system.
    - Global managers for Settings, Music, and SFX.

---

## 🚀 Getting Started

### Prerequisites
- [Godot Engine 4.6.2](https://godotengine.org/download)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/Propeller-Hat-Games/the-clicking-hour.git
   ```
2. Open the project in Godot:
   - Launch Godot.
   - Click **Import** and select the `project.godot` file in the cloned directory.
3. Run the game:
   - Press **F5** or the Play button in the top-right corner.

---

## 🛠️ Development

This project uses **[GDScript Toolkit](https://github.com/Scony/godot-gdscript-toolkit)** for linting and formatting, integrated with **pre-commit** hooks and **GitHub Actions**.

### Prerequisites for Developers
- **Python 3.10+**

### One-command setup (Linux, macOS, Windows)

```bash
python setup_dev.py
```

This script will:
- Create an isolated Python virtual environment (`.venv/`)
- Install all dev tools (`pre-commit`, `gdtoolkit`) at the pinned versions
- Install git hooks so linting runs automatically on every commit

#### Options

```bash
python setup_dev.py --reset   # Wipe and recreate the venv from scratch
python setup_dev.py --hooks   # Only (re)install the pre-commit git hooks
```

### Running Checks Manually

Activate the virtual environment first:

```bash
# Linux / macOS
source .venv/bin/activate

# Windows
.venv\Scripts\activate
```

Then:

```bash
# Run all hooks on every file
pre-commit run --all-files

# Or run specific tools directly
gdlint scripts/
gdformat --check scripts/
```

---

## 📂 Project Structure

- `assets/`: Music, sounds, sprites, and shaders.
- `scenes/`: Godot scenes (.tscn) for UI, environment, and waiters.
- `scripts/`: GDScript logic organized by system (game loop, menus, UI).
- `project.godot`: Main Godot project configuration.

---

## 📜 Credits

**Propeller Hat Game Team**

- **Art:** Ferdinand Del Re (Flamasar)
- **Music:** Gaspard Ternoy
- **Code:**
  - Baptiste May
  - Lucas Guglielmetti
  - Hugo Louis Joseph
  - Kamil Charbenaga
  - Clément Thery

---

## ⚖️ License

This project is licensed under the **Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)** license.

- **Attribution**: You must give appropriate credit to **Propeller Hat Game Team**.
- **Non-Commercial**: You may not use the material for commercial purposes.
- **ShareAlike**: If you remix, transform, or build upon the material, you must distribute your contributions under the same license as the original.

See the [LICENSE](LICENSE) file for the full legal text.

*Made with ❤️ using Godot Engine.*
