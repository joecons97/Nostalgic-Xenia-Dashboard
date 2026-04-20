# 🎮 Nostalgic Xenia Dashboard

A faithful recreation of the **Xbox 360 NXE (New Xbox Experience) dashboard** built in **Unity**, designed to run on PC and provide a console-like experience.

This project is not a launcher skin or front-end wrapper - it is a **standalone dashboard UI system** inspired directly by the Xbox 360 NXE interface, aiming to recreate its navigation flow, visual structure, and overall “console OS” feel.

---

## 🧱 Overview

Nostalgic Xenia Dashboard recreates the Xbox 360 NXE experience with a focus on:

- Tile-based dashboard navigation
- Controller-first UX design
- Smooth horizontal / vertical menu flow
- Console-style transitions and focus system
- Game library browsing and launching
- Extensible plugin architecture

The goal is to capture the feeling of using an Xbox 360 at its peak - not just visually, but structurally and interactively.

---
## 🏞️ Screenshots
<img width="1701" height="962" alt="image" src="https://github.com/user-attachments/assets/11b781ae-7b51-4317-801a-7ee62c6bb0a7" />
<img width="1710" height="953" alt="image" src="https://github.com/user-attachments/assets/9938dc6b-f1ea-4aa5-a4a6-2fda25fa39a4" />
<img width="1698" height="954" alt="image" src="https://github.com/user-attachments/assets/15e421d4-d428-46f6-8dd7-124c57ccd298" />
<img width="1694" height="952" alt="image" src="https://github.com/user-attachments/assets/e57c9e7f-8bb7-440f-8738-3078dcd12ad7" />

---

## ✨ Features

- 🧱 NXE-style dashboard recreation in Unity  
- 🎮 Controller-first navigation system  
- 📺 Tile-based UI layout  
- 📚 Game library browsing  
- 🖼️ Cover art + metadata support  
- ⚡ Smooth console-style transitions  
- 🔌 Plugin system for extensibility  
- 🖥️ Designed for a full-screen, console-like PC experience  

---

## 🔌 Plugin System

The dashboard includes a modular plugin system that allows functionality to be extended without modifying the core application.

Plugins are loaded automatically at startup and can provide:

- External game library integrations  
- Third-party platform support (Steam, Ubisoft Connect, etc.) 

---

## 📦 Official Plugins

### 🟢 Steam Plugin
Adds integration with the Steam library, allowing Steam games to appear inside the dashboard alongside other game sources.

👉 https://github.com/joecons97/NXDSteamPlugin

---

### 🔵 Ubisoft Connect (UPlay) Plugin
Adds support for Ubisoft Connect / UPlay game libraries.

👉 https://github.com/joecons97/NXDUPlayPlugin

---

## 📥 Installing Plugins

To install a plugin:

1. Download the plugin `.zip` from the plugin’s releases page  
2. Place the `.zip` file directly into:

```text
%AppData%\NXD\Plugins
```
Example:
```text
C:\Users\[YourName]\AppData\Roaming\NXD\Plugins
```
Restart the dashboard
The plugin will be automatically detected and loaded at startup
#### Notes
Plugins must remain as .zip files - do not extract them.</br>
Each plugin is loaded directly from its archive.</br>
To uninstall a plugin, simply remove the .zip file.</br>

## 🚧 Notes
This project is a UI recreation, not an official Xbox product
Inspired by the Xbox 360 NXE dashboard design language
Designed for personal, nostalgic, and educational use
Plugin compatibility may vary depending on dashboard version
Some integrations rely on third-party services or APIs
## 🧠 Design Philosophy

The goal of this project is to replicate the experience of the Xbox 360 dashboard, not just its appearance:

- Immediate controller responsiveness
- Clear hierarchical navigation
- Minimal desktop interference
- Strong visual identity through tiles and motion
--A system that feels like a console OS, not a PC app
## 💡 Future Ideas
- User profiles / gamertag system
- Achievements layer (emulated or external APIs)
- Additional dashboard themes (Blades / NXE hybrid variants)
- More plugin integrations (Epic Games, GOG, etc.)
- Full controller-based setup flow
- Improved animation system for transitions
