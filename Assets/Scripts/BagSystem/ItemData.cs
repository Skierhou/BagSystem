using System;
using System.Collections.Generic;

namespace SkierFramework
{
    [System.Serializable]
    public class ItemData
    {
        /// <summary>
        /// 道具的唯一Id
        /// </summary>
        public ulong id;
        /// <summary>
        /// 道具的配置Id
        /// </summary>
        public int metaId;
        /// <summary>
        /// 道具堆叠数量
        /// </summary>
        public int count;
        /// <summary>
        /// 槽位
        /// </summary>
        public int slot;
        /// <summary>
        /// Kv数据:所有道具身上的属性
        /// </summary>
        public Dictionary<ItemKey, long> keyValues;
        /// <summary>
        /// 字符串数据:为了名称等一些字符串的属性，以及一些玩法数据
        /// 玩法数据的用法举个例子：装备道具，存在随机词条等无法使用一个long来解决的属性
        /// </summary>
        public Dictionary<ItemKey, string> keyValueStrs;
        /// <summary>
        /// 该道具身上的背包
        /// </summary>
        public Dictionary<BagType, BagData> itemBags;
    }
}
