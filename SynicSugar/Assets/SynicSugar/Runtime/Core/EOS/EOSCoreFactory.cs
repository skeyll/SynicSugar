using SynicSugar.Base;
using SynicSugar.Auth;
using SynicSugar.MatchMake;
using SynicSugar.P2P;

namespace SynicSugar {
    internal sealed class EOSCoreFactory : SynicSugarCoreFactory {
        public override string CoreName { get; protected set; } = "EOSDefault";
        AuthenticationCore auth;
        public EOSCoreFactory (){
            auth = new EOSAuthentication();
        }
        public override SynicSugarCoreFactory CreateInstance(){
            return new EOSCoreFactory();
        }
        public override AuthenticationCore GetAuthenticationCore(){
            return auth;
        }
        public override MatchmakingCore GetMatchmakingCore(uint maxSearch){
            return new EOSMatchmaking(maxSearch);
        }
        public override SessionCore GetSessionCore(){
            return new EOSSessionManager();
        }
    }
}
