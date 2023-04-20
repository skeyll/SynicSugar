using System;
namespace SynicSugar.P2P {
    //For base
    [AttributeUsage(AttributeTargets.Class,
    Inherited = false)]
    /// <summary>
    /// For each user data like game-character and user-data.
    /// </summary>
    public sealed class NetworkPlayerAttribute : Attribute {
        public bool useGetInstance;
        public NetworkPlayerAttribute(){
        }
        public NetworkPlayerAttribute(bool useGetInstance){
            this.useGetInstance = useGetInstance;
        }
    }
    [AttributeUsage(AttributeTargets.Class,
    Inherited = false)]
    /// <summary>
    /// For shared data like game state and time.
    /// </summary>
    public sealed class NetworkCommonsAttribute : Attribute {
        public bool useGetInstance;
        public NetworkCommonsAttribute(){
        }
        public NetworkCommonsAttribute(bool useGetInstance){
            this.useGetInstance = useGetInstance;
        }
    }
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
    [AttributeUsage(AttributeTargets.Method,
    Inherited = false)]
    public sealed class RpcAttribute : Attribute {
    }
    [AttributeUsage(AttributeTargets.Method,
    Inherited = false)]
    public sealed class TargetRpcAttribute : Attribute {
    }
    
    /// <summary>
    /// SourceGenerator attach this automatically. If necessary, pass True to NetworkPlayerAttributes.
    /// </summary>
    public interface IGetPlayer{
    }
    /// <summary>
    /// SourceGenerator attach this automatically. If necessary, pass True to NetworkCommonsAttributes.
    /// </summary>
    public interface IGetCommons{
    }
}