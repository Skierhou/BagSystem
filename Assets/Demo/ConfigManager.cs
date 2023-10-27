using System;
using System.Collections.Generic;

namespace SkierFramework
{
    /// <summary>
    /// 自行更换配置系统
    /// </summary>
    public class ConfigManager
    {
        public static ConfigManager Instance { get; } = new ConfigManager();

        public Dictionary<Type, Dictionary<int, IConfig>> configs;

        private ConfigManager()
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
                    qualityType = (ItemQualityType)(i / 2),
                    itemType = ItemType.Gold
                });
            }
        }
        public T GetConfig<T>(int id) where T : IConfig
        {
            if (configs != null
                && configs.TryGetValue(typeof(T), out var map)
                && map.TryGetValue(id, out var config))
            {
                return (T)config;
            }
            return default(T);
        }
    }
}
