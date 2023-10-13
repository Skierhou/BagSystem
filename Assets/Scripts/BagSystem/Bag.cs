using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    public class Bag
    {
        /// <summary>
        /// 背包数据
        /// </summary>
        private BagData _bag;
        /// <summary>
        /// 道具逻辑类
        /// </summary>
        private Dictionary<ulong, Item> _allItems;
        /// <summary>
        /// 带槽位的道具
        /// </summary>
        private List<Item> _itemSlots;
        /// <summary>
        /// 排序
        /// </summary>
        private IComparer<Item> _comparer;
        /// <summary>
        /// 排序后的道具缓存
        /// </summary>
        private List<Item> _sortItemsNonEmpty;

        /// <summary>
        /// 背包类型
        /// </summary>
        public BagType BagType => _bag.bagType;
        /// <summary>
        /// 所有槽位
        /// </summary>
        public List<Item> Slots => _itemSlots;
        /// <summary>
        /// 所有道具
        /// </summary>
        public Dictionary<ulong, Item> AllItems => _allItems;
        /// <summary>
        /// 穿戴的背包，支持一种背包多套穿戴组合，可一键替换等功能
        /// </summary>
        public Dictionary<int, Dictionary<int, ulong>> Wears => _bag.wears;
        /// <summary>
        /// 当前使用的穿戴Id
        /// </summary>
        public int UseWearId => _bag.useWearId;
        /// <summary>
        /// 穿戴变化:背包类型，穿戴id
        /// </summary>
        public Action<BagType, int> OnBagWearChange;
        /// <summary>
        /// 背包变化:背包类型
        /// </summary>
        public Action<BagType> OnBagChange;

        /// <summary>
        /// 构造
        /// </summary>
        public Bag(BagData bag)
        {
            _bag = bag;
            _allItems = new Dictionary<ulong, Item>();
            _itemSlots = new List<Item>();
            for (int i = 0; i < bag.bagSize; i++)
            {
                _itemSlots.Add(null);
            }
            foreach (var item in bag.allItems)
            {
                var logic = new Item(item.Value, this);
                _itemSlots[item.Value.slot] = logic;
                _allItems.Add(item.Key, logic);
            }
        }

        /// <summary>
        /// 设置排序规则
        /// </summary>
        public void SetSortComparer(Comparer<Item> comparer)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// 变化大小
        /// </summary>
        public void ChangeSize(int size)
        {
            _bag.bagSize = size;
            while (_itemSlots.Count < size)
            {
                _itemSlots.Add(null);
            }

            // 通常不会存在背包缩水
            while (_itemSlots.Count > size)
            {
                if (_itemSlots[_itemSlots.Count - 1] != null)
                {
                    // 说明这个丢了，不过一般不允许缩小空间只有增加
                }
                _itemSlots.RemoveAt(_itemSlots.Count - 1);
            }
        }

        /// <summary>
        /// 移动道具
        /// </summary>
        public bool MoveItem(Item item, int moveToSlot)
        {
            if (item == null || moveToSlot < 0 || _bag.bagSize <= moveToSlot || _itemSlots[item.itemData.slot] != item)
            {
                return false;
            }
            var endItem = _itemSlots[moveToSlot];
            if (endItem != null)
            {
                endItem.itemData.slot = item.itemData.slot;
                _itemSlots[item.itemData.slot] = endItem;
            }
            _itemSlots[moveToSlot] = item;
            item.itemData.slot = moveToSlot;
            return true;
        }

        /// <summary>
        /// 获得空格子或者相同的格子
        /// </summary>
        public bool GetEmptyOrSameSlot(int metaId, out int index)
        {
            index = -1;
            for (int i = 0; i < _itemSlots.Count; i++)
            {
                ulong invId = _itemSlots[i] == null ? 0 : _itemSlots[i].id;
                if ((invId == 0 || !_bag.allItems.ContainsKey(invId)) && index < 0)
                {
                    index = i;
                }
                else if (invId != 0
                    && _bag.allItems.TryGetValue(invId, out ItemData item)
                    && item.metaId == metaId && !GetItemById(item.id).IsFull)
                {
                    index = i;
                    return true;
                }
            }
            return index >= 0;
        }

        /// <summary>
        /// 获得道具
        /// </summary>
        public int AcquireItem(int inMetaId, int inAmount = 1, ulong guid = 0)
        {
            int cache = inAmount;
            while (inAmount > 0 && GetEmptyOrSameSlot(inMetaId, out int index))
            {
                // 新道具
                ItemData item = null;
                if (_itemSlots[index] == null || !_bag.allItems.TryGetValue(_itemSlots[index].id, out item))
                {
                    guid = guid == 0 ? GenerateItemGuid() : guid;
                    item = ObjectPool<ItemData>.Get();
                    item.metaId = inMetaId;
                    item.id = guid;
                    item.count = 0;
                    _bag.allItems.Add(guid, item);
                    item.slot = index;
                    _itemSlots[index] = GetItemById(item.id);
                }
                inAmount -= GetItemById(item.id).AddAmount(inAmount);
            }

            if (inAmount > 0)
            {
                // 说明背包满了
                Debug.Log("背包塞满了！！！");
            }

            if (inAmount != cache)
            {
                OnBagChange?.Invoke(BagType);
            }

            //返回实际操作几个
            return cache - inAmount;
        }

        /// <summary>
        /// 获得道具
        /// </summary>
        public void AcquireItem(ItemData item, bool isNotify = false)
        {
            while (item != null && item.count > 0 && GetEmptyOrSameSlot(item.metaId, out int index))
            {
                if (_itemSlots[index] == null)
                {
                    _bag.allItems.Add(item.id, item);
                    item.slot = index;
                    _itemSlots[index] = GetItemById(item.id);
                    item = null;
                }
                else
                {
                    item.count -= _itemSlots[index].AddAmount(item.count);
                }
            }
            if (isNotify)
            {
                OnBagChange?.Invoke(BagType);
            }
        }

        /// <summary>
        /// 删除道具
        /// </summary>
        public int DelItem(ulong id, int count)
        {
            var item = GetItemById(id);
            int changeCount = 0;
            if (item != null)
            {
                changeCount = item.AddAmount(-count);
                if (item.count <= 0)
                {
                    for (int i = 0; i < _itemSlots.Count; i++)
                    {
                        if (_itemSlots[i] != null && _itemSlots[i].id == id)
                        {
                            _itemSlots[i] = null;
                            break;
                        }
                    }
                    _bag.allItems.Remove(id);
                    if (_bag.wearIdRefs.TryGetValue(id, out int wearedId))
                    {
                        foreach (var wearPair in Wears)
                        {
                            if (((1 << wearPair.Key) & wearedId) != 0)
                            {
                                foreach (var pair in wearPair.Value)
                                {
                                    if (pair.Value == id)
                                    {
                                        OnBagWearChange?.Invoke(BagType, pair.Key);
                                        wearPair.Value.Remove(pair.Key);
                                        break;
                                    }
                                }
                            }
                        }
                        _bag.wearIdRefs.Remove(id);
                    }
                    _allItems.Remove(item.id);
                }
                OnBagChange?.Invoke(BagType);
            }
            return changeCount;
        }
        /// <summary>
        /// 删除道具
        /// </summary>
        public void DelItem(int metaId, int count)
        {
            var removes = ListPool<ulong>.Get();
            foreach (var item in _bag.allItems.Values)
            {
                if (item.metaId == metaId)
                {
                    count -= GetItemById(item.id).AddAmount(-count);

                    if (item.count <= 0)
                    {
                        removes.Add(item.id);
                    }

                    if (count <= 0)
                    {
                        break;
                    }
                }
            }
            foreach (var id in removes)
            {
                DelItem(id, 0);
            }
            ListPool<ulong>.Release(removes);
        }

        /// <summary>
        /// 穿戴
        /// </summary>
        /// <param name="slot">槽位</param>
        /// <param name="id">itemId</param>
        /// <param name="wearId">穿戴id</param>
        public void Wear(int slot, ulong id, int wearId = -1)
        {
            if (wearId < 0)
            {
                wearId = _bag.useWearId;
            }

            if (!_bag.wears.TryGetValue(wearId, out var wear))
            {
                wear = new Dictionary<int, ulong>();
                _bag.wears.Add(wearId, wear);
            }
            if (wear.ContainsKey(slot))
            {
                UnWear(slot, wearId, false);
            }
            wear.Add(slot, id);
            _bag.wearIdRefs.TryGetValue(id, out int wearedSlot);
            _bag.wearIdRefs[id] = wearedSlot | (1 << wearId);
            OnBagWearChange?.Invoke(BagType, wearId);
        }

        /// <summary>
        /// 获得穿戴
        /// </summary>
        public Dictionary<int, ulong> GetWear(int wearId = -1)
        {
            if (wearId < 0)
            {
                wearId = _bag.useWearId;
            }
            if (!_bag.wears.TryGetValue(wearId, out var wear))
            {
                wear = new Dictionary<int, ulong>();
                _bag.wears.Add(wearId, wear);
            }
            return wear;
        }

        /// <summary>
        /// 是否穿戴
        /// </summary>
        public bool IsWear(ulong id, int wearId = -1)
        {
            if (wearId < 0)
            {
                wearId = _bag.useWearId;
            }
            if (_bag.wearIdRefs.TryGetValue(id, out int wearedIds))
            {
                if ((wearedIds & (1 << wearId)) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 通过ItemId 获取 所处穿戴背包Id
        /// </summary>
        public int GetWearIdByItemId(ulong id)
        {
            if (_bag.wearIdRefs.TryGetValue(id, out int wearedId))
            {
                foreach (var key in Wears.Keys)
                {
                    if (((1 << key) & wearedId) != 0)
                    {
                        return key;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 卸下
        /// </summary>
        public void UnWear(int slot, int wearId = -1, bool isNotify = true)
        {
            if (wearId < 0)
            {
                wearId = _bag.useWearId;
            }
            if (Wears.TryGetValue(wearId, out var wear)
                && wear.TryGetValue(slot, out ulong id))
            {
                if (_bag.wearIdRefs.TryGetValue(id, out int wearedId))
                {
                    // 移除一位
                    _bag.wearIdRefs[id] = (int.MaxValue ^ (1 << wearId)) & wearedId;
                }
                wear.Remove(slot);

                if (isNotify)
                {
                    OnBagWearChange?.Invoke(BagType, wearId);
                }
            }
        }

        /// <summary>
        /// 卸下
        /// </summary>
        public void UnWear(ulong id, int wearId = -1)
        {
            if (wearId < 0)
            {
                wearId = _bag.useWearId;
            }
            if (Wears.TryGetValue(wearId, out var wear))
            {
                foreach (var item in wear)
                {
                    if (item.Value == id)
                    {
                        UnWear(item.Key, wearId);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 获得一个穿戴空位
        /// </summary>
        public int GetEmptyWearSlot(int wearId = -1)
        {
            if (wearId < 0)
            {
                wearId = _bag.useWearId;
            }
            if (Wears.TryGetValue(wearId, out var wear))
            {
                for (int i = wear.Count; i >= 0; i--)
                {
                    if (!wear.ContainsKey(i))
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 整理
        /// </summary>
        public void Sort()
        {
            var dict = _bag.allItems;
            _bag.allItems = new Dictionary<ulong, ItemData>();
            for (int i = 0; i < _itemSlots.Count; i++)
            {
                _itemSlots[i] = null;
            }
            foreach (var item in dict.Values)
            {
                if (item != null)
                {
                    AcquireItem(item);
                    ObjectPool<ItemData>.Release(item);
                }
            }
            if (_comparer != null)
            {
                _itemSlots.Sort(_comparer);
                for (int i = 0; i < _itemSlots.Count; i++)
                {
                    if (_itemSlots[i] != null)
                    {
                        _itemSlots[i].itemData.slot = i;
                    }
                }
            }
        }

        /// <summary>
        /// 所有存在的道具，没有空位
        /// </summary>
        public List<Item> GetNonEmptyList()
        {
            if (_sortItemsNonEmpty == null)
            {
                _sortItemsNonEmpty = ListPool<Item>.Get();
            }
            else
            {
                _sortItemsNonEmpty.Clear();
            }
            for (int i = 0; i < _itemSlots.Count; i++)
            {
                if (_itemSlots[i] != null)
                {
                    _sortItemsNonEmpty.Add(_itemSlots[i]);
                }
            }
            return _sortItemsNonEmpty;
        }

        /// <summary>
        /// 获取道具
        /// </summary>
        public Item GetItemById(ulong id)
        {
            if (!_bag.allItems.TryGetValue(id, out var itemData))
            {
                return null;
            }
            if (_allItems == null)
            {
                _allItems = new Dictionary<ulong, Item>();
            }
            if (!_allItems.TryGetValue(id, out var itemLogic))
            {
                itemLogic = new Item(itemData, this);
                _allItems.Add(id, itemLogic);
            }
            return itemLogic;
        }

        /// <summary>
        /// 获取道具
        /// </summary>
        public Item GetItemByMetaId(int id)
        {
            foreach (var item in AllItems.Values)
            {
                if (item.metaId == id)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            _itemSlots.Clear();
            _bag.allItems.Clear();
            _bag.wears.Clear();
            _bag.wearIdRefs.Clear();
            _bag.useWearId = 0;
            if (_sortItemsNonEmpty != null)
            {
                ListPool<Item>.Release(_sortItemsNonEmpty);
                _sortItemsNonEmpty = null;
            }
        }

        #region Static
        private static IdGenerate _idGenerate = new IdGenerate();
        public static ulong GenerateItemGuid()
        {
            return _idGenerate.GenerateId();
        }
        #endregion
    }
}