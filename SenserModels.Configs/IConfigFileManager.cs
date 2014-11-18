using System;
using System.Text;

namespace SenserModels.Config
{
    /// <summary>
    /// CnTaxLawyer 配置管理类接口
    /// </summary>
    public interface IConfigFileManager
    {
        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <returns></returns>
        IConfigInfo LoadConfig();


        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <returns></returns>
        bool SaveConfig();
    }
}
