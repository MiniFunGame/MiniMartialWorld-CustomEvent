# 🧭 MiniMartialWorld-CustomEvent（事件制作教程）

```
MiniMartialWorld-CustomStrategy
├─ DLLBuilder/           # 你的 C# Mod 工程（编译成 .dll）
├─ ContentSample/        # 创意工坊示例（放置编译好的 .dll 与预览图）
└─ README.md             # 本说明（你正在看）
```

本教程讲解如何编写并打包**自定义事件（Event / FightEvent）**，并以「示例解析」的统一格式给出完整代码与注释。

---

## 快速开始（3 步）
1. 在 `DLLBuilder/` 新建或打开你的 C# 工程（Unity 兼容的 .NET 版本），参照模板添加依赖引用。
2. 在工程中创建你的事件脚本：继承 `Event`（剧情/对话/奖励）或 `FightEvent`（直接进入战斗）。
3. 编译生成 `YourMod.dll`，与预览图一起放入 `ContentSample/`，添加对应图片，上传创意工坊即可加载。

> **提示**：`getWeight()` 返回值 > 0 才会被抽到。可结合**时间/技能等级/历史次数**等作为出现条件。

---

## 最小模板（可直接复制改名）

```csharp
using System;
using System.Collections.Generic;

[System.Serializable]
public class MyFirstEvent : Event
{
    public MyFirstEvent() {
        Quality   = Quality.Rare; // 稀有度
        TimeLimit = 1;            // 执行次数上限（可选）
        LoadLimit = 2;            // 出现次数上限（可选）
        // Uncounted = true;      // 可跳过事件（可选）
    }

    public override string getName()        => "我的第一条事件";
    public override string getDescription() => "一句简短的事件描述。";
    public override string getImageName()   => "事件图片键名";

    // 触发：卡池权重；效果：>0 才会入池
    public override float getWeight() {
        return 3;
    }

    // 触发：对话结束；效果：发奖励/改属性/开选择面板等
    public override void execute() {
        string input = "【文本】这是我的第一条事件。【标题】事件标题";
        List<Slot> slots = SlotParser.ParseSlots(input);
        Slot last = slots[slots.Count - 1];
        last.OnFinished = () => {
            AttributeManager.ModifyExperience(50);
        };
        DisplayControl.HaveDialog(slots[0]); // 别忘了把第一个槽给对话系统
    }
}
```

---

## 示例解析

### MartialArtsMuralDiscovery2

```csharp
// === 事件：古壁武图（MartialArtsMuralDiscovery2）===
// 类型：探索 / 对话奖励

[System.Serializable]
public class MartialArtsMuralDiscovery2 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;               // 稀有事件
        return "古壁武图";
    }

    public MartialArtsMuralDiscovery2()
    {
        TimeLimit = 1;                        // 执行次数上限
        LoadLimit = 2;                        // 最高出现次数
    }
    // getDescription：事件描述文本（用于卡片/预览）
    public override string getDescription()
    {
        return "你在探查古遗迹时，偶然发现了一面布满尘埃的古老壁画。";
    }


    public override void execute()
    {
        string input =
          "【文本】你在一处古遗迹中前行，突然眼前一亮，发现了一幅斑驳却依旧清晰的武学壁画。"
        + "【文本】壁画上的人物似乎在演练一门奇异的掌法，身形飘逸而凌厉，仿佛能破空碎石。"
        + "【文本】你意识到这幅壁画极有价值，但也需要更多时间方能彻底领悟。";

        List<Slot> slots = SlotParser.ParseSlots(input);
        Slot last = slots[slots.Count - 1];
        last.OnFinished = () =>
        {
            AttributeManager.ModifyExperience(80);   // 经验 +80
            AttributeManager.ModifyComprehension(1); // 悟性 +1
        };
        DisplayControl.HaveDialog(slots[0]);
    }

    public override string getImageName() => "遗迹";

    public override float getWeight()
    {
        // 采矿等级<2不出现；否则强制入候选并按等级加权
        if (Controller.GameData.Player.SkillList.Mining.getLevel() < 2) return 0;

        return 4 + Controller.GameData.Player.SkillList.Mining.getLevel();
    }
}
```

要点：首次探索/早期引导型事件；与**采矿等级**相关，满足后“强制展示”并给予经验与悟性。

---

### ObserveMural_2

```csharp
// === 事件：壁画参悟（ObserveMural_2）===
// 类型：后续 / 对话奖励 / 串联

[System.Serializable]
public class ObserveMural_2 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;
        return "壁画参悟";
    }

    public ObserveMural_2()
    {
        TimeLimit = 1;
        LoadLimit = 2;
    }
    // getDescription：事件描述文本（用于卡片/预览）
    public override string getDescription()
    {
        return "你静心观摩古壁上的武学图像，试图从中悟得一二。";
    }

    public override void execute()
    {
        string input =
          "【文本】你凝神观想壁画，画中人物的身影仿佛活了过来，在你眼前演练掌法。"
        + "【文本】那是一门以气破石的刚猛掌法，每一击都似要震碎山岳。"
        + "【文本】（你对刚猛武学的理解加深了）";

        var slots = SlotParser.ParseSlots(input);
        var last  = slots[slots.Count - 1];
        last.OnFinished = () =>
        {
            AttributeManager.ModifyExperience(60);
            AttributeManager.ModifyStrength(1); // 臂力 +1
        };
        DisplayControl.HaveDialog(slots[0]);
    }

    public override string getImageName() => "遗迹";

    public override float getWeight()
    {
        // 只有“古壁武图”已触发过才出现
        if (EventTracker.GetExecutionCount("MartialArtsMuralDiscovery2") > 0)
        {
            ForcedDisplay = true;
            return 5;
        }
        return 0;
    }
}
```

要点：**串联事件**，依赖 “古壁武图” 的执行次数，奖励经验与**臂力**。

---

### ElderEvent_1

```csharp
// === 事件：长老（ElderEvent_1）===
// 类型：教学 / 早期权重

[System.Serializable]
public class ElderEvent_1 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;
        return "长老";
    }

    public ElderEvent_1() { TimeLimit = 1; }

    public override string getDescription() => "外门长老在教导门下弟子.";

    public override void execute()
    {
        string input = "【文本】桀桀桀，以此功吞天地之气，夺众生之灵，锻无上魔躯.【标题】外门长老";
        var slots = SlotParser.ParseSlots(input);
        var last  = slots[slots.Count - 1];
        last.OnFinished = () =>
        {
            AttributeManager.ModifyAttack(3); // 攻击 +3
        };
        DisplayControl.HaveDialog(slots[0]);
    }

    public override string getImageName() => "长老2";

    public override float getWeight()
    {
        if (Controller.GameData.Time > 36) return 0; // 仅早期，前三年才会出现
        return 3;
    }
}
```

要点：早期教学，结束后提升**攻击**，时间晚于阈值则不再出现。

---

### Teaching

```csharp
// === 事件：武功教学（Teaching）===
// 类型：选择 / 给予技能

[System.Serializable]
public class Teaching : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;
        return "武功教学";
    }

    public override string getDescription() => "外门长老在为新入门的弟子传授武功.";

    public override void execute()
    {
        var list = new List<string> { "基础剑法", "混混飞踢", "灵蛇拳法" };
        DisplayControl.SkillChoose(list);     // 弹出技能选择面板
    }

    public override string getImageName() => "长老";
    public override float getWeight() => 0;   // 由剧情触发/外部控制
}
```

要点：直接弹出**技能选择**窗口，不主动入池（权重 0）。

---

### Teaching_Next_2

```csharp
// === 事件：修炼教学（Teaching_Next_2）===
// 类型：奖励 / 延迟发放计略

[System.Serializable]
public class Teaching_Next_2 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;
        return "修炼教学";
    }

    public override string getDescription() => "外门长老正在耐心讲解策略运用的精妙之处.";

    public override void execute()
    {
        // 延迟发放：计略/效果在后续统一结算
        DisplayControl.ChooseNewStrategyLater(
            new List<object> { new MeticulousAward(), new GangYu().GetVariants().ToList()[1], new ZhanFeng().GetVariants().ToList()[3] });
    }

    public override string getImageName() => "墨长老";
    public override float getWeight() => 0;
}
```

要点：用于发放**计略**的“延迟奖励面板”。

---

### MedicalSkill_1

```csharp
// === 事件：药理讲解（MedicalSkill_1）===
// 类型：教学 / 属性成长

[System.Serializable]
public class MedicalSkill_1 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;
        return "药理讲解";
    }

    public MedicalSkill_1() { TimeLimit = 2; }

    public override string getDescription() => "药堂的长老来讲解药理基础";

    public override void execute()
    {
        string input = "【文本】“赤血晶，乃炼石之精。其色赤如火，其质坚似金。若得此晶，可引天地之灵气，壮己之血气，益寿延年.”【标题】药堂长老";
        var slots = SlotParser.ParseSlots(input);
        var last  = slots[slots.Count - 1];
        last.OnFinished = () =>
        {
            Controller.GameData.Player.SkillList.Medicine.AddExp(100); // 医术 +100 经验
            AttributeManager.ModifyMaxHealth(30);                      // 血量上限 +30
        };
        DisplayControl.HaveDialog(slots[0]);
    }

    public override string getImageName() => "药堂";

    public override float getWeight()
    {
        // 医学系列前置执行次数 >1 时开放
        if (EventTracker.GetExecutionCount("MedicalSkill") > 1) return 1;
        return 0;
    }
}
```

要点：解锁式教学事件，提升**医术经验**与**血量上限**；有前置执行次数限制。

---

### CollectMedicinalMaterials

```csharp
// === 事件：药物采集（CollectMedicinalMaterials）===
// 类型：采集 / 随机掉落 / 技能经验

[System.Serializable]
public class CollectMedicinalMaterials : Event
{
    public CollectMedicinalMaterials()
    {
        Quality   = Quality.Uncommon;
        Uncounted = true;                      // 不计入出现次数统计
    }

    public override string getName()        => "药物采集";
    public override string getDescription() => "宗门呼吁具备医学知识的武者搜集珍贵药物，以备不时之需。";

    public override void execute()
    {
        Random rng = new Random();
        // 运气 = 随机 + 医术贡献
        float luckFactor = (float)(rng.NextDouble() * 0.5 + 0.05 * Controller.GameData.Player.SkillList.Medicine.getLevel());

        string input =
              luckFactor < 0.5f ? "【文本】今天的运气不太好，你只找到了少量药材。【标题】药草采集"
            : luckFactor < 0.8f ? "【文本】你今天收获颇丰。【标题】药草采集"
            : "【文本】你今天收获满满。【标题】药草采集";

        var slots = SlotParser.ParseSlots(input);
        var last  = slots[slots.Count - 1];
        last.OnFinished = () =>
        {
            // 结算随机掉落（赤阳草 / 朱果 / 龙血草）
            int itemsCount = rng.Next(1, 4);
            for (int i = 0; i < itemsCount; i++)
            {
                float itemLuck = (float)(rng.NextDouble() * luckFactor);
                if      (itemLuck < 0.5f) ItemManager.AddItem("赤阳草", 1);
                else if (itemLuck < 0.8f) ItemManager.AddItem("朱果", 1);
                else                      ItemManager.AddItem("龙血草", 1);
            }
            Controller.GameData.Player.SkillList.Medicine.AddExp(50);  // 医术经验
        };
        DisplayControl.HaveDialog(slots[0]);
    }

    public override string getImageName() => "采药";

    public override float getWeight()
    {
        // 早期（<60）且医术>3 才能入池
        if (Controller.GameData.Time < 60 && Controller.GameData.Player.SkillList.Medicine.getLevel() > 3) return 1;
        return 0;
    }
}
```

要点：**不计次数**的采集事件；根据“医术等级 + 随机”决定掉落丰富度，并提升医术经验。

---

### JiangJiaoYinZhuDuanTianLeiEvent

```csharp
// === 事件：江蛟印主段天雷（JiangJiaoYinZhuDuanTianLeiEvent）===
// 类型：Boss 战（FightEvent）/ 奖励计略 / 收尾剧情

[System.Serializable]
public class JiangJiaoYinZhuDuanTianLeiEvent : FightEvent
{
    public override string getName()
    {
        Quality  = Quality.Epic;
        TimeLimit = 1;
        return "江蛟印主段天雷★★★★";
    }

    public JiangJiaoYinZhuDuanTianLeiEvent()
    {
        TimeLimit = 1;
    }

    public override string getDescription()
    {
        return "手执前朝治水官印，自称江上巡水使，三十六道铁索闸，皆听他一令升落。";
    }

    public override string getImageName() => "江蛟印主段天雷";
    public override float getWeight() => 1;

    public override void onLoad()
    {
        base.onLoad();
        Character character = Controller.FightController.GetCharacter(3800);
        character.PersonalData.FamilyName = "段";
        character.PersonalData.SecondName = "天雷";
        character.PersonalData.Gender     = 1;

        // 战斗技能
        character.CombatSkills = new List<int>
        {
            SkillBuilder.GetSkillIndexByName("天罡破"),
            SkillBuilder.GetSkillIndexByName("霸拳裂甲"),
            SkillBuilder.GetSkillIndexByName("持盾猛击"),
            SkillBuilder.GetSkillIndexByName("强袭"),
            SkillBuilder.GetSkillIndexByName("金钟罩")
        };

        // 天赋（被动）
        TalentManager.EquipAndAddTalent(new TalentFromEffect(new CuoRui()), character.TalentPart);

        EnermyList = new List<Character> { character };
    }

    public void win()
    {
        // 胜利奖励与收尾剧情
        DisplayControl.TriggerStrategyAward(Quality.Rare);
        BossDefeat_DuanTianLei();
    }

    public override void execute()
    {
        DisplayControl.HaveFight(EnermyList, win, null, "蛟锁大江");
        DisplayControl.ShowFight();
        AttributeManager.ModifyLootMoney(200); // 增加铜钱
        AttributeManager.AddItemToLoot("青阳草", 0.5f);
        AttributeManager.AddItemToLoot("壮骨丹", 0.5f);
    }
    public static void BossDefeat_DuanTianLei()
    {
        string input = "【文本】你将他逼入死局，三面水闸尽数封锁。退路已断，段天雷却神情肃穆，目光掠过你，缓缓抬起那枚早已锈蚀的官印。“为国守江三十载，今日……仍不负圣恩。”【文本】话音落下，江底轰鸣骤起——三十六道铁索闸下早已暗藏火药，此刻尽数引爆。水柱冲天，铁索横飞，他的身影随巨响湮没。曾经的巡水使，就此葬身于他誓死守护的江流之上。";
        List<Slot> slots = SlotParser.ParseSlots(input);
        DisplayControl.HaveDialog(slots[0]);
    }
}
```

## 常见坑位与建议
- **对话流程**：一定把 `slots[0]` 传给 `DisplayControl.HaveDialog(slots[0])`，否则不会显示对话。
- **权重调试**：想立刻测试可临时 `ForcedDisplay = true（强制展示）` 或直接返回较大的权重。
- **连锁事件**：使用 `EventTracker.GetExecutionCount("前置类名")` 判定前置是否已发生。

