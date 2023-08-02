using SynicSugar.MatchMake;
using UnityEngine;

namespace  SynicSugar.Samples {
    public class MatchMakeConfig : MonoBehaviour
    {
        public enum Langugage{
            EN, JA
        }
        public static MatchGUIState SetMatchingText(Langugage langugage){
            MatchGUIState descriptions = new MatchGUIState();
            switch(langugage){
                case Langugage.JA:
                    descriptions.searchLobby = "対戦相手検索中・・・";
                    descriptions.waitothers = "対戦相手を探しています";
                    descriptions.tryconnect = "接続中・・・";
                    descriptions.success = "成功！";
                    descriptions.fail = "失敗！";
                    descriptions.trycancel = "マッチングキャンセル中・・・";
                break;
                default:
                    descriptions.searchLobby = "Searching for an opponent...";
                    descriptions.waitothers = "Waiting for an opponent...";
                    descriptions.tryconnect = "Try to connect...";
                    descriptions.success = "Success MatchMaking";
                    descriptions.fail = "Fail to match make";
                    descriptions.trycancel = "Try to Disconnect...";
                break;
            }
            return descriptions;
        }
    }
}
