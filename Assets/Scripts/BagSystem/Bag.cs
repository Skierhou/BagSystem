using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SkierFramework
{
    [System.Serializable]
    public class Bag
    {
        /// <summary>
        /// 背包类型
        /// </summary>
        [JsonRequired]
        private BagType _bagType;
        /// <summary>
        /// 槽位
        /// </summary>
        [JsonRequired]
        private List<Item> _slots;
        /// <summary>
        /// 道具
        /// </summary>
        [JsonRequired]
        private Dictionary<ulong, Item> _allItems;
        /// <summary>
        /// 多套穿戴
        /// </summary>
        [JsonRequired]
        private Dictionary<int, Dictionary<int, ulong>> _wears;
        /// <summary>
        /// 判断这个id是否有穿戴
        /// </summary>
        [JsonRequired]
        private Dictionary<ulong, int> _wearIdRefs;
        /// <summary>
        /// 当时使用的穿戴Id
        /// </summary>
        [JsonRequired]
        private int _useWearId;
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
        [JsonIgnore]
        public BagType BagType => _bagType;
        /// <summary>
        /// 所有槽位，用于显示
        /// </summary>
        [JsonIgnore]
        public List<Item> Slots => _slots;
        /// <summary>
        /// 所有道具
        /// </summary>
        [JsonIgnore]
        public Dictionary<ulong, Item> AllItems => _allItems;
        /// <summary>
        /// 穿戴的背包，支持一种背包多套穿戴组合，可一键替换等功能
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, Dictionary<int, ulong>> Wears => _wears;
        /// <summary>
        /// 当前使用的穿戴Id
        /// </summary>
        [JsonIgnore]
        public int UseWearId => _useWearId;
        /// <summary>
        /// 穿戴变化:背包类型，穿戴id
        /// </summary>
        [NonSerialized]
        [JsonIgnore]
        public Action<BagType, int> OnBag_WearChange;
        /// <summary>
        /// 背包变化:背包类型
        /// </summary>
        [NonSerialized]
        [JsonIgnore]
        public Action<BagType> OnBag_Change;

        /// <summary>
        /// 初始化背包
        /// </summary>
        public void InitBag(BagType bagType, int size, IComparer<Item> comparer = null)
        {
            _slots = new List<Item>(size);
            for (int i = 0; i < size; i++)
            {
                _slots.Add(null);
            }
            _bagType = bagType;
            _allItems = new Dictionary<ulong, Item>();
            _wears = new Dictionary<int, Dictionary<int, ulong>>();
            _wearIdRefs = new Dictionary<ulong, int>();
            _comparer = comparer;

            if (_comparer == null)
            {
                _comparer = Comparer<Item>.Create((a, b) =>
                {
                    int num = a.metaId.CompareTo(b.metaId);
                    if (num == 0)
                    {
                        return a.id.CompareTo(b.id);
                    }
                    return num;
                });
            }
        }

        /// <summary>
        /// 变化大小
        /// </summary>
        public void ChangeSize(int size)
        {
            while (_slots.Count < size)
            {
                _slots.Add(null);
            }

            if (_slots.Count > size)
            {
                Sort();

                while (_slots.Count > size)
                {
                    if (_slots[_slots.Count - 1] != null)
                    {
                        // 说明这个丢了，不过一般不允许缩小空间只有增加
                    }
                    _slots.RemoveAt(_slots.Count - 1);
                }
            }
        }

        /// <summary>
        /// 获得空格子或者相同的格子
        /// </summary>
        public bool GetEmptyOrSameSlot(int metaId, out int index)
        {
            index = -1;
            for (int i = 0; i < _slots.Count; i++)
            {
                ulong invId = _slots[i] == null ? 0 : _slots[i].id;
                if ((invId == 0 || !_allItems.ContainsKey(invId)) && index < 0)
                {
                    index = i;
                }
                else if (invId != 0
                    && _allItems.TryGetValue(invId, out Item item)
                    && item.metaId == metaId && !item.IsFull)
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
                Item item = null;
                if (_slots[index] == null || !_allItems.TryGetValue(_slots[index].id, out item))
                {
                    guid = guid == 0 ? GenerateItemGuid() : guid;
                    item = ObjectPool<Item>.Get();
                    item.metaId = inMetaId;
                    item.id = guid;
                    item.count = 0;
                    item.bag = this;
                    _slots[index] = item;
                    _allItems.Add(guid, item);
                }
                inAmount -= item.AddAmount(inAmount);
            }

            if (inAmount > 0)
            {
                // 说明背包满了
                Debug.Log("背包塞满了！！！");
            }

            if (inAmount != cache)
            {
                OnBag_Change?.Invoke(BagType);
            }

            //返回实际操作几个
            return cache - inAmount;
        }

        /// <summary>
        /// 获得道具
        /// </summary>
        public void AcquireItem(Item item, bool isNotify = false)
        {
            while (item != null && item.count > 0 && GetEmptyOrSameSlot(item.metaId, out int index))
            {
                if (_slots[index] == null)
                {
                    item.bag = this;
                    _slots[index] = item;
                    _allItems.Add(item.id, item);
                    item = null;
                }
                else
                {
                    item.count -= _slots[index].AddAmount(item.count);
                }
            }
            if (isNotify)
            {
                OnBag_Change?.Invoke(BagType);
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
                    for (int i = 0; i < _slots.Count; i++)
                    {
                        if (_slots[i] != null && _slots[i].id == id)
                        {
                            _slots[i] = null;
                            break;
                        }
                    }
                    _allItems.Remove(id);
                    if (_wearIdRefs.TryGetValue(id, out int wearedId))
                    {
                        foreach (var wearPair in Wears)
                        {
                            if (((1 << wearPair.Key) & wearedId) != 0)
                            {
                                foreach (var pair in wearPair.Value)
                                {
                                    if (pair.Value == id)
                                    {
                                        OnBag_WearChange?.Invoke(BagType, pair.Key);
                                        wearPair.Value.Remove(pair.Key);
                                        break;
                                    }
                                }
                            }
                        }
                        _wearIdRefs.Remove(id);
                    }
                }
                OnBag_Change?.Invoke(BagType);
            }
            return changeCount;
        }
        /// <summary>
        /// 删除道具
        /// </summary>
        public void DelItem(int metaId, int count)
        {
            var removes = ListPool<ulong>.Get();
            foreach (var item in _allItems.Values)
            {
                if (item.metaId == metaId)
                {
                    count -= item.AddAmount(-count);

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
                wearId = _useWearId;
            }

            if (!_wears.TryGetValue(wearId, out var wear))
            {
                wear = new Dictionary<int, ulong>();
                _wears.Add(wearId, wear);
            }
            if (wear.ContainsKey(slot))
            {
                UnWear(slot, wearId, false);
            }
            wear.Add(slot, id);
            _wearIdRefs.TryGetValue(id, out int wearedSlot);
            _wearIdRefs[id] = wearedSlot | (1 << wearId);
            OnBag_WearChange?.Invoke(BagType, wearId);
        }

        /// <summary>
        /// 获得穿戴
        /// </summary>
        public Dictionary<int, ulong> GetWear(int wearId = -1)
        {
            if (wearId < 0)
            {
                wearId = _useWearId;
            }
            if (!_wears.TryGetValue(wearId, out var wear))
            {
                wear = new Dictionary<int, ulong>();
                _wears.Add(wearId, wear);
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
                wearId = _useWearId;
            }
            if (_wearIdRefs.TryGetValue(id, out int wearedIds))
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
            if (_wearIdRefs.TryGetValue(id, out int wearedId))
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
                wearId = _useWearId;
            }
            if (Wears.TryGetValue(wearId, out var wear)
                && wear.TryGetValue(slot, out ulong id))
            {
                if (_wearIdRefs.TryGetValue(id, out int wearedId))
                {
                    // 移除一位
                    _wearIdRefs[id] = (int.MaxValue ^ (1 << wearId)) & wearedId;
                }
                wear.Remove(slot);

                if (isNotify)
                {
                    OnBag_WearChange?.Invoke(BagType, wearId);
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
                wearId = _useWearId;
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
                wearId = _useWearId;
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
            var dict = _allItems;
            _allItems = new Dictionary<ulong, Item>();
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i] = null;
            }
            foreach (var item in dict.Values)
            {
                if (item != null)
                {
                    AcquireItem(item);
                    ObjectPool<Item>.Release(item);
                }
            }
            _slots.Sort(_comparer);
        }


        /// <summary>
        /// 所有存在的道具，没有空位
        /// </summary>
        public List<Item> GetSortItems()
        {
            if (_comparer != null)
            {
                _slots.Sort(_comparer);
            }
            if (_sortItemsNonEmpty == null)
            {
                _sortItemsNonEmpty = ListPool<Item>.Get();
            }
            else
            {
                _sortItemsNonEmpty.Clear();
            }
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null)
                {
                    _sortItemsNonEmpty.Add(_slots[i]);
                }
            }
            return _sortItemsNonEmpty;
        }

        /// <summary>
        /// 获取道具
        /// </summary>
        public Item GetItemById(ulong id)
        {
            if (_allItems.TryGetValue(id, out Item item))
                return item;
            return null;
        }

        /// <summary>
        /// 获取道具
        /// </summary>
        public Item GetItemByMetaId(int id)
        {
            foreach (var item in _allItems.Values)
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
            _slots.Clear();
            foreach (var item in _allItems.Values)
            {
                item.Release();
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