using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;
namespace SynicSugar.Samples.Tank {
    [NetworkCommons(true)]
    public partial class TankRoundTimer {
        int MaxRoundTime = 120;
        float reamingTime;
        [Synic(0)] public uint startTimestamp;
        Text timerText;
        CancellationTokenSource timerTokenSource;
        
        public TankRoundTimer(int roundTime, Text text){
            ConnectHub.Instance.RegisterInstance(this);
            MaxRoundTime = roundTime;
            timerText = text;
        }
        /// <summary>
        /// Reset timestamp value before set value for this round.
        /// </summary>
        internal void ResetTimestamp(){
            startTimestamp = 0;
        }
        /// <summary>
        /// Host decide this value
        /// </summary>
        /// <param name="startTimeStampOfThisRound">Host's timestamp + some room for lag</param>
        [Rpc]
        public void SetTimestamp(uint startTimeStampOfThisRound){
            startTimestamp = startTimeStampOfThisRound;
        }
        /// <summary>
        /// Calculate reamingtime by startTimestamp and SesstionTimeStamp.
        /// </summary>
        /// <returns>The time until starting this round</returns>
        internal uint SetTimer(){
            uint currentTimeStamp = p2pInfo.Instance.GetSessionTimestamp();
            //If there is still time to start, set MaxTime and return the waiting time.
            if(currentTimeStamp < startTimestamp){
                reamingTime = MaxRoundTime;
                return startTimestamp - currentTimeStamp;
            }

            reamingTime = startTimestamp + 120 - currentTimeStamp;
            return 0;
        }
        /// <summary>
        /// Count Round time
        /// </summary>
        /// <returns></returns>
        internal async UniTask StartTimer(){
            timerTokenSource = new CancellationTokenSource();
            try {
                await CountTimer(timerTokenSource.Token);
            } catch (OperationCanceledException) {
                reamingTime = 0f;
            } 
        }
        async UniTask CountTimer(CancellationToken token){
            int currentTime = (int)reamingTime;
            while(reamingTime > 0f){
                reamingTime -= Time.deltaTime;

                if((int)reamingTime < currentTime){
                    currentTime = (int)reamingTime;
                    timerText.text = currentTime.ToString();
                }

                await UniTask.Yield(token);
            }
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
        internal float GetCurrentTime(){
            return reamingTime;
        }
    }
}