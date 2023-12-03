using SynicSugar.P2P;
using UnityEngine.UI;
using UnityEngine;
using MemoryPack;

namespace SynicSugar.Samples {
    [NetworkPlayer(true)]
    public partial class Player : MonoBehaviour {
        public Text hpText, damageText;

        public Status status = new Status();
        [SerializeField] InputField chatName, chatContent;
        int _currentTurn;
        public int currentTurn { 
            get { return _currentTurn; }
            private set {
                _currentTurn = value;

                ConnectHub.Instance.GetInstance<BattleSystem>().currentTurn++;
            }
        }

        public void InitPlayerStatus(){
            _currentTurn = 0;

            status.CurrentHP = 10;
            status.AttackDamage = 0;

            hpText.text = status.CurrentHP.ToString();
        }
        
        public void DecideDamage(){
            if(currentTurn >= ConnectHub.Instance.GetInstance<BattleSystem>().currentTurn){
                return;
            }
            damageText.text = ConnectHub.Instance.GetInstance<BattleSystem>().sliderValue.text;
            ReflectDamage(p2pInfo.Instance.CurrentRemoteUserIds[0] ,int.Parse(damageText.text));
        }
        [TargetRpc]
        public void ReflectDamage(UserId id, int damage){
            status.AttackDamage = damage;
            currentTurn++;
        }
        public void DecideChat(){
            UpdateChat(p2pInfo.Instance.CurrentRemoteUserIds[0], new ChatContent(){ Name = chatName.text, Contents = chatContent.text });
        }
        [TargetRpc]
        public void UpdateChat(UserId id, ChatContent contents){
            string tmp = isLocal ? $"★{contents.Name}:" : $"☆{contents.Name}:";
            tmp += $" {contents.Contents}{System.Environment.NewLine}";
            ConnectHub.Instance.GetInstance<BattleSystem>().chatText.text += tmp;
        }
    }
    [MemoryPackable]
    public partial class ChatContent {
        public string Name;
        public string Contents;
    }
}