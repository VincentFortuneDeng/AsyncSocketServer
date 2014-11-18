using System;

namespace SenserModels.Common
{
	/// <summary>
	/// YTX自定义异常类。
	/// </summary>
	public class YTXException : Exception
	{
		public YTXException()
		{
			//
		}


		public YTXException(string msg) : base(msg)
		{
			//
		}
	}
}
