using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.P2P;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;
using NATTypeE = Epic.OnlineServices.P2P.NATType;
namespace SynicSugar.P2P {
    internal class NatRelayManager {
        internal P2PInterface P2PHandle;

        internal async UniTask Init(){
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
            await QueryNATType();
        }

        bool gettingNATType;
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

            await UniTask.WaitUntil(() => !gettingNATType);
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