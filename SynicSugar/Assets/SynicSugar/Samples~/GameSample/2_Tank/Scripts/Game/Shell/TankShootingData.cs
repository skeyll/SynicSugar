using UnityEngine;
using MemoryPack;
using SynicSugar.P2P;

namespace SynicSugar.Samples.Tank {
    [MemoryPackable]
    public partial struct TankShootingData {
        public float FiredTimeStamp;
        public TankShellTransform shellTransform;
        public float Power;
        /// <summary>
        /// Get Latency from shooting remote.
        /// </summary>
        /// <value></value>
        internal float GetLatencyBetweenRemoteAndLocal() {
            return ConnectHub.Instance.GetInstance<TankRoundTimer>().GetCurrentTime() - FiredTimeStamp;
        }
        static public TankShootingData GenerateShootingPacket (float power, Transform turret){
            return new TankShootingData () 
            { 
                FiredTimeStamp = ConnectHub.Instance.GetInstance<TankRoundTimer>().GetCurrentTime(),
                Power = power,
                shellTransform = new TankShellTransform(turret)
            };
        }
    }
}