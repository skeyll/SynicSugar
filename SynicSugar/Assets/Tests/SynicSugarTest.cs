using SynicSugar.P2P;
using UnityEngine;
using MemoryPack;
namespace SynicSugar.Test {
    [NetworkPlayer]
    public partial class SynicSugarTest : MonoBehaviour {
        public int noSyncData;
        [SyncVar]
        public int intTest;
        [SyncVar]
        public TestDataClass testData;
        public void NoRpcMethod(){
            noSyncData += 1;
            noSyncData += 1;

            intTest += 1;
            intTest += 1;
        }
        [Rpc]
        public void TestRpc(TestDataClass testData){
            noSyncData += 1;
            noSyncData += 1;

            intTest += 1;

            intTest += 1;

            intTest = 53251;
        }

        [TargetRpc]
        public void TestTargetRpc(UserId id){
            noSyncData += 1;
            noSyncData += 1;

            intTest += 1;
            intTest += 1;
        }
    }

    [MemoryPackable]
    public partial class TestDataClass{
        public int intA;
        public int intB;
    }
}
