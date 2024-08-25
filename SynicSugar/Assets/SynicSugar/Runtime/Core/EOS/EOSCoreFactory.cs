using SynicSugar.Auth;
using SynicSugar.Auth.Base;
using SynicSugar.MatchMake;
using SynicSugar.MatchMake.Base;
namespace SynicSugar {
    internal sealed class EOSCoreFactory : SynicSugarCoreFactory {
        AuthenticationCore auth;
        public EOSCoreFactory (){
            auth = new EOSAuthentication();
        }
        public override AuthenticationCore GetAuthenticationCore(){
            return auth;
        }
        public override MatchmakingCore GetMatchmakingCore(uint maxSearch){
            return new EOSMatchmaking(maxSearch);
        }
    }
}
