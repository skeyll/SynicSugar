using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
using Cysharp.Threading.Tasks;

namespace SynicSugar.Samples.Tank 
{
    public class TankLobbyMemberState : MonoBehaviour 
    {
        public string State { get; private set; }
        [SerializeField] private Text text;
        public Button Kick;
        //SynicSugar caches all UserIDs during one-match for zelo-allocate in runtime. For safety, the cache can only be created from EOS unique UserId.
        //We get UserId by UserId.GetUserId(userId) that returns the UserId if it exists in the cache, or null if not.
        internal void SetData(UserId id, string name, string userlevel)
        {
            State = $"{name} : {userlevel}";
            text.text = State;
            //Can't kick self.
            if(MatchMakeManager.Instance.isLocalUserId(id)) return;
            
            Kick.onClick.AddListener(() => MatchMakeManager.Instance.KickTargetFromLobby(id).Forget());

            if(MatchMakeManager.Instance.isHost)
            {
                Kick.gameObject.SetActive(true);
            }
        }
        internal void SwitchKickButtonActive(bool isActivate)
        {
            Kick.gameObject.SetActive(isActivate);
        }
    }
}