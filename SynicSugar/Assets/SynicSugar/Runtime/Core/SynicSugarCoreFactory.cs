using SynicSugar.Base;

namespace SynicSugar {
    public abstract class SynicSugarCoreFactory {
        public abstract string CoreName { get; protected set; }
        public abstract AuthenticationCore GetAuthenticationCore();
        public abstract MatchmakingCore GetMatchmakingCore(uint maxSearch);
        public abstract SessionCore GetSessionCore();
    }
}