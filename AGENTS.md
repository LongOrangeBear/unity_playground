# Unity AI Agents — Operating Rules

## ⛔ Non-Negotiables (Stop-the-Line)

### Prefabs
1. **Prefer editing Prefab Assets** in Prefab Mode, not scene instances
2. **Do NOT create accidental Prefab Overrides** in scenes
3. **In Prefab Mode "In Context"**: DO NOT edit Transform values → switch to **Isolation**
4. If override is intentional → explain why and propose Prefab Variant if appropriate

### UI Toolkit
1. **Do NOT edit inline styles** unless explicitly allowed
2. **Edit Style Classes / USS selectors**, not per-element overrides
3. Runtime state changes via `AddToClassList()` / `RemoveFromClassList()`
4. If style "doesn't apply" → check for inline overrides first

### Addressables / Play Mode Safety
1. If Play Mode uses **Asset Database** → NEVER mutate loaded assets in-place
2. **Always**: `Instantiate()` copies and modify copies only
3. Changes to addressable assets in Play Mode CAN persist → use VCS

### ScriptableObjects
1. Changes during **Editor Play Mode CAN persist** → always track via VCS
2. In **Build**: changes are NOT persistent between sessions
3. Treat SO data as **read-only** for save systems

### Scenes
1. **Multi-scene editing** preferred for large systems
2. Runtime additive loading: `LoadSceneMode.Additive` + `SceneManager.UnloadSceneAsync`
3. **If Light Probes used**: Call `LightProbes.Tetrahedralize()` after additive load
4. Editor: use `EditorSceneManager` | Runtime: use `SceneManager`

### UGUI
1. Disable `Raycast Target` on non-interactive graphics (decorations, text, icons)
2. Split canvases: static vs dynamic content

---

## Code Style

### Identifiers
- **Avoid** special chars / Unicode (breaks Unity CLI tools)
- `camelCase`: locals, parameters
- `_camelCase`: private fields
- `PascalCase`: public API, classes, methods
- `UPPER_SNAKE_CASE`: constants
- `kebab-case`: USS class names

### Events
- Names: verb phrases (`OpeningDoor`, `DoorOpened`)
- Prefer `System.Action<T>`
- Emitter methods: prefix with `On...` (`OnDamageReceived`)

### Booleans
- Prefix with verb: `isDead`, `hasWeapon`, `canJump`

### Namespaces
- PascalCase, hierarchical with dots
- Reflect folder structure: `MyGame.Combat.Weapons`

---

## Deliverables (Every Task)

1. **List of changed files** (paths)
2. **How to wire in Unity** (components, Inspector settings, layers/tags)
3. **Verification steps** (what to check in Editor)
4. **Risk notes**:
   - Any created overrides
   - Any inline styles
   - Play Mode persistence concerns

---

## Project Context

- **Unity**: 6.x
- **Render Pipeline**: URP
- **Input**: New Input System
- **Force Text Serialization**: Enabled
- **Target Platforms**: PC (expandable)

### Project Structure
```
Assets/
├── Scripts/          # C# code
│   ├── Gameplay/
│   ├── UI/
│   ├── Systems/
│   └── Editor/
├── Prefabs/
├── Materials/
├── PhysicsMaterials/
├── Scenes/
├── Data/             # ScriptableObjects
├── Tests/
│   ├── Editor/
│   └── Runtime/
└── Plans/
```

### Code Style Quick Reference
| Element | Convention | Example |
|---------|------------|---------|
| Public fields | PascalCase | `public float Speed` |
| Private fields | _camelCase | `private float _speed` |
| Methods | PascalCase | `void Initialize()` |
| Constants | UPPER_SNAKE | `const int MAX_HP` |
| USS classes | kebab-case | `.button--active` |

### Unity Lifecycle Order
1. `Awake()` — cache GetComponent
2. `Start()` — external relationships
3. `OnEnable/OnDisable` — events
4. `FixedUpdate()` — physics
5. `Update()` — logic
6. `LateUpdate()` — camera

---

## Workflow (MUST FOLLOW)

1. **Plan first**: outline steps + touched files/objects
2. **Implement in small commits**: one logical change at a time
3. **After each change**: Wait for compilation → Read Console → Fix errors
4. **Never without plan**: packages, ProjectSettings, mass refactoring

---

## Escalation to Tooling

If any step requires Editor interaction **not possible via file edits**:
- **Propose a Unity Editor script** (`MenuItem` or utility) to automate it
- Examples: Baking lighting, NavMesh generation, Asset import settings
- This makes the operation repeatable and version-controlled

---

## After New Scripts: "How to Wire"

Every new script MUST include:
1. Where to attach (GameObject name/type)
2. Required components
3. Inspector settings (references, layers, tags)
4. Expected runtime behavior

---

## Verification Checklist

After each implementation:
- [ ] Script compiles without errors
- [ ] No Console warnings/errors
- [ ] No unintended Prefab Overrides (Inspector → Overrides)
- [ ] No unintended inline styles (UI Toolkit)
- [ ] Serialized references intact
- [ ] Play Mode safety respected
- [ ] .meta files properly tracked
- [ ] **Active Scene set correctly** (for instantiation)
- [ ] **LightProbes.Tetrahedralize()** called if additive loading with probes
- [ ] **UI Builder edits via Style Class panel**, not Hierarchy

---

## Nested Prefab via Scene Override

If a prefab is nested via scene as override:
- **(a)** Leave override **consciously** with documented reason, OR
- **(b)** **Apply** to convert to proper nested prefab structure

---

## Inline Style Exceptions (UI Toolkit)

Inline styles are allowed ONLY for:
- Unique per-element values (e.g., individual icons on many identical buttons)
- Must be **explicitly documented** with justification
