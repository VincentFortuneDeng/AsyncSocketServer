﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTERIXDecode
{
    class CAT62I200UserData
    {

        public static void DecodeCAT62I200(byte[] Data)
        {
            // Increase data buffer index so it ready for the next data item.
            CAT62.CurrentDataBufferOctalIndex = CAT62.CurrentDataBufferOctalIndex + 1;
        }
    }
}
