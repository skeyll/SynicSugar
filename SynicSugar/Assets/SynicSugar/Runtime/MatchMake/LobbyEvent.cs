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
    #region Event
        public event Action EnableStart;
        public event Action EnableCloseAndDisableStart;
    #endregion
    #region Text
        /// <summary>
        /// Until join or create lobby.<br />
        /// Stop to push StartMatchMake button.
        /// </summary>
        public string StartMatchmaking;
        /// <summary>
        /// Wait for opponents in Lobby until close Matchmake.<br />
        /// Can kick member and cancel matchmake.
        /// </summary>
        public string WaitForOpponents;
        /// <summary>
        /// After filld lobby, start to exchange data for p2p.<br />
        /// Stop to kick and cancel.
        /// </summary>
        public string CloseMatchmaking;
        /// <summary>
        /// Can start p2p and game.
        /// </summary>
        public string ReadyForConnection;
        /// <summary>
        /// Leave or destroy lobby.<br />
        /// Stop to kick and cancel.
        /// </summary>
        public string TryToCancel;
    #endregion
        internal enum State {
            Standby, Start, Wait, Close, Ready, Cancel
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
                case State.Close:
                    stateText.text = CloseMatchmaking;
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
