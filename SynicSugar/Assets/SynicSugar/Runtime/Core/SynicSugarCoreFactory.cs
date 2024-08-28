using SynicSugar.Auth.Base;
using SynicSugar.Base;
using SynicSugar.MatchMake.Base;

namespace SynicSugar {
    public abstract class SynicSugarCoreFactory {
        public abstract AuthenticationCore GetAuthenticationCore();
        public abstract MatchmakingCore GetMatchmakingCore(uint maxSearch);
        public abstract SessionCore GetSessionCore();
    }
}