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
using MemoryPack;

namespace SynicSugar.TitleStorage {
    public static class EOSTitleStorage {
        //Options
        const uint MAXCHUNKLENGTH = 4 * 4 * 4096;
        public static TransferProgressEvent ProgressInfo = new();
        //Query
        static Dictionary<string, uint> FileMetaDatas = new ();
        //Read
        static TransferInProgress CurrentTransfer = new();

        #region Query
        //FileName, FileSizeBytes
        /// <summary>
        /// Query the file List from backend. Hold FileSizeBytes to read file.
        /// When we know the filename to want to get, can also call ReadFile not to call this.
        /// </summary>
        /// <param name="tags"></param>
        public static async UniTask<List<string>> QueryFileList(string[] tags, CancellationToken token = default(CancellationToken)){
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
        public static async UniTask<List<string>> QueryFileList(List<string> tags, CancellationToken token = default(CancellationToken)){
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
        static async UniTask<bool> QueryFileList(Utf8String[] tags){
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
        #region Download
        /// <summary>
        /// Exsist target? If not, Download it from EOS server. <br />
        /// After call this, we can load the target as Resources or AssetBundle.
        /// </summary>
        /// <param name="fileName">target</param>
        /// <returns>If true, Exsist in locaal or fetch file from backend.</returns>
        public static async UniTask<bool> FetchFile(string fileName) {
            TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            //Prep.
            //If can use local data, use the query data.
            if(!FileMetaDatas.ContainsKey(fileName)){
                //Get query
                var queryOptions = new QueryFileOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = fileName
                };

                bool finishQuery = false;
                bool findFile = false;
                titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
                await UniTask.WaitUntil(() => finishQuery);
                if(!findFile){
                    Debug.LogError("ReadFile: can't query meta data.");
                    return false;
                }
                void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
                    findFile = data.ResultCode == ResultE.Success;
                    finishQuery = true;
                }
                //Get target data
                var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = fileName
                };
                titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

                if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
                    Debug.LogError("ReadFile: can't find the meta data in query.");
                    return false;
                }
                FileMetaDatas.Add(fileMetadata?.Filename, (uint)fileMetadata?.FileSizeBytes);
            }
            if(string.Compare(fileName, CurrentTransfer.FileName,true) == 0){
                Debug.LogError("ReadFile: This call is Duplicateds. Downloading it now.");
                return false;
            }
            //Need download
            //Init for new transfer
            CancelCurrentTransfer();
            ProgressInfo.CurrentFileName = fileName; 
            CurrentTransfer.Init(fileName);

            //Read file
            ReadFileOptions readOptions = new ReadFileOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = fileName,
                ReadChunkLengthBytes = MAXCHUNKLENGTH,
                ReadFileDataCallback = OnReadFileData,
                FileTransferProgressCallback = OnFileTransferProgress
            };

            bool finishRead = false;
            bool getFile = false;
            TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
            ProgressInfo.CurrentHandle = transferRequest;

            await UniTask.WaitUntil(() => finishRead);
            
            void OnReadFileComplete(ref ReadFileCallbackInfo data){
                getFile = data.ResultCode == ResultE.Success;
                if (!getFile){
                    Debug.LogErrorFormat("ReadFile: OnFileReceived is Failure. {0}", data.ResultCode);
                }
                FinishFileDownload(data.Filename, getFile);
                finishRead = true;
            }

            transferRequest.Release();
            return getFile;
        }
        //TODO: Improve Performance
        /// <summary>
        /// Exsist targets? If not, Download them from EOS server. <br />
        /// After call this, we load target as Resources or AssetBundle.
        /// </summary>
        /// <param name="fileNames">targets</param>
        /// <returns>If true, Exsist in locaal or fetch file from backend</returns>
        public static async UniTask<bool> FetchFiles(string[] fileNames) {
            bool getFile = false;
            foreach(var f in fileNames){
                getFile = await FetchFile(f);
                if(!getFile){
                    Debug.LogErrorFormat("CanReadFiles: Can't read {0}", f);
                    return false;
                }
            }
            return getFile;
        }
        static void FinishFileDownload(string fileName, bool success){
            if (string.Compare(fileName, CurrentTransfer.FileName, true) != 0){
                Debug.LogError("ReadFile: Failure.  This is a wrong download.");
                return;
            }

            if (!CurrentTransfer.isDownload){
                Debug.LogError("ReadFile: Failure. Read incorrect data.");
                return;
            }

            if (success){
                if (!CurrentTransfer.Done()){
                    Debug.LogError("ReadFile: Failure. Not enough data to read.");
                }

                if (fileName == ProgressInfo.CurrentFileName){
                    ClearCurrentTransfer();
                }
            }

        #if SYNICSUGAR_LOG
            Debug.Log("ReadFile: Finish to read " + fileName);
        #endif

            CurrentTransfer.Init();

            if (fileName.Equals(ProgressInfo.CurrentFileName, StringComparison.OrdinalIgnoreCase)){
                ClearCurrentTransfer();
            }
        }
        static void ClearCurrentTransfer(){
            ProgressInfo.CurrentFileName = string.Empty;

            if (ProgressInfo.CurrentHandle != null){
                ProgressInfo.CurrentHandle = null;
            }
        }

        static ReadResult OnReadFileData(ref ReadFileDataCallbackInfo data){
            if (data.DataChunk == null){
                Debug.LogError("ReadFile: Failure. Data pointer is null.");
                return ReadResult.RrFailrequest;
            }

            //Failure and Cancel
            if (string.Compare(data.Filename, CurrentTransfer.FileName, true) != 0){
                Debug.LogError("ReadFile: Failure. Read incorrect data.");
                return ReadResult.RrCancelrequest;
            }

            if (!CurrentTransfer.isDownload){
                Debug.LogError("ReadFile: Failure. Unintended Download.");
                return ReadResult.RrFailrequest;
            }
            //Prep data array
            if (CurrentTransfer.TotalFileSize == 0){
                CurrentTransfer.InitData(data.TotalFileSizeBytes);
                CurrentTransfer.TotalFileSize = data.TotalFileSizeBytes;
            }

            data.DataChunk.Array.CopyTo(CurrentTransfer.Data, CurrentTransfer.CurrentIndex);
            CurrentTransfer.CurrentIndex += (uint)data.DataChunk.Count;

            //The last or not
            return ReadResult.RrContinuereading;//data.IsLastChunk ? ReadResult.RrFailrequest : ReadResult.RrContinuereading;
        }
        /// <summary>
        /// Optional callback function to be informed of download progress, if the file is not already locally cached.
        /// </summary>
        /// <param name="data"></param>
        static void OnFileTransferProgress (ref FileTransferProgressCallbackInfo data){
            if (data.TotalFileSizeBytes > 0){
                ProgressInfo.InProgressing(data.BytesTransferred / data.TotalFileSizeBytes);
            }
        }
        /// <summary>
        /// Clear previously cached file data. 
        /// </summary>
        public static async UniTask<bool> DeleteCache(){
            TitleStorageInterface storageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            //Get query
            var option = new DeleteCacheOptions {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            bool finishDeleted = false;
            bool canDelete = false;
            storageInterface.DeleteCache(ref option, null, OnDeleteCacheComplete);

            await UniTask.WaitUntil(() => finishDeleted);

            void OnDeleteCacheComplete(ref DeleteCacheCallbackInfo data){
                canDelete = data.ResultCode == ResultE.Success;
                if(!canDelete){
                    Debug.LogError("DeleteCache: Failure");
                }
                finishDeleted = true;
            }
            return canDelete;
        }
        #endregion
        #region LoadFile
    #if SYNICSUGAR_ADDRESSABLE
        /// <summary>
        /// Load file with Addressable. When data exists in local, it is just loaded; if not, it is downloaded from Server and then loaded.
        /// These Resource is managed in batches, and need to call Release() on changing scene.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async UniTask<T> LoadFromAssetBundle<T>(string filePath) where T : class {
            TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            //Prep.
            //If can use local data, use the query data.
            if(!FileMetaDatas.ContainsKey(filePath)){
                //Get query
                var queryOptions = new QueryFileOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = filePath
                };

                bool finishQuery = false;
                bool findFile = false;
                titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
                await UniTask.WaitUntil(() => finishQuery);
                if(!findFile){
                    Debug.LogError("LoadFile: can't query meta data.");
                    return null;
                }
                void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
                    findFile = data.ResultCode == ResultE.Success;
                    finishQuery = true;
                }
                //Get target data
                var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = filePath
                };
                titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

                if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
                    Debug.LogError("LoadFile: can't find the meta data in query.");
                    return null;
                }
                FileMetaDatas.Add(fileMetadata?.Filename, (uint)fileMetadata?.FileSizeBytes);
            }
            if(string.Compare(filePath, CurrentTransfer.FileName,true) == 0){
                Debug.LogError("LoadFile: This call is Duplicateds. Already is loading it.");
                return null;
            }
            //Try to get
            if(AddressableHelper.Exist<T>(filePath)){
                return await AddressableHelper.LoadAddressableAsync<T>(filePath);
            }

            //Not exsit file, fetch it from EOS.
            //Init for new transfer
            CancelCurrentTransfer();
            ProgressInfo.CurrentFileName = filePath; 
            CurrentTransfer.Init(filePath);

            //Read file
            ReadFileOptions readOptions = new ReadFileOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = filePath,
                ReadChunkLengthBytes = MAXCHUNKLENGTH,
                ReadFileDataCallback = OnReadFileData,
                FileTransferProgressCallback = OnFileTransferProgress
            };

            bool finishRead = false;
            bool getFile = false;
            TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
            ProgressInfo.CurrentHandle = transferRequest;

            await UniTask.WaitUntil(() => finishRead);
            byte[] result = new byte[0];
            
            void OnReadFileComplete(ref ReadFileCallbackInfo data){
                getFile = data.ResultCode == ResultE.Success;
                if (!getFile){
                    Debug.LogErrorFormat("ReadFile: OnFileReceived is Failure. {0}", data.ResultCode);
                }
                FinishFileDownload(data.Filename, getFile);
                finishRead = true;
            }

            transferRequest.Release();
            return await AddressableHelper.LoadAddressableAsync<T>(filePath);
        }
        /// <summary>
        /// Destroy all used resources. Call before transition to next scene.
        /// </summary>
        public static void ReleaseAddressables() {
            AddressableHelper.ReleaseUsedResources();
        }
    #endif
        /// <summary>
        /// Get File with file name (in QueryFileList).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static async UniTask<T> LoadFromResources<T>(string fileName) where T : class {
            TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            //Prep.
            //If can use local data, use the query data.
            if(!FileMetaDatas.ContainsKey(fileName)){
                //Get query
                var queryOptions = new QueryFileOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = fileName
                };

                bool finishQuery = false;
                bool findFile = false;
                titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
                await UniTask.WaitUntil(() => finishQuery);
                if(!findFile){
                    Debug.LogError("LoadFile: can't query meta data.");
                    return null;
                }
                void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
                    findFile = data.ResultCode == ResultE.Success;
                    finishQuery = true;
                }
                //Get target data
                var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = fileName
                };
                titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

                if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
                    Debug.LogError("LoadFile: can't find the meta data in query.");
                    return null;
                }
                FileMetaDatas.Add(fileMetadata?.Filename, (uint)fileMetadata?.FileSizeBytes);
            }
            if(string.Compare(fileName, CurrentTransfer.FileName,true) == 0){
                Debug.LogError("LoadFile: This call is Duplicateds. Already is loading it.");
                return null;
            }

            //Init for new transfer
            CancelCurrentTransfer();
            ProgressInfo.CurrentFileName = fileName; 
            CurrentTransfer.Init(fileName);

            //Read file
            ReadFileOptions readOptions = new ReadFileOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = fileName,
                ReadChunkLengthBytes = MAXCHUNKLENGTH,
                ReadFileDataCallback = OnReadFileData,
                FileTransferProgressCallback = OnFileTransferProgress
            };

            bool finishRead = false;
            bool getFile = false;
            TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
            ProgressInfo.CurrentHandle = transferRequest;

            await UniTask.WaitUntil(() => finishRead);
            byte[] result = new byte[0];
            
            void OnReadFileComplete(ref ReadFileCallbackInfo data){
                getFile = data.ResultCode == ResultE.Success;
                if (!getFile){
                    Debug.LogErrorFormat("LoadFile: OnFileReceived is Failure. {0}", data.ResultCode);
                }
                FinishFileDownload(data.Filename, getFile, out result);
                finishRead = true;
            }

            transferRequest.Release();
            return MemoryPackSerializer.Deserialize<T>(result);
        }
        static void FinishFileDownload(string fileName, bool success, out byte[] result){
            if (string.Compare(fileName, CurrentTransfer.FileName, true) != 0){
                Debug.LogError("LoadFile: Failure.  This is a wrong download.");
                result = null;
                return;
            }

            if (!CurrentTransfer.isDownload){
                Debug.LogError("LoadFile: Failure. Read incorrect data.");
                result = null;
                return;
            }

            if (success){
                if (!CurrentTransfer.Done()){
                    Debug.LogError("LoadFile: Failure. Not enough data to read.");
                }

                if (fileName == ProgressInfo.CurrentFileName){
                    ClearCurrentTransfer();
                }
            }
            result = new byte[CurrentTransfer.Data.Length];
            Buffer.BlockCopy(CurrentTransfer.Data, 0, result, 0, result.Length);
            string test = string.Empty;
            for(int i = 1; i < 10; i++){
                test += result[CurrentTransfer.Data.Length - i];
            }

        #if SYNICSUGAR_LOG
            Debug.Log("LoadFile: Finish to read " + fileName + result.Length);
        #endif

            CurrentTransfer.Init();

            if (fileName.Equals(ProgressInfo.CurrentFileName, StringComparison.OrdinalIgnoreCase)){
                ClearCurrentTransfer();
            }
        }
        
        static void CancelCurrentTransfer() {
            if (ProgressInfo.CurrentHandle != null){
                ResultE cancelResult = ProgressInfo.CurrentHandle.CancelRequest();

                if (cancelResult == ResultE.Success){
                    if(string.Compare(ProgressInfo.CurrentFileName, CurrentTransfer.FileName, true) == 0){
                        CurrentTransfer.Data = null;
                    }
                }
            }

            ClearCurrentTransfer();
        }
        #endregion
    }
}

// using Epic.OnlineServices;
// using Epic.OnlineServices.TitleStorage;
// using PlayEveryWare.EpicOnlineServices;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using ResultE = Epic.OnlineServices.Result;
// using System;
// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Collections.Generic;
// using MemoryPack;
//
// Update MEMO
// SynicSugar's TitleStorage will be fully integrated into Addressable to use.
// In other words, we will use EOS as a Remote server, fetch the catalog from EOS, and update assets based on the catalog.
// If encryption is available on all platforms, we use the encrypted data to be saved locally. If not, the raw data will be used.
// I'm currently working on the way to update and use a catalog.
//
// namespace SynicSugar.TitleStorage {
//     public static class EOSTitleStorage {
//         //Options
//         const uint MAXCHUNKLENGTH = 4 * 4 * 4096;
//         static string METAFILENAME = "sstsmeta.byte";
//         static string METAFILEPATH = $"{Application.persistentDataPath}/synicsugar";
//         public static TransferProgressEvent ProgressInfo = new();
//         //Query
//         static Dictionary<string, EOSFileMetaData> MetaData = new ();
//         //Read
//         static TransferInProgress CurrentTransfer = new();

//         #region Setup
//         /// <summary>
//         /// Load local meta data with default name and path.<br />
//         /// </summary>
//         public async static UniTask Init(){
//             var data = await File.ReadAllBytesAsync(Path.Combine(METAFILEPATH, METAFILENAME));
//             MetaData = MemoryPackSerializer.Deserialize<Dictionary<string, EOSFileMetaData>>(data);
//         }
//         /// <summary>
//         /// Set options and Load local meta data.<br />
//         /// </summary>
//         /// <param name="metafileName">File name to be saved meta data</param>
//         /// <param name="metafilePath">Path metafileName's file</param>
//         public async static UniTask Init(string metafileName, string metafilePath){
//             METAFILEPATH = metafilePath;
//             METAFILENAME = metafileName;

//             var data = await File.ReadAllBytesAsync(Path.Combine(metafilePath, metafileName));
//             MetaData = MemoryPackSerializer.Deserialize<Dictionary<string, EOSFileMetaData>>(data);
//         }
//         #endregion
//         #region Catalog
//     #if !SYNICSUGAR_ADDRESSABLE
//         /// <summary>
//         /// Check Addressable catalog data updated by calling QueryFile("catalog.json").<br />
//         /// This just checks for differences in MetaData's MD5Hash, so to actually update it, call UpdateCatalog().
//         /// </summary>
//         /// <param name="catalogName">File to get</param>
//         /// <param name="token"></param>
//         /// <returns></returns>
//         public static async UniTask<bool> CheckForCatalogUpdates(string catalogName = "catalog.json", CancellationToken token = default(CancellationToken)){
//             return await QueryFile(catalogName, token);
//         }
        
//         /// <summary>
//         /// Check Addressable catalog data updated by calling QueryFile("catalog.json").<br />
//         /// This just checks for differences in MetaData's MD5Hash, so to actually update it, call UpdateCatalog().
//         /// </summary>
//         /// <param name="catalogName">File to get</param>
//         /// <param name="metafileSavedPath">Local Path to save the MetaFile. Default is Application.persistentDataPath</param>
//         /// <param name="token"></param>
//         /// <returns></returns>
//         // public static async UniTask<bool> UpdateCatalog(string catalogName = "catalog.json", string catalogSavedPath = "", CancellationToken token = default(CancellationToken)){
//         //     await FetchFile(catalogName, metafileSavedPath, token)
//         //     return true;
//         // }
//     #endif
//         #endregion
//         #region Query File
//         /// <summary>
//         /// Query a specific file's metadata. And update local metadata list.
//         /// </summary>
//         /// <param name="fileName">File to get</param>
//         public static async UniTask<bool> QueryFile(string fileName, CancellationToken token = default(CancellationToken)){
//             QueryFileOptions queryOptions = new QueryFileOptions(){
//                 LocalUserId = EOSManager.Instance.GetProductUserId(),
//                 Filename = fileName
//             };

//             TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
//             bool finishQuery = false;
//             bool canGetFileName = false;
//             titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileComplete);
//             await UniTask.WaitUntil(() => finishQuery, cancellationToken: token);

//             return canGetFileName;
            

//             void OnQueryFileComplete(ref QueryFileCallbackInfo data) {
//                 //Result
//                 canGetFileName = data.ResultCode == ResultE.Success;

//                 if (!canGetFileName){
//                     Debug.LogErrorFormat("QueryFile: Failure by {0}", data.ResultCode);
//                     finishQuery = true;
//                     return;
//                 }
//                 CopyFileMetadataByFilenameOptions options = new CopyFileMetadataByFilenameOptions(){
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = fileName
//                 };
//                 //Copy meta data
//                 titleStorageInterface.CopyFileMetadataByFilename(ref options, out FileMetadata? fileMetadata);
//                 if(string.IsNullOrEmpty(fileMetadata?.Filename)){
//                     Debug.LogErrorFormat("QueryFile: Meta file is null.", data.ResultCode);
//                     finishQuery = true;
//                     return;
//                 }

//                 if(MetaData.ContainsKey(fileMetadata?.Filename)){
//                     //No need update
//                     if(fileMetadata?.MD5Hash == MetaData[fileName].MD5Hash){
//                         finishQuery = true;
//                         return;
//                     }
//                     //New data
//                     MetaData[fileName].MD5Hash = fileMetadata?.MD5Hash;
//                     MetaData[fileName].NeedUpdate = true;
//                     MetaData[fileName].SizeByte = (uint)fileMetadata?.FileSizeBytes;
//                 }else{
//                     MetaData.Add(fileMetadata?.Filename, new EOSFileMetaData(fileMetadata?.MD5Hash, true, (uint)fileMetadata?.FileSizeBytes));
//                 }

//                 File.WriteAllBytes(Path.Combine(METAFILEPATH, METAFILENAME), MemoryPackSerializer.Serialize(MetaData));

//                 finishQuery = true;
//             }
//         }
//         #endregion



//         #region Query List
//         /// <summary>
//         /// Query the file List with tag from backend. <br />
//         /// When know the filename to get, can call ReadFile directly.
//         /// </summary>
//         /// <param name="tags">Tag set on EOS dev portal.</param>
//         public static async UniTask<List<string>> QueryFileList(string[] tags, CancellationToken token = default(CancellationToken)){
//             Utf8String[] utf8StringTags = new Utf8String[tags.Length];

//             for (int i = 0; i < tags.Length; ++i){
//                 utf8StringTags[i] = tags[i];
//             }
            
//             List<string> fileList = await QueryFileList(utf8StringTags, token);
//             return fileList;
//         }
//         /// <summary>
//         /// Query the file List with tag from backend. <br />
//         /// When know the filename to get, can call ReadFile directly.
//         /// </summary>
//         /// <param name="tags">Tag set on EOS dev portal.</param>
//         public static async UniTask<List<string>> QueryFileList(List<string> tags, CancellationToken token = default(CancellationToken)){
//             Utf8String[] utf8StringTags = new Utf8String[tags.Count];

//             for (int i = 0; i < tags.Count; ++i){
//                 utf8StringTags[i] = tags[i];
//             }
//             List<string> fileList = await QueryFileList(utf8StringTags, token);
//             return fileList;
//         }
//         /// <summary>
//         /// Entity for QueryFileList. Get file count, then create File namelist with the index.
//         /// </summary>
//         /// <param name="tags"></param>
//         static async UniTask<List<string>> QueryFileList(Utf8String[] tags, CancellationToken token){
//             QueryFileListOptions queryOptions = new QueryFileListOptions(){
//                 LocalUserId = EOSManager.Instance.GetProductUserId(),
//                 ListOfTags = tags
//             };

//             TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
//             bool finishQuery = false;
//             List<string> fileList = new();
//             titleStorageInterface.QueryFileList(ref queryOptions, null, OnQueryFileListComplete);
//             await UniTask.WaitUntil(() => finishQuery, cancellationToken: token);

//             return fileList;

//             void OnQueryFileListComplete(ref QueryFileListCallbackInfo data) {
//                 if (data.ResultCode != ResultE.Success){
//                     Debug.LogErrorFormat("QueryFileList: Failure by {0}", data.ResultCode);
//                     finishQuery = true;
//                     return;
//                 }

//                 //Create file list with index
//                 for (uint fileIndex = 0; fileIndex < data.FileCount; fileIndex++) {
//                     CopyFileMetadataAtIndexOptions indexOptions = new CopyFileMetadataAtIndexOptions(){
//                         LocalUserId = EOSManager.Instance.GetProductUserId(),
//                         Index = fileIndex
//                     };

//                     titleStorageInterface.CopyFileMetadataAtIndex(ref indexOptions, out FileMetadata? fileMetadata);

//                     if (fileMetadata == null){
//                         continue;
//                     }
//                     //only for tag
//                     fileList.Add(fileMetadata?.Filename);
//                     //For meta file
//                     if(MetaData.ContainsKey(fileMetadata?.Filename)){
//                         //No need update
//                         if(fileMetadata?.MD5Hash == MetaData[fileMetadata?.Filename].MD5Hash){
//                             finishQuery = true;
//                             continue;
//                         }
//                         //New data
//                         MetaData[fileMetadata?.Filename].MD5Hash = fileMetadata?.MD5Hash;
//                         MetaData[fileMetadata?.Filename].NeedUpdate = true;
//                         MetaData[fileMetadata?.Filename].SizeByte = (uint)fileMetadata?.FileSizeBytes;
//                     }else{
//                         MetaData.Add(fileMetadata?.Filename, new EOSFileMetaData(fileMetadata?.MD5Hash, true, (uint)fileMetadata?.FileSizeBytes));
//                     }
//                 }
//                 File.WriteAllBytes(Path.Combine(METAFILEPATH, METAFILENAME), MemoryPackSerializer.Serialize(MetaData));
//                 finishQuery = true;
//             }
//         }
//         #endregion




//         #region Download
//         /// <summary>
//         /// Exsist target? If not, Download it from EOS server. <br />
//         /// After call this, we can load the target as Resources or AssetBundle.
//         /// </summary>
//         /// <param name="fileName">target</param>
//         /// <returns>If true, Exsist in locaal or fetch file from backend.</returns>
//         public static async UniTask<bool> FetchFile(string fileName) {
//             TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
//             //Prep.
//             //If can use local data, use the query data.
//             if(!MetaData.ContainsKey(fileName)){
//                 //Get query
//                 var queryOptions = new QueryFileOptions {
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = fileName
//                 };

//                 bool finishQuery = false;
//                 bool findFile = false;
//                 titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
//                 await UniTask.WaitUntil(() => finishQuery);
//                 if(!findFile){
//                     Debug.LogError("ReadFile: can't query meta data.");
//                     return false;
//                 }
//                 void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
//                     findFile = data.ResultCode == ResultE.Success;
//                     finishQuery = true;
//                 }
//                 //Get target data
//                 var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = fileName
//                 };
//                 titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

//                 if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
//                     Debug.LogError("ReadFile: can't find the meta data in query.");
//                     return false;
//                 }
//                 // MetaData.Add(fileMetadata?.Filename, new EOSFileMetaData(fileMetadata?.MD5Hash, (uint)fileMetadata?.FileSizeBytes));
//             }
//             if(string.Compare(fileName, CurrentTransfer.FileName,true) == 0){
//                 Debug.LogError("ReadFile: This call is Duplicateds. Downloading it now.");
//                 return false;
//             }
//             //Need download
//             //Init for new transfer
//             CancelCurrentTransfer();
//             ProgressInfo.CurrentFileName = fileName; 
//             CurrentTransfer.Init(fileName);

//             //Read file
//             ReadFileOptions readOptions = new ReadFileOptions(){
//                 LocalUserId = EOSManager.Instance.GetProductUserId(),
//                 Filename = fileName,
//                 ReadChunkLengthBytes = MAXCHUNKLENGTH,
//                 ReadFileDataCallback = OnReadFileData,
//                 FileTransferProgressCallback = OnFileTransferProgress
//             };

//             bool finishRead = false;
//             bool getFile = false;
//             TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
//             ProgressInfo.CurrentHandle = transferRequest;

//             await UniTask.WaitUntil(() => finishRead);
            
//             void OnReadFileComplete(ref ReadFileCallbackInfo data){
//                 getFile = data.ResultCode == ResultE.Success;
//                 if (!getFile){
//                     Debug.LogErrorFormat("ReadFile: OnFileReceived is Failure. {0}", data.ResultCode);
//                 }
//                 FinishFileDownload(data.Filename, getFile);
//                 finishRead = true;
//             }

//             transferRequest.Release();
//             return getFile;
//         }
//         //TODO: Improve Performance
//         /// <summary>
//         /// Exsist targets? If not, Download them from EOS server. <br />
//         /// After call this, we load target as Resources or AssetBundle.
//         /// </summary>
//         /// <param name="fileNames">targets</param>
//         /// <returns>If true, Exsist in locaal or fetch file from backend</returns>
//         public static async UniTask<bool> FetchFiles(string[] fileNames) {
//             bool getFile = false;
//             foreach(var f in fileNames){
//                 getFile = await FetchFile(f);
//                 if(!getFile){
//                     Debug.LogErrorFormat("CanReadFiles: Can't read {0}", f);
//                     return false;
//                 }
//             }
//             return getFile;
//         }
//         static void FinishFileDownload(string fileName, bool success){
//             if (string.Compare(fileName, CurrentTransfer.FileName, true) != 0){
//                 Debug.LogError("ReadFile: Failure.  This is a wrong download.");
//                 return;
//             }

//             if (!CurrentTransfer.isDownload){
//                 Debug.LogError("ReadFile: Failure. Read incorrect data.");
//                 return;
//             }

//             if (success){
//                 if (!CurrentTransfer.Done()){
//                     Debug.LogError("ReadFile: Failure. Not enough data to read.");
//                 }

//                 if (fileName == ProgressInfo.CurrentFileName){
//                     ClearCurrentTransfer();
//                 }
//             }

//         #if SYNICSUGAR_LOG
//             Debug.Log("ReadFile: Finish to read " + fileName);
//         #endif

//             CurrentTransfer.Init();

//             if (fileName.Equals(ProgressInfo.CurrentFileName, StringComparison.OrdinalIgnoreCase)){
//                 ClearCurrentTransfer();
//             }
//         }
//         static void ClearCurrentTransfer(){
//             ProgressInfo.CurrentFileName = string.Empty;

//             if (ProgressInfo.CurrentHandle != null){
//                 ProgressInfo.CurrentHandle = null;
//             }
//         }

//         static ReadResult OnReadFileData(ref ReadFileDataCallbackInfo data){
//             if (data.DataChunk == null){
//                 Debug.LogError("ReadFile: Failure. Data pointer is null.");
//                 return ReadResult.RrFailrequest;
//             }

//             //Failure and Cancel
//             if (string.Compare(data.Filename, CurrentTransfer.FileName, true) != 0){
//                 Debug.LogError("ReadFile: Failure. Read incorrect data.");
//                 return ReadResult.RrCancelrequest;
//             }

//             if (!CurrentTransfer.isDownload){
//                 Debug.LogError("ReadFile: Failure. Unintended Download.");
//                 return ReadResult.RrFailrequest;
//             }
//             //Prep data array
//             if (CurrentTransfer.TotalFileSize == 0){
//                 CurrentTransfer.InitData(data.TotalFileSizeBytes);
//                 CurrentTransfer.TotalFileSize = data.TotalFileSizeBytes;
//             }

//             data.DataChunk.Array.CopyTo(CurrentTransfer.Data, CurrentTransfer.CurrentIndex);
//             CurrentTransfer.CurrentIndex += (uint)data.DataChunk.Count;

//             //The last or not
//             return ReadResult.RrContinuereading;//data.IsLastChunk ? ReadResult.RrFailrequest : ReadResult.RrContinuereading;
//         }
//         /// <summary>
//         /// Optional callback function to be informed of download progress, if the file is not already locally cached.
//         /// </summary>
//         /// <param name="data"></param>
//         static void OnFileTransferProgress (ref FileTransferProgressCallbackInfo data){
//             if (data.TotalFileSizeBytes > 0){
//                 ProgressInfo.InProgressing(data.BytesTransferred / data.TotalFileSizeBytes);
//             }
//         }
//         /// <summary>
//         /// Clear previously cached file data. 
//         /// </summary>
//         public static async UniTask<bool> DeleteCache(){
//             TitleStorageInterface storageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
//             //Get query
//             var option = new DeleteCacheOptions {
//                 LocalUserId = EOSManager.Instance.GetProductUserId()
//             };
//             bool finishDeleted = false;
//             bool canDelete = false;
//             storageInterface.DeleteCache(ref option, null, OnDeleteCacheComplete);

//             await UniTask.WaitUntil(() => finishDeleted);

//             void OnDeleteCacheComplete(ref DeleteCacheCallbackInfo data){
//                 canDelete = data.ResultCode == ResultE.Success;
//                 if(!canDelete){
//                     Debug.LogError("DeleteCache: Failure");
//                 }
//                 finishDeleted = true;
//             }
//             return canDelete;
//         }
//         #endregion
//         #region LoadFile
//     #if SYNICSUGAR_ADDRESSABLE
//         /// <summary>
//         /// Load file with Addressable. When data exists in local, it is just loaded; if not, it is downloaded from Server and then loaded.
//         /// These Resource is managed in batches, and need to call Release() on changing scene.
//         /// </summary>
//         /// <typeparam name="T"></typeparam>
//         /// <param name="filePath"></param>
//         /// <returns></returns>
//         public static async UniTask<T> LoadFromAssetBundle<T>(string filePath) where T : class {
//             TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
//             //Prep.
//             //If can use local data, use the query data.
//             if(!FileMetaDatas.ContainsKey(filePath)){
//                 //Get query
//                 var queryOptions = new QueryFileOptions {
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = filePath
//                 };

//                 bool finishQuery = false;
//                 bool findFile = false;
//                 titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
//                 await UniTask.WaitUntil(() => finishQuery);
//                 if(!findFile){
//                     Debug.LogError("LoadFile: can't query meta data.");
//                     return null;
//                 }
//                 void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
//                     findFile = data.ResultCode == ResultE.Success;
//                     finishQuery = true;
//                 }
//                 //Get target data
//                 var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = filePath
//                 };
//                 titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

//                 if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
//                     Debug.LogError("LoadFile: can't find the meta data in query.");
//                     return null;
//                 }
//                 FileMetaDatas.Add(fileMetadata?.Filename, (uint)fileMetadata?.FileSizeBytes);
//             }
//             if(string.Compare(filePath, CurrentTransfer.FileName,true) == 0){
//                 Debug.LogError("LoadFile: This call is Duplicateds. Already is loading it.");
//                 return null;
//             }
//             //Try to get
//             if(AddressableHelper.Exist<T>(filePath)){
//                 return await AddressableHelper.LoadAddressableAsync<T>(filePath);
//             }

//             //Not exsit file, fetch it from EOS.
//             //Init for new transfer
//             CancelCurrentTransfer();
//             ProgressInfo.CurrentFileName = filePath; 
//             CurrentTransfer.Init(filePath);

//             //Read file
//             ReadFileOptions readOptions = new ReadFileOptions(){
//                 LocalUserId = EOSManager.Instance.GetProductUserId(),
//                 Filename = filePath,
//                 ReadChunkLengthBytes = MAXCHUNKLENGTH,
//                 ReadFileDataCallback = OnReadFileData,
//                 FileTransferProgressCallback = OnFileTransferProgress
//             };

//             bool finishRead = false;
//             bool getFile = false;
//             TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
//             ProgressInfo.CurrentHandle = transferRequest;

//             await UniTask.WaitUntil(() => finishRead);
//             byte[] result = new byte[0];
            
//             void OnReadFileComplete(ref ReadFileCallbackInfo data){
//                 getFile = data.ResultCode == ResultE.Success;
//                 if (!getFile){
//                     Debug.LogErrorFormat("ReadFile: OnFileReceived is Failure. {0}", data.ResultCode);
//                 }
//                 FinishFileDownload(data.Filename, getFile);
//                 finishRead = true;
//             }

//             transferRequest.Release();
//             return await AddressableHelper.LoadAddressableAsync<T>(filePath);
//         }
//         /// <summary>
//         /// Destroy all used resources. Call before transition to next scene.
//         /// </summary>
//         public static void ReleaseAddressables() {
//             AddressableHelper.ReleaseUsedResources();
//         }
//     #endif
//         /// <summary>
//         /// Get File with file name (in QueryFileList).
//         /// </summary>
//         /// <param name="fileName"></param>
//         /// <returns></returns>
//         static async UniTask<T> LoadFromResources<T>(string fileName) where T : class {
//             TitleStorageInterface titleStorageInterface = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
//             //Prep.
//             //If can use local data, use the query data.
//             if(!MetaData.ContainsKey(fileName)){
//                 //Get query
//                 var queryOptions = new QueryFileOptions {
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = fileName
//                 };

//                 bool finishQuery = false;
//                 bool findFile = false;
//                 titleStorageInterface.QueryFile(ref queryOptions, null, OnQueryFileCompleteCallback);
//                 await UniTask.WaitUntil(() => finishQuery);
//                 if(!findFile){
//                     Debug.LogError("LoadFile: can't query meta data.");
//                     return null;
//                 }
//                 void OnQueryFileCompleteCallback(ref QueryFileCallbackInfo data){
//                     findFile = data.ResultCode == ResultE.Success;
//                     finishQuery = true;
//                 }
//                 //Get target data
//                 var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions {
//                     LocalUserId = EOSManager.Instance.GetProductUserId(),
//                     Filename = fileName
//                 };
//                 titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

//                 if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata?.Filename)){
//                     Debug.LogError("LoadFile: can't find the meta data in query.");
//                     return null;
//                 }
//                 // MetaData.Add(fileMetadata?.Filename, new EOSFileMetaData(fileMetadata?.MD5Hash, (uint)fileMetadata?.FileSizeBytes));
//             }
//             if(string.Compare(fileName, CurrentTransfer.FileName,true) == 0){
//                 Debug.LogError("LoadFile: This call is Duplicateds. Already is loading it.");
//                 return null;
//             }

//             //Init for new transfer
//             CancelCurrentTransfer();
//             ProgressInfo.CurrentFileName = fileName; 
//             CurrentTransfer.Init(fileName);

//             //Read file
//             ReadFileOptions readOptions = new ReadFileOptions(){
//                 LocalUserId = EOSManager.Instance.GetProductUserId(),
//                 Filename = fileName,
//                 ReadChunkLengthBytes = MAXCHUNKLENGTH,
//                 ReadFileDataCallback = OnReadFileData,
//                 FileTransferProgressCallback = OnFileTransferProgress
//             };

//             bool finishRead = false;
//             bool getFile = false;
//             TitleStorageFileTransferRequest transferRequest = titleStorageInterface.ReadFile(ref readOptions, null, OnReadFileComplete);
//             ProgressInfo.CurrentHandle = transferRequest;

//             await UniTask.WaitUntil(() => finishRead);
//             byte[] result = new byte[0];
            
//             void OnReadFileComplete(ref ReadFileCallbackInfo data){
//                 getFile = data.ResultCode == ResultE.Success;
//                 if (!getFile){
//                     Debug.LogErrorFormat("LoadFile: OnFileReceived is Failure. {0}", data.ResultCode);
//                 }
//                 FinishFileDownload(data.Filename, getFile, out result);
//                 finishRead = true;
//             }

//             transferRequest.Release();
//             return MemoryPackSerializer.Deserialize<T>(result);
//         }
//         static void FinishFileDownload(string fileName, bool success, out byte[] result){
//             if (string.Compare(fileName, CurrentTransfer.FileName, true) != 0){
//                 Debug.LogError("LoadFile: Failure. This is a wrong download.");
//                 result = null;
//                 return;
//             }

//             if (!CurrentTransfer.isDownload){
//                 Debug.LogError("LoadFile: Failure. Read incorrect data.");
//                 result = null;
//                 return;
//             }

//             if (success){
//                 if (!CurrentTransfer.Done()){
//                     Debug.LogError("LoadFile: Failure. Not enough data to read.");
//                 }

//                 if (fileName == ProgressInfo.CurrentFileName){
//                     ClearCurrentTransfer();
//                 }
//             }
//             result = new byte[CurrentTransfer.Data.Length];
//             Buffer.BlockCopy(CurrentTransfer.Data, 0, result, 0, result.Length);

//         #if SYNICSUGAR_LOG
//             Debug.Log("LoadFile: Finish to read " + fileName + result.Length);
//         #endif

//             CurrentTransfer.Init();

//             if (fileName.Equals(ProgressInfo.CurrentFileName, StringComparison.OrdinalIgnoreCase)){
//                 ClearCurrentTransfer();
//             }
//         }
        
//         static void CancelCurrentTransfer() {
//             if (ProgressInfo.CurrentHandle != null){
//                 ResultE cancelResult = ProgressInfo.CurrentHandle.CancelRequest();

//                 if (cancelResult == ResultE.Success){
//                     if(string.Compare(ProgressInfo.CurrentFileName, CurrentTransfer.FileName, true) == 0){
//                         CurrentTransfer.Data = null;
//                     }
//                 }
//             }

//             ClearCurrentTransfer();
//         }
//         #endregion
//     }
// }
