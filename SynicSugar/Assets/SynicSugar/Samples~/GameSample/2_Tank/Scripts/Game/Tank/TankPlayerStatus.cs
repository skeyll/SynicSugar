using UnityEngine;
using MemoryPack;

namespace SynicSugar.Samples.Tank {
    [MemoryPackable]
    public partial class TankPlayerStatus {
        public string Name;
        public int MaxHP;
        public float CurrentHP;
        public float Speed;
        public int Attack;
        public Vector3 RespawnPosition;
        public Quaternion RespawnQuaternion;
        // If you don't need to sync by Synic, add MemoryPackIgnore attribute.
        [MemoryPackIgnore] public bool isReady; //This player ready to start game?

        [MemoryPackConstructor]
        public TankPlayerStatus (string name, int maxhp, float currenthp, float speed, int attack, Vector3 respawnPosition, Quaternion respawnQuaternion){
            Name = name;
            MaxHP = maxhp;
            CurrentHP = currenthp;
            Speed = speed;
            Attack = attack;
            RespawnPosition = respawnPosition;
            RespawnQuaternion = respawnQuaternion;
        }
        internal TankPlayerStatus(){}
    }
}
