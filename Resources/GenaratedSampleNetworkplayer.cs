
namespace YOURNAMESPACE {
    public partial class GenaratedSampleNetworkplayer : INetworkOwner, IGetPlayer {
        //For Basis
        UserId _ownerUserID;
        public UserId OwnerUserID {
            get { return _ownerUserID; }
            set {
                _ownerUserID = UserId.GetUserId(value);
                ConnectHub.Instance.RegisterInstance(_ownerUserID, this);
            }
        }
        public void SetOwnerID(UserId value){
            OwnerUserID = value;
        }
        /// <summary>
        /// Is this the instance's local? Invalid in Awake. 
        /// </summary>
        public bool isLocal { get { return p2pInfo.Instance.IsLoaclUser(_ownerUserID); } }
        
        /// <summary>
        /// Is this the id's instance? Invalid in Awake. 
        /// </summary>
        public bool ThisOwnerIs(UserId id){
            return id == _ownerUserID;
        }
        //---For Sync var---
        //These have not been changed since the beginning.
        //So performance is not good

        //For private SyncVar
        // internal void SetLocalYOURSYNCVAR(YOURSYNCVARARG value) {
        //     YOURSYNCVAR = value;
        // }
        bool isWaitingYOURSYNCVARInterval;
        void StartSynicYOURSYNCVAR() {
            if(isWaitingYOURSYNCVARInterval){
                return;
            }
            isWaitingYOURSYNCVARInterval = true;
            SyniYOURSYNCVAR().Forget();
        }

        async UniTask SyniYOURSYNCVAR(){
            var preValue = YOURSYNCVAR;

            EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.YOURSYNCVAR, MemoryPack.MemoryPackSerializer.Serialize(YOURSYNCVAR)).Forget();
            await UniTask.Delay(IntervalTime.OnConfig);
            
            if(p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){
                return;
            }
            if(preValue == YOURSYNCVAR){
                isWaitingYOURSYNCVARInterval = false;
                return;
            }
            SynicYOURSYNCVAR().Forget();
        }

        //---For RPC---
        //This could be changed to a method of creating and inserting this process-self in IL in future.
        //Compromise because it was complicated.
        //This function is inserted at the top of the process with the RPC attribute
        void SynicSugarRpc_YOURRPC(YOURPRCARG value) {
            if(isLocal){
                EOSp2p.SendLargePacketsToAll((byte)ConnectHub.CHANNELLIST.YOURRPC, MemoryPack.MemoryPackSerializer.Serialize(value)).Forget();
            }
        }
    }
}

