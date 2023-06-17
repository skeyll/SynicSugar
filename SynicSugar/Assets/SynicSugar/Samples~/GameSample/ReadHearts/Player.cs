using SynicSugar.P2P;
using SynicSugar.MatchMake;
using UnityEngine.UI;
using UnityEngine;
using MemoryPack;

namespace SynicSugar.Samples {
    [NetworkPlayer]
    public partial class Player : MonoBehaviour {
        public Text hpText, damageText;

        public Status status = new Status();
        [SerializeField] bool isLocalPlayer;
        [SerializeField] InputField chatName, chatContent;
        int _currentTurn;
        public int currentTurn { 
            get { return _currentTurn; }
            private set {
                _currentTurn = value;

                BattleSystem.Instance.currentTurn++;
            }
        }
        void Start(){
            //API's "isLocal" is enabled after the ID is set to the instance.
            if(isLocalPlayer){ 
                SetOwnerID(p2pManager.Instance.userIds.LocalUserId);
                BattleSystem.Instance.player = this;
            }else{
                SetOwnerID(p2pManager.Instance.userIds.RemoteUserIds[0]);
                BattleSystem.Instance.opponent = this;
            }
        }

        public void InitPlayerStatus(){
            _currentTurn = 0;

            status.CurrentHP = 10;
            status.AttackDamage = 0;

            hpText.text = status.CurrentHP.ToString();
        }
        
        public void DecideDamage(){
            if(currentTurn >= BattleSystem.Instance.currentTurn){
                return;
            }
            damageText.text = BattleSystem.Instance.sliderValue.text;
            ReflectDamage(p2pManager.Instance.userIds.RemoteUserIds[0] ,int.Parse(damageText.text));
        }
        [TargetRpc]
        public void ReflectDamage(UserId id, int damage){
            status.AttackDamage = damage;
            currentTurn++;
        }

        public void DecideChat(){
            UpdateChat(p2pManager.Instance.userIds.RemoteUserIds[0], new ChatContent(){ Name = chatName.text, Contents = chatContent.text });
        }
        [TargetRpc]
        public void UpdateChat(UserId id, ChatContent contents){
            string tmp = isLocalPlayer ? $"★{contents.Name}:" : $"☆{contents.Name}:";
            tmp += $" {contents.Contents}{System.Environment.NewLine}";
            BattleSystem.Instance.chatText.text = BattleSystem.Instance.chatText.text + tmp;
        }
    }
    [MemoryPackable]
    public partial class ChatContent {
        public string Name;
        public string Contents;
    }
}