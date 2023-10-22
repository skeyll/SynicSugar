using Epic.OnlineServices;
using Epic.OnlineServices.TitleStorage;
using PlayEveryWare.EpicOnlineServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace SynicSugar.TitleStorage {
    public class EOSTitleStorage : MonoBehaviour {
        const uint MAXCHUNKLENGTH = 4 * 4 * 4096;
        public TransferProgressEvent progressInfo = new();
        #region Query
        
        //FileName, FileSizeBytes
        Dictionary<string, uint> FileMetaDatas = new ();

        /// <summary>
        /// Query the file List from backend. Hold FileSizeBytes to read file.
        /// When we know the filename to want to get, can also call ReadFile not to call this.
        /// </summary>
        /// <param name="tags"></param>
        public async UniTask<List<string>> QueryFileList(string[] tags, CancellationToken token = default(CancellationToken)){
            Utf8String[] utf8StringTags = new Utf8String[tags.Length];

            for (int i = 0; i < tags.Length; ++i){
                utf8StringTags[i] = tags[i];
            }
            await QueryFileList(utf8StringTags);
            
            return FileMetaDatas.Keys.ToList();
        }
        /// <summary>
        /// Query the file List from backend. Hold FileSizeBytes to read file.
        /// When we know the filename to want to get, can also call ReadFile not to call this.
        /// </summary>
        /// <param name="tags"></param>
        public async UniTask<List<string>> QueryFileList(List<string> tags, CancellationToken token = default(CancellationToken)){
            Utf8String[] utf8StringTags = new Utf8String[tags.Count];

            for (int i = 0; i < tags.Count; ++i){
                utf8StringTags[i] = tags[i];
            }
            await QueryFileList(utf8StringTags);
            return FileMetaDatas.Keys.ToList();
        }
        /// <summary>
        /// Substance for QueryFileList. Get file count, then create File namelist with the index.
        /// MEMO: Is FileMetadata's Release unnecessary in C#?
        /// </summary>
        /// <param name="tags"></param>
        async UniTask<bool> QueryFileList(Utf8String[] tags){
            QueryFileListOptions queryOptions = new QueryFileListOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                ListOfTags = tags
            };

            TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            bool finishQuery = false;
            bool canGetFileNames = false;
            titleStorageInterface.QueryFileList(ref queryOptions, null, OnQueryFileListComplete);
            await UniTask.WaitUntil(() => finishQuery);
            return canGetFileNames;

            void OnQueryFileListComplete(ref QueryFileListCallbackInfo data) {
                //Result
                canGetFileNames = data.ResultCode == ResultE.Success;

                if (!canGetFileNames){
                    Debug.LogErrorFormat("QueryFileList: Failure by {0}", data.ResultCode);
                    finishQuery = true;
                    return;
                }

                FileMetaDatas.Clear();
                //Create file list with index
                for (uint fileIndex = 0; fileIndex < data.FileCount; fileIndex++) {
                    CopyFileMetadataAtIndexOptions indexOptions = new CopyFileMetadataAtIndexOptions(){
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        Index = fileIndex
                    };

                    titleStorageInterface.CopyFileMetadataAtIndex(ref indexOptions, out FileMetadata? fileMetadata);

                    if (fileMetadata != null){
                        if (!string.IsNullOrEmpty(fileMetadata?.Filename)){
                            FileMetaDatas.Add(fileMetadata?.Filename, (uint)fileMetadata?.FileSizeBytes);
                        }
                    }
                }
                finishQuery = true;
            }
        }
        #endregion
        #region ReadFile
        
        private TitleStorageFileTransferRequest CurrentTransferRequest = null;
        Dictionary<string, TransferInProgress> TransfersInProgress = new Dictionary<string, TransferInProgress>();
        
        private Dictionary<string, string> StorageData = new Dictionary<string, string>();
        /// <summary>
        /// Get File with file name (in QueryFileList).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async UniTask ReadFile(string fileName){
            TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            //Prep
            if(!FileMetaDatas.ContainsKey(fileName)){
                //Get size
                var queryOptions = new QueryFileOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = fileName
                };

                bool finishQuery = false;
                bool findFile = false;
                titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
                await UniTask.WaitUntil(() => finishQuery);
                if(!findFile){
                    //Failure
                    return;
                }

                void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
                    findFile = data.ResultCode == ResultE.Success;
                    finishQuery = true;
                }
                var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = fileName
                };
                titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

                if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
                    //Failure
                    return;
                }
                FileMetaDatas.Add(fileMetadata?.Filename, (uint)fileMetadata?.FileSizeBytes);
            }
            //Read file
            ReadFileOptions readOptions = new ReadFileOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = fileName,
                ReadChunkLengthBytes = MAXCHUNKLENGTH,
                ReadFileDataCallback = OnFileDataReceived,
                FileTransferProgressCallback = OnFileTransferProgress
            };

            bool finishRead = false;
            bool getFile = false;
            TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
            
            CancelCurrentTransfer();
            // CurrentTransferHandle = transferReq;

            TransferInProgress newTransfer = new () {
                isDownloading = true,
                Data = new byte[FileMetaDatas[fileName]]
            };

            TransfersInProgress.Add(fileName, newTransfer);

            progressInfo.CurrentFileName = fileName;

            await UniTask.WaitUntil(() => finishRead);

            void OnReadFileComplete(ref ReadFileCallbackInfo data){
                getFile = data.ResultCode == ResultE.Success;
                if (!getFile){
                    Debug.LogErrorFormat("ReadFile: OnFileReceived is Failure. {0}", data.ResultCode);
                }
                FinishFileDownload(data.Filename, getFile, data.ResultCode);
                finishRead = true;
            }
        }
        public void FinishFileDownload(string fileName, bool success, ResultE result){
            if (!TransfersInProgress.TryGetValue(fileName, out TransferInProgress transfer)){
                Debug.LogErrorFormat("ReadFile: '{0}' was not found in TransfersInProgress.", fileName);
                return;
            }

            if (!transfer.isDownloading){
                Debug.LogError("ReadFile: Failure. Download/upload mismatch.");
                return;
            }

            if (!transfer.Done() || success){
                if (!transfer.Done()){
                    Debug.LogError("ReadFile: Failure. Not enough data for file");
                }

                TransfersInProgress.Remove(fileName);

                if (fileName == progressInfo.CurrentFileName){
                    ClearCurrentTransfer();
                }
            }

            string fileData = string.Empty;
            if (transfer.TotalSize > 0){
                fileData = System.Text.Encoding.UTF8.GetString(transfer.Data);
            }

            StorageData.Add(fileName, fileData);

            Debug.LogFormat("[EOS SDK] Title storage: file read finished: '{0}' Size: {1}.", fileName, fileData.Length);

            TransfersInProgress.Remove(fileName);

            if (fileName.Equals(progressInfo.CurrentFileName, StringComparison.OrdinalIgnoreCase)){
                ClearCurrentTransfer();
            }
        }
        
        private void CancelCurrentTransfer() {
            if (CurrentTransferRequest != null){
                ResultE cancelResult = CurrentTransferRequest.CancelRequest();

                if (cancelResult == ResultE.Success){
                    TransfersInProgress.TryGetValue(progressInfo.CurrentFileName, out TransferInProgress transfer);

                    if (transfer != null){
                        if (transfer.isDownloading) {
                            Debug.Log("ReadFile: Download is canceled");
                        }else{
                            Debug.Log("ReadFile: Upload is canceled");
                        }

                        TransfersInProgress.Remove(progressInfo.CurrentFileName);
                    }
                }
            }

            ClearCurrentTransfer();
        }
        private void ClearCurrentTransfer(){
            progressInfo.CurrentFileName = string.Empty;

            if (CurrentTransferRequest != null){
                CurrentTransferRequest = null;
            }
        }

        ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data){
            return ReceiveData(data.Filename, data.DataChunk, data.TotalFileSizeBytes);
        }
        ReadResult ReceiveData(string fileName, ArraySegment<byte> data, uint totalSize){
            if (data == null){
                Debug.LogError("ReadFile: Failure. Data pointer is null.");
                return ReadResult.RrFailrequest;
            }

            TransfersInProgress.TryGetValue(fileName, out TransferInProgress transfer);
            //Failure and Cancel
            if (transfer == null){
                return ReadResult.RrCancelrequest;
            }

            if (!transfer.isDownloading){
                Debug.LogError("ReadFile: Failure. Download/upload mismatch.");
                return ReadResult.RrFailrequest;
            }

            //First update
            if (transfer.CurrentIndex == 0 && transfer.TotalSize == 0){
                transfer.TotalSize = totalSize;

                if (transfer.TotalSize == 0){
                    return ReadResult.RrContinuereading;
                }
            }

            // Make enough room
            if (transfer.TotalSize - transfer.CurrentIndex >= data.Count){
                data.Array.CopyTo(transfer.Data, transfer.CurrentIndex);
                transfer.CurrentIndex += (uint)data.Count;

                return ReadResult.RrContinuereading;
            } else {
                Debug.LogError("ReadFile: Failure. Too much of it.");
                return ReadResult.RrFailrequest;
            }
        }
        
        private void OnFileTransferProgress (ref FileTransferProgressCallbackInfo data){
            if (data.TotalFileSizeBytes > 0){
                progressInfo.InProgressing(data.BytesTransferred / data.TotalFileSizeBytes);
            }
        }

        #endregion
    }
}
