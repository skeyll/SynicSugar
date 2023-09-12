using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;

namespace SynicSugar.Samples{
    public class LobbyMemberState : MonoBehaviour {
        public string State { get; private set; }
        [SerializeField] Text text;
        public Button Kick;
        //SynicSugar caches all UserIDs during one-match for zelo-allocate in runtime. For safety, the cache can only be created from EOS unique UserId.
        //We get UserId by UserId.GetUserId(userId) that returns the UserId if it exists in the cache, or null if not.
        public void SetData(UserId id, string name, string userlevel){
            State = $"{name} : {userlevel}";
            text.text = State;

            if(MatchMakeManager.Instance.isHost && !MatchMakeManager.Instance.isLocalUserId(id)){
                Kick.gameObject.SetActive(true);
                Kick.onClick.AddListener(() => MatchMakeManager.Instance.KickTargetFromLobby(id).Forget());
            }
        }
    }
}
