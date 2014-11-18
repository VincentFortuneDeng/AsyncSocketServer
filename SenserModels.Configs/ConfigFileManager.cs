/*----------------------------------------------------------------
// Copyright (C) 2010 精英智通
// 版权所有。 
//
// 文件名：CalibrationParamaterConfigFileManager.cs
// 文件功能描述：CalibrationParamater专用序列化配置管理类
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
using System.IO;

namespace SenserModels.Config
{
    /// <summary>
    ///  Software配置管理类
    /// </summary>
    public class ConfigFileManager<T> : DefaultConfigFileManager where T : IConfigInfo
    {
        private static T m_configinfo;
        private static string strPath = "config\\" + typeof(T).Name + ".config";

        /// <summary>
        /// 文件修改时间
        /// </summary>
        private static DateTime m_fileoldchange;

        /// <summary>
        /// 初始化文件修改时间和对象实例
        /// </summary>
        static ConfigFileManager()
        {
            m_fileoldchange = System.IO.File.GetLastWriteTime(ConfigFilePath);
            m_configinfo = (T)DefaultConfigFileManager.DeserializeInfo(ConfigFilePath, typeof(T));
        }

        /// <summary>
        /// 当前的配置实例
        /// </summary>
        public new static IConfigInfo ConfigInfo
        {
            get { return (IConfigInfo)m_configinfo; }
            set { m_configinfo = (T)value; }
        }


        /// <summary>
        /// 配置文件所在路径
        /// </summary>
        public static string filename = null;


        /// <summary>
        /// 获取配置文件所在路径
        /// </summary>
        public new static string ConfigFilePath
        {
            get
            {
                if (filename == null)
                {
                    filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
                }

                return filename;
            }
        }

        /// <summary>
        /// 返回配置类实例
        /// </summary>
        /// <returns></returns>
        public static T LoadConfig()
        {
            ConfigInfo = DefaultConfigFileManager.LoadConfig(ref m_fileoldchange, ConfigFilePath, ConfigInfo);
            return (T)ConfigInfo;
        }

        /// <summary>
        /// 保存配置类实例
        /// </summary>
        /// <returns></returns>
        public override bool SaveConfig()
        {
            return base.SaveConfig(ConfigFilePath, ConfigInfo);
        }
    }
}
