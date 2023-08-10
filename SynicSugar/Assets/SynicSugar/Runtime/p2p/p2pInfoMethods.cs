using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace SynicSugar.P2P {
    public static class p2pInfoMethods {

        /// <summary>
        /// For initial connection. But, after 10 sec, always end.
        /// </summary>
        /// <returns></returns>
        static async internal UniTask WaitConnectPreparation(){
            await UniTask.WhenAny(UniTask.WaitUntil(() => !p2pInfo.Instance.ConnectionNotifier.completeConnectPreparetion), UniTask.Delay(10000));

            #if SYNICSUGAR_LOG
                Debug.Log("All connections ready.");
            #endif
            if(!p2pConfig.Instance.UseDisconnectedEarlyNotify){
                p2pConnectorForOtherAssembly.Instance.RemoveNotifyPeerConnectionnEstablished();
            }
        }
    }   
}