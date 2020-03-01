# 重构说明 Refactoring Description

## 项目说明 Project Description

- Alterful 当前的线上版本（~~[现行版本(已下线)]()~~）为闭源项目，现已终止维护。

  The current online version of Alterful (~~[current version]()~~) is a closed source project, and maintenance has been terminated.

- 该仓库为 Alterful 的开源版本库，使用 C# 对现行版本进行完全重构。

  This repository is an open source version Library of Alterful, which uses C# to completely reconstruct the current version.

- 此仓库项目正在持续更新中，重构完成后将取代现行版本，届时 Alterful 将转型为开源项目并重启维护。

  This repository project is being updated continuously. After the reconstruction, it will replace the current version. At that time, Alterful will transform into an open source project and restart maintenance.

## 重构内容概览 Refactoring Content Overview

- 整体架构及实现细节全部重构。

  Reconstruction overall architecture and all of the implementation details.

- 取消指令输入窗口之外的所有可视化界面。

  Cancel all visual interfaces except command input window.

- 引入新的指令类型：常指令。

  Introduce new instruction types: Const Instruction.

- 引入新的宏指令类型：配置宏。

  Introduce new macro instruction types: @set.

- 引入主题系统。

  Introduction of theme system.

## 核心开发（阶段一） Core Development (Stage 1) ✅

全部完成（All done） ✅

未开始（Not yet begun） ❌

部分完成（Partially completed） 🔳

> 已完成与未开始项都将保持收起状态，部分完成项将展开子项。
>
> Both **All done** and **Not yet begun** items will remain collapsed, and **Partially completed** items will expand children.

### 模块 Modular Diagram

- 热键监视 Hotkey Monitor ✅
- 指令解析器 Instruction Parser ✅
- 执行器 Actuator ✅

![Modular Diagram](https://i.loli.net/2019/11/01/eoX5AbgLduMQ8Kj.png)

-----

### 基础功能 Basic Functions

- 文件功能 File Function ✅
- 系统功能 System Function ✅
- ~~辅助功能 Auxiliary Function~~

![Basic Functions](https://i.loli.net/2019/11/01/VkGIW2uqACZoQws.png)

------

### 指令系统 Instruction System

- 文件指令 File Instruction ✅
- 宏指令 Macro Instruction ✅
- 命令行指令 Command Instruction ✅ 
- 常指令 Const Instruction ✅

![Instruction System](https://i.loli.net/2019/11/01/yZrzJ8RN2PiIW45.jpg)

## 深度开发（阶段二） Improvement Development (Stage 2) 🔳

- 常指令语法扩展
  - 常指令重载 ✅
  - 常指令组合 ✅
  - 常指令的特化与偏特化 ✅
  
- 指令补全进阶
  - 启动名智能补全 ✅
  - 常引用名智能补全 ✅
  - 宏指令智能补全 ✅
  - 常指令智能补全 ✅
- 命令行交互
  - 记忆工作目录 ✅
  - 异步命令行 ❌
- 规范常引用解析域 ✅

## 扫尾工作（阶段三）  Concluding Work (Stage 3) 🔳

- 版本检查 🔳
  - 版本号检查 ✅
  - 文件差异检查 ✅
- 自动更新 ✅
- 自动扫描启动项 ❌
- Alterful 配置宏 ✅
- 主题设置 ✅
- 系统右键菜单植入 ✅
- 右键菜单调用实现 ✅
- 互斥检测 ✅
- 注册表配置 ✅
- 帮助 & 提示 ❌
- 局部再重构 ❌

## 自测与内测（阶段四）Self Test & Internal Test (Stage 4) ❌

- 暂无记录

