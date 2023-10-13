using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    [System.Serializable]
    public class BagConfig : IConfig
    {
        /// <summary>
        /// 默认背包容量
        /// </summary>
        public const int DEFAULT_BAG_CAPACITY = 1000;

        public int ID => (int)bagType;

        [InspectorName("背包类型")]
        public BagType bagType;

        [InspectorName("初始大小")]
        public int initSize = DEFAULT_BAG_CAPACITY;

        // 背包扩容等配置
    }
}
