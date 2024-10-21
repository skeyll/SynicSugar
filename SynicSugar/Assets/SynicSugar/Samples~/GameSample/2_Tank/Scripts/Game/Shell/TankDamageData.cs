using MemoryPack;
namespace SynicSugar.Samples.Tank 
{
    [MemoryPackable]
    public partial class TankDamageData 
    {
        public string AttackerId;
        public float Damage;
        [MemoryPackConstructor]
        public TankDamageData (string attackerid, float damage) 
        {
            AttackerId = attackerid;
            Damage = damage;
        }
    }
}
