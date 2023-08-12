using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.P2P;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;
using NATTypeE = Epic.OnlineServices.P2P.NATType;
namespace SynicSugar.P2P {
    internal class p2pInfoMethod {
        internal P2PInterface P2PHandle;

        internal void Init(){
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
            QueryNATType().Forget();
        }

        bool gettingNATType;
        // MEMO: Maybe, this has bug now. In fact, the packets are discarded until about 2 secs after this.
        /// <summary>
        /// For initial connection. But, after 10 sec, always end.
        /// </summary>
        /// <returns></returns>
        static async internal UniTask WaitConnectPreparation(){
            await UniTask.WhenAny(UniTask.WaitUntil(() => p2pInfo.Instance.ConnectionNotifier.completeConnectPreparetion), UniTask.Delay(10000));

            #if SYNICSUGAR_LOG
                Debug.Log("All connections is ready.");
            #endif
            if(!p2pConfig.Instance.UseDisconnectedEarlyNotify){
                p2pConnectorForOtherAssembly.Instance.RemoveNotifyPeerConnectionnEstablished();
            }
        }
        /// <summary>
        /// For initial connection. After 10 sec, make it false.
        /// </summary>
        /// <returns></returns>
        static async internal UniTask DisableDelayedDeliveryAfterElapsed(){
            await UniTask.Delay(10000);
            p2pConfig.Instance.AllowDelayedDelivery = false;
        }
        /// <summary>
        /// Query the current NAT-type of our connection.
        /// </summary>
        async internal UniTask QueryNATType(){
            if(gettingNATType){
                return;
            }
            var options = new QueryNATTypeOptions();
            gettingNATType = true;
            P2PHandle.QueryNATType(ref options, null, OnQueryNATTypeCompleteCallback);

            await UniTask.WaitUntil(() => gettingNATType);
        }
        void OnQueryNATTypeCompleteCallback (ref OnQueryNATTypeCompleteInfo data){
            gettingNATType = false;
            if (data.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("OnQueryNATTypeCompleteCallback: QueryNATType is failed. error: {0}", data.ResultCode);
                return;
            }
        }
        /// <summary>
        /// Get our last-queried NAT-type, if it has been successfully queried.
        /// </summary>
        internal NATType GetNATType(){
            var options = new GetNATTypeOptions();
            ResultE result = P2PHandle.GetNATType(ref options, out NATTypeE natType);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("GetNatType: error while retrieving NAT Type: {0}", result);
                return NATType.Unknown;
            }

            return result is ResultE.NotFound ?  NATType.Unknown : (NATType)natType;
        }

    }   
}