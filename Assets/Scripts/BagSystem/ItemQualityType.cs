using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    /// <summary>
    /// 道具品质
    /// </summary>
    public enum ItemQualityType
    {
        /// <summary>
        /// 灰色(粗糙)
        /// </summary>
        [InspectorName("灰色(粗糙)")]
        Rouch = 0,
        /// <summary>
        /// 白色(普通)
        /// </summary>
        [InspectorName("白色(普通)")]
        Normal = 1,
        /// <summary>
        /// 绿色(优秀)
        /// </summary>
        [InspectorName("绿色(优秀)")]
        Excellent = 2,
        /// <summary>
        /// 蓝色(精良)
        /// </summary>
        [InspectorName("蓝色(精良)")]
        Superior = 3,
        /// <summary>
        /// 紫色(史诗)
        /// </summary>
        [InspectorName("紫色(史诗)")]
        Epic = 4,
        /// <summary>
        /// 橙色(传说)
        /// </summary>
        [InspectorName("橙色(传说)")]
        Legend = 5,
    }

}
