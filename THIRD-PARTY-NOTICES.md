# Third-party notices / 第三方声明

## SDL2 2.32.10

The bundled, unmodified Windows x64 `SDL2.dll` provides controller support.
SDL2 is licensed under the zlib license, separately from this project's MIT license.
See [SDL-LICENSE.txt](SDL-LICENSE.txt) and [SDL-NOTICE.txt](SDL-NOTICE.txt).

随附的未修改 SDL2 Windows x64 运行库用于手柄支持，按 zlib 许可证分发，
不受本项目 MIT 许可证重新授权。原始来源：

- <https://github.com/libsdl-org/SDL/releases/tag/release-2.32.10>
- Source revision: `5d249570393f7a37e037abf22cd6012a4cc56a71`

## Game names, artwork and services / 游戏名称、图片和服务

Game names, trademarks, store metadata and downloaded cover artwork belong to their
respective owners. They are not relicensed under MIT. No downloaded game covers,
game binaries, user libraries or platform account data are included in this repository.
Cover requests use third-party services such as Steam and official publisher sites;
availability and matching accuracy may change. GAMES is not affiliated with Valve,
Epic Games, Riot Games, Sony, Microsoft or Nintendo.

游戏名称、商标、商店资料及下载的封面归各自权利人所有，不按 MIT 许可证授权。
仓库不附带下载的游戏封面、游戏本体、个人游戏库或平台账号资料。
本项目与上述平台或厂商没有隶属或官方合作关系。

## Project artwork / 项目美术

The application icon and built-in emulator placeholder poster are drawn by project
code. They are presentation assets, not official endorsements. Third-party names
and marks remain the property of their respective owners.

## Build-only compiler / 仅用于构建的编译器

The native entry is compiled with TinyCC 0.9.27, an LGPL-licensed build tool.
The compiler is downloaded during building and is not included in release assets.
The pinned upstream ZIP has SHA-256
`34a721949a2583fdff725312da092fa0f5f1f284b702e6f811c6954714faabb2`.
The build uses `-nostdlib` and a project-owned entry point, linking only to Windows
system DLLs rather than TinyCC CRT/runtime objects.

TinyCC 是仅用于构建的 LGPL 编译工具，不随发布程序分发。
构建脚本校验固定安装包哈希；镜像下载的文件也必须完全匹配同一哈希。

- Upstream: <https://bellard.org/tcc/>
- Archive: <https://download.savannah.gnu.org/releases/tinycc/tcc-0.9.27-win64-bin.zip>
- Verified byte-identical mirror: <https://github.com/phoenixthrush/Tiny-C-Compiler/releases/tag/0.9.27-win64>
