using System;
using System.Collections.Generic;

namespace SkierFramework
{
    public class Item
    {
        /// <summary>
        /// 道具数据
        /// </summary>
        public ItemData itemData;
        /// <summary>
        /// 所处背包
        /// </summary>
        public Bag OwnerBag;
        /// <summary>
        /// 道具配置
        /// </summary>
        public ItemConfig ItemConfig;
        /// <summary>
        /// 背包逻辑类
        /// </summary>
        public Dictionary<BagType, Bag> itemBags;
        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => ItemConfig.overlayCount == itemData.count;
        public ulong id => itemData.id;
        public int count => itemData.count;
        public int metaId => itemData.metaId;
        public Dictionary<ItemKey, long> keyValues => itemData.keyValues;
        public Dictionary<ItemKey, string> keyValueStrs => itemData.keyValueStrs;

        public Item(ItemData itemData, Bag bagLogic)
        {
            this.itemData = itemData;
            this.OwnerBag = bagLogic;
            if (itemData.metaId > 0)
            {
                ItemConfig = BagSystem.Instance.GetConfig<ItemConfig>(itemData.metaId);
            }
            if (itemData.itemBags != null)
            {
                foreach (var item in itemData.itemBags)
                {
                    GetOrAddLogic(item.Key);
                }
            }
        }

        #region 数值属性
        /// <summary>
        /// 获取属性
        /// </summary>
        public long Get(ItemKey itemKey, long defaultValue = 0)
        {
            if (itemData.keyValues != null)
            {
                itemData.keyValues.TryGetValue(itemKey, out long value);
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        public void Set(ItemKey itemKey, long value)
        {
            if (itemData.keyValues == null)
            {
                itemData.keyValues = new Dictionary<ItemKey, long>();
            }
            itemData.keyValues[itemKey] = value;
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
            if (itemData.keyValueStrs != null)
            {
                itemData.keyValueStrs.TryGetValue(itemKey, out string value);
                return value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        public void SetStr(ItemKey itemKey, string value)
        {
            if (itemData.keyValueStrs == null)
            {
                itemData.keyValueStrs = new Dictionary<ItemKey, string>();
            }
            itemData.keyValueStrs[itemKey] = value;
        }
        #endregion

        #region 背包
        /// <summary>
        /// 加数量
        /// </summary>
        /// <param name="amount">加几个</param>
        /// <returns>返回实际变化了几个</returns>
        public int AddAmount(int amount)
        {
            int changeCount;
            int total = itemData.count + amount;
            if (total > ItemConfig.overlayCount)
            {
                changeCount = ItemConfig.overlayCount - itemData.count;
                itemData.count = ItemConfig.overlayCount;
            }
            else if (total <= 0)
            {
                changeCount = itemData.count;
                itemData.count = 0;
            }
            else
            {
                itemData.count += amount;
                changeCount = amount;
            }
            return UnityEngine.Mathf.Abs(changeCount);
        }

        /// <summary>
        /// 新增背包
        /// </summary>
        public Bag GetOrAddBag(BagType bagType)
        {
            if (itemData.itemBags == null)
            {
                itemData.itemBags = new Dictionary<BagType, BagData>();
            }
            if (!itemData.itemBags.TryGetValue(bagType, out BagData bag))
            {
                var bagConfig = BagSystem.Instance.GetConfig<BagConfig>((int)bagType);
                int size = bagConfig != null ? bagConfig.initSize : BagConfig.DEFAULT_BAG_CAPACITY;
                bag = new BagData(bagType, size);
                itemData.itemBags.Add(bagType, bag);
            }
            return GetOrAddLogic(bagType);
        }

        /// <summary>
        /// 获得背包
        /// </summary>
        public Bag GetBag(BagType bagType)
        {
            if (itemData.itemBags == null) return null;
            if (!itemData.itemBags.TryGetValue(bagType, out var bag))
            {
                return null;
            }
            return GetOrAddLogic(bagType);
        }

        private Bag GetOrAddLogic(BagType bagType)
        {
            if (itemBags == null)
            {
                itemBags = new Dictionary<BagType, Bag>();
            }
            if (!itemBags.TryGetValue(bagType, out var logic))
            {
                itemData.itemBags.TryGetValue(bagType, out var bag);
                logic = new Bag(bag);
                itemBags.Add(bagType, logic);
            }
            return logic;
        }
        #endregion

        #region 系统
        
        #endregion

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            ItemConfig = null;
        }
    }
}
