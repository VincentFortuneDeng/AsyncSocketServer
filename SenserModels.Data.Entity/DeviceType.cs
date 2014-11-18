using System;
using System.Collections.Generic;
using System.Text;

namespace SenserModels.Entity
{
    public enum DeviceType
    {
        PUOP,    //泵出口压力：      Pump outlet pressure	 
        PUIP,    //泵进口压力：      Pump inlet pressure
        ULTF,     //超声波流量计：	Ultrasonic Flow
        /*WISP, //注水站压力：	    Water injection station pressure
        WDSP,   //配水站压力：	    Water distribution stations pressure
        WIWP,   //注水井压力：	    Water injection well pressure	*/
        ELEP,     //电参数：          Electrical parameters
        Unknown,     
    }
}
