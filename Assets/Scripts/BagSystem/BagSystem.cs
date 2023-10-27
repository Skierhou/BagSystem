﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    /// <summary>
    /// 在无多角色的游戏中，这个就可以认为是整个游戏信息
    /// </summary>
    public class BagSystem
    {
        /// <summary>
        /// 在选角完成后才赋值，退出账号后删除，相当于是当前角色
        /// </summary>
        public static BagSystem Instance { get; set; }
        /// <summary>
        /// 当前角色：当前角色背包，以及当前角色数据全部在这里
        /// </summary>
        private Item _roleItem;
        /// <summary>
        /// 背包排序规则:(不同背包排序规则不同，在这里直接设置)
        /// </summary>
        private Dictionary<BagType, Comparer<Item>> _bagComparers = new Dictionary<BagType, Comparer<Item>> { };
        /// <summary>
        /// 默认背包排序
        /// </summary>
        private Comparer<Item> _defaultCompare = Comparer<Item>.Create(BagCompare);
        /// <summary>
        /// 背包穿戴变更
        /// </summary>
        public Action<BagType, int> OnBagWearChange;
        /// <summary>
        /// 背包变更
        /// </summary>
        public Action<BagType> OnBagChange;
        /// <summary>
        /// 道具变更
        /// </summary>
        public Action<Item> OnItemChange;
        /// <summary>
        /// 当前角色
        /// </summary>
        public Item RoleItem => _roleItem;

        /// <summary>
        /// 初始化角色数据，这个数据可能是读存档
        /// </summary>
        public BagSystem InitRole(ItemData entityData)
        {
            _roleItem = new Item(entityData, null);
            if (_roleItem.itemBags == null)
            {
                _roleItem.itemBags = new Dictionary<BagType, Bag>();
            }
            foreach (var bag in _roleItem.itemBags.Values)
            {
                InitBag(bag.BagType);
            }
            return this;
        }

        /// <summary>
        /// 获取或新增背包
        /// </summary>
        public Bag GetOrAddBag(BagType bagType)
        {
            var bag = _roleItem.GetBag(bagType);
            if (bag == null)
            {
                bag = InitBag(bagType);
            }
            return bag;
        }

        /// <summary>
        /// 初始化背包
        /// </summary>
        public Bag InitBag(BagType bagType)
        {
            _bagComparers.TryGetValue(bagType, out var comparer);
            var bag = _roleItem.GetOrAddBag(bagType);
            bag.SetSortComparer(comparer ?? _defaultCompare);
            bag.OnBagChange += (bagType) => { OnBagChange?.Invoke(bagType); };
            bag.OnBagWearChange += (bagType, wearId) => { OnBagWearChange?.Invoke(bagType, wearId); };
            bag.OnItemChange += (item) => { OnItemChange?.Invoke(item); };
            return bag;
        }

        /// <summary>
        /// 默认排序
        /// </summary>
        private static int BagCompare(Item a, Item b)
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

        /// <summary>
        /// 获得道具
        /// </summary>
        public void AcquireItem(int id, int count)
        {
            var itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(id);
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
            var itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(metaId);
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

        public bool IsCanCost(OneItem oneItem, bool isToast = true)
        {
            if (GetCount(oneItem.id) >= oneItem.num)
            {
                return true;
            }
            else if (isToast)
            {
                Debug.Log(string.Format("{0}不足", oneItem.id));
            }
            return false;
        }
    }
}
