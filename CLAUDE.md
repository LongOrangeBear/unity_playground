# Claude Code — Unity Workflow

**Follow [AGENTS.md](file:///home/meow/work/unity/test3/AGENTS.md) strictly.**

---

## When Given a Task

1. **Identify Unity surface area**: Scenes / Prefabs / UI Toolkit / UGUI / Addressables / ScriptableObjects
2. **Produce a minimal plan** (max 8 bullets)
3. **Implement** via file edits (C#, UXML, USS, asmdef, etc.)
4. **Verify**:
   - Build/compile succeeds
   - No unintended Prefab Overrides (Inspector → Overrides)
   - No UI Toolkit inline overrides (unless explicitly approved)
   - Play Mode safety respected for Addressables + ScriptableObjects
   - Active Scene set correctly
5. **Report**:
   - Changed files (paths)
   - Editor verification steps
   - Rollback plan (git revert points)

---

## Escalation to Tooling

If any step requires Editor interaction not possible via file edits:
- **Propose a Unity Editor script** (`MenuItem` or Companion API) to make it automatable
- Examples: Baking lighting, NavMesh, Asset import settings

---

## Deliverables (Every Task)

1. **List of changed files** (paths)
2. **How to wire in Unity** (components, Inspector settings, layers/tags)
3. **Verification steps** (what to check in Editor)
4. **Risk notes**:
   - Any created overrides (explain why if intentional)
   - Any inline styles (explain exception if allowed)
   - Play Mode persistence concerns
5. **Rollback plan** (git commit/revert reference)

---

## Project Context

See full rules in [AGENTS.md](file:///home/meow/work/unity/test3/AGENTS.md) and [GEMINI.md](file:///home/meow/work/unity/test3/GEMINI.md).

- **Unity**: 6.x
- **Render Pipeline**: URP
- **Input**: New Input System
- **Force Text Serialization**: Enabled (required for AI to read .prefab/.unity)
