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
        public Vector3 RespawnPos;
        // If you don't need to sync by Synic, add MemoryPackIgnore attribute.
        [MemoryPackIgnore] public bool isReady; //This player ready to start game?

        [MemoryPackConstructor]
        public TankPlayerStatus (string name, int maxhp, float currenthp, float speed, int attack, Vector3 respawnPos){
            Name = name;
            MaxHP = maxhp;
            CurrentHP = currenthp;
            Speed = speed;
            Attack = attack;
            RespawnPos = respawnPos;
        }
        internal TankPlayerStatus(){}
    }
}
