using SynicSugar.MatchMake;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SynicSugar.Samples{
    public class MatchMakeConditions : MonoBehaviour {
#region Condition
        public enum GameMode{ Rank, Casual, Friend }
        public enum Region{ ASIA, EU, NA }
        GameMode mode;
        Region region;
        int level;
#endregion
        [SerializeField] Text matchConditions;
        void Start(){
            UpdateConditionsText();
        }

        public Lobby GetLobbyCondition(){
            //Create conditions
            Lobby lobbyCondition = MatchMakeManager.GenerateLobbyObject(new string[3]{SceneManager.GetActiveScene().name, mode.ToString(), region.ToString()});
            
            lobbyCondition.MaxLobbyMembers = 2; //2-64

            LobbyAttribute attribute = new LobbyAttribute();
            attribute.Key = "Level";
            attribute.SetValue(level);
            attribute.ComparisonOperator = Epic.OnlineServices.ComparisonOp.Equal;
            lobbyCondition.Attributes.Add(attribute);
            
            EOSDebug.Instance.Log(matchConditions.text);
            return lobbyCondition;

        }
#region MatchMakeConditions
        public void SetGameMode(int value){
            mode = (GameMode)value;
            UpdateConditionsText();
        }
        public void SetRegion(int value){
            region = (Region)value;
            UpdateConditionsText();
        }
        public void ChangeMatchingLevel(int value){
            level = value;
            UpdateConditionsText();
        }
#endregion
        void UpdateConditionsText(){
            matchConditions.text = $"Bucket {mode}:{region} // Level {level}";
        }
    }
}
