using System.Collections.Generic;
using SynicSugar.MatchMake;
namespace SynicSugar.Samples {
    public interface IMatchmakingConditions {
        public string[] GenerateBucket();
        public List<AttributeData> GenerateMatchmakingAttributes();
    }
}