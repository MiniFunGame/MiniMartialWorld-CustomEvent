
using System;
using System.Collections.Generic;
using System.Linq;

public class MartialArtsMuralDiscovery2 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;               // 稀有事件
        return "古遗迹";
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

[System.Serializable]
public class ObserveMural_2 : Event
{
    public override string getName()
    {
        Quality = Quality.Rare;
        return "参悟古壁";
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
          "【文本】你凝神观想壁画，画中人物的身影仿佛活了一般"
        + "【文本】那是一门以气破石的刚猛掌法，每一击都似要震碎山岳。"
        + "【文本】（你对刚猛武学的理解加深了）";

        var slots = SlotParser.ParseSlots(input);
        var last = slots[slots.Count - 1];
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
