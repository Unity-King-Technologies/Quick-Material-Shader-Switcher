# Quick Material / Shader Switcher for Unity

A productivity-focused Unity Editor extension that enables **instant material and shader switching** without repeatedly opening the Inspector. Designed to dramatically speed up **visual prototyping, look development, and scene dressing** workflows.

---

## Overview

When working on environments, props, or characters, developers and artists frequently need to **compare multiple materials or shaders** across one or many GameObjects. Unity’s default workflow requires opening the Inspector, locating materials, and reassigning them repeatedly — a process that becomes slow and disruptive at scale.

**Quick Material / Shader Switcher** solves this problem by providing a **dedicated Editor Window** that allows:
- One-click material or shader swapping
- Bulk assignment across multiple selected objects
- Favorites for frequently used assets
- Drag-and-drop workflow support

---

## Problem Statement

### Pain Points in Default Unity Workflow
- Repetitive Inspector navigation
- Slow iteration when testing multiple visual styles
- Inefficient bulk material changes
- Context switching between Project window and Inspector
- No quick comparison mechanism

These issues become especially noticeable during:
- Environment art iteration
- Lighting and mood exploration
- Shader experimentation
- Rapid prototyping phases

---

## Solution

A **custom Unity Editor Window** that centralizes material and shader management into a fast, visual, and intuitive interface.

---

## Key Features

### Material Browser
- Lists **all materials in the project** or **only materials used by selected objects**
- Search and filter support
- Thumbnail previews for quick identification

### Shader Switcher
- Instantly swap shaders on selected materials
- Preserve material properties when possible
- Useful for testing:
  - URP vs Built-in
  - Lit vs Unlit
  - Custom shader variants

### One-Click Assignment
- Click a material to instantly apply it to:
  - Selected GameObject
  - All selected objects
  - Child renderers (optional)

### Drag & Drop Support
- Drag materials from the window directly onto:
  - Scene objects
  - Inspector material slots

### Favorites System
- Mark materials or shaders as favorites
- Persist favorites across sessions
- Ideal for commonly reused assets

### Multi-Selection Support
- Apply changes to multiple objects simultaneously
- Works with:
  - MeshRenderer
  - SkinnedMeshRenderer
  - SpriteRenderer (optional extension)

---

## Use Cases

### Game Developers
- Rapidly test different surface looks during gameplay prototyping
- Switch between performance-friendly shaders
- Quickly standardize materials across prefabs

### Environment & Level Artists
- Compare materials visually without breaking focus
- Speed up scene dressing
- Maintain consistent visual themes

### Technical Artists
- Debug shader variants
- Test fallback shaders
- Analyze visual impact of shader changes

### Prototyping & R&D
- Fast visual iteration
- Reduced friction during experimentation
- Ideal for early-stage concept validation

---

## Technical Architecture

### Core Components

```

Editor/
├── QuickMaterialSwitcherWindow.cs
├── MaterialDatabaseUtility.cs
├── FavoritesManager.cs
├── MaterialApplier.cs
└── UI/
├── MaterialListView.cs
└── ShaderDropdown.cs

```

### Key Systems

#### EditorWindow
- Custom Unity `EditorWindow`
- Dockable UI
- Realtime selection tracking

#### Asset Database Integration
- Uses `AssetDatabase.FindAssets()` to fetch materials
- Efficient caching for performance

#### Selection Handling
- Unity `Selection` API
- Auto-detects active GameObjects
- Supports multi-object operations

#### Undo System
- Integrated with Unity `Undo.RecordObject`
- Fully undoable material changes

---

## ⚙️ Installation

### Manual Installation
1. Download or clone this repository
2. Copy the folder into:

Assets/Editor/QuickMaterialSwitcher/

3. Open Unity
4. Navigate to:

Tools → Quick Material / Shader Switcher

---

## Performance Considerations

- Material lists are cached to avoid repeated AssetDatabase calls
- UI virtualization for large material libraries
- Lightweight Editor-only execution (no runtime overhead)

---

## Compatibility

- Unity 2020 LTS and above
- Built-in Render Pipeline
- URP (partial / full support)
- HDRP (shader switching supported, UI customizable)

---

## 🤝 Contributing

Contributions are welcome!
- Fork the repository
- Create a feature branch
- Submit a pull request with clear documentation

---

## License

MIT License
Free to use, modify, and distribute in both personal and commercial projects.

---

## Disclaimer

**UnityKing is not responsible for any damage, data loss, or issues that may arise from the use of this tool.** Use at your own risk. Always backup your project before making bulk changes or modifications to materials and shaders.

---

## Acknowledgements

Inspired by real-world production workflows and common Unity Editor bottlenecks encountered during rapid iteration and prototyping.

---

## Feedback & Support

If you encounter issues or have feature requests, please open an issue in the repository.

Happy developing
