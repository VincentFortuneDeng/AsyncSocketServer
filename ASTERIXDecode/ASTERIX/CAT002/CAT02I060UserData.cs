﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTERIXDecode
{
    class CAT02I060UserData
    {
        public static void DecodeCAT02I060(byte[] Data)
        {
            CAT02.CurrentDataBufferOctalIndex = CAT02.CurrentDataBufferOctalIndex + 1;
        }
    }
}
