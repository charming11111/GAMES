# 更新日志 / Changelog

## v1.0.3

- 启动时先恢复本地游戏库和封面，再后台扫描并更新变化。
- 缓存原子保存并保留备份，目录暂不可用时保留已缓存游戏。
- Restore the cached library before background discovery; save snapshots atomically with backup recovery.

## v1.0.2

- 修正刺客信条、龙之信条等带英文所有格名称的封面匹配。
- 窗口重新激活时恢复已有缓存但未显示的封面，不重新扫描游戏目录。
- Fixed possessive-name cover matching and recovery of cached images on window activation.

## v1.0.1

- 增加不依赖 .NET 的原生启动入口。未安装 .NET Framework 4.8+ 时，先提示并提供微软官方下载地址。
- 用户确认后才打开网页；不自动下载、安装或更改系统运行环境。已安装的兼容高版本正常放行。
- 保持单 EXE，保留图标，在同一进程中启动原有游戏库；按内容哈希校验并缓存内嵌程序。
- 增加原生依赖检查、版本边界、损坏文件拒绝和中文带空格路径测试。

- Added a native, .NET-independent prerequisite entry point with an official Microsoft download prompt.
- Opens the website only after user confirmation; no automatic installation or registry changes.
- Preserves single-EXE delivery, application icon and same-process execution, with verified payload caching.
- Added native dependency, runtime-boundary, corrupted-bundle and Unicode-path checks.

## v1.0.0

首个开源版本：本地游戏库、手柄导航、封面、主题、音效和托盘支持。

Initial open-source release: local library, controller navigation, covers, themes,
UI audio and system-tray support.
