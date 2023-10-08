using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SkierFramework.Demo
{
    public class BagDemo:MonoBehaviour
    {
        public void Start()
        {
            BagSystem.Instance.OnBag_Change += (bagType) =>
            {
                Debug.LogWarning($"[背包事件]{bagType}背包刷新了");
            };
            BagSystem.Instance.OnBag_WearChange += (bagType, wearId) =>
            {
                Debug.LogWarning($"[背包事件]{bagType}背包的{wearId}穿戴背包更新了");
            };

            // 加道具
            for (int i = 1; i <= 3; i++)
            {
                BagSystem.Instance.AcquireItem(i, 100);
            }
            Log("加道具");

            // 删道具1
            BagSystem.Instance.DelItem(1, 50);
            Log("删道具1");

            // 删道具3
            BagSystem.Instance.DelItem(3, 1000);
            Log("删道具3");

            // 穿戴道具
            var bag = BagSystem.Instance.GetOrAddBag(BagType.Default);
            var item1 = bag.GetItemByMetaId(1);
            var item2 = bag.GetItemByMetaId(2);
            int slot = 100; //槽位自行设置，槽位冲突时会自动替换
            int wearId = 1; //穿戴背包0-31
            bag.Wear(slot, item1.id, wearId);
            Log("穿戴道具1");

            bag.Wear(slot, item2.id, wearId);
            Log("穿戴道具2");

            // 卸下道具
            bag.UnWear(slot, wearId);
            Log("卸下穿戴");

            // 人物升1级
            var roleItem = BagSystem.Instance.RoleItem;
            roleItem.Add(ItemKey.Level, 1);
            roleItem.SetStr(ItemKey.Name, "巴拉巴拉");
            Log("人物属性修改");

            // 道具10点攻击力
            item1.Set(ItemKey.Attack, 10);
            Log("道具修改", item1);

            // 整理
            bag.Sort();
            Log("整理");

            // 存档时，只需要存档这个RoleItem即可
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(BagSystem.Instance.RoleItem);
            Debug.Log("存档：" + json);
            var jsonRoleItem = Newtonsoft.Json.JsonConvert.DeserializeObject<Item>(json);
            Log("新角色1：", jsonRoleItem);

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, BagSystem.Instance.RoleItem);
            var array = ms.ToArray();
            ms.Seek(0, SeekOrigin.Begin);
            var newRoleItem = bf.Deserialize(ms) as Item;
            Log("新角色2：", newRoleItem);
        }

        private void Log(string title, Item item = null)
        {
            if (item == null)
            {
                item = BagSystem.Instance.RoleItem;
            }
            string log = $"{title}\n";
            if (item.keyValues != null)
            {
                foreach (var pair in item.keyValues)
                {
                    log += $"{{{pair.Key}={pair.Value}}},";
                }
                log += "\n";
            }
            if (item.keyValueStrs != null)
            {
                foreach (var pair in item.keyValueStrs)
                {
                    log += $"{{{pair.Key}={pair.Value}}},";
                }
                log += "\n";
            }
            if (item.itemBags != null)
            {
                var bags = item.itemBags;
                foreach (var bag in bags.Values)
                {
                    log += $"[{bag.BagType}]：";
                    foreach (var temp in bag.AllItems.Values)
                    {
                        log += $"{{id={temp.id},metaId={temp.metaId},count={temp.count},穿戴={bag.IsWear(temp.id, 1)}}},";
                    }
                    log += "\n";
                }
            }
            Debug.Log(log);
        }

        #region 配置读取
        public static Dictionary<Type, Dictionary<int, IConfig>> configs;
        static BagDemo()
        {
            configs = new Dictionary<Type, Dictionary<int, IConfig>>();
            Dictionary<int, IConfig> itemConfig = new Dictionary<int, IConfig>();
            configs.Add(typeof(ItemConfig), itemConfig);

            for (int i = 1; i <= 10; i++)
            {
                itemConfig.Add(i, new ItemConfig
                {
                    id = i,
                    bagType = BagType.Default,
                    overlayCount = 10000,
                    name = "测试" + i,
                    itemType = ItemType.Gold
                });
            }
        }
        public static T GetConfig<T>(int id) where T : IConfig
        {
            if (configs != null
                && configs.TryGetValue(typeof(T), out var map)
                && map.TryGetValue(id, out var config))
            {
                return (T)config;
            }
            return default(T);
        }
        #endregion
    }
}
