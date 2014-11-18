using System;
using System.Text;

namespace SenserModels.Common.Xml
{
    public class InvalidXmlException : YTXException
    {
        public InvalidXmlException(string message)
            : base(message)
        {
        }
    }
}
