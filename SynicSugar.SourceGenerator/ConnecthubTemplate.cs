﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SynicSugarGenerator {
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    
    
    public partial class ConnecthubTemplate : ConnecthubTemplateBase {
        
        public virtual string TransformText() {
            this.GenerationEnvironment = null;
            
            #line 9 ""
            this.Write("// <auto-generated>\n// THIS (.cs) FILE IS GENERATED BY SynicSugarGenerator. DO NO" +
                    "T CHANGE IT.\n// </auto-generated>\n#pragma warning disable CS0164 // This label h" +
                    "as not been referenced\n#pragma warning disable CS0436 // Type conflicts with the" +
                    " imported type\n\nusing UnityEngine;\nusing MemoryPack;\nusing MemoryPack.Compressio" +
                    "n;\nusing System;\nusing System.Collections.Generic;\nusing System.Threading;\nusing" +
                    " Cysharp.Threading.Tasks;\nusing Epic.OnlineServices;\nusing SynicSugar.RTC;\nnames" +
                    "pace SynicSugar.P2P {\n    internal sealed class ConnectHub : IPacketConvert {\n  " +
                    "      //Singleton\n        private static Lazy<ConnectHub> instance = new Lazy<Co" +
                    "nnectHub>(() => new ConnectHub());\n        public static ConnectHub Instance => " +
                    "instance.Value;\n\n        private ConnectHub(){\n            syncTokenSource = new" +
                    " CancellationTokenSource();\n        }\n        INetworkCore _networkCore;\n       " +
                    " INetworkCore NetworkCore { \n            get { \n                if(_networkCore " +
                    "== null){\n                    _networkCore = p2pConfig.Instance.GetNetworkCore()" +
                    ";\n                }\n                return _networkCore;\n            }\n        }" +
                    "\n        /// <summary>\n        /// SyncToken is managed with the connection\'s va" +
                    "lid state.\n        /// </summary>\n        CancellationTokenSource syncTokenSourc" +
                    "e;\n\n        public CancellationToken GetSyncToken(){\n            return syncToke" +
                    "nSource.Token;\n        }\n\n        void Dispose() {\n            syncTokenSource?." +
                    "Cancel();\n            syncTokenSource?.Dispose();\n            _networkCore = nul" +
                    "l;\n        }\n        /// <summary>\n        /// Disposes of the ConnectHub instan" +
                    "ce and its associated data. <br />\n        /// GC-managed data is explicitly dis" +
                    "posed here as well, since this is called at a timing where processing overhead i" +
                    "s not a concern.\n        /// </summary>\n        void ResetInstance() {\n         " +
                    "   Dispose();\n            ClearReferenceDictionaries();\n            instance = n" +
                    "ew Lazy<ConnectHub>(() => new ConnectHub());\n        }\n        //Start\n        /" +
                    "// <summary>\n        /// Start the packet receiver. Call after creating the Netw" +
                    "ork Instance required for reception.<br />\n        /// This cannot be called wit" +
                    "h other Receiver same time. If start the other Receiver, ConenctHub stop this Re" +
                    "ceiver automatically before start the new one.\n        /// </summary>\n        //" +
                    "/ <param name=\"receiveTiming\">The timing that packet receiver gets packet from b" +
                    "uffer.</param>\n        /// <param name=\"maxBatchSize\">How many times during 1 FP" +
                    "S are received</param>\n        public Result StartPacketReceiver(PacketReceiveTi" +
                    "ming receiveTiming = PacketReceiveTiming.Update, uint maxBatchSize = 1){\n       " +
                    " #if SYNICSUGAR_PACKETINFO\n            string chs = string.Empty;\n            st" +
                    "ring[] chList = Enum.GetNames(typeof(ConnectHub.Channels));\n            foreach(" +
                    "var l in chList){\n                chs += l.ToString() + \", \";\n            }\n    " +
                    "        Debug.Log($\"ch info: amount {chList.Length} / {chs}\");\n        #endif\n  " +
                    "          return NetworkCore.StartPacketReceiver(this, receiveTiming, maxBatchSi" +
                    "ze);\n        }\n        \n        /// <summary>\n        /// To get only SynicPacke" +
                    "t in burst FPS. Call after creating the Network Instance required for reception." +
                    "<br />\n        /// This cannot be called with other Receiver same time. If start" +
                    " the other Receiver, ConenctHub stop this Receiver automatically before start th" +
                    "e new one.\n        /// </summary>\n        /// <param name=\"maxBatchSize\">How man" +
                    "y times during 1 FPS are received</param>\n        public Result StartSynicReceiv" +
                    "er(uint maxBatchSize = 1){\n            return NetworkCore.StartSynicReceiver(thi" +
                    "s, maxBatchSize);\n        }\n        //Pause receiver\n        /// <summary>\n     " +
                    "   /// Pause getting a packet from the buffer. To re-start, call StartPacketRece" +
                    "iver().<br />\n        /// *Packet receiving to the buffer is continue. If the pa" +
                    "cket is over the buffer, subsequent packets are discarded.\n        /// </summary" +
                    ">\n        [Obsolete(\"This is old. StopPacketReceiver is new one.\")] \n        pub" +
                    "lic Result PausePacketReceiver(){\n            return NetworkCore.StopPacketRecei" +
                    "ver();\n        }\n        //Stop receiver\n        /// <summary>\n        /// Stop " +
                    "getting a packet from the buffer. To re-start, call StartPacketReceiver().<br />" +
                    "\n        /// *Packet receiving to the buffer is continue. If the packet is over " +
                    "the buffer, subsequent packets are discarded.\n        /// </summary>\n        pub" +
                    "lic Result StopPacketReceiver(){\n            return NetworkCore.StopPacketReceiv" +
                    "er();\n        }\n\n        //Pause Reciving buffer\n        /// <summary>\n        /" +
                    "// Pause receiving a packet to the receive buffer. To re-start, call RestartConn" +
                    "ections(). <br />\n        /// After call this, packets will have been discarded " +
                    "until connection will re-open.\n        /// </summary>\n        /// <param name=\"i" +
                    "sForced\">If True, force to stop and clear current packet queue. <br />\n        /" +
                    "// If false, process current queue, then stop it.</param>\n        public async U" +
                    "niTask<Result> PauseConnections(bool isForced = false, CancellationToken cancelT" +
                    "oken = default(CancellationToken)){\n            syncTokenSource.Cancel();\n      " +
                    "      return await NetworkCore.PauseConnections(isForced, cancelToken);\n        " +
                    "}\n        /// <summary>\n        /// Prepare to receive packets in advance. If us" +
                    "er sent a packet, it can also open connection to get packets without this.\n     " +
                    "   /// </summary>\n        public Result RestartConnections(){\n            Networ" +
                    "kCore.RestartConnections();\n            syncTokenSource = new CancellationTokenS" +
                    "ource();\n            return StartPacketReceiver();\n        }\n        \n        //" +
                    "/ <summary>\n        /// Stop receiver, close all connections and remove the noti" +
                    "fy events.\n        /// Then, the user leave the lobby. The last user closes the " +
                    "lobby in Backend. <br />\n        /// If the host calls this method to leave the " +
                    "Lobby, a host migration will occur, assigning a new host.\n        /// <param nam" +
                    "e=\"destroyManager\">Destroy NetworkManager after exit lobby.</param>\n        /// " +
                    "<param name=\"cleanupMemberCountChanged\">Need to call MatchMakeManager.Instance.M" +
                    "atchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>" +
                    "\n        /// <param name=\"cancelToken\">Cancel token for this task</param>\n      " +
                    "  /// </summary>\n        public async UniTask<Result> ExitSession(bool destroyMa" +
                    "nager = true, bool cleanupMemberCountChanged = false, CancellationToken cancelTo" +
                    "ken = default(CancellationToken)){\n            Result result = await NetworkCore" +
                    ".ExitSession(destroyManager, cleanupMemberCountChanged, cancelToken);\n          " +
                    "  \n            if(result == Result.Success){\n                ResetInstance();\n  " +
                    "          }\n            return result;\n        }\n        /// <summary>\n        /" +
                    "// Stop receiver, close all connections and remove the notify events. Then, Host" +
                    " closes and Guest leaves the Lobby.　<br />\n        /// When Host closes Lobby, G" +
                    "uests are automatically kicked out from the Lobby. <br />\n        /// If the Lob" +
                    "by is closed with this API during a session, the Lobby will be destroyed, ending" +
                    " all peer-to-peer connections not only between the Host and Guests but also amon" +
                    "g Guests. Following this, ConnectionNotifier will invoke OnLobbyClosed with the " +
                    "Reason.LobbyClosed.\n        /// <param name=\"destroyManager\">Destroy NetworkMana" +
                    "ger after exit lobby.</param>\n        /// <param name=\"cleanupMemberCountChanged" +
                    "\">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountCh" +
                    "anged(id, false) after exit lobby?</param>\n        /// <param name=\"cancelToken\"" +
                    ">Cancel token for this task</param>\n        /// </summary>\n        public async " +
                    "UniTask<Result> CloseSession(bool destroyManager = true, bool cleanupMemberCount" +
                    "Changed = false, CancellationToken cancelToken = default(CancellationToken)){\n  " +
                    "          Result result = await NetworkCore.CloseSession(destroyManager, cleanup" +
                    "MemberCountChanged, cancelToken);\n            \n            if(result == Result.S" +
                    "uccess){\n                ResetInstance();\n            }\n            \n           " +
                    " return result;\n        }\n\n        /// <summary>\n        /// Destory offline-lob" +
                    "by and stop offline mode.<br />\n        /// This process is valid only when a se" +
                    "ssion is created by CreateOfflineLobby.\n        /// <param name=\"destroyManager\"" +
                    ">Destroy NetworkManager after exit lobby.</param>\n        /// <param name=\"clean" +
                    "upMemberCountChanged\">Need to call MatchMakeManager.Instance.MatchMakingGUIEvent" +
                    "s.LobbyMemberCountChanged(id, false) after exit lobby?</param>\n        /// <para" +
                    "m name=\"cancelToken\">Cancel token for this task</param>\n        /// </summary>\n " +
                    "       public async UniTask<Result> DestoryOfflineLobby(bool destroyManager = tr" +
                    "ue, bool cleanupMemberCountChanged = false, CancellationToken cancelToken = defa" +
                    "ult(CancellationToken)){\n            Result result = await NetworkCore.DestoryOf" +
                    "flineLobby(destroyManager, cleanupMemberCountChanged, cancelToken);\n\n           " +
                    " if(result == Result.Success){\n                ResetInstance();\n            }\n  " +
                    "          \n            return result;\n        }\n\n        [Obsolete(\"This is old." +
                    " Channels is new one\")]\n        public enum CHANNELLIST {\n            ");
            
            #line default
            #line hidden
            
            #line 183 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncList ));
            
            #line default
            #line hidden
            
            #line 183 ""
            this.Write("\n        }\n        //(for elements)\n        /// <summary>\n        /// Ch\'s list a" +
                    "llocated for SendPacket.\n        /// </summary>\n        public enum Channels {\n " +
                    "           ");
            
            #line default
            #line hidden
            
            #line 190 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncList ));
            
            #line default
            #line hidden
            
            #line 190 ""
            this.Write(@"
        }
        //For Synic(UserId, value)
        Dictionary<string, byte[]> synicBuffer = new Dictionary<string, byte[]>();
        Dictionary<string, SynicPacketInfomation> synicPacketInfo = new Dictionary<string, SynicPacketInfomation>();

        //For LargePacket(UserId, (ch, value))
        Dictionary<string, Dictionary<byte, byte[]>> largeBuffer = new Dictionary<string,Dictionary<byte, byte[]>>();
        Dictionary<string, Dictionary<byte, LargePacketsInfomation>> largePacketInfo = new Dictionary<string, Dictionary<byte, LargePacketsInfomation>>();

        //Ref(for class)");
            
            #line default
            #line hidden
            
            #line 200 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( Reference ));
            
            #line default
            #line hidden
            
            #line 200 ""
            this.Write("\n\n        //Clear ref\n        private void ClearReferenceDictionaries(){ ");
            
            #line default
            #line hidden
            
            #line 203 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( ClearReference ));
            
            #line default
            #line hidden
            
            #line 203 ""
            this.Write("\n            synicBuffer.Clear();\n            synicPacketInfo.Clear();\n          " +
                    "  largeBuffer.Clear();\n            largePacketInfo.Clear();\n        }\n\n        /" +
                    "/Register(for class)");
            
            #line default
            #line hidden
            
            #line 210 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( Register ));
            
            #line default
            #line hidden
            
            #line 210 ""
            this.Write(@"
        
        /// <summary>
        /// Get the NetworkPlayer instance registered with ConnectHub.
        /// </summary>
        /// <param name=""id"">UserID to get</param>
        /// <returns>T's instance</returns>
        public T GetUserInstance<T>(UserId id) where T : IGetPlayer {");
            
            #line default
            #line hidden
            
            #line 217 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( PlayeInstance ));
            
            #line default
            #line hidden
            
            #line 217 ""
            this.Write(@"
            return default(T);
        }
        
        /// <summary>
        /// Get the NetworkCommons instance registered with ConnectHub.
        /// </summary>
        /// <returns>T's instance</returns>
        public T GetInstance<T>() where T : IGetCommons {");
            
            #line default
            #line hidden
            
            #line 225 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( CommonsInstance ));
            
            #line default
            #line hidden
            
            #line 225 ""
            this.Write(@"
            return default(T);
        }

        /// <summary>
        /// Remote RPC is invoked with received value.
        /// </summary>
        void IPacketConvert.ConvertFromPacket(ref byte ch, string id, ref ArraySegment<byte> payload){
            switch((Channels)ch){");
            
            #line default
            #line hidden
            
            #line 233 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( PacketConvert ));
            
            #line default
            #line hidden
            
            #line 233 ""
            this.Write("\n                case Channels.ObtainPing:\n                    EOSp2p.SendPacket(" +
                    "(byte)Channels.ReturnPong, payload, UserId.GetUserId(id));\n                retur" +
                    "n;\n                case Channels.ReturnPong:\n                    NetworkCore.Get" +
                    "Pong(id, payload);\n                return;\n                case Channels.Synic:\n" +
                    "                    string userId = id;\n                    bool restoredPacket " +
                    "= RestoreSynicPackets(ref ch, ref userId, ref payload);\n                    if(!" +
                    "restoredPacket){\n    #if SYNICSUGAR_LOG \n                        Debug.LogFormat" +
                    "(\"ConvertFormPacket: Restore packet is in progress for {0} from {1}.\", userId, i" +
                    "d);\n    #endif\n                        return;\n                    }\n           " +
                    "         \n                    OverwrittenSynicWithRemoteData(userId);\n\n         " +
                    "           NetworkCore.UpdateSynicStatus(userId, synicPacketInfo[userId].phase);" +
                    "\n\n                    //Init\n                    synicBuffer.Remove(userId);\n   " +
                    "                 synicPacketInfo.Remove(userId);\n\n                    //Stop ove" +
                    "rwriting localuser data with host data.\n                    if(p2pInfo.Instance." +
                    "IsLoaclUser(userId)){\n                        NetworkCore.StopOverwritingLocalUs" +
                    "erData();\n                    }\n                #if SYNICSUGAR_LOG \n            " +
                    "        Debug.LogFormat(\"ConvertFormPacket: Success overwriting {0}\'s data by {1" +
                    "}\"!, userId, id);\n                #endif\n                return;           \n    " +
                    "        }\n        }\n\n        /// <summary>\n        /// Re-Send RPC with last rec" +
                    "orded information.<br />\n        /// To send disconnected peers after some time." +
                    " SynicSugar retransmit to connecting-peers.<br />\n        /// To record, pass tr" +
                    "ue to attribute.\n        /// </summary>\n        public void ResendLastRPC(){\n   " +
                    "         if(p2pInfo.Instance.LastRPCIsLargePacket){\n                EOSp2p.SendL" +
                    "argePacketsToAll(p2pInfo.Instance.LastRPCch, p2pInfo.Instance.LastRPCPayload).Fo" +
                    "rget();\n                return;\n            }\n            EOSp2p.SendPacketToAll" +
                    "(p2pInfo.Instance.LastRPCch, p2pInfo.Instance.LastRPCPayload).Forget();\n        " +
                    "}\n        /// <summary>\n        /// Re-Send RPC to the specific target with last" +
                    " recorded information.<br />\n        /// In order to send disconnected peers aft" +
                    "er the some time. SynicSugar has retransmission to connecting-peers for the reli" +
                    "ability.<br />\n        /// To record, pass true to attribute.\n        /// </summ" +
                    "ary>\n        /// <param name=\"target\"></param>\n        public void ResendLastRPC" +
                    "ToTarget(UserId target){\n            if(p2pInfo.Instance.LastRPCIsLargePacket){\n" +
                    "                EOSp2p.SendLargePackets(p2pInfo.Instance.LastRPCch, p2pInfo.Inst" +
                    "ance.LastRPCPayload, target).Forget();\n                return;\n            }\n   " +
                    "         EOSp2p.SendPacket(p2pInfo.Instance.LastRPCch, p2pInfo.Instance.LastRPCP" +
                    "ayload, target);\n        }\n        /// <summary>\n        /// Re-Send TargetRPC w" +
                    "ith last recorded information.<br />\n        /// In order to send disconnected p" +
                    "eers after the some time. SynicSugar has retransmission to connecting-peers for " +
                    "the reliability.<br />\n        /// To record, pass true to attribute.\n        //" +
                    "/ </summary>\n        public void ResendLastTargetRPC(){\n            if(p2pInfo.I" +
                    "nstance.LastTargetRPCIsLargePacket){\n                EOSp2p.SendLargePackets(p2p" +
                    "Info.Instance.LastTargetRPCch, p2pInfo.Instance.LastTargetRPCPayload, p2pInfo.In" +
                    "stance.LastTargetRPCUserId).Forget();\n                return;\n            }\n    " +
                    "        EOSp2p.SendPacket(p2pInfo.Instance.LastTargetRPCch, p2pInfo.Instance.Las" +
                    "tTargetRPCPayload, p2pInfo.Instance.LastTargetRPCUserId);\n        }\n\n        ");
            
            #line default
            #line hidden
            
            #line 307 ""
 if (needSyncSynic) { 
            
            #line default
            #line hidden
            
            #line 308 ""
            this.Write("        \n        /// <summary>\n        /// Sync all Synic variables. This is very" +
                    " heavy because it handles multiple data and repeats compression and serializatio" +
                    "n.\n        /// </summary>\n        /// <param name=\"targetId\">Target to be synced" +
                    " by this local user.</param>\n        /// /// <param name=\"type\">Whose data Host " +
                    "sends in Host\'s local. When set WithTarget or WithOthers, can overwrite the targ" +
                    "et\'s local data in Host\'s local data.</param>\n        /// <param name=\"syncedPha" +
                    "se\">Phase to be synced. If syncSinglePhase is false, sync all variables in the p" +
                    "hase up to this point.</param>\n        /// <param name=\"syncSinglePhase\">If true" +
                    ", send only variables in syncedPhase.</param>\n        public async void SyncSyni" +
                    "c(UserId targetId, SynicType type, byte syncedPhase = 9, bool syncSinglePhase = " +
                    "false){\n            //Sync local data to target local\n            SynicContainer" +
                    " synicContainer = GenerateSynicContainer(p2pInfo.Instance.LocalUserId.ToString()" +
                    ", syncedPhase, syncSinglePhase);\n\n            using var selfCompressor  = new Br" +
                    "otliCompressor();\n            MemoryPackSerializer.Serialize(selfCompressor, syn" +
                    "icContainer);\n\n            EOSp2p.SendSynicPackets((byte)Channels.Synic, selfCom" +
                    "pressor.ToArray(), targetId, p2pInfo.Instance.LocalUserId, syncedPhase, syncSing" +
                    "lePhase);\n\n            if(type == SynicType.OnlySelf || !p2pInfo.Instance.IsHost" +
                    "()){\n                return;\n            }\n            \n            if(type == S" +
                    "ynicType.WithOthers){\n                foreach(var id in p2pInfo.Instance.Disconn" +
                    "ectedUserIds){\n                    synicContainer = GenerateSynicContainer(id.To" +
                    "String(), syncedPhase, syncSinglePhase);\n\n                    using var targetCo" +
                    "mpressor  = new BrotliCompressor();\n                    MemoryPackSerializer.Ser" +
                    "ialize(targetCompressor, synicContainer);\n\n                    EOSp2p.SendSynicP" +
                    "ackets((byte)Channels.Synic, targetCompressor.ToArray(), targetId, id, syncedPha" +
                    "se, syncSinglePhase);\n                    await UniTask.Yield();\n               " +
                    " }\n            }\n            \n            //Sync target data in local to target " +
                    "local\n            synicContainer = GenerateSynicContainer(targetId.ToString(), s" +
                    "yncedPhase, syncSinglePhase);\n\n            using var reconnecterCompressor  = ne" +
                    "w BrotliCompressor();\n            MemoryPackSerializer.Serialize(reconnecterComp" +
                    "ressor, synicContainer);\n\n            EOSp2p.SendSynicPackets((byte)Channels.Syn" +
                    "ic, reconnecterCompressor.ToArray(), targetId, targetId, syncedPhase, syncSingle" +
                    "Phase);\n        }\n        \n        SynicContainer GenerateSynicContainer(string " +
                    "id, byte syncedPhase, bool syncSinglePhase){\n            SynicContainer synicCon" +
                    "tainer = new SynicContainer();\n            switch(syncedPhase){");
            
            #line default
            #line hidden
            
            #line 352 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( GenerateSynicContainer ));
            
            #line default
            #line hidden
            
            #line 352 ""
            this.Write("\n                default:\n                goto case 9;\n            }\n            " +
                    "return synicContainer;\n        }\n        ");
            
            #line default
            #line hidden
            
            #line 358 ""
 } 
            
            #line default
            #line hidden
            
            #line 359 ""
            this.Write("        \n        //Synced 0 = index, 1 = additional packet amount\n        bool Re" +
                    "storeLargePackets(ref byte ch, string id, ref ArraySegment<byte> payload){\n     " +
                    "       //Prep\n            if(!largeBuffer.ContainsKey(id)){\n                larg" +
                    "ePacketInfo.Add(id, new Dictionary<byte, LargePacketsInfomation>());\n           " +
                    "     largeBuffer.Add(id, new Dictionary<byte, byte[]>());\n            }\n        " +
                    "    if(!largeBuffer[id].ContainsKey(ch)){\n                largePacketInfo[id].Ad" +
                    "d(ch, new LargePacketsInfomation(){ additionalPacketsAmount = payload[1] });\n   " +
                    "             //Prep enough byte[]\n                largeBuffer[id].Add(ch, new by" +
                    "te[(payload[1] + 1) * EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE]);\n            }\n\n     " +
                    "       int packetIndex = payload[0];\n            int offset = packetIndex * EOSp" +
                    "2p.MAX_LARGEPACKET_PAYLOADSIZE;\n\n    #if SYNICSUGAR_PACKETINFO\n            Debug" +
                    ".Log($\"RestoreLargePackets: ch {ch}({(Channels)ch}) from {id} / packet index {pa" +
                    "yload[0]}/{payload[1]}\");\n    #endif\n            //Remove header\n            Spa" +
                    "n<byte> packetPayload = payload.Slice(2);\n            largePacketInfo[id][ch].cu" +
                    "rrentSize += packetPayload.Length;\n            //Copy Byte from what come in\n   " +
                    "         Buffer.BlockCopy(packetPayload.ToArray(), 0, largeBuffer[id][ch], offse" +
                    "t, packetPayload.Length);\n\n            //Comming all?\n            //We don\'t kno" +
                    "w real packet size. So we need + 1166.\n            //This first conditon for emp" +
                    "ty packet.\n            return largePacketInfo[id][ch].additionalPacketsAmount ==" +
                    " 0 || largePacketInfo[id][ch].currentSize + EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE >" +
                    " largeBuffer[id][ch].Length ? true : false;\n        }\n\n        // 0-packet index" +
                    ", 1-additional packet amount, 2-complex data[1bit-isOnly, 4bits-phase, 3bits use" +
                    "rType], 3-data\'s user index\n        bool RestoreSynicPackets(ref byte ch, ref st" +
                    "ring id, ref ArraySegment<byte> payload){\n            //Set target id\n          " +
                    "  int userDataType = (int)(payload[2] & 0x07);\n            if(userDataType == 0)" +
                    "{\n                if(p2pInfo.Instance.IsHost(id) && p2pInfo.Instance.IsReconnect" +
                    "er){\n                    id = p2pInfo.Instance.LocalUserId.ToString();\n         " +
                    "       }else{\n                    return false;\n                }\n            }e" +
                    "lse if(userDataType == 2){\n                if(p2pInfo.Instance.IsHost(id) && p2p" +
                    "Info.Instance.IsReconnecter){\n                    id = p2pInfo.Instance.AllUserI" +
                    "ds[payload[3]].ToString();\n                }else{\n                    return fal" +
                    "se;\n                }\n            }\n\n            if(!synicBuffer.ContainsKey(id)" +
                    "){\n                synicPacketInfo.Add(id, new SynicPacketInfomation(){  basis =" +
                    " new (){ additionalPacketsAmount = payload[1]}, \n                               " +
                    "                                             phase = (byte)((payload[2] >> 3) & " +
                    "0x0F), \n                                                                        " +
                    "    isSinglePhase = (payload[2] & 0x80) != 0 });\n                //Prep enough b" +
                    "yte[]\n                synicBuffer.Add(id, new byte[(payload[1] + 1) * EOSp2p.MAX" +
                    "_LARGEPACKET_PAYLOADSIZE]);\n            }\n            int packetIndex = payload[" +
                    "0];\n            int offset = packetIndex * EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE;\n\n" +
                    "    #if SYNICSUGAR_PACKETINFO\n            Debug.Log($\"RestoreSynicPacket(Synic):" +
                    " ch {ch}({(Channels)ch}) / Data\'s userID {id} / packet index {payload[0]}/{paylo" +
                    "ad[1]}\");\n    #endif\n            //Remove header\n            Span<byte> packetPa" +
                    "yload = payload.Slice(4);\n            synicPacketInfo[id].basis.currentSize += p" +
                    "acketPayload.Length;\n            //Copy Byte from what come in\n            Buffe" +
                    "r.BlockCopy(packetPayload.ToArray(), 0, synicBuffer[id], offset, packetPayload.L" +
                    "ength);\n            //Comming all?\n            //We don\'t know real packet size." +
                    " So we need + 1166.\n            //This first conditon for empty packet.\n        " +
                    "    return synicPacketInfo[id].basis.additionalPacketsAmount == 0 || synicPacket" +
                    "Info[id].basis.currentSize + EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE > synicBuffer[id" +
                    "].Length ? true : false;\n        }\n\n        /// <summary>\n        /// Call from " +
                    "ConvertFormPacket.\n        /// </summary>\n        void OverwrittenSynicWithRemot" +
                    "eData(string overwriterUserId){\n            //Deserialize packet\n            usi" +
                    "ng var decompressor = new BrotliDecompressor();\n            Span<byte> transmitt" +
                    "edPaylaod = new Span<byte>(synicBuffer[overwriterUserId]);\n\n            var deco" +
                    "mpressedBuffer = decompressor.Decompress(transmittedPaylaod.Slice(0, synicPacket" +
                    "Info[overwriterUserId].basis.currentSize));\n            SynicContainer container" +
                    " = MemoryPackSerializer.Deserialize<SynicContainer>(decompressedBuffer);\n\n      " +
                    "      //Packet data\n            int phase = synicPacketInfo[overwriterUserId].ph" +
                    "ase;\n            bool syncSinglePhase = synicPacketInfo[overwriterUserId].isSing" +
                    "lePhase;\n\n            switch(phase){");
            
            #line default
            #line hidden
            
            #line 448 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncedInvoker ));
            
            #line default
            #line hidden
            
            #line 448 ""
            this.Write("\n                default:\n                goto case 9;\n            }\n        }\n  " +
                    "      ");
            
            #line default
            #line hidden
            
            #line 453 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncedItems ));
            
            #line default
            #line hidden
            
            #line 453 ""
            this.Write("\n    }\n}");
            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        public virtual void Initialize() {
        }
    }
    
    public class ConnecthubTemplateBase {
        
        private global::System.Text.StringBuilder builder;
        
        private global::System.Collections.Generic.IDictionary<string, object> session;
        
        private global::System.CodeDom.Compiler.CompilerErrorCollection errors;
        
        private string currentIndent = string.Empty;
        
        private global::System.Collections.Generic.Stack<int> indents;
        
        private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();
        
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session {
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
        
        public global::System.Text.StringBuilder GenerationEnvironment {
            get {
                if ((this.builder == null)) {
                    this.builder = new global::System.Text.StringBuilder();
                }
                return this.builder;
            }
            set {
                this.builder = value;
            }
        }
        
        protected global::System.CodeDom.Compiler.CompilerErrorCollection Errors {
            get {
                if ((this.errors == null)) {
                    this.errors = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errors;
            }
        }
        
        public string CurrentIndent {
            get {
                return this.currentIndent;
            }
        }
        
        private global::System.Collections.Generic.Stack<int> Indents {
            get {
                if ((this.indents == null)) {
                    this.indents = new global::System.Collections.Generic.Stack<int>();
                }
                return this.indents;
            }
        }
        
        public ToStringInstanceHelper ToStringHelper {
            get {
                return this._toStringHelper;
            }
        }
        
        public void Error(string message) {
            this.Errors.Add(new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message));
        }
        
        public void Warning(string message) {
            global::System.CodeDom.Compiler.CompilerError val = new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message);
            val.IsWarning = true;
            this.Errors.Add(val);
        }
        
        public string PopIndent() {
            if ((this.Indents.Count == 0)) {
                return string.Empty;
            }
            int lastPos = (this.currentIndent.Length - this.Indents.Pop());
            string last = this.currentIndent.Substring(lastPos);
            this.currentIndent = this.currentIndent.Substring(0, lastPos);
            return last;
        }
        
        public void PushIndent(string indent) {
            this.Indents.Push(indent.Length);
            this.currentIndent = (this.currentIndent + indent);
        }
        
        public void ClearIndent() {
            this.currentIndent = string.Empty;
            this.Indents.Clear();
        }
        
        public void Write(string textToAppend) {
            this.GenerationEnvironment.Append(textToAppend);
        }
        
        public void Write(string format, params object[] args) {
            this.GenerationEnvironment.AppendFormat(format, args);
        }
        
        public void WriteLine(string textToAppend) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendLine(textToAppend);
        }
        
        public void WriteLine(string format, params object[] args) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendFormat(format, args);
            this.GenerationEnvironment.AppendLine();
        }
        
        public class ToStringInstanceHelper {
            
            private global::System.IFormatProvider formatProvider = global::System.Globalization.CultureInfo.InvariantCulture;
            
            public global::System.IFormatProvider FormatProvider {
                get {
                    return this.formatProvider;
                }
                set {
                    if ((value != null)) {
                        this.formatProvider = value;
                    }
                }
            }
            
            public string ToStringWithCulture(object objectToConvert) {
                if ((objectToConvert == null)) {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                global::System.Type type = objectToConvert.GetType();
                global::System.Type iConvertibleType = typeof(global::System.IConvertible);
                if (iConvertibleType.IsAssignableFrom(type)) {
                    return ((global::System.IConvertible)(objectToConvert)).ToString(this.formatProvider);
                }
                global::System.Reflection.MethodInfo methInfo = type.GetMethod("ToString", new global::System.Type[] {
                            iConvertibleType});
                if ((methInfo != null)) {
                    return ((string)(methInfo.Invoke(objectToConvert, new object[] {
                                this.formatProvider})));
                }
                return objectToConvert.ToString();
            }
        }
    }
}
