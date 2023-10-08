using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    /// <summary>
    /// 把这个BagSystem当成当前角色
    /// </summary>
    public class BagSystem : Singleton<BagSystem>
    {
        /// <summary>
        /// 默认背包容量
        /// </summary>
        public int DEFAULT_BAG_CAPACITY = 50;

        /// <summary>
        /// 当前角色：当前角色背包，以及当前角色数据全部在这里
        /// </summary>
        private Item _roleItem = new Item();
        /// <summary>
        /// 默认排序
        /// </summary>
        private Comparer<Item> _defaultCompare = Comparer<Item>.Create(DefaultBagCompare);
        /// <summary>
        /// 背包排序规则:(不同背包排序规则不同，在这里直接设置)
        /// </summary>
        private Dictionary<BagType, Comparer<Item>> _bagComparers = new Dictionary<BagType, Comparer<Item>> { };
        /// <summary>
        /// 背包穿戴变更
        /// </summary>
        public Action<BagType, int> OnBag_WearChange;
        /// <summary>
        /// 背包变更
        /// </summary>
        public Action<BagType> OnBag_Change;
        /// <summary>
        /// 当前角色
        /// </summary>
        public Item RoleItem => _roleItem;

        /// <summary>
        /// 获取配置，需自行接入配置系统
        /// </summary>
        public T GetConfig<T>(int id) where T : IConfig
        {
            return Demo.BagDemo.GetConfig<T>(id);
        }

        /// <summary>
        /// 获取或新增背包
        /// </summary>
        public Bag GetOrAddBag(BagType bagType)
        {
            var bag = _roleItem.GetBag(bagType);
            if (bag == null)
            {
                _bagComparers.TryGetValue(bagType, out var comparer);
                bag = _roleItem.AddBag(bagType, comparer ?? _defaultCompare);
                bag.OnBag_Change += (bagType) => { OnBag_Change?.Invoke(bagType); };
                bag.OnBag_WearChange += (bagType, wearId) => { OnBag_WearChange?.Invoke(bagType, wearId); };
            }
            return bag;
        }

        /// <summary>
        /// 获得道具
        /// </summary>
        public void AcquireItem(int id, int count)
        {
            var itemConfig = GetConfig<ItemConfig>(id);
            if (itemConfig == null)
            {
                Debug.LogError("找不到配置：" + id);
                return;
            }

            var bag = GetOrAddBag(itemConfig.bagType);

            if (count > 0)
            {
                // 如果是唯一的话 需要转换成其他道具
                if (itemConfig.isUnique && bag.GetItemByMetaId(id) != null)
                {
                    // 道具转化
                    if (itemConfig.uniqueReward != null)
                    {
                        foreach (var item in itemConfig.uniqueReward)
                        {
                            AcquireItem(item.id, item.num * count);
                        }
                    }
                }
                else
                {
                    bag.AcquireItem(id, count);
                }
            }
            else
            {
                bag.DelItem(id, Math.Abs(count));
            }
        }

        /// <summary>
        /// 删除道具
        /// </summary>
        public void DelItem(int id, int count)
        {
            AcquireItem(id, -Math.Abs(count));
        }

        /// <summary>
        /// 道具数量
        /// </summary>
        public long GetCount(int metaId)
        {
            var itemConfig = GetConfig<ItemConfig>(metaId);
            if (itemConfig != null)
            {
                long result = 0;
                var bag = _roleItem.GetBag(itemConfig.bagType);
                if (bag != null)
                {
                    foreach (var item in bag.AllItems.Values)
                    {
                        if (item.metaId == metaId)
                        {
                            result += item.count;
                        }
                    }
                }
                return result;
            }
            return 0;
        }

        /// <summary>
        /// 通过Id获取道具
        /// </summary>
        public Item GetItem(ulong id)
        {
            if (_roleItem.itemBags != null)
            {
                foreach (var bag in _roleItem.itemBags.Values)
                {
                    var item = bag.GetItemById(id);
                    if (item != null)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 默认的排序方式
        /// </summary>
        private static int DefaultBagCompare(Item a, Item b)
        {
            if (a == b)
            {
                return 0;
            }
            else if (a == null)
            {
                return -1;
            }
            else if (b == null)
            {
                return 1;
            }
            if (a.ItemConfig.qualityType == b.ItemConfig.qualityType)
            {
                if (a.metaId == b.metaId)
                {
                    return a.id.CompareTo(b.id);
                }
                else
                {
                    return a.metaId.CompareTo(b.metaId);
                }
            }
            else
            {
                return b.ItemConfig.qualityType.CompareTo(a.ItemConfig.qualityType);
            }
        }

    }
}
