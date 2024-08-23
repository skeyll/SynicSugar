using SynicSugar.Auth;
using SynicSugar.MatchMake;

namespace SynicSugar {
    public abstract class SynicSugarCoreFactoryBase {
        public abstract AuthenticationCore GetAuthenticationCore();
        public abstract MatchmakingCore GetMatchmakingCore(uint maxSearch);
    }
}