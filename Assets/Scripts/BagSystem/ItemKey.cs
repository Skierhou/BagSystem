using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    public enum ItemKey
    {
        Quality = 1, //品质
        Level = 2,   //等级
        Exp = 3,     //经验

        HP = 100,    //血量
        Defense,     //防御
        Attack,      //攻击力
        // ... 等等， 如血量，防御等词条性质的其实不能简单放在这里，因为存在百分比和直接加成等，当然也可以直接用两个Key分别表示百分比值和加成值。
    }
}
