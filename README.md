# BusyBox 多功能效率工具箱

![Version](https://img.shields.io/badge/Version-v1.0.0-blue?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-green?style=for-the-badge)
![Framework](https://img.shields.io/badge/.NET-6.0-purple?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-orange?style=for-the-badge)

**一款基于番茄工作法的多功能 Windows 桌面效率工具**

集成了 AI 智能对话、科学计算器、笔记待办等多种实用功能

[🌐 官网](https://busybox.devynhub.org) • [📚 文档](https://busybox.devynhub.org/#/documents) • [❓ 帮助](https://busybox.devynhub.org/#/help) • [💬 反馈](https://busybox.devynhub.org/#/feedback)

***

## 目录

- [项目概述](#项目概述)
- [核心功能](#核心功能)
- [技术原理](#技术原理)
- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [构建指南](#构建指南)
- [配置说明](#配置说明)
- [常见问题](#常见问题)
- [贡献指南](#贡献指南)
- [许可证](#许可证)

***

## 项目概述

### 简介

BusyBox 是一款专为 Windows 用户打造的多功能效率工具箱。以番茄工作法为核心时间管理方法，辅以多种实用工具，帮助用户提升工作效率和生活品质。

### 设计理念

| 理念     | 说明                 |
| ------ | ------------------ |
| **专注** | 基于番茄工作法，帮助用户保持专注状态 |
| **集成** | 多种常用工具集成于一体，减少应用切换 |
| **简洁** | 界面简洁直观，操作便捷高效      |
| **隐私** | 数据本地存储，保护用户隐私      |

### 官网资源

| 资源      | 链接                                                                  | 说明        |
| ------- | ------------------------------------------------------------------- | --------- |
| 🏠 项目官网 | [busybox.devynhub.org](https://busybox.devynhub.org)                | 产品介绍与下载   |
| 📚 使用文档 | [documents](https://busybox.devynhub.org/#/documents)               | 新手入门与功能详解 |
| ❓ 帮助中心  | [help](https://busybox.devynhub.org/#/help)                         | 常见问题与使用指南 |
| 💬 用户反馈 | [feedback](https://busybox.devynhub.org/#/feedback)                 | 问题反馈与建议   |
| 💝 支持项目 | [donate](https://busybox.devynhub.org/#/donate)                     | 支持开发者     |
| 🙏 致谢页面 | [acknowledgements](https://busybox.devynhub.org/#/acknowledgements) | 开源致谢      |

***

## 核心功能

### 1. 番茄工作法计时器

**功能说明**

番茄工作法是一种时间管理方法，通过将工作时间分割为 25 分钟的专注时段（称为"番茄钟"），每个番茄钟结束后休息 5 分钟，帮助用户保持高效专注。

**功能特性**

| 功能    | 说明                  |
| ----- | ------------------- |
| 专注计时  | 默认 25 分钟专注时段，可自定义时长 |
| 休息提醒  | 专注结束自动切换至休息模式       |
| 项目管理  | 创建多个项目，设置不同时长       |
| 工作流预设 | 内置"高效学习"、"快速工作"等模板  |
| 全屏模式  | 沉浸式专注体验，按 F11 切换    |
| 主题切换  | 支持自定义界面主题           |

**使用场景**

- 📖 学习备考：使用工作流预设规划学习计划
- 💼 工作任务：追踪项目进度，保持专注
- 🏃 健身锻炼：设置运动与休息间隔

> 💡 **更多使用技巧请访问** **[帮助中心](https://busybox.devynhub.org/#/help)**

***

### 2. AI 智能助手

**功能说明**

集成多种 AI 大语言模型，支持本地模型和云端 API，提供智能对话服务。

**支持平台**

| 平台       | 类型     | 特点                    |
| -------- | ------ | --------------------- |
| OpenAI   | 云端 API | GPT-4、GPT-4o-mini 等模型 |
| DeepSeek | 云端 API | 国产大模型，中文能力强           |
| 豆包       | 云端 API | 字节跳动出品，多模态支持          |
| 本地模型     | 本地推理   | 支持 GGUF/GGML 等格式，隐私保护 |

**技术参数**

| 参数          | 说明                |
| ----------- | ----------------- |
| Max Tokens  | 最大生成令牌数，控制响应长度    |
| Temperature | 温度参数，控制输出随机性（0-2） |
| Top P       | 核采样参数，控制输出多样性     |
| Top K       | 限制候选词数量           |
| 其它参数        | 数十个其他功能           |

**功能特性**

- 🔄 实时流式响应
- 💾 聊天历史自动保存
- 📊 Token 使用统计
- ⚡ 速率限制保护

> ⚠️ **注意**：使用云端 API 需要自行申请 API Key，本程序不提供内置密钥。

> 💡 **详细配置教程请访问** **[文档中心](https://busybox.devynhub.org/#/documents)**

***

### 3. 科学计算器

**功能说明**

功能完整的科学计算器，支持基础运算、科学函数、进制转换、编码处理等。

**功能模块**

#### 基础运算

| 运算   | 示例              |
| ---- | --------------- |
| 四则运算 | `+` `-` `×` `÷` |
| 幂运算  | `x²` `x³` `xʸ`  |
| 根运算  | `√x` `³√x`      |
| 百分比  | `%`             |
| 阶乘   | `n!`            |

#### 三角函数

| 函数                 | 说明       |
| ------------------ | -------- |
| sin / cos / tan    | 正弦、余弦、正切 |
| asin / acos / atan | 反三角函数    |
| sinh / cosh / tanh | 双曲函数     |

> 支持角度制（DEG）和弧度制（RAD）切换

#### 对数运算

| 函数  | 说明            |
| --- | ------------- |
| ln  | 自然对数（以 e 为底）  |
| log | 常用对数（以 10 为底） |
| eˣ  | e 的 x 次方      |
| 10ˣ | 10 的 x 次方     |

#### 进制转换

| 进制   | 说明      |
| ---- | ------- |
| 二进制  | Base-2  |
| 八进制  | Base-8  |
| 十进制  | Base-10 |
| 十六进制 | Base-16 |

#### 编码转换

| 编码            | 说明         |
| ------------- | ---------- |
| ASCII/Unicode | 字符编码转换     |
| URL 编码        | URL 安全编码   |
| Base64        | Base64 编解码 |

#### 哈希计算

| 算法      | 输出长度    |
| ------- | ------- |
| MD5     | 128 bit |
| SHA-1   | 160 bit |
| SHA-256 | 256 bit |
| SHA-384 | 384 bit |
| SHA-512 | 512 bit |

#### CRC 校验

- CRC32 校验值计算
- 支持文本和十六进制输入

#### 微积分工具

- 数值导数计算
- 定积分近似计算

***

### 4. 规划中的功能

> ⚠️ **以下功能已在用户端规划开发中，因程序异常或未完全完成，暂未上传至源码**
>
> - **格式转换器**：音频、视频、文档等多种格式的相互转换
> - **智能设备**：设备连接管理，网络/蓝牙通信
>
> 💡 **完整功能介绍请访问** **[官网](https://busybox.devynhub.org)**

### 5. 实用工具集

#### 快速便签

- 轻量级笔记工具
- 支持多窗口
- 自动保存

#### 待办事项

- 任务列表管理
- 进度追踪
- 完成状态标记

#### 主机信息

- 系统信息查看
- 硬件配置显示
- IP 地址查询

***

## 技术原理

### 架构设计

```
┌─────────────────────────────────────────────────────────────────┐
│                        表现层 (Presentation)                       │
│                                                                   │
│    ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐  │
│    │MainForm  │  │ AIForm   │  │CalcForm  │  │ 规划中的功能   │  │
│    │番茄计时器│  │AI对话    │  │计算器    │  │格式转换/设备  │  │
│    └──────────┘  └──────────┘  └──────────┘  └──────────────┘  │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│                        业务逻辑层 (Business Logic)                │
│                                                                   │
│    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│    │Timer Engine  │  │ AI Engine    │  │ Calc Engine  │        │
│    │计时引擎      │  │ AI引擎       │  │ 计算引擎     │        │
│    └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│                        数据访问层 (Data Access)                   │
│                                                                   │
│    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│    │File I/O      │  │HTTP Client   │  │JSON Parser   │        │
│    │文件读写      │  │网络请求      │  │数据序列化    │        │
│    └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│                        基础设施层 (Infrastructure)                │
│                                                                   │
│    ┌───────────────────────────────────────────────────────┐    │
│    │ .NET 6.0 Runtime │ Windows Forms │ NAudio │ JSON    │    │
│    └───────────────────────────────────────────────────────┘    │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### 核心技术栈

| 技术                | 版本    | 用途        |
| ----------------- | ----- | --------- |
| .NET              | 6.0   | 运行时框架     |
| Windows Forms     | -     | 图形用户界面    |
| NAudio            | 2.2.x | 音频播放处理    |
| Newtonsoft.Json   | 13.x  | JSON 序列化  |
| HttpClient        | -     | HTTP 网络请求 |
| System.Management | -     | 系统信息获取    |

### 关键实现

#### 番茄计时器

```
原理：基于 System.Windows.Forms.Timer 实现
间隔：1000ms（1秒）
状态机：运行 → 暂停 → 继续 / 结束 → 休息
```

#### AI 对话

```
原理：基于 HttpClient 异步请求
协议：RESTful API
格式：JSON 请求/响应
流式：支持 Server-Sent Events
```

#### 科学计算器

```
原理：表达式解析与求值
算法：递归下降解析器
支持：运算符优先级、函数调用、括号嵌套
```

***

## 快速开始

### 环境要求

| 要求   | 最低配置                     | 推荐配置                     |
| ---- | ------------------------ | ------------------------ |
| 操作系统 | Windows 10               | Windows 11               |
| 运行时  | .NET 6.0 Desktop Runtime | .NET 6.0 Desktop Runtime |
| 内存   | 4 GB RAM                 | 8 GB RAM                 |
| 磁盘   | 200 MB                   | 500 MB                   |
| 网络   | -                        | 宽带连接（使用 AI 功能）           |

### 安装运行

#### 方式一：下载发布版本

前往 [官网下载页面](https://busybox.devynhub.org/#/download) 获取最新版本。

#### 方式二：从源码构建

```bash
# 1. 克隆仓库
git clone https://github.com/YINIANnew/BusyBox.git
cd BusyBox

# 2. 还原依赖
dotnet restore

# 3. 构建项目
dotnet build src/BusyBoxMVP/BusyBoxMVP.csproj -c Release

# 4. 运行程序
dotnet run --project src/BusyBoxMVP/BusyBoxMVP.csproj
```

#### 方式三：发布可执行文件

```bash
# 发布为自包含应用（无需安装 .NET）
dotnet publish src/BusyBoxMVP/BusyBoxMVP.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true
```

***

## 项目结构

```
BusyBox/
│
├── src/                                # 源代码目录
│   └── BusyBoxMVP/                     # 主项目
│       │
│       ├── BusyBoxMVP.csproj           # 项目配置文件
│       ├── Program.cs                  # 程序入口点
│       │
│       ├── MainForm.cs                 # 主窗口（番茄计时器）
│       │   ├── MainForm                # 主窗体类
│       │   ├── ToolsForm               # 工具选择窗体
│       │   ├── SettingsForm            # 设置窗体
│       │   ├── ExitConfirmForm         # 退出确认窗体
│       │   ├── ThemeManagerForm        # 主题管理窗体
│       │   └── MusicPlayerForm         # 音乐播放窗体
│       │
│       ├── AIForm.cs                   # AI 对话模块
│       │   ├── AIForm                  # AI 平台选择窗体
│       │   ├── AIChatForm              # 对话交互窗体
│       │   ├── AIState                 # 状态管理类
│       │   └── ChatMessage             # 消息数据类
│       │
│       ├── ScientificCalculatorForm.cs # 科学计算器模块
│       │   ├── ScientificCalculatorForm    # 计算器主窗体
│       │   ├── 表达式解析器            # 数学表达式解析
│       │   ├── 进制转换工具            # Base 转换
│       │   ├── 编码转换工具            # Encoding 转换
│       │   ├── 哈希计算工具            # Hash 计算
│       │   └── 微积分工具              # 导数/积分计算
│       │
│       ├── InfoForm.cs                 # 信息模块
│       │   ├── 系统信息显示            # System info
│       │   ├── 运行日志                # Runtime logs
│       │   └── 快捷链接                # Quick links
│       │
│       ├── TodoForm.cs                 # 待办事项模块
│       ├── NotesForm.cs                # 便签模块
│       ├── HostInfoForm.cs             # 主机信息模块
│       │
│       ├── Themes/
│           └── ThemeManager.cs         # 主题管理系统
│
│       └── (规划中)                    # 以下功能规划开发中，暂未上传
│           ├── ConvertForm.cs          # 格式转换模块
│           └── SmartDeviceForm.cs      # 智能设备模块
│
├── README.md                           # 项目说明文档
└── LICENSE                             # MIT 许可证
```

***

## 构建指南

### 开发环境

| 工具                 | 说明     |
| ------------------ | ------ |
| Visual Studio 2022 | 推荐 IDE |
| VS Code            | 轻量级编辑器 |
| .NET 6.0 SDK       | 必需     |

### 项目配置

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NAudio" Version="2.2.1" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

### NuGet 依赖

| 包名              | 版本     | 说明       |
| --------------- | ------ | -------- |
| NAudio          | 2.2.1  | 音频播放库    |
| Newtonsoft.Json | 13.0.3 | JSON 处理库 |

***

## 配置说明

### 数据存储

程序数据存储在用户应用数据目录：

```
%APPDATA%\BusyBox\
├── settings.txt           # 应用设置（当前主题）
├── first_launch.txt       # 首次启动标记
├── install_date.txt       # 安装日期记录
├── ai_state.json          # AI 使用状态统计
├── ChatLog_*.json         # 对话历史记录
└── Logs\                  # 运行日志目录
    └── log_YYYYMMDD.txt   # 按日期存储
```

### 配置文件格式

**settings.txt** - 主题设置

```
theme1
```

**ai\_state.json** - AI 状态

```json
{
  "LastDateReset": "2024-01-01T00:00:00",
  "DailyTokensUsed": 1500,
  "LastRequestTime": "2024-01-01T12:30:00",
  "RequestCount10Min": 3,
  "Last10MinReset": "2024-01-01T12:25:00"
}
```

### 程序权限

| 权限类型 | 用途说明                 |
| ---- | -------------------- |
| 文件系统 | 读写配置、保存数据、导出日志       |
| 网络访问 | AI API 调用、IP 查询、资源下载 |
| 系统信息 | 读取硬件信息、系统配置          |
| 音频设备 | 播放提示音、音乐文件           |

> 详细权限说明请查看程序内"信息 → 程序权限"

***

## 常见问题

### 安装与运行

**Q: 程序无法启动，提示缺少 .NET？**

A: 需要安装 .NET 6.0 Desktop Runtime。请前往 [Microsoft 官网](https://dotnet.microsoft.com/download/dotnet/6.0) 下载安装。

**Q: 程序被杀毒软件拦截？**

A: 这是误报，可添加信任后运行。如不放心，可从源码自行编译。

### 其他问题

> 更多问题请访问 [帮助中心](https://busybox.devynhub.org/#/help) 或 [提交反馈](https://busybox.devynhub.org/#/feedback)

***

## 贡献指南

欢迎参与项目开发！

### 参与方式

1. **报告问题** - 在 Issues 中提交 Bug 报告
2. **功能建议** - 提出新功能想法
3. **代码贡献** - 提交 Pull Request
4. **文档改进** - 完善文档内容

### 开发流程

```bash
# 1. Fork 仓库
# 2. 创建分支
git checkout -b feature/your-feature

# 3. 提交更改
git commit -m "Add: your feature"

# 4. 推送分支
git push origin feature/your-feature

# 5. 创建 Pull Request
```

### 代码规范

- 使用 `PascalCase` 命名公共成员
- 使用 `_camelCase` 命名私有字段
- 异步方法使用 `async/await`
- 关键操作添加异常处理

***

## 许可证

本项目采用 MIT 许可证开源。

```
MIT License

Copyright (c) 2024 BusyBox

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

***

## 致谢

感谢以下开源项目：

- [.NET](https://dotnet.microsoft.com/) - 跨平台开发框架
- [NAudio](https://github.com/naudio/NAudio) - .NET 音频库
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON 框架
- [番茄工作法](https://en.wikipedia.org/wiki/Pomodoro_Technique) - 时间管理方法

> 完整致谢列表请访问 [致谢页面](https://busybox.devynhub.org/#/acknowledgements)

***

**Built with ❤️ for productivity**

**[🌐 官网](https://busybox.devynhub.org)** **•** **[📚 文档](https://busybox.devynhub.org/#/documents)** **•** **[❓ 帮助](https://busybox.devynhub.org/#/help)** **•** **[💬 反馈](https://busybox.devynhub.org/#/feedback)**

**Version: v1.0.0**

***备注：该markdown文档撰写有AI参与***
