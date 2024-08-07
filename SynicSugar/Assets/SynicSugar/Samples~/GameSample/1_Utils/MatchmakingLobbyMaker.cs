using SynicSugar.MatchMake;

namespace SynicSugar.Samples {
    public class MatchmakingLobbyMaker {
        IMatchmakingConditions conditions;
        /// <summary>
        /// Set each conditons interface instance.
        /// </summary>
        /// <param name="conditions"></param>
        internal MatchmakingLobbyMaker(IMatchmakingConditions conditions){
            this.conditions = conditions;
        }

        public Lobby GenerateConditionLobbyObject(uint MaxLobbyMembers = 2, bool useRTC = false){
            //Create lobby for conditions
            Lobby lobby = MatchMakeManager.GenerateLobbyObject(conditions.GenerateBucket(), MaxLobbyMembers, useVoiceChat: useRTC);

            lobby.Attributes = conditions.GenerateMatchmakingAttributes();
            
            return lobby;
        }
    }
}