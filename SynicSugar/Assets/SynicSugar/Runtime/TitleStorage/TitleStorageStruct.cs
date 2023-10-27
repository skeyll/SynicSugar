using System;
namespace SynicSugar.TitleStorage {
    public class TransferInProgress {
        public string FileName;
        public bool isDownload;
        public byte[] Data;
        /// <summary>
        /// To copy
        /// </summary>
        public uint CurrentIndex;
        public uint TotalFileSize;
        public void Init(){
            FileName = string.Empty;
            isDownload = true;
            TotalFileSize = 0;
            Data = null;
        }
        public void Init(string fileName){
            FileName = fileName;
            isDownload = true;
            TotalFileSize = 0;
        }
        /// <summary>
        /// Call on first read.
        /// </summary>
        /// <param name="DataSize"></param>
        public void InitData(uint DataSize){
            Data = new byte[DataSize];
        }
        public bool Done(){
            return CurrentIndex == TotalFileSize;
        }
    }
}