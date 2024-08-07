using SynicSugar.P2P;
namespace SynicSugar.Samples.Tank {
    public class TankResultData {
        public UserId UserId;
        public string Name;
        public float RemainHP;
        public TankResultData(UserId userid, string name, float remainhp) {
            UserId = userid;
            Name = name;
            RemainHP = remainhp;
        }
    }
}
