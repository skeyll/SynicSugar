using System;
using UnityEngine;
using MemoryPack;

namespace SynicSugar.Samples.Tank {
    [MemoryPackable]
    public partial struct TankShootingData {
        public string FiredTimeStamp;
        public TankShellTransform shellTransform;
        public float Power;
        /// <summary>
        /// Get Latency from shooting remote.
        /// </summary>
        /// <value></value>
        internal float GetLatencyBetweenRemoteAndLocal() { 
            TimeSpan delta = DateTime.UtcNow - DateTime.Parse(FiredTimeStamp, null, System.Globalization.DateTimeStyles.RoundtripKind);
            return (float)delta.TotalSeconds;
        }
        static public TankShootingData GenerateShootingPacket (float power, Transform turret){
            return new TankShootingData () 
            { 
                FiredTimeStamp = DateTime.UtcNow.ToString("o"),
                Power = power,
                shellTransform = new TankShellTransform(turret)
            };
        }
    }
}