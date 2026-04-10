# Thaumiel Map Editor (TME)

> **This project is currently in early development. Expect bugs, missing features, and breaking changes.**

Thaumiel Map Editor (TME) is a Unity based map editor designed to help developers and designers create, edit, and manage game maps. TME provides an in editor interface for placing and configuring a wide range of game objects, from interactive elements and lighting to props and navigation aids.

---

## Features

> Features are actively being developed. The list below reflects what is currently implemented.

- **Unity powered editor interface** — Built directly in Unity
- **Object spawning system** — Place a wide variety of typed objects onto your map from a structured spawn panel
- **Client side object support** — Certain object types are spawned on the client only and do not affect server state
- **Save & load maps** — Persist your work in a custom map formatt

---

## Supported Object Types

TME supports spawning the following object types. Objects marked as Client are spawned locally on the client and are not replicated to the server.

| Object Type | Scope |
|-|-|
| Doors | Server |
| Clutter | Server |
| Interactables | Server |
| Pickups | Server |
| Lockers | Server |
| Waypoints | Server |
| Cameras | Server |
| Targets | Server |
| Teleporters | Server |
| Primitives | Client |
| Lights | Client |
| Capybaras | Client |

---
## Usage & Commands

To view the full list of in-game console and admin commands, permissions, and aliases, please see our **[Commands Documentation](Commands.md)**.

## Installation

### Prerequisites

Before getting started, make sure you have the following installed:
- [Unity Hub](https://unity.com/download)

### Getting Started

1. Download the repository from here: https://www.github.com/Mr-Baguetter/ThaumielMapEditorUnityProject

2. Extract the downloaded file.

3. Open the project in Unity Hub by clicking Add and selecting the extracted folder.

---

Discord Community: https://discord.gg/N8qrNHf4s9

*Thaumiel Map Editor is a work in progress. Contributions, feedback, and bug reports are welcome.*