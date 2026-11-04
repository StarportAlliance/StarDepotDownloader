StarDepotDownloader
===================

基于 SteamKit2 的 Steam Depot 下载工具。

## 安装

从 [Releases 页面](https://github.com/StarportAlliance/StarDepotDownloader/releases/latest) 下载对应平台的压缩包。

## 用法

### 下载某个应用的单个或全部 Depot

```powershell
./StarDepotDownloader -app <id> [-depot <id> [-manifest <id>]]
                      [-username <username> [-password <password>]] [其他选项]
```

示例：`./StarDepotDownloader -app 730 -depot 731 -manifest 7617088375292372759`

默认使用匿名账号（[可在此查看该账号可访问的应用](https://steamdb.info/sub/17906/)）。

如需使用自己的账号，请指定 `-username <username>`。如果未指定 `-password` 参数，程序会以交互方式询问密码。

### 使用 pubfile id 下载创意工坊项目

```powershell
./StarDepotDownloader -app <id> -pubfile <id> [-username <username> [-password <password>]]
```

示例：`./StarDepotDownloader -app 730 -pubfile 1885082371`

### 使用 ugc id 下载创意工坊项目

```powershell
./StarDepotDownloader -app <id> -ugc <id> [-username <username> [-password <password>]]
```

示例：`./StarDepotDownloader -app 730 -ugc 770604181014286929`

## 参数

#### 身份验证

参数                    | 说明
----------------------- | -----------
`-username <user>`      | 用于访问受限内容的账号用户名。
`-password <pass>`      | 用于访问受限内容的账号密码。
`-remember-password`    | 设置后会记住该用户的密码，供后续登录使用。（将 `-username <username> -remember-password` 作为登录凭据）
`-qr`                   | 显示登录二维码，可用 Steam 手机应用扫描。
`-no-mobile`            | 优先输入 2FA 验证码，而不是在 Steam 手机应用中确认。
`-loginid <#>`          | 十进制 32 位整数形式的 Steam LogonID，并发运行多个 StarDepotDownloader 实例时必须指定。

#### 下载

参数                     | 说明
------------------------ | -----------
`-app <#>`               | 要下载的 AppID。
`-depot <#>`             | 要下载的 DepotID。
`-manifest <id>`         | 要下载内容的 manifest id（需要配合 `-depot`，默认为当前分支的最新版本）。
`-ugc <#>`               | 要下载的 UGC ID。
`-pubfile <#>`           | 要下载的 PublishedFileId。（会自动解析为 UGC id）
`-branch <branchname>`   | 如果存在则从指定分支下载（默认：Public）。
`-branchpassword <pass>` | 分支密码（如适用）。

#### 下载配置

参数                     | 说明
----------------------- | -----------
`-all-platforms`        | 使用 `-app` 时下载所有平台专属的 Depot。
`-os <os>`              | 下载游戏对应的操作系统（windows、macos 或 linux，默认：程序当前运行的操作系统）
`-osarch <arch>`        | 下载游戏对应的架构（32 或 64，默认：宿主架构）
`-all-archs`            | 使用 `-app` 时下载所有架构专属的 Depot。
`-all-languages`        | 使用 `-app` 时下载所有语言专属的 Depot。
`-language <lang>`      | 下载游戏对应的语言（默认：english）
`-lowviolence`          | 使用 `-app` 时下载低暴力版本 Depot。
`-dir <installdir>`     | 存放下载文件的目录。
`-filelist <file.txt>`  | 本地文件路径，该文件包含要从 manifest 下载的文件列表。如需使用正则匹配，请在文件路径前加上 `regex:`。每个文件路径独占一行。
`-validate`             | 对已下载文件进行校验和验证。
`-manifest-only`        | 下载将下载的 Depot 的可读 manifest。
`-cellid <#>`           | 覆盖要下载的内容服务器 CellID。
`-max-downloads <#>`    | 并发下载分块的最大数量。（默认：8）
`-use-lancache`         | 强制通过局域网中的 Lancache 实例下载。

#### 其他

参数                    | 说明
----------------------- | -----------
`-debug`                | 启用详细的调试日志。
`-V` 或 `--version`     | 输出版本与运行时信息。

## 常见问题

### 为什么每次运行程序都要求输入两步验证码？
你的两步验证码用于验证 Steam 会话。你需要用 `-remember-password` 来「记住」会话，该参数会持久化 Steam 会话的登录密钥。

### 账号已连接到 Steam 时，可以运行 StarDepotDownloader 吗？
如果共用同一个 LoginID，任何 Steam 连接都会被断开。你可以用 `-loginid` 指定不同的 LoginID。

### 为什么包含特殊字符的密码无法使用？必须在命令行中指定密码吗？
如果用 `-password` 参数传入包含特殊字符的密码，你需要按所用 Shell 的规则正确转义。只要指定了 `-username`，就不必在命令行中包含 `-password` 参数，程序会以交互方式提示你输入密码。

### 下载旧 manifest id 时返回 401 或没有返回 manifest 代码
尝试使用 Steam 账号登录，使用匿名账号时可能会出现此问题。

Steam 允许开发者阻止下载旧 manifest，这种情况下即使参数看似正确，也不会返回 manifest 代码。

### 为什么下载速度慢且经常连接超时？
下载旧版本时，缓存服务器可能没有现成的分块可用，从而导致下载变慢。
尝试增大 `-max-downloads` 以更充分地利用网络带宽。
