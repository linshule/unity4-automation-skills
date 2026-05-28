# Unity 4.x Automation Skills

Automate Unity 4.6.8 (and 4.x) projects: batchmode builds, editor scripting, asset batch processing, and CI integration.

Designed for Chinese university undergraduate game development courses that still use legacy Unity versions in their curriculum.

## Features

- **Batchmode Build** — Command-line builds via `-batchmode -executeMethod` with log-based error detection
- **Multi-platform Build Wrapper** — PowerShell script for cross-platform CI builds
- **Editor Scripting Reference** — MenuItem, AssetPostprocessor, EditorWindow, AssetDatabase APIs tailored for Unity 4.x
- **Batch Asset Processing** — Ready-to-use C# scripts for bulk texture format, size, and mipmap conversion
- **CI/CD Integration** — Patterns for Jenkins and similar pipelines with Unity 4.x
- **Limitations & Pitfalls** — Comprehensive reference of Unity 4.x API gaps, .NET constraints, and known workarounds

## Contents

```
unity4-automation/
├── SKILL.md                         # Main skill definition & workflows
├── agents/
│   └── openai.yaml                  # OpenCode agent interface config
├── references/
│   ├── batchmode.md                 # Batchmode CLI & BuildOptions reference
│   ├── editor_scripting.md          # MenuItem, Postprocessor, EditorWindow, AssetDatabase APIs
│   └── limitations.md               # Unity 4.x API gaps, shader names, C# constraints, CC pitfalls
└── scripts/
    ├── batch_set_texture_format.cs  # Batch texture processor (max size, mipmaps, format)
    └── unity_batch_build.ps1        # Multi-platform PowerShell build wrapper
```

## Usage

### As an AI Agent Skill

This skill works with any AI coding assistant that supports `SKILL.md`:

- **Claude Code / Claude Desktop** — place `unity4-automation/` under `.claude/skills/`
- **OpenCode** — place `unity4-automation/` under `.opencode/skills/` or `.agents/skills/`
- **Cursor** — place `unity4-automation/` under `.cursor/skills/`

### Standalone Scripts

The scripts under `scripts/` and references under `references/` are standalone — copy them directly into your Unity project:

| File | Destination |
|------|-------------|
| `scripts/batch_set_texture_format.cs` | `Assets/Editor/` |
| `scripts/unity_batch_build.ps1` | Project root or CI pipeline directory |

## 中文文档

[README.zh-CN.md](README.zh-CN.md) — 中文版说明文档

## License

MIT
