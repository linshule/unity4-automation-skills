# Unity 4.x 自动化技能包

面向**国内高校本科游戏开发课程**中仍在使用的遗留 Unity 版本（4.6.8 及 4.x），提供批量构建、编辑器脚本、资源批处理、CI/CD 集成等自动化方案。

## 功能

- **命令行构建** — 通过 `-batchmode -executeMethod` 实现在无头环境下的自动化构建，配合日志扫描检测错误
- **多平台构建脚本** — PowerShell 包装器，支持一次性构建多个目标平台
- **编辑器脚本参考** — MenuItem、AssetPostprocessor、EditorWindow、AssetDatabase 等 API 的中文参考
- **批量资源处理** — 开箱即用的 C# 脚本，一键批量修改纹理格式、尺寸、Mipmap 开关
- **CI/CD 集成指南** — 适配 Jenkins 等持续集成流水线的 Unity 4.x 方案
- **踩坑大全** — Unity 4.x 特有的 API 差异、.NET 限制、已知问题和解决方案

## 目录结构

```
unity4-automation/
├── SKILL.md                         # 主技能定义与工作流（AI 读取）
├── agents/
│   └── openai.yaml                  # OpenCode Agent 界面配置
├── references/
│   ├── batchmode.md                 # 命令行参数与 BuildOptions 参考
│   ├── editor_scripting.md          # 编辑器脚本 API 参考
│   └── limitations.md               # Unity 4.x 限制与避坑指南
└── scripts/
    ├── batch_set_texture_format.cs  # 批量纹理处理器（尺寸、Mipmap、格式）
    └── unity_batch_build.ps1        # 多平台构建 PowerShell 脚本
```

## 使用方法

### 作为 AI Agent Skill 使用

本技能兼容任何支持 `SKILL.md` 机制的 AI 编程助手：

| AI 工具 | 放置路径 |
|---------|----------|
| **Claude Code / Claude Desktop** | `.claude/skills/unity4-automation/` |
| **OpenCode** | `.opencode/skills/unity4-automation/` 或 `.agents/skills/unity4-automation/` |
| **Cursor** | `.cursor/skills/unity4-automation/` |

### 直接复制脚本

脚本和参考文档可以独立使用，直接复制到你的 Unity 项目即可：

| 文件 | 复制到 |
|------|--------|
| `scripts/batch_set_texture_format.cs` | `Assets/Editor/` |
| `scripts/unity_batch_build.ps1` | 项目根目录或 CI 流水线目录 |

## 适用场景

- 🏫 **本科课程教学** — 课程仍在使用 Unity 4.x 版本，需要自动化构建和批量处理
- 🛠 **作业批量评测** — 助教需要批量编译学生项目并检查构建结果
- 🔄 **老旧项目维护** — 需要为遗留 Unity 4.x 项目搭建 CI/CD 流水线
- 📚 **学习参考** — 学习 Unity 4.x 编辑器脚本开发和自动化工作流

## 使用示例

### PowerShell 批量构建

```powershell
.\unity4-automation\scripts\unity_batch_build.ps1 `
    -UnityPath "C:\Program Files (x86)\Unity\Editor\Unity.exe" `
    -ProjectPath "C:\MyUnityProject" `
    -ExecuteMethod "Builder.DoBuild" `
    -Targets @("StandaloneWindows", "Android")
```

### 编辑器脚本批量处理纹理

打开 Unity → 菜单栏 → `Tools > Batch >` 选择需要的操作：
- `Set All Textures Max Size 256` — 所有纹理最大尺寸改为 256
- `Disable Mipmaps on All Textures` — 关闭所有纹理的 Mipmap
- `Set All Textures to Compressed` — 所有纹理改为压缩格式

## 许可证

MIT
