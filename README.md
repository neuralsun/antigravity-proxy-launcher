<div align="center">

# 🚀 Antigravity Proxy Launcher

### Windows 一键恢复 Antigravity 免 TUN 代理

让 Antigravity 在 Clash / Mihomo 不开启 TUN 或虚拟网卡的情况下稳定使用代理。

<p>
  <a href="https://github.com/neuralsun/antigravity-proxy-launcher/releases"><img src="https://img.shields.io/github/v/release/neuralsun/antigravity-proxy-launcher?style=for-the-badge&logo=github&color=7c3aed" alt="Release"></a>
  <a href="https://github.com/neuralsun/antigravity-proxy-launcher/actions"><img src="https://img.shields.io/github/actions/workflow/status/neuralsun/antigravity-proxy-launcher/build.yml?style=for-the-badge&logo=github-actions&label=build" alt="Build"></a>
  <a href="https://github.com/yuaotian/antigravity-proxy"><img src="https://img.shields.io/badge/upstream-Antigravity--Proxy-06b6d4?style=for-the-badge" alt="Upstream"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-22c55e?style=for-the-badge" alt="License"></a>
</p>

<strong>双击一次</strong> → 自动修复代理文件 → 自动启动 Antigravity

</div>

---

## ✨ 为什么需要它？

Antigravity 更新后，安装目录中的代理 DLL 可能被清理，原来的免 TUN 配置因此失效。这个启动器把恢复流程压缩成一次点击：每次启动前重新部署匹配的代理文件，并启动最新的 Antigravity。

| 传统做法 | 使用本项目 |
|:---|:---|
| 手动寻找安装目录 | 自动定位 <code>Antigravity.exe</code> |
| 手动复制 DLL 和配置 | 自动覆盖并备份 |
| 更新后重复排查 | 再次双击即可恢复 |
| 开启 TUN 接管全局流量 | 只处理 Antigravity 相关进程 |

## 🎯 功能亮点

<div align="center">

| 🔎 自动发现 | 🧰 自动修复 | 🛡️ 自动备份 | 🌐 端口同步 |
|:---:|:---:|:---:|:---:|
| 定位安装目录 | 重新复制 payload | 保留旧文件 | 读取 Clash 配置 |

</div>

- **自动寻找安装目录**：检查常见用户目录、Program Files 和卸载注册表路径。
- **更新后快速恢复**：重新部署 <code>version.dll</code> 与 <code>config.json</code>。
- **自动备份**：替换前按时间保存旧文件，方便回滚。
- **自动读取代理端口**：优先读取 Clash Verge 的 <code>mixed-port</code>、<code>socks-port</code> 或 <code>port</code>。
- **免 TUN**：不创建虚拟网卡，不改变系统全局网络路由。
- **一键启动**：完成部署后自动启动 Antigravity。

## ⚡ 快速开始

### 1. 准备代理

启动 Clash Verge / Mihomo，并确保本机有可用的 SOCKS5 或 mixed 端口。TUN / 虚拟网卡模式可以保持关闭。

### 2. 下载并解压

从 [Releases](https://github.com/neuralsun/antigravity-proxy-launcher/releases) 下载压缩包，保持下面的目录结构：

~~~text
AntigravityProxyLauncher.exe
payload/
├─ version.dll
└─ config.json
~~~

### 3. 双击启动

~~~text
双击启动器 → 寻找 Antigravity.exe → 关闭旧进程
        → 备份文件 → 同步 Clash 端口 → 复制 payload → 启动 Antigravity
~~~

### 4. 更新后再次双击

Antigravity 更新后如果代理失效，直接再次双击启动器即可重新部署。

## 🧩 工作原理

本项目是 [yuaotian/antigravity-proxy](https://github.com/yuaotian/antigravity-proxy) 的 Windows 一键部署与启动封装：

~~~text
┌──────────────────────┐      复制代理文件      ┌─────────────────────┐
│ Antigravity Launcher │ ────────────────────▶ │ Antigravity 目录    │
└──────────────────────┘                       └──────────┬──────────┘
                                                          │ SOCKS5 / mixed
                                                          ▼
                                                   ┌──────────────┐
                                                   │ Clash/Mihomo │
                                                   └──────────────┘
~~~

启动器只负责目标程序的部署和启动；实际网络 Hook 与代理协议由上游 payload 实现。

## 🛠️ 从源码构建

要求：Windows PowerShell 5+，以及系统可用的 .NET Framework 编译支持。

~~~powershell
powershell -ExecutionPolicy Bypass -File .\\build_antigravity_launcher.ps1
~~~

构建后请保持：<code>AntigravityProxyLauncher.exe</code> 与 <code>payload\\version.dll</code>、<code>payload\\config.json</code> 同级。

## 📁 项目结构

~~~text
.
├─ AntigravityProxyLauncher.cs       # 启动器源码
├─ AntigravityProxyLauncher.exe      # 可直接运行的 Windows 程序
├─ build_antigravity_launcher.ps1    # 本地构建脚本
├─ run_antigravity_launcher.cmd      # 命令行启动入口
├─ payload/                          # 上游 x64 代理文件
└─ .github/workflows/build.yml       # GitHub Actions 构建检查
~~~

## 🧯 常见问题

<details><summary><strong>找不到 Antigravity.exe</strong></summary>
程序会自动检查 <code>%LOCALAPPDATA%\\Programs\\Antigravity</code>、Program Files 和 Windows 卸载注册表。请确认 Antigravity 已安装。
</details>

<details><summary><strong>复制文件时提示访问被拒绝</strong></summary>
完全退出 Antigravity 后再运行启动器。如果安装目录受系统保护，请右键启动器选择“以管理员身份运行”。
</details>

<details><summary><strong>代理端口没有自动识别</strong></summary>
启动器默认使用 7897。如果你的 Clash 端口不同，可以修改 payload/config.json 中的 proxy.port。
</details>

<details><summary><strong>对话出现 User location is not supported</strong></summary>
这通常是代理出口 IP 的地区、ASN 或机房属性被服务端限制。先确认 DLL 日志中存在 SOCKS5 握手成功记录，再更换代理出口节点。
</details>

## 📜 日志与备份

默认位置：<code>%LOCALAPPDATA%\\AntigravityProxyLauncher</code>，包含 launcher.log 和按时间命名的 backups 目录。本地应用数据目录不可写时会回退到临时目录。

## 🔐 安全与许可

- 本项目不创建 TUN 网卡，也不接管系统全局流量。
- 只建议对你拥有或获授权修改的 Antigravity 安装使用。
- payload 来自 [Antigravity-Proxy v2.4](https://github.com/yuaotian/antigravity-proxy/releases/tag/v2.4)，其许可遵循上游项目的 LICENSE.txt。
- 启动器源码和构建脚本采用 MIT License，详见 [LICENSE](LICENSE)。

<div align="center">

### 🌟 如果它帮你省下了每次更新后的重复操作，欢迎点一个 Star

</div>
