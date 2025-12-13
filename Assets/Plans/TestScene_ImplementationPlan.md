# Implementation Plan: Test Scene with Player & UI

## Goal
Create a functional test scene with a movable Player and on-screen UI controls.

## Proposed Changes

### 1. Scene & Environment
| Item | Details |
|------|---------|
| **Scene** | `Assets/Scenes/TestScene.unity` |
| **Camera** | Position (0, 10, -10), Rotation (45, 0, 0) — Top-down isometric |
| **Skybox** | Solid color (Dark Blue) for visual clarity |
| **Lighting** | Directional Light, Soft Shadows, Ambient Intensity 0.5 |
| **Ground** | Plane at (0, 0, 0), Scale (2, 1, 2), Material: "GroundMat" (Grey) |
| **Walls** | 4 Cubes (thin, tall) surrounding the area |
| **Physics Material** | "ZeroFriction" (Friction: 0, Bounciness: 0) |

### 2. Player
| Item | Details |
|------|---------|
| **GameObject** | Capsule "Player" at (0, 1.1, 0) |
| **Material** | "PlayerMat" (Bright Red) |
| **Rigidbody** | UseGravity=true, Drag=5, **Freeze Rotation X/Z** |
| **CapsuleCollider** | PhysMat = ZeroFriction |
| **Script** | `PlayerController.cs` |

**PlayerController.cs**:
- `[SerializeField] float moveSpeed = 5f;` — Configurable in Inspector
- `Move(Vector3 direction)` — Called by UI
- Uses `FixedUpdate` + `Rigidbody.AddForce` for physics-based movement
- **Keyboard Fallback**: WASD/Arrows for Editor testing

### 3. UI
| Item | Details |
|------|---------|
| **Canvas** | Screen Space - Overlay, CanvasScaler (Scale With Screen Size) |
| **EventSystem** | Standard (auto-created) |
| **Button Panel** | Bottom-left, 4 buttons (↑ ↓ ← →), Size: 100x100 each |
| **Visual Feedback** | Button color change on press (Pressed Color: Darker) |
| **Script** | `MoveButton.cs` (attached to each button) |

**MoveButton.cs**:
- Implements `IPointerDownHandler`, `IPointerUpHandler`
- `[SerializeField] Vector3 moveDirection;` — Set per button (e.g., 0,0,1 for Up)
- Finds `PlayerController` and calls `Move(direction)` every `Update` while pressed

### 4. Folder Structure
```
Assets/
├── Scenes/
│   └── TestScene.unity
├── Scripts/
│   ├── PlayerController.cs
│   └── MoveButton.cs
├── Materials/
│   ├── GroundMat.mat
│   ├── PlayerMat.mat
│   └── WallMat.mat
└── PhysicsMaterials/
    └── ZeroFriction.physicMaterial
```

## Verification Plan
1. **Compile**: Check console for errors
2. **Hierarchy**: Verify all GameObjects exist
3. **Play Mode**: 
   - Test WASD keyboard input
   - Test UI button hold-to-move
   - Verify player stays on ground and doesn't tip
4. **Screenshot**: Capture scene to verify visual setup
