# Gemini CLI — Unity Workflow

> **Core Rules**: See [AGENTS.md](file:///home/meow/work/unity/test3/AGENTS.md) — this is the authoritative ruleset.

---

## Operating Mode

- Prefer **deterministic changes**: code + assets layout
- Never "guess" about Prefab Overrides → always instruct how to verify via Inspector → Overrides
- For UI Toolkit: always prefer USS selectors and classList state switches
- Never mutate addressable assets in Play Mode → `Instantiate()` copies only

---

## When Given a Task

1. **Identify Unity surface area**: Scenes / Prefabs / UI Toolkit / UGUI / Addressables / ScriptableObjects
2. **Produce a minimal plan** (max 8 bullets)
3. **Implement** via file edits (C#, UXML, USS, asmdef, etc.)
4. **Verify**:
   - Build/compile succeeds
   - No unintended Prefab Overrides
   - No UI Toolkit inline overrides
   - Play Mode safety respected
   - Active Scene set correctly
5. **Report**:
   - Changed files (paths)
   - Editor verification steps
   - Rollback plan

---

## Escalation to Tooling

If any step requires Editor interaction not possible via file edits:
- **Propose a Unity Editor script** (`MenuItem` or utility) to automate it
- Examples: Baking lighting, NavMesh, Asset import settings

---

## Output Contract (Every Task)

1. **Exact file paths modified**
2. **Commands run** (build/tests)
3. **Unity Editor checklist** for manual validation
4. **Risk notes**: overrides, inline styles, Play Mode persistence
5. **Rollback plan** (git commit/revert reference)

---

## Project Context

- **Unity**: 6.x
- **Render Pipeline**: URP
- **Input**: New Input System
- **Force Text Serialization**: Enabled (required for AI to read .prefab/.unity)
- **Target Platforms**: PC (expandable)
- **Folder conventions**: `Assets/{Scripts,Prefabs,Materials,Scenes,Data,Plans}`

---

## Project Structure

```
Assets/
├── Scripts/          # Core game logic
│   ├── Gameplay/
│   ├── UI/
│   ├── Systems/
│   └── Editor/
├── Prefabs/          # Prefab assets
├── Materials/        # Materials and shaders
├── PhysicsMaterials/ # Physics materials
├── Scenes/           # Scene files
├── Data/             # ScriptableObjects
└── Plans/            # Implementation plans
```

---

## MCP Integration

- Unity MCP server running via **stdio** transport (not HTTP)
- Use `validate_script` before applying code changes
- Consider custom MCP tools for project-specific operations

---

## References

- [AGENTS.md](file:///home/meow/work/unity/test3/AGENTS.md) — Operating rules (non-negotiables, checklists)
- [CLAUDE.md](file:///home/meow/work/unity/test3/CLAUDE.md) — Claude-specific workflow
- [Assets/Plans/PromptTemplates.md](file:///home/meow/work/unity/test3/Assets/Plans/PromptTemplates.md) — Reusable prompts
