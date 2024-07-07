using System;
namespace SynicSugar.P2P {
    //For sync
    [AttributeUsage(AttributeTargets.Field,
    Inherited = false)]
    public sealed class SyncVarAttribute : Attribute {
        //Only NetworkCommons
        public bool isHost;
        //If not set, the Manager's value is used. 
        public int syncInterval;
        public SyncVarAttribute(){
        }
        /// <summary>
        /// For NetworkCommons 
        /// </summary>
        /// <param name="isOnlyHost"></param>
        public SyncVarAttribute(bool isOnlyHost){
            isHost = isOnlyHost;
        }
        public SyncVarAttribute(bool isOnlyHost, int syncIntervalMs){
            isHost = isOnlyHost;
            syncInterval = syncIntervalMs;
        }
        public SyncVarAttribute(int syncIntervalMs){
            syncInterval = syncIntervalMs;
        }
    }
}