#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using SynicSugar.P2P;
using UnityEngine.UI;

namespace SynicSugar.MatchMake {
    public class MemberUpdatedNotifier {
        /// <summary>
        /// Invoke when a user attributes is updated in current lobby.</ br>
        /// </summary>
        public event Action<UserId> OnAttributesUpdated;

        public void Register(Action<UserId> attributesUpdated){
            OnAttributesUpdated += attributesUpdated;
        }
        internal void Clear(){
            OnAttributesUpdated = null;
        }
        internal void MemberAttributesUpdated(UserId target){
            OnAttributesUpdated?.Invoke(target);
        }
    }
    public class MatchMakingGUIEvents {
        public MatchMakingGUIEvents(){}
        public MatchMakingGUIEvents(Text displayedStateText){
            stateText = displayedStateText;
        }
        /// <summary>
        /// To display text state.
        /// </summary>
        public Text stateText;
        public bool canKick { get; internal set; }
    #region Event
        /// <summary>
        /// After push start button, prevent to be re-pressed matchmaking button.
        /// </summary>
        public event Action DisableStart;
        /// <summary>
        /// After join and create lobby, accept cancel matchmake and host's kick member.
        /// </summary>
        public event Action EnableCancelKick;
        /// <summary>
        /// This is enabled when the required members are passed to the Matchmaking API.<br />
        /// After filled in the minimum number of members, can manually close Lobby to start p2p.
        /// </summary>
        public event Action EnableManualFinish;
        /// <summary>
        /// After complete or cancel matchmaking, prevent to change lobby state.
        /// </summary>
        public event Action DisableCancelKickFinish;
    #endregion
    #region Text
        /// <summary>
        /// Until join or create lobby.<br />
        /// Invoke DisableStart().
        /// </summary>
        public string StartMatchmaking;
        /// <summary>
        /// Wait for opponents in Lobby until finish Matchmake.<br />
        /// Invoke EnableCancelKick(). canKick becomes true.
        /// </summary>
        public string WaitForOpponents;
        /// <summary>
        /// After Filled lobby or close the matchmaking in manually, start to exchange data for p2p.<br />
        /// Invoke DisableCancelKickFinish().
        /// </summary>
        public string FinishMatchmaking;
        /// <summary>
        /// Can start p2p and game.<br />
        /// (Move to GameScene or that UI mode.)
        /// </summary>
        public string ReadyForConnection;
        /// <summary>
        /// Leave or destroy lobby.<br />
        /// Invoke DisableCancelKickFinish().
        /// </summary>
        public string TryToCancel;
    #endregion
        internal enum State {
            Standby, Start, Wait, Finish, Ready, Cancel
        }
        internal void ChangeState(State state){
            if(stateText == null){
                return;
            }
            switch(state){
                case State.Standby:
                    stateText.text = System.String.Empty;
                break;
                case State.Start:
                    stateText.text = StartMatchmaking;
                break;
                case State.Wait:
                    stateText.text = WaitForOpponents;
                break;
                case State.Finish:
                    stateText.text = FinishMatchmaking;
                break;
                case State.Ready:
                    stateText.text = ReadyForConnection;
                break;
                case State.Cancel:
                    stateText.text = TryToCancel;
                break;
            }
        }
    }
}
