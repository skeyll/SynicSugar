using SynicSugar.MatchMake;
namespace SynicSugar {
    internal sealed class SynicSugarCoreFactory : SynicSugarCoreFactoryBase {
        public override MatchmakingCore GenerateMatchmakingCore(uint maxSearch){
            return new EOSLobby(maxSearch);
        }
    }
}
