namespace SynicSugar.TitleStorage {
    public class TransferInProgress {
        public bool isDownloading = true;
        public uint CurrentIndex = 0;
        public byte[] Data;
        uint transferSize = 0;

        public uint TotalSize{
            get { return transferSize; }
            set {
                transferSize = value;
            }
        }
        public bool Done(){
            return TotalSize == CurrentIndex;
        }
    }
}