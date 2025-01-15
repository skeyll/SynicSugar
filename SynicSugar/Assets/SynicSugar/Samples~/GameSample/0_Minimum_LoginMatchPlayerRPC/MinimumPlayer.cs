using UnityEngine;
using SynicSugar;
using SynicSugar.P2P;
using SynicSugar.Samples;

namespace SynicSugarSample.Minimum {
    [NetworkPlayer]
    public partial class MinimumPlayer
    {
        public MinimumPlayer(UserId id){
            //Need set UserId.
            //If generated via SynicObject, this process is done automatically.
            SetOwnerID(id);

            //Send RPC
            int RandomValue = Random.Range(0, 100);
            int RandomValue2 = Random.Range(0, 100);
            //This process is called three times: twice when created locally, and once when called as an RPC from a remote source.
            //Processe called by a MinimumPlayer without its own ID are not sent to the opponent!
            //If you want to prevent this process from being called by other users' instances during creation, add `if(isLocal)`.
            HelloworldFromPlayer(RandomValue, RandomValue2, true);
        }
        
        [Rpc]
        public void HelloworldFromPlayer(int randomInt, int randomInt2 = 0, bool isLocal = false){
            SynicSugarDebug.Instance.Log($"Hello World from {OwnerUserID} / {randomInt} / {randomInt2} / This process is called locally {isLocal}");
        }

    }
}