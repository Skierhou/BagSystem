using System;
using System.Collections.Generic;

namespace SkierFramework
{
    public enum BagType
    {
        None,
        Default,    //默认背包(将消耗品,材料等归于一个，如果游戏中是分开显示，那么背包中也分开)
        Hero,       //英雄
        Skill,      //技能
        Equip,      //装备
        Soldier,    //小兵
        Pet,        //宠物
        Currency,   //货币
    }
}
