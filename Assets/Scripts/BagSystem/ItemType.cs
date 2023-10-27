using System;
using System.Collections.Generic;

namespace SkierFramework
{
    /// <summary>
    /// 道具子类型
    /// </summary>
    public enum ItemType
    {
        None = 0,

        // 货币类----------------------------
        Gold = 100,         //金币
        Diamond = 101,      //钻石

        // 装备类----------------------------
        Equipment_Begin = 200,
        Sword = 201,
        Bow = 202,
        Shied = 203,

        // 弓箭类----------------------------
        Arraw_Default = 400,
        Arraw_Fire = 401,
    }

}
