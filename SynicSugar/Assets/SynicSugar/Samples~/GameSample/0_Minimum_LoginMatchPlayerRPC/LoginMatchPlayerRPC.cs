using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar;
using SynicSugar.Auth;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using SynicSugar.Samples;

namespace SynicSugarSample.Minimum {
    //This is a ｐractical  minimal configuration.
    //Create a NetworkPlayer via NetworkCommons and from there send a further creation completion message.
    [NetworkCommons]
    public partial class LoginMatchPlayerRPC : MonoBehaviour
    {
        async UniTaskVoid Start()
        {
            //Login
            var result = await SynicSugarAuthentication.Login();
        
            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("Failur: Login");
                return;
            }
            SynicSugarDebug.Instance.Log("Success: Login");

            //Matchmaking
            string[] bucket = new string[1]{"MinimumB"};
            Lobby lobby = MatchMakeManager.GenerateLobbyObject(bucket, 2, false);
            //Add notify for matchmaking
            RegisterMatchMakingNotify();

            result = await MatchMakeManager.Instance.SearchAndCreateLobby(lobby);

            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("Failur: Matchmaking");
                return;
            }
            SynicSugarDebug.Instance.Log("Success: Matchmaking");

            //P2P

            //Registering the instance
            //　Register Commons　to generate Player via here.
            ConnectHub.Instance.RegisterInstance(this);

            // Receiving the packet
            result = ConnectHub.Instance.StartPacketReceiver();

            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("Failur: Start PacketReciver");
                return;
            }
            SynicSugarDebug.Instance.Log("Success: Start PacketReciver");

            // Sending an RPC
            CreatePlayerObject(p2pInfo.Instance.LocalUserId);
            
            // If Helloworld is displayed on the other client, it is successful.
        }
        /// <summary>
        /// It is difficult to know when to start matching, so we will only add a notification for after the room is set up.
        /// </summary>
        private void RegisterMatchMakingNotify(){
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += () => SynicSugarDebug.Instance.Log($"Waiting for opponents");
        }
        [Rpc]
        public void CreatePlayerObject(UserId id)
        {
            SynicSugarDebug.Instance.Log($"Created MinimumPlayer with {id}");
            //Send Rpc from MinimumPlayer's constructer.
            new MinimumPlayer(id);
        }
    }
}