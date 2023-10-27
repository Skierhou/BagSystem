using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
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
