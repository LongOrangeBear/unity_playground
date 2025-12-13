# Implementation Plan: Test Scene with Player & UI

> **Prerequisites**: Follow [AGENTS.md](file:///home/meow/work/unity/test3/AGENTS.md) rules strictly.

## Goal
Create a functional test scene with a movable Player and on-screen UI controls for testing input and physics.

---

## Phase 1: Assets Creation

### 1.1 Physics Material
**Path**: `Assets/PhysicsMaterials/ZeroFriction.physicMaterial`
| Property | Value |
|----------|-------|
| Dynamic Friction | 0 |
| Static Friction | 0 |
| Bounciness | 0 |

### 1.2 Materials
| Material | Path | Color |
|----------|------|-------|
| GroundMat | `Assets/Materials/GroundMat.mat` | Grey (#808080) |
| PlayerMat | `Assets/Materials/PlayerMat.mat` | Bright Red (#FF3333) |
| WallMat | `Assets/Materials/WallMat.mat` | Dark Grey (#404040) |

---

## Phase 2: Scripts

### 2.1 PlayerController.cs
**Path**: `Assets/Scripts/Gameplay/PlayerController.cs`

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    
    private Rigidbody _rb;
    private Vector3 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 direction)
    {
        _moveInput = direction;
    }

    private void Update()
    {
        // Keyboard fallback (WASD)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            Vector3 keyInput = Vector3.zero;
            if (keyboard.wKey.isPressed) keyInput.z += 1;
            if (keyboard.sKey.isPressed) keyInput.z -= 1;
            if (keyboard.aKey.isPressed) keyInput.x -= 1;
            if (keyboard.dKey.isPressed) keyInput.x += 1;
            
            if (keyInput != Vector3.zero)
                _moveInput = keyInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        if (_moveInput != Vector3.zero)
        {
            _rb.AddForce(_moveInput * _moveSpeed, ForceMode.Force);
            _moveInput = Vector3.zero;
        }
    }
}
```

**How to Wire**:
1. Attach to Player GameObject (Capsule)
2. Requires: `Rigidbody` component
3. Inspector: Set `_moveSpeed` (default: 5)

---

### 2.2 MoveButton.cs
**Path**: `Assets/Scripts/UI/MoveButton.cs`

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Vector3 _moveDirection;
    
    private PlayerController _player;
    private bool _isPressed;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player == null)
            Debug.LogError("[MoveButton] PlayerController not found!");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
    }

    private void Update()
    {
        if (_isPressed && _player != null)
        {
            _player.Move(_moveDirection.normalized);
        }
    }
}
```

**How to Wire**:
1. Attach to each UI Button
2. Inspector: Set `_moveDirection`:
   - Up: (0, 0, 1)
   - Down: (0, 0, -1)
   - Left: (-1, 0, 0)
   - Right: (1, 0, 0)

---

## Phase 3: Scene Setup

### 3.1 Create Scene
**Path**: `Assets/Scenes/TestScene.unity`

### 3.2 Environment GameObjects
| GameObject | Type | Transform | Notes |
|------------|------|-----------|-------|
| **Ground** | Plane | Pos: (0,0,0), Scale: (2,1,2) | Material: GroundMat, Add MeshCollider |
| **Wall_North** | Cube | Pos: (0,1,10), Scale: (20,2,0.5) | Material: WallMat |
| **Wall_South** | Cube | Pos: (0,1,-10), Scale: (20,2,0.5) | Material: WallMat |
| **Wall_East** | Cube | Pos: (10,1,0), Scale: (0.5,2,20) | Material: WallMat |
| **Wall_West** | Cube | Pos: (-10,1,0), Scale: (0.5,2,20) | Material: WallMat |

### 3.3 Player GameObject
| Property | Value |
|----------|-------|
| **Type** | Capsule |
| **Name** | Player |
| **Position** | (0, 1.1, 0) |
| **Material** | PlayerMat |
| **Rigidbody** | UseGravity: true, Drag: 5, Constraints: Freeze Rotation X, Z |
| **CapsuleCollider** | Physics Material: ZeroFriction |
| **Script** | PlayerController |

### 3.4 Camera
| Property | Value |
|----------|-------|
| **Position** | (0, 10, -10) |
| **Rotation** | (45, 0, 0) |
| **Clear Flags** | Solid Color (Dark Blue) |

### 3.5 UI Hierarchy
```
Canvas (Screen Space - Overlay)
├── EventSystem
└── ButtonPanel (Anchor: Bottom-Left)
    ├── BtnUp (100x100, Top)
    ├── BtnDown (100x100, Bottom)
    ├── BtnLeft (100x100, Left)
    └── BtnRight (100x100, Right)
```

Each button:
- `MoveButton` script attached
- `Button` component with Pressed Color: #999999

---

## Phase 4: Verification Checklist

- [ ] Scripts compile without errors
- [ ] No Console warnings
- [ ] Player moves with WASD keys
- [ ] Player moves with UI buttons (hold to move)
- [ ] Player doesn't tip over (Freeze Rotation working)
- [ ] Player stays within walls
- [ ] Ground has collider (player doesn't fall through)
- [ ] Active Scene set correctly
- [ ] No unintended Prefab Overrides

---

## Rollback Plan
```bash
git revert HEAD  # If issues found after commit
```

---

## Execution Order
1. Create Physics Material
2. Create Materials (3x)
3. Create Scripts (2x) → Wait for compilation
4. Create Scene
5. Add GameObjects
6. Configure Player
7. Configure Camera
8. Create UI Canvas + Buttons
9. Run Verification Checklist
10. Commit: `git commit -m "feat: TestScene with Player and UI controls"`
