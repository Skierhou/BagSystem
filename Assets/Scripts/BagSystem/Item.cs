using System;
using System.Collections.Generic;

namespace SkierFramework
{
    [System.Serializable]
    public class Item
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
        /// 字符串数据:为了名称等一些字符串的属性，其实除了名称很少有字符串数据，可以把name这个字段拿出来
        /// </summary>
        public Dictionary<ItemKey, string> keyValueStrs;
        /// <summary>
        /// 该道具身上的背包
        /// </summary>
        public Dictionary<BagType, Bag> itemBags;

        /// <summary>
        /// 所处背包
        /// </summary>
        [System.NonSerialized]
        [Newtonsoft.Json.JsonIgnore]
        public Bag bag;
        /// <summary>
        /// 道具配置
        /// </summary>
        [System.NonSerialized]
        [Newtonsoft.Json.JsonIgnore]
        private ItemConfig itemConfig;

        [Newtonsoft.Json.JsonIgnore]
        public ItemConfig ItemConfig
        {
            get
            {
                if (itemConfig == null)
                {
                    itemConfig = BagSystem.Instance.GetConfig<ItemConfig>(metaId);
                }
                return itemConfig;
            }
        }
        /// <summary>
        /// 是否已满
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsFull => ItemConfig.overlayCount == count;

        /// <summary>
        /// 加数量
        /// </summary>
        /// <param name="amount">加几个</param>
        /// <returns>返回实际变化了几个</returns>
        public int AddAmount(int amount)
        {
            int changeCount;
            int total = count + amount;
            if (total > ItemConfig.overlayCount)
            {
                changeCount = ItemConfig.overlayCount - count;
                count = ItemConfig.overlayCount;
            }
            else if (total <= 0)
            {
                changeCount = count;
                count = 0;
            }
            else
            {
                count += amount;
                changeCount = amount;
            }
            return UnityEngine.Mathf.Abs(changeCount);
        }

        #region 数值属性
        /// <summary>
        /// 获取属性
        /// </summary>
        public long Get(ItemKey itemKey, long defaultValue = 0)
        {
            if (keyValues != null)
            {
                keyValues.TryGetValue(itemKey, out long value);
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        public void Set(ItemKey itemKey, long value)
        {
            if (keyValues == null)
            {
                keyValues = new Dictionary<ItemKey, long>();
            }
            keyValues[itemKey] = value;
        }

        /// <summary>
        /// 修改属性
        /// </summary>
        public void Add(ItemKey itemKey, long add)
        {
            Set(itemKey, Get(itemKey) + add);
        }
        #endregion

        #region 字符串属性
        /// <summary>
        /// 获取属性
        /// </summary>
        public string GetStr(ItemKey itemKey)
        {
            if (keyValueStrs != null)
            {
                keyValueStrs.TryGetValue(itemKey, out string value);
                return value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        public void SetStr(ItemKey itemKey, string value)
        {
            if (keyValueStrs == null)
            {
                keyValueStrs = new Dictionary<ItemKey, string>();
            }
            keyValueStrs[itemKey] = value;
        }
        #endregion

        #region 背包
        /// <summary>
        /// 新增背包
        /// </summary>
        public Bag AddBag(BagType bagType, Comparer<Item> comparer = null)
        {
            if (itemBags == null)
            {
                itemBags = new Dictionary<BagType, Bag>();
            }

            if (!itemBags.TryGetValue(bagType, out Bag bag))
            {
                bag = new Bag();
                var bagConfig = BagSystem.Instance.GetConfig<BagConfig>((int)bagType);
                int size = bagConfig != null ? bagConfig.initSize : BagSystem.Instance.DEFAULT_BAG_CAPACITY;
                bag.InitBag(bagType, size, comparer);
                itemBags.Add(bagType, bag);
            }
            return bag;
        }

        /// <summary>
        /// 获得背包
        /// </summary>
        public Bag GetBag(BagType bagType)
        {
            if (itemBags == null) return null;
            if (itemBags.TryGetValue(bagType, out var bag))
            {
                return bag;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            id = 0;
            metaId = 0;
            count = 0;
            itemConfig = null;
            bag = null;
            if (keyValues != null)
            {
                keyValues.Clear();
            }
            ObjectPool<Item>.Release(this);
        }
    }

}
