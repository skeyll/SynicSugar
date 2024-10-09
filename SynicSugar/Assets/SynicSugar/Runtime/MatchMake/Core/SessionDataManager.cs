using UnityEngine;
using System;
using System.IO;
using MemoryPack;
using SynicSugar.P2P;

namespace SynicSugar.MatchMake {

    internal class SessionDataManager
    {
        private string filePath;
        const string fileName = "ss_sessiondata.dat";
        internal SessionDataManager(){
            filePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        /// <summary>
        /// Save LobbyId and DataTime on starting session.
        /// </summary>
        /// <param name="data"></param>
        internal bool SaveSessionData(SessionData data)
        {
            try
            {
                File.WriteAllBytes(filePath, MemoryPackSerializer.Serialize(data));
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.Log($"SaveSessionData: Don't have access permission. This user cannot come back to game as reconnecter. {e.Message}");
                return false;
            }
            catch (IOException e)
            {
                Debug.Log($"SaveSessionData: An error occurred during file operation. This user cannot come back to game as reconnecter. {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Debug.Log($"SaveSessionData: An unexpected error has occurred. This user cannot come back to game as reconnecter. {e.Message}");
                return false;
            }

        #if SYNICSUGAR_LOG
            Debug.Log($"SaveSessionData: Save SessionData to {filePath}.");
        #endif
            return true;
        }

        /// <summary>
        /// Load SessionData and check it with LobbyId
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        internal SessionData LoadSessionData(string lobbyId)
        {
            if (File.Exists(filePath))
            {
                byte[] binaryData = File.ReadAllBytes(filePath);

                SessionData data = MemoryPackSerializer.Deserialize<SessionData>(binaryData);

                if(lobbyId != data.LobbyID){
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