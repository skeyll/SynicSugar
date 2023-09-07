#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using SynicSugar.P2P;

namespace SynicSugar.MatchMake {
    public class LobbyMemberUpdateNotifier {
        /// <summary>
        /// Invoke when a user attributes is updated in current lobby.</ br>
        /// </summary>
        public event Action<UserId> MemberAttributesUpdated;

        public void Register(Action<UserId> memberAttributesUpdated){
            MemberAttributesUpdated += memberAttributesUpdated;
        }
        internal void Clear(){
            MemberAttributesUpdated = null;
        }
        internal void OnMemberAttributesUpdated(UserId target){
            MemberAttributesUpdated?.Invoke(target);
        }
    }
}
