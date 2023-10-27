#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using Epic.OnlineServices.TitleStorage;

namespace SynicSugar.TitleStorage {
    public class TransferProgressEvent {
        /// <summary>
        /// Display Progress on GUI.
        /// </summary>
        public Action<string, float> InProgress;
        public string CurrentFileName { get; internal set; } = string.Empty;
        internal TitleStorageFileTransferRequest CurrentHandle = null;
        
        public void Register(Action<string, float> inProgress){
            InProgress += inProgress;
        }
        public void Clear(){
            InProgress = null;
        }
        internal void InProgressing(float progress){
            InProgress?.Invoke(CurrentFileName, progress);
        }
    }
}
