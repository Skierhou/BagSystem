using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    /// <summary>
    /// 道具品质
    /// </summary>
    public enum ItemQualityType
    {
        /// <summary>
        /// 灰色(粗糙)
        /// </summary>
        [InspectorName("灰色(粗糙)")]
        Rouch = 0,
        /// <summary>
        /// 白色(普通)
        /// </summary>
        [InspectorName("白色(普通)")]
        Normal = 1,
        /// <summary>
        /// 绿色(优秀)
        /// </summary>
        [InspectorName("绿色(优秀)")]
        Excellent = 2,
        /// <summary>
        /// 蓝色(精良)
        /// </summary>
        [InspectorName("蓝色(精良)")]
        Superior = 3,
        /// <summary>
        /// 紫色(史诗)
        /// </summary>
        [InspectorName("紫色(史诗)")]
        Epic = 4,
        /// <summary>
        /// 橙色(传说)
        /// </summary>
        [InspectorName("橙色(传说)")]
        Legend = 5,
    }

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

    [System.Serializable]
    public struct OneItem
    {
        [InspectorName("道具")]
        public int id;
        [InspectorName("数量")]
        public int num;

        public OneItem(int id, int num)
        {
            this.id = id;
            this.num = num;
        }
    }

    [System.Serializable]
    public class ItemConfig : IConfig
    {
        public int id;
        [InspectorName("名称")]
        public string name;
        [InspectorName("描述")]
        public string des;
        [InspectorName("图标")]
        public string icon;
        [InspectorName("掉落显示")]
        public string prefabPath;       //预制体路径,掉落显示
        [InspectorName("最大可叠加多少个")]
        public int overlayCount = 1;    //最大可叠加多少个
        [InspectorName("背包类型")]
        public BagType bagType;         //背包类型
        [InspectorName("道具类型")]
        public ItemType itemType;      //道具类型
        [InspectorName("道具品质")]
        public ItemQualityType qualityType;//道具品质
        [InspectorName("是否唯一")]
        public bool isUnique;                   //是否唯一
        [InspectorName("重复后转化的道具")]
        public List<OneItem> uniqueReward;      //唯一重复后的转化道具

        public int ID => id;
    }

}
