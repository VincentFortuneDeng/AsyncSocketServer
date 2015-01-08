using AsyncSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTERIXDecode
{
    public class ASTERIXDecoder
    {
        public event EventHandler<RadarMessageEventArg> OnRadarMessageComepleted;

        public void ProcessRadarData(object sender, AsyncUserToken token)
        {

            RadarMessage radarMessage = new RadarMessage();

            if(OnRadarMessageComepleted != null) {
                OnRadarMessageComepleted.Invoke(this, new RadarMessageEventArg(radarMessage));
            }
        }
    }
}
