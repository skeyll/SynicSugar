using SynicSugar.Base;

namespace SynicSugar {
    public abstract class SynicSugarCoreFactory {
        public abstract AuthenticationCore GetAuthenticationCore();
        public abstract MatchmakingCore GetMatchmakingCore(uint maxSearch);
        public abstract SessionCore GetSessionCore();
    }
}