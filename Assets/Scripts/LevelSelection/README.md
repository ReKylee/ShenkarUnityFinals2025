# Level Selection System Setup Guide

## Overview
This level selection system integrates with your existing GameFlowManager, GameDataCoordinator, and event system to provide a complete level selection experience with NES-style navigation and transitions.

## Components Created

### Core Components:
1. **LevelData.cs** - Data structures and builder/factory patterns for level information
2. **LevelPoint.cs** - Component for level GameObjects with visual feedback
3. **LevelSelector.cs** - Handles navigation between levels and selector movement
4. **LevelSelectionManager.cs** - Main coordinator that manages the entire system
5. **LevelSelectionDirector.cs** - Director pattern for building level data
6. **ItemSelectScreen.cs** - Shows the "item select screen" before level starts
7. **NESCrossfade.cs** - NES-style crossfade transitions between scenes

## Setup Instructions

### 1. Scene Setup
Create a level selection scene with the following hierarchy:

**Option A: Child Scope (Recommended)**
```
LevelSelectionScene
├── GameManager (with GameLifetimeScope)
│   └── LevelSelectionManager (with LevelSelectionLifetimeScope)
├── LevelContainer (empty GameObject to hold level points)
│   ├── Level_01 (with LevelPoint script)
│   ├── Level_02 (with LevelPoint script)
│   ├── Level_03 (with LevelPoint script)
│   └── ... (more levels)
├── Selector (GameObject that moves between levels)
├── UI Canvas
│   ├── ItemSelectScreen (with ItemSelectScreen script)
│   └── CrossfadeOverlay (with NESCrossfade script)
└── LevelSelector (with LevelSelector script)
```

**Option B: Separate Scopes**
If you need separate scopes, make sure both GameLifetimeScope and LevelSelectionLifetimeScope are at the root level, and the level selection scope will inherit from the game scope.

### 2. VContainer Setup
- Ensure your main scene has a `GameLifetimeScope` that registers core services
- The `LevelSelectionLifetimeScope` should be either a child of the GameLifetimeScope GameObject or set up to inherit from it
- This allows the level selection components to access `IEventBus`, `IGameDataService`, etc.

### 3. Level Point Configuration
For each level GameObject:
1. Add the **LevelPoint** component
2. Configure:
   - Level Name (e.g., "Level_01")
   - Display Name (e.g., "Forest Stage")
   - Scene Name (the actual scene file name)
   - Level Icon (sprite for the level)
   - Icon Renderer (SpriteRenderer for the icon)
   - Lock Renderer (SpriteRenderer for lock overlay)

### 4. Level Selection Manager Configuration
1. Assign all level GameObjects to the levelGameObjects list
2. Set the levelContainer reference
3. Assign the LevelSelector, ItemSelectScreen, and NESCrossfade components
4. Configure input keys (defaults to arrow keys + Enter)

### 5. Selector Configuration
1. Assign the selector GameObject (this moves between levels)
2. Set move speed and grid width for navigation
3. Add audio clips for navigation, selection, and locked sounds

### 6. Item Select Screen Setup
1. Create a UI Image for the item select screen
2. Place your "item select screen.png" sprite in Resources folder
3. Configure display duration and input settings

### 7. Crossfade Setup
1. Create a UI Image that covers the full screen
2. Configure fade colors and NES-style effect settings

## Integration with Existing Systems

### GameData Integration
The system automatically:
- Saves/loads unlocked levels
- Tracks level completion status
- Stores best completion times
- Remembers selected level index

### Event System Integration
Publishes these events:
- `LevelSelectedEvent` - When a level is chosen
- `LevelNavigationEvent` - When navigating between levels
- `ItemSelectScreenRequestedEvent` - When showing item select
- `LevelLoadRequestedEvent` - When loading a level
- `LevelStartedEvent` - When level begins
- `LevelCompletedEvent` - When level is completed

### GameFlowManager Integration
- Added `LevelSelection` state to GameState enum
- Handles scene transitions through crossfade system
- Manages game state changes during level selection

## Usage Example

```csharp
// From main menu, transition to level selection
gameFlowManager.ChangeState(GameState.LevelSelection);

// The system will automatically:
// 1. Load unlocked levels from GameData
// 2. Position selector on last selected level
// 3. Handle input for navigation
// 4. Show item select screen on confirmation
// 5. Load the selected level scene with crossfade
```

## Navigation Controls
- **Arrow Keys**: Navigate between levels (Adventure Island III style)
- **Enter**: Confirm level selection
- **Locked levels**: Play sound and prevent selection

## Customization Points
- Modify navigation grid layout in LevelSelector
- Customize NES crossfade colors and timing
- Add more visual feedback to level points
- Extend level data with additional properties
- Customize item select screen behavior

## Factory and Builder Pattern Usage
- **LevelDataFactory**: Creates LevelData from GameObjects
- **LevelDataBuilder**: Fluent builder for LevelData construction
- **LevelSelectionDirector**: Orchestrates level data building

The system is fully integrated with your existing architecture and ready to use!
