using System;
using System.Collections.Generic;

namespace SkierFramework
{
    [System.Serializable]
    public class BagData
    {
        /// <summary>
        /// 背包类型
        /// </summary>
        public BagType bagType;
        /// <summary>
        /// 道具
        /// </summary>
        public Dictionary<ulong, ItemData> allItems;
        /// <summary>
        /// 多套穿戴
        /// </summary>
        public Dictionary<int, Dictionary<int, ulong>> wears;
        /// <summary>
        /// 判断这个id是否有穿戴
        /// </summary>
        public Dictionary<ulong, int> wearIdRefs;
        /// <summary>
        /// 当时使用的穿戴Id
        /// </summary>
        public int useWearId;
        /// <summary>
        /// 背包容量
        /// </summary>
        public int bagSize;

        /// <summary>
        /// 初始化背包
        /// </summary>
        public BagData(BagType bagType, int size)
        {
            allItems = new Dictionary<ulong, ItemData>();
            wears = new Dictionary<int, Dictionary<int, ulong>>();
            wearIdRefs = new Dictionary<ulong, int>();
            this.bagType = bagType;
            this.useWearId = 0;
            this.bagSize = size;
        }
    }

}
