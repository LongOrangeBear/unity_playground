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
3. **Brains & Body Separation**:
   - **Never** put logic scripts on the root object you intend to `SetActive(false)`
   - Structure: `[Parent] Logic_GO` (Always Active) -> `[Child] Visual_Panel` (Toggled)


### UI Creation via Editor Scripts
1. **Prefer programmatic UI creation** via Editor scripts (`MenuItem`) over manual scene setup
2. Create Canvas, panels, Text, Button via `AddComponent<>` and `RectTransform`
3. Link references using `SerializedObject.FindProperty().objectReferenceValue`
4. Font: use `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` for default
5. **Pivot-Anchor Law**: If anchoring to a side/corner, set Pivot to that same side/corner (e.g., Top-Left Anchor (0,1) -> Top-Left Pivot (0,1))
6. **Defensive Refactoring**: When changing helper signatures, find ALL usages (Cmd+Shift+F) before compiling
7. **Result**: reproducible UI setup without manual Inspector work


### New Input System
1. **EventSystem MUST use `InputSystemUIInputModule`** — NOT `StandaloneInputModule`
2. `StandaloneInputModule` causes `InvalidOperationException` with New Input System
3. When creating UI Canvas with EventSystem programmatically:
   - Remove `StandaloneInputModule` if present
   - Add `UnityEngine.InputSystem.UI.InputSystemUIInputModule`
4. For keyboard input in scripts: use `Keyboard.current` (not `Input.GetKey`)

### Scene Creation Checklist
1. **Always add Main Camera** — new scenes are EMPTY (no camera, no lighting)
2. Camera setup:
   - Tag: `MainCamera`
   - Add `AudioListener`
   - Set position/rotation for intended view
3. **Check Play Mode before scene operations** — `create`/`save`/`load` fail in Play Mode
4. When creating environment: verify Ground has collider (Plane has MeshCollider by default)

### MCP Tool Limitations
1. **PhysicMaterial** — NOT supported via `manage_asset create`. Use Editor script.
2. **Component properties** — `manage_gameobject modify` has limited property support. Use `SerializedObject` in Editor scripts.
3. **Position/Rotation** — use array format `[x, y, z]`, not dict `{x:, y:, z:}`
4. **Timeout after domain reload** — wait for scripts to compile before calling menu items

### Movement Tuning
1. **Default speed must match level scale**:
   - Level 20x20 units → Speed ~40-60 (not 5!)
   - Formula: `speed = levelWidth / desiredCrossTimeSeconds`
2. **Rigidbody Drag affects acceleration**:
   - High drag (5) = sluggish start
   - Low drag (1-2) = snappy response
3. **Always verify movement feel in Play Mode** before finalizing

### Runtime Material Creation
1. `Shader.Find()` may return null at runtime (shader not in build) → **check for null**
2. Safer alternatives:
   - `renderer.material` — use primitive's existing material
   - `Resources.Load<Material>()` — explicitly included
3. If using `Shader.Find()`: wrap in null-check and provide fallback

### Collision Callbacks (CharacterController)
1. `OnControllerColliderHit` fires **every frame** while in contact — NOT once per collision
2. For one-time events (death, damage): **debounce with timer or flag**
3. If invincibility/shield needed:
   - Prevent death (logic check)
   - Consider disabling obstacle collider for pass-through

### Debug Logging Best Practices
1. In collision/Update callbacks: **prefer conditional logging** or remove after debugging
2. Good: Log state changes (`"Shield activated"`, `"Died"`)
3. Caution: Log in loops only during active debugging, remove for production
4. Consider `#if UNITY_EDITOR` or `[Conditional("DEBUG")]` for dev-only logs

### Spawn Rate Defaults
1. Initial spawn rates should be **visible for testing** (higher than intended)
2. Power-ups: start with 20-30% chance, tune down for balance
3. Verify spawn feels right in Play Mode before committing

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

1. **Clarify First**: If there are ANY questions or ambiguities, **ASK the user** before starting.
2. **Plan first**: outline steps + touched files/objects
3. **Implement in small commits**: one logical change at a time
3. **After each change**: Wait for compilation → Read Console → Fix errors
4. **Never without plan**: packages, ProjectSettings, mass refactoring

---

## Escalation to Tooling

If any step requires Editor interaction **not possible via file edits**:
- **Propose a Unity Editor script** (`MenuItem` or utility) to automate it
- Examples: Baking lighting, NavMesh generation, Asset import settings
- This makes the operation repeatable and version-controlled

### ⚠️ Before Flagging "Manual Configuration Required"
1. **STOP** and ask: "Can this be automated via an Editor script?"
2. Common automatable tasks:
   - Assigning prefab references to components → `SerializedObject.FindProperty()`
   - Setting component properties → `SerializedProperty`
   - Configuring layers/tags → `SerializedObject` on prefabs
   - Creating scene objects → `GameObject.CreatePrimitive()` + `AddComponent<>()`
3. **Prefer** `[InitializeOnLoad]` or `[MenuItem]` scripts over manual steps
4. Only flag as "manual" if truly requires visual judgment (e.g., adjusting camera view)

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

### Compilation & Console
- [ ] Script compiles without errors
- [ ] No Console warnings/errors
- [ ] .meta files properly tracked

### Scene Integrity
- [ ] **Main Camera exists** with tag `MainCamera`
- [ ] **EventSystem uses `InputSystemUIInputModule`** (if New Input System)
- [ ] Active Scene set correctly (for instantiation)
- [ ] No unintended Prefab Overrides (Inspector → Overrides)
- [ ] Serialized references intact
- [ ] Ground/colliders prevent falling through

### UI
- [ ] No unintended inline styles (UI Toolkit)
- [ ] UI Builder edits via Style Class panel, not Hierarchy
- [ ] Raycast Target disabled on decorative elements

### Play Mode Safety
- [ ] Play Mode is STOPPED before scene save/create
- [ ] Changes during Play Mode noted (ScriptableObjects persist!)
- [ ] Addressable assets: use `Instantiate()` copies only

### Gameplay Feel
- [ ] Movement speed matches level scale
- [ ] Rigidbody drag tuned for responsiveness
- [ ] Input verified: keyboard AND UI controls working

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

---

## Phase Retrospective Practice

**After completing each implementation phase**, conduct a brief retrospective:

1. **What assumptions were made?** — Were they verified?
2. **What steps required iteration?** — Could they have been caught earlier?
3. **What new patterns emerged?** — Should they become reusable utilities?
4. **What would make this faster next time?** — Tooling, templates, better validation?

Record key insights in **BOTH**:
1. Implementation plan under `### Lessons Learned` (project-specific)
2. `AGENTS.md` under `## General Recommendations` (reusable patterns)

---

## General Recommendations (from Experience)

### Verify After Create
- When creating assets via tools, **check the result** before proceeding
- Properties passed during creation may not apply as expected
- Prefer explicit setters over creation-time configuration

### Automation First
- Before suggesting manual steps, **consider Editor scripts**
- Many Inspector operations can be automated via `SerializedObject`
- `[InitializeOnLoad]` runs after domain reload — useful for one-time setup

### Anticipate Timing Issues
- New scripts/menus may not be available immediately
- Domain reload takes time — wait for compilation before invoking
- Use `EditorApplication.delayCall` for post-reload execution

### Incremental Testing
- Test each step before moving to the next
- Don't assume multi-step operations all succeeded
- Prefer small batches with verification over large batches

### Default to Visibility
- Spawn rates, colors, sizes should be **obviously visible** during development
- Silent failures are harder to debug than visible ones
- Make things obvious first, tune down later

### Prefab Asset Modifications
- `sharedMaterial` edits on prefab assets may not persist via Editor scripts
- For reliable material changes on prefabs, **edit the .prefab YAML directly** (`m_Materials` GUID)
- **(b)** **Apply** to convert to proper nested prefab structure
318: - Always verify prefab changes after saving

---

## Phase 8 Retrospective (Mistakes & Fixes)

### 1. Physics vs. Visuals Separation
*   **Mistake:** Applied Squash & Stretch (Scale) to the Root object containing `CharacterController`. This broke physics interactions and grounding.
*   **Rule:** **NEVER** scale the Root object if it involves physics. Always create a child object named `Visual` (or similar) and apply all scale/rotation animations to it.

### 4.4 Shaders & Materials (URP)
- **Property Naming**: ALWAYS use `_BaseColor` (not `_MainColor`) for the primary color to ensure compatibility with `Material.color`, SRP Batcher, and standard Inspectors.
- **World Space UVs**: For procedural or scalable environment geometry (walls, floors), prefer World Space UVs (`positionWS.xz`) to prevent texture stretching.
- **Vertex Displacement**: If using vertex displacement (e.g., Curved World), ALWAYS calculate UVs and Shading logic using the **Original** (non-displaced) position to avoid texture "swimming" or warping.
- **Visual Clarity**: Prioritize **Gameplay Readability** over style tropes.
    - Avoid high-contrast grids on large surfaces (causes eye strain/Moiré).
    - Obstacles must be solid enough to judge distance (avoid pure holograms).

### 4.5 Automation Scripts
- **Scene Persistence**: Automation scripts modifying Scene objects must execute in **Edit Mode** (Stopped).
    - **Rule**: Changes made to GameObjects during Play Mode are temporary. Ensure your `[MenuItem]` or `[InitializeOnLoad]` scripts check `!Application.isPlaying` before attempting to apply permanent changes or set objects dirty.
- **Scene vs Prefab**: Modifying a Prefab Asset does not always update the Scene instance if overrides exist. in **Edit Mode**, explicitly refresh the scene instance if needed.

### 2. Collider Runtime Modification
*   **Mistake:** Modified `CharacterController.height` assuming pivot is at FEET. When pivot is CENTER, reducing height enables gravity to snap player down, causing clipping on restore.
*   **Rule:** When animating collider size, **calculate the bottom position** manually and adjust `center.y` to keep the bottom fixed relative to the world, regardless of pivot.

### 3. VFX Visibility Debugging
*   **Mistake:** Used standard URP shaders for Trails without verifying settings, leading to "invisible trail" confusion.
*   **Rule:** For new VFX (Trails, Lines), **start with `Sprites/Default`** (pink/white) or `Unlit/Color` to guarantee visibility before tuning aesthetics. If it handles logic, make it ugly but visible first.

### 4. Serialization Traps
*   **Mistake:** Changed script default `bool = true` hoping it would update existing scene instances. It didn't (serialized `false` persisted).
*   **Rule:** If changing a default value on an existing component, **force the value in `Awake()`/`Start()`** during development or use `OnValidate` to update inspector values. Don't trust field initializers for existing assets.
