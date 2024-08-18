using SynicSugar.MatchMake;
namespace SynicSugar {
    internal sealed class SynicSugarCoreFactory : SynicSugarCoreFactoryBase {
        public override MatchmakingCore GenerateMatchmakingCore(uint maxSearch, int matchmakingTimeout, int initialConnectionTimeout){
            return new EOSLobby2(maxSearch, matchmakingTimeout, initialConnectionTimeout);
        }
    }
}
