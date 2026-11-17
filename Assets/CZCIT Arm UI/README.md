# CZCIT Arm UI 26.11.17

**作者：BeiWuCN · MIT · Unity 2022.3 · VR**

将任意 World Space Canvas 停靠在左腕。翻腕显示，右手指向拖拽柄并按住 UI Press 可将面板拉出。松手时靠近左腕就吸附回去，远离则留在世界空间。

> **先做一个能点的平面面板，再装到手腕。** 这样可以分别确认 UI 输入、追踪以及模型偏移。Arm UI 可以用普通 uGUI 材质，无需安装 Glass UI。

## 1. 导入并检查 XR 输入

1. 在 Unity 选择 **Assets → Import Package → Custom Package…**，导入 Arm UI，资源位于 **Assets/CZCIT Arm UI**。
2. 配置 Unity OpenXR 和 XR Interaction Toolkit。确保目标平台已启用 OpenXR，并启用与手柄匹配的交互配置。
3. 在场景中使用带 **XR UI Input Module** 的 EventSystem。右手使用 XR Ray Interactor，启用 UI 交互，并给控制器绑定 **UI Press**。
4. 给测试 Canvas 设置 **World Space**，World Camera 指向 XR 相机，添加 **Tracked Device Graphic Raycaster**。先用右手射线点一下普通 Button。能点了，再接腕部停靠。

**SteamVR 可以作为 Windows 的 OpenXR 运行时。** 在 SteamVR 中设为当前 OpenXR 运行时，并在 Unity 中用实际手柄的交互配置。直接用 Valve SteamVR Input 的工程需要另接输入和追踪适配。当前发布的组件尚未在 SteamVR 头显上实测。

## 2. 用默认控制器搭第一块腕部面板

```text
XR Origin
├─ XR Camera                       → Viewer
├─ Left Controller                 → 左手追踪对象
│  └─ LeftDock                     → 左腕停靠点，可移动和转向
└─ Right Controller                → Right Hand

ArmUI Controller                   → WristPanelDock
WristCanvas                        → World Space Canvas + CanvasGroup
├─ Panel                           → 背景 Image
├─ DragHandle                      → Image + WristPanelHandle
└─ ReturnButton                    → Button
```

**WristCanvas 放在稳定的场景根节点下。** 这样解下的面板能留在世界空间。把 LeftDock 放在左手追踪对象下，让停靠点跟着手走。

| Inspector 字段 | 绑定方式 |
| --- | --- |
| **Left Dock** | 拖入左手下的 LeftDock 空物体。它的世界位置是吸附中心。 |
| **Right Hand** | 拖入右手控制器的追踪 Transform。 |
| **Viewer** | 拖入 XR Camera 的 Transform。 |
| **Panel** | 拖入 WristCanvas 上的 CanvasGroup。 |
| **Reveal Axis** | LeftDock 局部坐标中的翻腕检测轴，默认 (0, 1, 0)。 |
| **Reveal Dot** | 翻腕显示阈值，默认 0.15。数值提高会要求检测轴更加朝向世界上方。 |
| **Snap Distance** | 松手时允许吸附的距离，默认 0.22 世界单位。 |

在 DragHandle 上勾选 Image 的 **Raycast Target**，把 WristPanelDock 拖给 WristPanelHandle。背景和纯装饰标签关闭 Raycast Target。把归位按钮的 **On Click()** 绑定到 **WristPanelDock.ReturnToWrist**。

可从 Canvas 尺寸 **360 × 220**、均匀 Scale **0.001** 起步。在一单位一米的场景内，这相当于约 36 × 22 厘米。进入 VR 翻腕，观察布局，再按可读性和遮挡情况调整。

## 3. 换成自定义手臂或全身模型

**先决定你希望面板跟追踪，还是跟模型骨骼。**

- **只换外观，交互仍跟控制器**。保留 LeftDock 在左手追踪对象下，Right Hand 继续绑定右手追踪对象。调整 LeftDock 局部位置，让面板避开新模型的手背。这适合模型有 IK 延迟、手指动画或空闲姿态的场景。
- **需要贴在模型袖口上**。在左手腕骨骼下放一个独立 LeftDock。保留 Right Hand 的真实控制器绑定。腕骨需要随玩家追踪更新，当前组件仍用 Unity XR 的左手追踪状态决定是否显示。测试抬腕、转腕和 IK 切换，确认面板不会穿袖口或抖动。

**别按模型的轴名猜方向。** 不同 FBX 的腕骨局部轴可能不同。选中 LeftDock，把 Scene 工具坐标切到 **Local**，对照手背朝向，选合适的轴。手背朝上才想显示，而当前是掌心朝上显示时，可将 Reveal Axis 从 (0, 1, 0) 改为 (0, -1, 0) 测试。

按这个顺序调，可快速定位问题：

1. **先调停靠位置。** 暂时把 Reveal Dot 设为 -1，方便追踪有效时持续看到面板。在 Scene 中移动 LeftDock，每次沿一轴少量挪动。以袖口或手背上方为起点，给握拳动作留出空间。
2. **再调朝向检测。** 把 Reveal Dot 恢复到 0.15，依次试手背朝上、手臂自然下垂、双手靠近。根据结果调 Reveal Axis 或 LeftDock 旋转。**组件会让停靠面板朝向相机，旋转 LeftDock 主要影响显隐检测。**
3. **最后调吸附范围。** 先把面板拖出，在离左腕不同距离的位置松手。吸附用的是 **Canvas 根节点的位置与 LeftDock 的距离**。如果视觉上已经靠近却吸不住，先查 Canvas Pivot 和 LeftDock 位置，再适量增大 Snap Distance。
4. **把结果保存到 Rig Prefab。** Play Mode 中临时调出来的位置退出后会恢复。记下最终坐标或复制组件值，退出后再应用到 Prefab。

## 4. 区分三个容易混淆的位置

| 想改变什么 | 应该改哪里 |
| --- | --- |
| 整块 UI 默认出现的位置 | 移动 **LeftDock**。停靠时 Canvas 根节点对齐它。 |
| 用户能抓住界面的哪一块 | 改 **DragHandle 的 RectTransform**。建议锚在面板底部中央，适当加宽可点区域。 |
| 离手腕多远会吸回去 | 改 **Snap Distance**。它调整距离阈值，吸附中心仍是 LeftDock。 |
| 想让吸附中心靠近面板边缘 | 调整 **Canvas Pivot**，再检查所有子控件布局。也可以把可见内容整体偏移，让 Canvas 根节点继续对齐左腕。 |

拖动开始时组件记住面板相对右手的偏移，避免画布跳到控制器中心。**右手绑定真实追踪 Transform**，不要绑在可能滞后的渲染手骨上。

## 5. 接入自己的 UI 逻辑

如果系统菜单关闭或玩家重置位置时需要归位，可以在业务脚本中调用同一个公开方法：

```csharp
using CZCIT.ArmUI;
using UnityEngine;

public sealed class WristMenuActions : MonoBehaviour
{
    [SerializeField] private WristPanelDock dock;

    public void ResetMenuPosition()
    {
        // 下一帧由 WristPanelDock 对齐左腕，并按翻腕朝向决定显隐。
        if (dock != null)
            dock.ReturnToWrist();
    }
}
```

## 6. 验收和排错

- **面板不出现**：先检查 XR 左手追踪，再临时将 Reveal Dot 调为 -1，排除手腕朝向问题。
- **按钮能点，拖拽柄不动**：检查拖拽柄的 Raycast Target、WristPanelHandle 的 Dock 引用以及右手追踪。拖拽要按住 UI Press 并移动指针，直到触发拖动事件。
- **松手后随玩家走**：检查 WristCanvas 有没有放在 XR Origin 或手腕骨骼下。要固定在世界里，应放在稳定的场景根节点下。
- **换模型后穿模或偏位**：调整 LeftDock 的局部位置。若视觉腕骨有 IK 延迟，可把停靠点放回追踪节点，再用位置偏移对准外观。
- **测试归位**：先远距离松手确认面板留在原地，再拖近左腕释放，最后用归位按钮验证一次。
- **测试追踪丢失**：手腕面板应在左手失去追踪时隐藏。右手在拖动中失去追踪后，再确认面板状态与再次抓取符合应用预期。
