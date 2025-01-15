using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar;
using SynicSugar.Auth;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using SynicSugar.Samples;

namespace SynicSugarSample.Minimum {
    //This is the minimum configuration until sending RPC.
    [NetworkCommons]
    public partial class LoginMatchRPC : MonoBehaviour
    {
        async UniTaskVoid Start()
        {
            //Login
            // In SynicSugar, the default login method is anonymous, using the DeviceID as a unique anonymous ID.
            var result = await SynicSugarAuthentication.Login();
        
            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("Failur: Login");
                return;
            }
            SynicSugarDebug.Instance.Log("Success: Login");

            //Matchmaking
            //Create the condition for matchmaking
            // An object is created by specifying conditions such as region and mode (referred to as a "bucket") that allow for faster searches. Additional attributes can be specified later.
            // The number of participants and whether voice chat is enabled are also specified here. By default, it's set to two participants without voice chat.
            string[] bucket = new string[1]{"MinimumA"};
            Lobby lobby = MatchMakeManager.GenerateLobbyObject(bucket);
            // The API provides methods for matching regardless of host or guest, as well as methods for search-only or create-only.
            // In this case, we will use the method that searches first, and if no room is found, it will create one.
            // Please note that there is about a 3-second lag between calling the API and the server reflecting the changes.
            result = await MatchMakeManager.Instance.SearchAndCreateLobby(lobby);

            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("Failur: Matchmaking");
                return;
            }
            SynicSugarDebug.Instance.Log("Success: Matchmaking");

            // P2P
            // By this point, the connection is already established.
            // However, it's not possible to send/receive anything freely. You need to configure either NetworkPlayer or NetworkCommons.
            // In this case, we'll use NetworkCommons, which is designed for transmitting system-related data.
            // Make sure to add [NetworkCommons] and make the class a public partial class.

            // Registering the instance
            // When you register NetworkCommons with ConnectHub, it will invoked any remotely executed processes from here.
            // NetworkPlayer is registered internally when the UserID is set on the object.
            ConnectHub.Instance.RegisterInstance(this);

            // Receiving the packet
            // After preparing the object that will process the received packet, start packet reciver.
            // If the packet is received first, an error will occur because there is no object to process the received packet.
            result = ConnectHub.Instance.StartPacketReceiver();

            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("Failur: Start PacketReciver");
                return;
            }
            SynicSugarDebug.Instance.Log("Success: Start PacketReciver");

            // Sending an RPC
            // By adding [Rpc] to the method you want to synchronize, it will be executed remotely.
            // Synchronization is possible up to the first argument. From the second argument or , you can set these with default values, but they will not be sent.
            // You can send any type that is supported by MemoryPack, and the maximum size for a standard packet (not a LargePacket) is 1170 bytes.
            SendHelloWorld(p2pInfo.Instance.LocalUserId);
            
            // If Helloworld is displayed on the other client, it is successful.
        }

        [Rpc]
        public void SendHelloWorld(UserId id)
        {
            SynicSugarDebug.Instance.Log($"Hello World from {id} / This is LocalId: {p2pInfo.Instance.IsLoaclUser(id)}");
        }
    }
}