# 更新日志 / Changelog

## v1.0.12

- 中英文说明新增深色、浅色及设置菜单演示截图。
- Added dark/light library and settings screenshots to both documentation versions.

- 退出按钮改为电源图标，继续使用退出确认。
- 修复盘根目录放有 GAMES.exe 时，多个游戏被错误合并、漏扫的问题。
- 多目录设置和底部状态同步，不再显示遗留的默认 E 盘。
- 全屏卡片尺寸按实际像素对齐，修复部分分辨率与缩放组合下少一列、右侧留白的问题。
- 补充黑暗之魂3、暗黑血统3、暗影火炬城、地平线4、禁闭求生2、极品飞车18、仁王2、巫师3的封面名称及商店编号匹配。
- 名称纠正后立即重试缺失封面，避免等待旧的失败重试间隔。

- Replaced the close glyph with a power icon while preserving exit confirmation.
- Fixed merged/missing games when scanning a drive containing the launcher, and synchronized the displayed scan directories.
- Aligned fullscreen card widths to physical pixels to preserve nine columns across resolutions and DPI scales.
- Improved title/App ID matching for eight games and retry missing artwork when a title is corrected.

## v1.0.11

- 右上角增加关闭图标，与设置中的退出操作共同打开退出确认，默认选中取消。
- 支持 PS L2 + R2 / Xbox LT + RT 打开退出确认；按住不重复弹窗，保留前台和松键保护。
- Add a top-right close icon and a trigger-chord exit confirmation with cancel selected by default.

## v1.0.10

- 将 SDL 设备读取移出界面线程，独立采样并合并待处理状态，避免读取阻塞菜单响应。
- 使用手柄状态更新接口，关闭未消费的 SDL 事件队列；丢弃过期状态，保留前台和松键保护。
- 增加按下、长按重复、松开再按及过期状态检查；提供可选本地手柄耗时诊断。
- Move SDL polling off the UI thread, coalesce snapshots, discard stale input and preserve foreground/release guards.

## v1.0.9

- 顶部手柄导航与单行布局一致，左右可连续经过全部图标，上下直接往返游戏列表。
- 手柄轮询使用输入优先级，减少焦点重置和整排按钮重绘，缩短长按首次重复等待。
- Align controller navigation with the single header row and prioritize input polling while reducing focus churn and icon repainting.

## v1.0.8

- 全屏长菜单优先使用约 92% 的窗口高度，通过行高和间距撑开。
- 菜单标题与选项字号放大并加粗，说明文字适度放大。
- Taller fullscreen menus with larger, heavier option text and readable descriptions.

## v1.0.7

- 增加菜单常规行高；随窗口高度逐步收紧间距，避免直接跳到最小布局。
- 优先利用可用高度，尽量保持文字和按钮尺寸；保留首次显示前完成布局。
- Use taller menu rows and continuously fit spacing to available height instead of switching to an overly compact layout.

## v1.0.6

- 修复菜单先大后小的跳变：内容构建和尺寸计算完成后才淡入显示。
- 菜单选项切换不再重复触发布局缩放。
- Fit the completed menu before revealing it; keep selection changes independent of sizing.

## v1.0.5

- 菜单根据窗口可用尺寸调整间距并等比例适配，所有选项完整显示，无需菜单滚动条。
- 覆盖设置、游戏操作及其他弹层；支持打开菜单后动态调整窗口大小。
- Responsive menus compact spacing and fit the available window without internal scrolling.

## v1.0.4

- 缓存目录时间戳和启动文件列表，未变化目录不重复枚举文件；新增、删除和子目录变化按需更新。
- F5 / 手动扫描保留完整核对，启动时仍先显示上次游戏库。
- Cache directory inventories for incremental startup checks; keep full verification available through manual scan.

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
