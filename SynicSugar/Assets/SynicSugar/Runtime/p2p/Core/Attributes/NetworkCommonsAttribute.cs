using System;
namespace SynicSugar.P2P {
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
}