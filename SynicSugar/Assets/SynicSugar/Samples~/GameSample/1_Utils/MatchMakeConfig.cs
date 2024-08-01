using SynicSugar.MatchMake;
using System.Collections.Generic;

namespace  SynicSugar.Samples {
    public class MatchMakeConfig {
        public enum Langugage{
            EN, JA
        }
        public static MatchMakingGUIEvents SetMatchingText(Langugage langugage){
            MatchMakingGUIEvents descriptions = new();
            switch(langugage){
                case Langugage.JA:
                    descriptions.StartMatchmaking = "マッチングを検索中";
                    descriptions.WaitForOpponents = "対戦相手を探しています";
                    descriptions.FinishMatchmaking = "接続準備中・・・";
                    descriptions.ReadyForConnection = "接続準備完了";
                    descriptions.TryToCancel = "マッチングキャンセル中・・・";
                    descriptions.StartReconnection = "再接続しています";
                break;
                default:
                    descriptions.StartMatchmaking = "Searching for lobby.";
                    descriptions.WaitForOpponents = "Waiting for opponents...";
                    descriptions.FinishMatchmaking = "Preparetion for Connection";
                    descriptions.ReadyForConnection = "Finish Matchmaking";
                    descriptions.TryToCancel = "Try to Disconnect...";
                    descriptions.StartReconnection = "Try to reconnection...";
                break;
            }
            return descriptions;
        }
    }
}
