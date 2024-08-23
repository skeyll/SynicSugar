using SynicSugar.Auth;
using SynicSugar.MatchMake;
namespace SynicSugar {
    internal sealed class SynicSugarCoreFactory : SynicSugarCoreFactoryBase {
        AuthenticationCore auth;
        public SynicSugarCoreFactory (){
            auth = new EOSAuthentication ();
        }
        public override AuthenticationCore GetAuthenticationCore(){
            return auth;
        }
        public override MatchmakingCore GetMatchmakingCore(uint maxSearch){
            return new EOSMatchmaking(maxSearch);
        }
    }
}
