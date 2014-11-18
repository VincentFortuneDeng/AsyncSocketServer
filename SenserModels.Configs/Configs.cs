/*----------------------------------------------------------------
// Copyright (C) 2010 精英智通
// 版权所有。 
//
// 文件名：CalibrationParamaterConfigs.cs
// 文件功能描述：CalibrationParamater类专用序列化配置类
//
// 
// 创建标识：邓守海20100125
//
// 修改标识：
// 修改描述：
//
// 修改标识：
// 修改描述：
//----------------------------------------------------------------*/

using System;
using System.Text;

namespace SenserModels.Config
{
    /// <summary>
    ///  Software配置类
    /// </summary>
    public class Configs<T> where T : IConfigInfo
    {
        /// <summary>
        /// 获取配置类实例
        /// </summary>
        /// <returns></returns>
        public static T GetConfig()
        {
            return ConfigFileManager<T>.LoadConfig();
        }

        /// <summary>
        /// 保存配置类实例
        /// </summary>
        /// <param name="Softwareconfiginfo"></param>
        /// <returns></returns>
        public static bool SaveConfig(T obj)
        {
            ConfigFileManager<T> cpcfm = new ConfigFileManager<T>();
            ConfigFileManager<T>.ConfigInfo = (T)obj;
            return cpcfm.SaveConfig();
        }
    }
}
