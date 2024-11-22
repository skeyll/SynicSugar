using UnityEngine;
using System;
using System.IO;
using MemoryPack;
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;

namespace SynicSugar.MatchMake {
    public class SessionDataManager
    {
        private string filePath;
        const uint ALLOWED_DIFF_SEC = 3;
        public SessionDataManager(string fileName)
        {
            filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.dat");
        }

        public static DateTime CalculateReconnecterTimeStamp(uint erapsedSec){
            DateTime estimatedTimestamp = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(erapsedSec));
            TimeSpan diff = p2pInfo.Instance.CurrentSessionStartUTC - estimatedTimestamp;
            
            // Check if the time difference exceeds allowed seconds, use estimated timestamp if true
            if (Math.Abs(diff.TotalSeconds) > ALLOWED_DIFF_SEC) 
            {
                #if SYNICSUGAR_LOG
                Debug.Log($"Adjusted timestamp: Difference exceeded {ALLOWED_DIFF_SEC} sec. Estimated: {estimatedTimestamp}, CurrentStartUTC: {p2pInfo.Instance.CurrentSessionStartUTC}");
                #endif
                return estimatedTimestamp;
            }
            return Math.Abs(diff.TotalSeconds) > 2 ? estimatedTimestamp : p2pInfo.Instance.CurrentSessionStartUTC;
        }

        /// <summary>
        /// Save LobbyId and DataTime on starting session.
        /// </summary>
        /// <param name="data"></param>
        internal async UniTaskVoid SaveSessionData(SessionData data)
        {
            try
            {
                await File.WriteAllBytesAsync(filePath, MemoryPackSerializer.Serialize(data));
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"SaveSessionData: Don't have access permission. {e.Message}");
            }
            catch (IOException e)
            {
                Debug.LogError($"SaveSessionData: An error occurred during file operation. {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSessionData: An unexpected error has occurred. {e.Message}");
            }

        #if SYNICSUGAR_LOG
            Debug.Log($"SaveSessionData: Save SessionData to {filePath}.");
        #endif
        }

        /// <summary>
        /// Load SessionData and check it with LobbyId
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        internal async UniTask<SessionData> LoadSessionData(string lobbyId)
        {
            if (File.Exists(filePath))
            {
                byte[] binaryData = await File.ReadAllBytesAsync(filePath);

                SessionData data = MemoryPackSerializer.Deserialize<SessionData>(binaryData);

                if(lobbyId != data.LobbyID)
                {
                    Debug.Log($"LoadSessionData: Failed to load SessionData. This data is not for {lobbyId}.");
                    return null;
                }
            #if SYNICSUGAR_LOG
                Debug.Log($"SaveSessionData: Success in loading SessionData.");
            #endif
                return data;
            }
            else
            {
                Debug.LogWarning("LoadSessionData: SessionData does not exist");
                return null;
            }
        }
    }
}