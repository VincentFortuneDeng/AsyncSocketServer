using System;
using System.Text.RegularExpressions;

namespace SenserModels.Common
{
    /// <summary>
    /// 正则表达式验证结果返回信息
    /// </summary>
	public struct RegexInfo
	{
        /// <summary>
        /// 错误消息
        /// </summary>
		public string ErrorMessage;
        /// <summary>
        /// 匹配结果
        /// </summary>
		public bool IsMatched;
	}
    
    /// <summary>
    /// Regexlib 的摘要说明。
    /// </summary>
    public class Regexlib
    {
		public Regexlib()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }


        ////搜索输入字符串并返回所有 href=“”值
        //public static string DumpHrefs(String inputString)
        //{
        //    Regex r;
        //    Match m;

        //    r = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))",RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //    for (m = r.Match(inputString); m.Success; m = m.NextMatch())
        //    {
        //        return ("Found href " + m.Groups[1]);
        //    }
        //}

		//验证是否为整数
        /// <summary>
        /// 验证是否是整数
        /// </summary>
        /// <param name="strIn">要验证的整数字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedIntegerInfo(string strIn)
		{
			RegexInfo intinfo = new RegexInfo();

			if (!(intinfo.IsMatched = Regex.IsMatch(strIn, @"\b\d+\b")))
			{
				intinfo.ErrorMessage = "不是整数";
			}

			return intinfo;
		}

        //验证Email地址
        /// <summary>
        /// 验证是否是电子邮件
        /// </summary>
        /// <param name="strIn">要验证的电子邮件字符串</param>
        /// <returns>验证结果</returns>
        public static RegexInfo ValidatedEmailInfo(string strIn)
        {
			RegexInfo emailinfo = new RegexInfo();
            // 当输入值为电子邮件格式是返回 True
			if (!(emailinfo.IsMatched = Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")))
			{
				emailinfo.ErrorMessage = "邮箱地址不正确";
			}

			return emailinfo;
        }


        //dd-MM-yy 的日期形式代替 mm/dd/yy 的日期形式。
        /// <summary>
        /// dd-MM-yy 的日期形式代替 mm/dd/yy 的日期形式。
        /// </summary>
        /// <param name="input">要转换的正日期字符串</param>
        /// <returns>转换后的日期字符串</returns>
        public static string MDYToDMY(String input)
        {
            return Regex.Replace(input, "\\b(?\\d{1,2})/(?\\d{1,2})/(?\\d{2,4})\\b", "${day}-${month}-${year}");
        }


        //验证是否为小数
        /// <summary>
        /// 验证是否为小数
        /// </summary>
        /// <param name="strIn">要验证的小数字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedDecimalInfo(string strIn)
        {
			RegexInfo decimalinfo = new RegexInfo();

			if (!(decimalinfo.IsMatched = Regex.IsMatch(strIn, @"[0].\d{1,2}|[1]")))
			{
				decimalinfo.ErrorMessage = "不是小数";
			}

			return decimalinfo;
        }


        //验证是否为电话号码
        /// <summary>
        /// 验证是否为电话号码
        /// </summary>
        /// <param name="strIn">要验证的电话号码字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedTelInfo(string strIn)
        {
			RegexInfo telinfo = new RegexInfo();

			if (!(telinfo.IsMatched = Regex.IsMatch(strIn, @"(\d+-)?(\d{4}-?\d{7}|\d{3}-?\d{8}|^\d{7,8})(-\d+)?")))
			{
				telinfo.ErrorMessage = "电话号码格式不正确";
			}

			return telinfo;
        }


        //验证年月日
        /// <summary>
        /// 验证是否是年月日格式
        /// </summary>
        /// <param name="strIn">要验证的电话日期字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedDateInfo(string strIn)
        {
			RegexInfo dateinfo = new RegexInfo();

			if (!(dateinfo.IsMatched = Regex.IsMatch(strIn, @"^2\d{3}-(?:0?[1-9]|1[0-2])-(?:0?[1-9]|[1-2]\d|3[0-1])(?:0?[1-9]|1\d|2[0-3]):(?:0?[1-9]|[1-5]\d):(?:0?[1-9]|[1-5]\d)$")))
			{
				dateinfo.ErrorMessage = "日期格式不正确";
			}

			return dateinfo;
        }


        //验证后缀名
        /// <summary>
        /// 验证后缀名是否为指定格式
        /// </summary>
        /// <param name="strIn">要验证的字符串</param>
        /// <returns>验证结果</returns>
        public static bool IsValidPostfix(string strIn)
        {
            return Regex.IsMatch(strIn, @"\.(?i:gif|jpg)$");
        }


        //验证字符是否在4至12之间
        /// <summary>
        /// 验证验证字符是否在4至12之间
        /// </summary>
        /// <param name="strIn">要验证的字符串</param>
        /// <returns>验证结果</returns>
        public static bool IsValidByte(string strIn)
        {
            return Regex.IsMatch(strIn, @"^[a-z]{4,12}$");
        }


        //验证IP
        /// <summary>
        /// 验证IP地址
        /// </summary>
        /// <param name="strIn">要验证的字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedIPInfo(string strIn)
        {
			RegexInfo ipinfo = new RegexInfo();

			if (!(ipinfo.IsMatched = Regex.IsMatch(strIn, @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$")))
			{
				ipinfo.ErrorMessage = "IP地址格式不正确";
			}

			return ipinfo; 
        }

		//验证邮政编码
        /// <summary>
        /// 验证邮政编码
        /// </summary>
        /// <param name="strIn">要验证的字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedPostalcodeInfo(string strIn)
		{
			RegexInfo postcodeinfo = new RegexInfo();

			if (!(postcodeinfo.IsMatched = Regex.IsMatch(strIn, @"\b\d{6}\b")))
			{
				postcodeinfo.ErrorMessage = "邮政编码格式不正确";
			}

			return postcodeinfo; 
		}

		//验证URL地址
        /// <summary>
        /// 验证URL地址
        /// </summary>
        /// <param name="strIn">要验证的URL字符串</param>
        /// <returns>验证结果</returns>
		public static RegexInfo ValidatedURLInfo(string strIn)
		{
			RegexInfo urlinfo = new RegexInfo();

			if (!(urlinfo.IsMatched = Regex.IsMatch(strIn, @"\b\d{6}\b")))
			{
				urlinfo.ErrorMessage = "URL地址格式不正确";
			}

			return urlinfo; 
		}
    }
}
