using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;
namespace SynicSugar.Samples.Tank {
    [NetworkCommons(true)]
    public partial class TankRoundTimer {
        int MaxRoundTime = 120;
        [Synic] public float reamingTime;
        Text timerText;
        CancellationTokenSource timerTokenSource;
        
        public TankRoundTimer(int roundTime, Text text){
            ConnectHub.Instance.RegisterInstance(this);
            MaxRoundTime = roundTime;
            timerText = text;
        }
        internal void SetTimer(){
            reamingTime = MaxRoundTime;
        }
        /// <summary>
        /// Count Round time
        /// </summary>
        /// <returns></returns>
        internal async UniTask StartTimer(){
            timerTokenSource = new CancellationTokenSource();
            await CountTimer(timerTokenSource.Token);
        }
        async UniTask CountTimer(CancellationToken token){
            while(!token.IsCancellationRequested && reamingTime > 0f){
                reamingTime -= Time.deltaTime;
                timerText.text = ((int)reamingTime).ToString();

                await UniTask.Yield(token);
            }
            reamingTime = 0f;
        }
        /// <summary>
        /// All local will probably finish at the same time, but just in case,　Host stop the timer manually.
        /// </summary>
        [Rpc]
        public void StopTimer(){
            if(timerTokenSource != null && timerTokenSource.Token.CanBeCanceled){
                timerTokenSource.Cancel();
            }
        }
        /// <summary>
        /// Use as Reconnecter flag.
        /// </summary>
        /// <returns></returns>
        internal bool RemainingIsMax(){
            return (int)reamingTime == MaxRoundTime;
        }
        internal float GetCurrentTime(){
            return reamingTime;
        }
    }
}