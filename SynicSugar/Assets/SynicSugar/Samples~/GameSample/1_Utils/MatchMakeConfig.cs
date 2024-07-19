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
        public static List<AttributeData> GenerateUserAttribute(){
            //We can set max 100 attributes.
            List<AttributeData> attributeData = new();
            //Name
            AttributeData attribute = new (){
                Key = "NAME"
            };
            string Name = GetRandomString();
            attribute.SetValue(Name);
            attributeData.Add(attribute);
            //Rank
            attribute = new (){
                Key = "LEVEL"
            };
            int Level = UnityEngine.Random.Range(0, 31);
            attribute.SetValue(Level);
            attributeData.Add(attribute);
            
            EOSDebug.Instance.Log($"UserName: {Name.ToString()} / Level: {Level}");

            return attributeData;
            
            string GetRandomString(){
                var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string name = System.String.Empty;
                var random = new System.Random();

                for (int i = 0; i < 6; i++){
                    name += sample[random.Next(sample.Length)];
                }
                return name;
            }
        }
    }
}
