﻿using System;
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

        /// <summary>
        /// 克隆
        /// </summary>
        public ItemData Clone()
        {
            ItemData clone = ObjectPool<ItemData>.Get();
            clone.id = id;
            clone.metaId = metaId;
            clone.count = count;
            if (keyValues != null)
            {
                clone.keyValues = new Dictionary<ItemKey, long>();
                foreach (var item in keyValues)
                {
                    clone.keyValues.Add(item.Key, item.Value);
                }
            }
            if (keyValueStrs != null)
            {
                clone.keyValueStrs = new Dictionary<ItemKey, string>();
                foreach (var item in keyValueStrs)
                {
                    clone.keyValueStrs.Add(item.Key, item.Value);
                }
            }
            if (itemBags != null)
            {
                clone.itemBags = new Dictionary<BagType, BagData>();
                foreach (var item in itemBags)
                {
                    clone.itemBags.Add(item.Key, item.Value);
                }
            }
            return clone;
        }
    }

}