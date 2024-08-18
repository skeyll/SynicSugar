using SynicSugar.MatchMake;

namespace SynicSugar {
    public abstract class SynicSugarCoreFactoryBase {
        public abstract MatchmakingCore GenerateMatchmakingCore(uint maxSearch, int matchmakingTimeout, int initialConnectionTimeout);
    }
}