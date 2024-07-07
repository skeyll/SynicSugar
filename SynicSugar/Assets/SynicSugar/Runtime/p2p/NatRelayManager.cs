using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.P2P;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;
using NATTypeE = Epic.OnlineServices.P2P.NATType;
namespace SynicSugar.P2P {
    internal sealed class NatRelayManager {
        internal P2PInterface P2PHandle;

        internal async UniTask Init(){
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
            await QueryNATType();
        }
        /// <summary>
        /// Set how relay servers are to be used. This setting does not immediately apply to existing connections, but may apply to existing connections if the connection requires renegotiation.<br /> 
        /// AllowRelay is default. In default, if the connection can be made via p2p, users connect directly; if it fails NAT Punch through, users use Relay(AWS) for the connection.<br />
        /// If it is set to anything other than AllowRelays, SetRelayControl is automatically called before the first connection. If SetRelayControl() is called after the connection, connection will switch between via Relay and p2p when the connect is not stable, so it is better to change this value in the editor or just before or after matching starts.
        /// </summary>
        /// <param name="relay">Default is AllowRelay</param>
        internal void SetRelayControl(RelayControl relay){
            SetRelayControlOptions options = new SetRelayControlOptions() { RelayControl = (Epic.OnlineServices.P2P.RelayControl)relay };
            var result = P2PHandle.SetRelayControl(ref options);
            if (result != ResultE.Success) {
                Debug.LogErrorFormat("SetRelayControl: Set Relay Control is failed. error: {0}", result);
                return;
            }
            p2pConfig.Instance.relayControl = relay;
            #if SYNICSUGAR_LOG
                Debug.Log($"SetRelayControl: SetRelayControl is Success. {result}");
            #endif
        }
        bool gettingNATType;
        /// <summary>
        /// Query the current NAT-type of our connection.
        /// </summary>
        internal async UniTask QueryNATType(){
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