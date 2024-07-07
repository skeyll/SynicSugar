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
}