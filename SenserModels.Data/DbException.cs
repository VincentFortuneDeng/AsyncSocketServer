using System;
using System.Text;

namespace SenserModels.Data
{
    public class DbException : SenserModels.Common.YTXException
    {
        public DbException(string message)
            : base(message)
        {
        }

        public int Number
        {
            get { return 0 ; }
        }

       
    }
}
