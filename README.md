<p align="center"><img src="app.png" width="88" alt="GAMES 图标"></p>

<h1 align="center">GAMES</h1>
<p align="center">精简的 Windows 本地游戏启动器</p>
<p align="center"><strong>简体中文</strong> · <a href="README.en.md">English</a></p>

GAMES 使用 C#、WPF 和 .NET Framework 构建，将本地游戏整理成封面宫格，
支持键盘、鼠标及 Xbox / PlayStation 手柄操作。无需 Electron 或浏览器运行时。

## 下载 Windows 版

前往 **[Releases 下载页](https://github.com/charming11111/GAMES/releases/latest)**：

- `GAMES.exe`：单 EXE，下载后直接运行。
- `GAMES-v1.0.11-windows-x64.zip`：程序、使用说明和许可证的完整压缩包。

运行环境：**Windows 10 / 11 x64，.NET Framework 4.8**。
程序未进行商业代码签名；请从本仓库的 Release 下载，并核对随版本提供的 SHA-256。

从 v1.0.11 起，EXE 外层为原生 Windows 启动入口。若没有检测到 .NET Framework
4.8 或更高版本，会先显示中文提示和
[微软官方下载地址](https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net48)。
选择“是”才打开下载页；不会自动下载或安装。安装 **Runtime / 运行时** 后重新打开程序即可。
已安装 4.8.1 或更高兼容版本时直接进入游戏库。

## 功能

- 多目录扫描：选择游戏目录或磁盘，启动时扫描；点击顶部扫描按钮或按 **F5** 手动更新。
- 安装目录识别、Epic 本机安装清单识别、常见辅助程序排除及重复安装合并。
- 在线补充封面、选择本地封面、手动指定 Steam App ID，以及可用时的中文名称匹配。
- 全部、收藏、最近游玩等筛选，以及名称、添加时间、游玩时间和次数排序。
- 深色、浅色、跟随系统主题；渐变呼吸灯选中效果。
- 窗口 / 全屏切换，可设置默认启动方式；全屏固定每行 9 张卡片。
- Xbox / PlayStation 手柄识别和自适应按键提示；后台不响应手柄，切回后先松键再恢复。
- 五套可试听的界面音效与独立音量档位。
- 最小化到系统托盘；启动游戏时显示等待效果。

## 第一次使用

1. 运行 `GAMES.exe`，进入 **设置 → 游戏扫描目录 → 添加扫描目录**。
2. 选择游戏实际安装目录或磁盘，可添加多个目录。
3. 等待扫描和封面补充。之后增减游戏，重启程序或按 **F5** 更新列表。
4. 确认游戏卡片打开操作菜单，可启动、收藏、更新封面、打开游戏目录或指定启动文件。

首次启动没有预设磁盘或个人游戏路径。移除扫描目录不会删除游戏；
“移除游戏”仅将条目隐藏，可在设置中的“已移除游戏”恢复。
在线封面需要联网，本地启动不依赖联网。

### 主要操作

| 操作 | 键盘 | 手柄 |
| --- | --- | --- |
| 移动 / 选择 | 方向键、Tab | 十字键 / 左摇杆 |
| 确认 / 游戏菜单 | Enter | A / × |
| 返回 | Esc | B / ○ |
| 设置 | F1 | Menu / Options |
| 全屏 / 窗口 | F11 | 设置菜单 |
| 手动扫描 | F5 | 顶部扫描按钮 |
| 搜索 | Ctrl+F、顶部放大镜 | 导航至搜索后确认 |
| 收藏 | 游戏菜单 | Y / △ |
| 筛选 | 顶部筛选按钮 | X / □ |
| 翻页 | 滚轮 | LB/RB、L1/R1 |

不同型号、连接方式及驱动可能影响手柄映射。

## 从源码构建

在 Windows x64 上克隆仓库，使用 Windows PowerShell 或 PowerShell 7：

```powershell
git clone https://github.com/charming11111/GAMES.git
cd GAMES
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -Test
```

输出为 `out\GAMES.exe`。构建脚本使用系统 .NET Framework 编译器，
不需要安装 Visual Studio、Node.js 或额外 NuGet 包。
首次构建会下载并校验固定版本的 TinyCC，用于编译不依赖 .NET 的小型原生启动入口；
需要联网，工具缓存于 `.tools`，不随程序分发。
`-Test` 会运行隔离临时目录中的回归检查，结果写入 `out\test-result.txt`。
这些检查覆盖程序行为，不代表所有游戏、显示器或手柄都经过实机验证。

仓库中的 `SDL2.dll` 会嵌入 EXE，首次运行时释放到应用数据目录。
第三方许可证同时嵌入并随发布压缩包提供。

单 EXE 中的托管程序首次运行时会解包到
`%LOCALAPPDATA%\GameShelf\Runtime\<内容哈希>`，之后校验并复用缓存；
游戏库仍在同一个系统进程中运行。启动入口只读取运行环境信息，不更改系统的 .NET 安装。

## 识别边界与数据

- EXE 识别采用目录规则、少量通用入口配置和平台清单，不能保证所有游戏识别准确。
  特殊启动器、多个渲染后端、反作弊入口或非标准目录可在游戏菜单中手动选择 EXE。
- 只会扫描已添加的目录；不会自动遍历所有磁盘，也不要求创建游戏快捷方式。
- 不会把 DLC 当作独立 Epic 游戏；不保证显示平台账号“已购买但未安装”的游戏。
- 封面匹配依赖第三方服务，可能缺失、失效或匹配错误，可使用本地封面或指定 App ID 修正。
- 动画目标为 144 帧/秒，**不是实测 FPS 保证**；性能取决于硬件与系统。
- 设置、封面缓存及本地错误日志位于 `%LOCALAPPDATA%\GameShelf`。
  网络请求会将用于查询的游戏名称发送给对应图片或商店服务；无需提供平台密码或 Token。

## 项目结构

| 文件 | 内容 |
| --- | --- |
| `Shell.cs`、`Visuals.cs`、`CompactHeader.cs` | 窗口、界面与顶部布局 |
| `Discovery.cs`、`entry-profiles.json` | 扫描、去重与通用启动入口规则 |
| `Controllers.cs`、`Navigation.cs` | 手柄与键盘导航 |
| `Metadata.cs`、`Cache.cs` | 封面与资料缓存 |
| `Audio.cs` | 本地合成音效 |
| `LaunchWatch.cs`、`Tray.cs` | 启动等待与托盘 |
| `ContinuousScroll.cs`、`Performance.cs` | 滚动与资源复用 |
| `Tests.cs`、`build.ps1` | 回归检查与构建 |
| `bootstrap/` | 原生运行环境检测、单 EXE 打包与 CLR 启动桥接 |

欢迎提交 Issue 或 Pull Request。报告识别或封面问题时请提供游戏名称、目录层级和
候选 EXE 名称，避免附带个人路径、账号信息、Token 或完整游戏文件。

## 许可证

本项目源码采用 [MIT License](LICENSE)。SDL2 使用 zlib 许可证。
游戏图片、商标与商店资料不属于本项目的 MIT 授权范围。
详见 [第三方声明](THIRD-PARTY-NOTICES.md)。
