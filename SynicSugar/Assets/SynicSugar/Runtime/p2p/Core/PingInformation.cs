using System;
using System.Collections.Generic;

namespace SynicSugar.P2P {
    public class PingInformation {
        internal int Ping;
        internal DateTime LastUpdatedLocalUTC;
        internal List<double> tmpPings = new List<double>();
    }
}