using System;
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
        /// 背包穿戴变更
        /// </summary>
        public Action<BagType, int> OnBagWearChange;
        /// <summary>
        /// 背包变更
        /// </summary>
        public Action<BagType> OnBagChange;
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
        /// 初始化角色数据，这个数据可能是读存档
        /// </summary>
        public BagSystem InitRole(ItemData entityData)
        {
            _roleItem = new Item(entityData, null);
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
                _bagComparers.TryGetValue(bagType, out var comparer);
                bag = _roleItem.GetOrAddBag(bagType);
                bag.OnBagChange += (bagType) => { OnBagChange?.Invoke(bagType); };
                bag.OnBagWearChange += (bagType, wearId) => { OnBagWearChange?.Invoke(bagType, wearId); };
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
