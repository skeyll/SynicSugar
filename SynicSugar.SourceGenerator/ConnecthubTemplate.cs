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
                    " Cysharp.Threading.Tasks;\nusing SynicSugar.RTC;\nnamespace SynicSugar.P2P {\n    i" +
                    "nternal sealed class ConnectHub : IPacketReciver {\n        //Singleton\n        p" +
                    "rivate static Lazy<ConnectHub> instance = new Lazy<ConnectHub>();\n        public" +
                    " static ConnectHub Instance => instance.Value;\n\n        public ConnectHub(){}\n  " +
                    "      byte ch_r;\n        string id_r;\n        ArraySegment<byte> payload_r;\n    " +
                    "    //Start\n        /// <summary>\n        /// Start the packet receiver. Call af" +
                    "ter creating the Network Instance required for reception.<br />\n        /// This" +
                    " cannot be called with other Receiver same time. If start the other Receiver, Co" +
                    "nenctHub stop this Receiver automatically before start the new one.\n        /// " +
                    "</summary>\n        /// <param name=\"receiveTiming\">The timing that packet receiv" +
                    "er gets packet from buffer.</param>\n        /// <param name=\"maxBatchSize\">How m" +
                    "any times during 1 FPS are received</param>\n        public void StartPacketRecei" +
                    "ver(PacketReceiveTiming receiveTiming = PacketReceiveTiming.Update, byte maxBatc" +
                    "hSize = 1){\n            p2pConnectorForOtherAssembly.Instance.StartPacketReceive" +
                    "r(this, receiveTiming, maxBatchSize);\n        }\n        \n        /// <summary>\n " +
                    "       /// To get only SynicPacket in burst FPS. Call after creating the Network" +
                    " Instance required for reception.<br />\n        /// This cannot be called with o" +
                    "ther Receiver same time. If start the other Receiver, ConenctHub stop this Recei" +
                    "ver automatically before start the new one.\n        /// </summary>\n        publi" +
                    "c void StartSynicReceiver(){\n            if(p2pConnectorForOtherAssembly.Instanc" +
                    "e.p2pToken != null && !p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancella" +
                    "tionRequested){\n                p2pConnectorForOtherAssembly.Instance.p2pToken.C" +
                    "ancel();\n            }\n\n            p2pConnectorForOtherAssembly.Instance.p2pTok" +
                    "en = new CancellationTokenSource();\n\n            ReciveSynicPackets(p2pConnector" +
                    "ForOtherAssembly.Instance.p2pToken.Token).Forget();\n        }\n        //Pause re" +
                    "ceiver\n        /// <summary>\n        /// Pause getting a packet from the buffer." +
                    " To re-start, call StartPacketReceiver().<br />\n        /// *Packet receiving to" +
                    " the buffer is continue. If the packet is over the buffer, subsequent packets ar" +
                    "e discarded.\n        /// </summary>\n        public void PausePacketReceiver(){\n " +
                    "           if(p2pConnectorForOtherAssembly.Instance.p2pToken != null && !p2pConn" +
                    "ectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){\n              " +
                    "  p2pConnectorForOtherAssembly.Instance.StopPacketReceiving();\n                p" +
                    "2pConnectorForOtherAssembly.Instance.p2pToken.Cancel();\n            }\n        }\n" +
                    "\n        //Pause Reciving buffer\n        /// <summary>\n        /// Pause receivi" +
                    "ng a packet to the receive buffer. To re-start, call RestartConnections(). <br /" +
                    ">\n        /// After call this, packets will have been discarded until connection" +
                    " will re-open.<br />\n        /// WARNING: This doesn\'t work as intended now. Can" +
                    "\'t stop receiving packets to buffer, so SynicSugar discard those packets before " +
                    "re-start.\n        /// </summary>\n        /// <param name=\"isForced\">If True, for" +
                    "ce to stop and clear current packet queue. <br />\n        /// If false, process " +
                    "current queue, then stop it.</param>\n        public async UniTask PauseConnectio" +
                    "ns(bool isForced = false, CancellationTokenSource cancelToken = default(Cancella" +
                    "tionTokenSource)){\n            if(cancelToken == default(CancellationTokenSource" +
                    ")){\n                cancelToken = new CancellationTokenSource();\n            }\n " +
                    "           await p2pConnectorForOtherAssembly.Instance.PauseConnections(isForced" +
                    ", cancelToken.Token);\n        }\n        /// <summary>\n        /// Prepare to rec" +
                    "eive packets in advance. If user sent a packet, it can also open connection to g" +
                    "et packets without this.\n        /// </summary>\n        public void RestartConne" +
                    "ctions(){\n            p2pConnectorForOtherAssembly.Instance.RestartConnections()" +
                    ";\n            StartPacketReceiver();\n        }\n        \n        /// <summary>\n  " +
                    "      /// Stop receiver, close all connections and remove the notify events.\n   " +
                    "     /// Then, the user leave the lobby.<br />\n        /// The last user closes " +
                    "the lobby in Backend.\n        /// <param name=\"destroyManager\">Destroy NetworkMa" +
                    "nager after exit lobby.</param>\n        /// <param name=\"cancelToken\">Cancel tok" +
                    "en for this task</param>\n        /// </summary>\n        public async UniTask<Res" +
                    "ult> ExitSession(bool destroyManager = true, CancellationToken cancelToken = def" +
                    "ault(CancellationToken)){\n            if(cancelToken == default(CancellationToke" +
                    "n)){\n                cancelToken = p2pConnectorForOtherAssembly.Instance.gameObj" +
                    "ect.GetCancellationTokenOnDestroy();\n            }\n            Result isSuccess " +
                    "= await p2pConnectorForOtherAssembly.Instance.ExitSession(destroyManager, cancel" +
                    "Token);\n            ClearReferenceDictionaries();\n            return isSuccess;\n" +
                    "        }\n        /// <summary>\n        /// Stop receiver, close all connections" +
                    " and remove the notify events.\n        /// Then, Host closes and Guest leaves th" +
                    "e Lobby.<br />\n        /// When Host closes Lobby, Guests are automatically kick" +
                    "ed out from the Lobby.\n        /// <param name=\"destroyManager\">Destroy NetworkM" +
                    "anager after exit lobby.</param>\n        /// <param name=\"cancelToken\">Cancel to" +
                    "ken for this task</param>\n        /// </summary>\n        public async UniTask<Re" +
                    "sult> CloseSession(bool destroyManager = true, CancellationToken cancelToken = d" +
                    "efault(CancellationToken)){\n            if(cancelToken == default(CancellationTo" +
                    "ken)){\n                cancelToken = p2pConnectorForOtherAssembly.Instance.gameO" +
                    "bject.GetCancellationTokenOnDestroy();\n            }\n            Result isSucces" +
                    "s = await p2pConnectorForOtherAssembly.Instance.CloseSession(destroyManager, can" +
                    "celToken);\n            ClearReferenceDictionaries();\n            return isSucces" +
                    "s;\n        }\n\n        async UniTask ReciveSynicPackets(CancellationToken token){" +
                    "\n            int count = p2pConfig.Instance.SynicReceiverBatchSize;\n\n           " +
                    " while(!token.IsCancellationRequested){\n                bool recivePacket = p2pC" +
                    "onnectorForOtherAssembly.Instance.GetSynicPacketFromBuffer(ref ch_r, ref id_r, r" +
                    "ef payload_r);\n                count--;\n\n                if(recivePacket){\n     " +
                    "               ConvertFromPacket(ref ch_r, ref id_r, ref payload_r);\n           " +
                    "     }\n\n                if(count == 0 || !recivePacket){\n                    awa" +
                    "it UniTask.Yield(PlayerLoopTiming.Update, cancellationToken : token);\n          " +
                    "          \n                    if(p2pConnectorForOtherAssembly.Instance == null)" +
                    "{\n                        break;\n                    }\n                    count" +
                    " = p2pConfig.Instance.SynicReceiverBatchSize;\n                }\n            }\n  " +
                    "      }\n\n        //(for elements)\n        public enum CHANNELLIST{\n            ");
            
            #line default
            #line hidden
            
            #line 146 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncList ));
            
            #line default
            #line hidden
            
            #line 146 ""
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
            
            #line 156 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( Reference ));
            
            #line default
            #line hidden
            
            #line 156 ""
            this.Write("\n\n        //Clear ref\n        private void ClearReferenceDictionaries(){ ");
            
            #line default
            #line hidden
            
            #line 159 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( ClearReference ));
            
            #line default
            #line hidden
            
            #line 159 ""
            this.Write("\n            synicBuffer.Clear();\n            synicPacketInfo.Clear();\n          " +
                    "  largeBuffer.Clear();\n            largePacketInfo.Clear();\n        }\n\n        /" +
                    "/Register(for class)");
            
            #line default
            #line hidden
            
            #line 166 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( Register ));
            
            #line default
            #line hidden
            
            #line 166 ""
            this.Write(@"
        
        /// <summary>
        /// Get the NetworkPlayer instance registered with ConnectHub.
        /// </summary>
        /// <param name=""id"">UserID to get</param>
        /// <returns>T's instance</returns>
        public T GetUserInstance<T>(UserId id) where T : IGetPlayer {");
            
            #line default
            #line hidden
            
            #line 173 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( PlayeInstance ));
            
            #line default
            #line hidden
            
            #line 173 ""
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
            
            #line 181 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( CommonsInstance ));
            
            #line default
            #line hidden
            
            #line 181 ""
            this.Write("\n            return default(T);\n        }\n\n        //SendPacket(for elements)\n   " +
                    "     public void ConvertFromPacket(ref byte ch, ref string id, ref ArraySegment<" +
                    "byte> payload){\n            switch((CHANNELLIST)ch){");
            
            #line default
            #line hidden
            
            #line 187 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( PacketConvert ));
            
            #line default
            #line hidden
            
            #line 187 ""
            this.Write("\n                case CHANNELLIST.ObtainPing:\n                    EOSp2p.SendPack" +
                    "et((byte)CHANNELLIST.ReturnPong, payload, UserId.GetUserId(id));\n               " +
                    " return;\n                case CHANNELLIST.ReturnPong:\n                    p2pCon" +
                    "nectorForOtherAssembly.Instance.GetPong(id, payload);\n                return;\n  " +
                    "              case CHANNELLIST.Synic:\n                    bool restoredPacket = " +
                    "RestoreSynicPackets(ref ch, ref id, ref payload);\n                    if(!restor" +
                    "edPacket){\n    #if SYNICSUGAR_LOG \n                        Debug.LogFormat(\"Conv" +
                    "ertFormPacket: Restore packet is in progress. for {0}\", id);\n    #endif\n        " +
                    "                return;\n                    }\n                    SyncedSynic(id" +
                    ");\n\n                    p2pConnectorForOtherAssembly.Instance.UpdateSyncedState(" +
                    "id, synicPacketInfo[id].phase);\n\n                    //Init\n                    " +
                    "synicBuffer.Remove(id);\n                    synicPacketInfo.Remove(id);\n\n       " +
                    "             //Change AcceptHostsSynic flag.\n                    if(p2pInfo.Inst" +
                    "ance.IsLoaclUser(id)){\n                        p2pConnectorForOtherAssembly.Inst" +
                    "ance.CloseHostSynic();\n                    }\n                    \n              " +
                    "  return;\n            }\n        }\n\n        /// <summary>\n        /// Re-Send RPC" +
                    " with last recorded information.<br />\n        /// To send disconnected peers af" +
                    "ter some time. SynicSugar retransmit to connecting-peers.<br />\n        /// To r" +
                    "ecord, pass true to attribute.\n        /// </summary>\n        public void Resend" +
                    "LastRPC(){\n            if(p2pInfo.Instance.LastRPCIsLargePacket){\n              " +
                    "  EOSp2p.SendLargePacketsToAll(p2pInfo.Instance.LastRPCch, p2pInfo.Instance.Last" +
                    "RPCPayload).Forget();\n                return;\n            }\n            EOSp2p.S" +
                    "endPacketToAll(p2pInfo.Instance.LastRPCch, p2pInfo.Instance.LastRPCPayload).Forg" +
                    "et();\n        }\n        /// <summary>\n        /// Re-Send RPC to the specific ta" +
                    "rget with last recorded information.<br />\n        /// In order to send disconne" +
                    "cted peers after the some time. SynicSugar has retransmission to connecting-peer" +
                    "s for the reliability.<br />\n        /// To record, pass true to attribute.\n    " +
                    "    /// </summary>\n        /// <param name=\"target\"></param>\n        public void" +
                    " ResendLastRPCToTarget(UserId target){\n            if(p2pInfo.Instance.LastRPCIs" +
                    "LargePacket){\n                EOSp2p.SendLargePackets(p2pInfo.Instance.LastRPCch" +
                    ", p2pInfo.Instance.LastRPCPayload, target).Forget();\n                return;\n   " +
                    "         }\n            EOSp2p.SendPacket(p2pInfo.Instance.LastRPCch, p2pInfo.Ins" +
                    "tance.LastRPCPayload, target);\n        }\n        /// <summary>\n        /// Re-Se" +
                    "nd TargetRPC with last recorded information.<br />\n        /// In order to send " +
                    "disconnected peers after the some time. SynicSugar has retransmission to connect" +
                    "ing-peers for the reliability.<br />\n        /// To record, pass true to attribu" +
                    "te.\n        /// </summary>\n        public void ResendLastTargetRPC(){\n          " +
                    "  if(p2pInfo.Instance.LastTargetRPCIsLargePacket){\n                EOSp2p.SendLa" +
                    "rgePackets(p2pInfo.Instance.LastTargetRPCch, p2pInfo.Instance.LastTargetRPCPaylo" +
                    "ad, p2pInfo.Instance.LastTargetRPCUserId).Forget();\n                return;\n    " +
                    "        }\n            EOSp2p.SendPacket(p2pInfo.Instance.LastTargetRPCch, p2pInf" +
                    "o.Instance.LastTargetRPCPayload, p2pInfo.Instance.LastTargetRPCUserId);\n        " +
                    "}\n\n        ");
            
            #line default
            #line hidden
            
            #line 257 ""
 if (needSyncSynic) { 
            
            #line default
            #line hidden
            
            #line 258 ""
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
                    " synicContainer = GenerateSynicContainer(p2pInfo.Instance.LocalUserId, syncedPha" +
                    "se, syncSinglePhase);\n\n            using var selfCompressor  = new BrotliCompres" +
                    "sor();\n            MemoryPackSerializer.Serialize(selfCompressor, synicContainer" +
                    ");\n\n            EOSp2p.SendSynicPackets((byte)CHANNELLIST.Synic, selfCompressor." +
                    "ToArray(), targetId, p2pInfo.Instance.LocalUserId, syncedPhase, syncSinglePhase)" +
                    ";\n\n            if(type == SynicType.OnlySelf || !p2pInfo.Instance.IsHost()){\n   " +
                    "             return;\n            }\n            if(type == SynicType.WithOthers){" +
                    "\n                foreach(var id in p2pInfo.Instance.DisconnectedUserIds){\n      " +
                    "              synicContainer = GenerateSynicContainer(id, syncedPhase, syncSingl" +
                    "ePhase);\n\n                    using var targetCompressor  = new BrotliCompressor" +
                    "();\n                    MemoryPackSerializer.Serialize(targetCompressor, synicCo" +
                    "ntainer);\n\n                    EOSp2p.SendSynicPackets((byte)CHANNELLIST.Synic, " +
                    "targetCompressor.ToArray(), targetId, id, syncedPhase, syncSinglePhase);\n       " +
                    "             \n                    await UniTask.Yield();\n                }\n     " +
                    "       }\n            \n            //Sync target data in local to target local\n  " +
                    "          synicContainer = GenerateSynicContainer(targetId, syncedPhase, syncSin" +
                    "glePhase);\n\n            using var reconnecterCompressor  = new BrotliCompressor(" +
                    ");\n            MemoryPackSerializer.Serialize(reconnecterCompressor, synicContai" +
                    "ner);\n\n            EOSp2p.SendSynicPackets((byte)CHANNELLIST.Synic, reconnecterC" +
                    "ompressor.ToArray(), targetId, targetId, syncedPhase, syncSinglePhase);\n        " +
                    "}\n        \n        SynicContainer GenerateSynicContainer(UserId id, byte syncedP" +
                    "hase, bool syncSinglePhase){\n            SynicContainer synicContainer = new Syn" +
                    "icContainer();\n            switch(syncedPhase){");
            
            #line default
            #line hidden
            
            #line 302 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( GenerateSynicContainer ));
            
            #line default
            #line hidden
            
            #line 302 ""
            this.Write("\n                default:\n                goto case 9;\n            }\n            " +
                    "return synicContainer;\n        }\n        ");
            
            #line default
            #line hidden
            
            #line 308 ""
 } 
            
            #line default
            #line hidden
            
            #line 309 ""
            this.Write("        \n        //Synced 0 = index, 1 = additional packet amount\n        bool Re" +
                    "storeLargePackets(ref byte ch, ref string id, ref ArraySegment<byte> payload){\n " +
                    "           //Prep\n            if(!largeBuffer.ContainsKey(id)){\n                " +
                    "largePacketInfo.Add(id, new Dictionary<byte, LargePacketsInfomation>());\n       " +
                    "         largeBuffer.Add(id, new Dictionary<byte, byte[]>());\n            }\n    " +
                    "        if(!largeBuffer[id].ContainsKey(ch)){\n                largePacketInfo[id" +
                    "].Add(ch, new LargePacketsInfomation(){ additionalPacketsAmount = payload[1] });" +
                    "\n                //Prep enough byte[]\n                largeBuffer[id].Add(ch, ne" +
                    "w byte[(payload[1] + 1) * EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE]);\n            }\n\n " +
                    "           int packetIndex = payload[0];\n            int offset = packetIndex * " +
                    "EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE;\n\n    #if SYNICSUGAR_LOG\n            Debug.Lo" +
                    "g($\"RestoreLargePackets: PacketInfo:: ch {ch} / index {payload[0]} / chunk {payl" +
                    "oad[1]}\");\n    #endif\n            //Remove header\n            Span<byte> packetP" +
                    "ayload = payload.Slice(2);\n            largePacketInfo[id][ch].currentSize += pa" +
                    "cketPayload.Length;\n            //Copy Byte from what come in\n            Buffer" +
                    ".BlockCopy(packetPayload.ToArray(), 0, largeBuffer[id][ch], offset, packetPayloa" +
                    "d.Length);\n\n            //Comming all?\n            //We don\'t know real packet s" +
                    "ize. So we need + 1166.\n            //This first conditon for empty packet.\n    " +
                    "        return largePacketInfo[id][ch].additionalPacketsAmount == 0 || largePack" +
                    "etInfo[id][ch].currentSize + EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE > largeBuffer[id" +
                    "][ch].Length ? true : false;\n        }\n\n        // 0-packet index, 1-additional " +
                    "packet amount, 2-complex data[1bit-isOnly, 4bits-phase, 3bits userType], 3-data\'" +
                    "s user index\n        bool RestoreSynicPackets(ref byte ch, ref string id, ref Ar" +
                    "raySegment<byte> payload){\n            //Set target id\n            int userDataT" +
                    "ype = (int)(payload[2] & 0x07);\n            if(userDataType == 0){\n             " +
                    "   if(p2pInfo.Instance.IsHost(id) && p2pInfo.Instance.IsReconnecter){\n          " +
                    "          id = p2pInfo.Instance.LocalUserId.ToString();\n                }else{\n " +
                    "                   return false;\n                }\n            }else if(userData" +
                    "Type == 2){\n                if(p2pInfo.Instance.IsHost(id) && p2pInfo.Instance.I" +
                    "sReconnecter){\n                    id = p2pInfo.Instance.AllUserIds[payload[3]]." +
                    "ToString();\n                }else{\n                    return false;\n           " +
                    "     }\n            }\n\n            if(!synicBuffer.ContainsKey(id)){\n            " +
                    "    synicPacketInfo.Add(id, new SynicPacketInfomation(){  basis = new (){ additi" +
                    "onalPacketsAmount = payload[1]}, \n                                              " +
                    "                              phase = (byte)((payload[2] >> 3) & 0x0F), \n       " +
                    "                                                                     isSinglePha" +
                    "se = (payload[2] & 0x80) != 0 });\n                //Prep enough byte[]\n         " +
                    "       synicBuffer.Add(id, new byte[(payload[1] + 1) * EOSp2p.MAX_LARGEPACKET_PA" +
                    "YLOADSIZE]);\n            }\n            int packetIndex = payload[0];\n           " +
                    " int offset = packetIndex * EOSp2p.MAX_LARGEPACKET_PAYLOADSIZE;\n\n    #if SYNICSU" +
                    "GAR_LOG\n            Debug.Log($\"RestoreSynicPackets: PacketInfo:: index {payload" +
                    "[0]} / chunk {payload[1]} / Data\'s userID {id}\");\n    #endif\n            //Remov" +
                    "e header\n            Span<byte> packetPayload = payload.Slice(4);\n            sy" +
                    "nicPacketInfo[id].basis.currentSize += packetPayload.Length;\n            //Copy " +
                    "Byte from what come in\n            Buffer.BlockCopy(packetPayload.ToArray(), 0, " +
                    "synicBuffer[id], offset, packetPayload.Length);\n            //Comming all?\n     " +
                    "       //We don\'t know real packet size. So we need + 1166.\n            //This f" +
                    "irst conditon for empty packet.\n            return synicPacketInfo[id].basis.add" +
                    "itionalPacketsAmount == 0 || synicPacketInfo[id].basis.currentSize + EOSp2p.MAX_" +
                    "LARGEPACKET_PAYLOADSIZE > synicBuffer[id].Length ? true : false;\n        }\n\n    " +
                    "    /// <summary>\n        /// Call from ConvertFormPacket.\n        /// </summary" +
                    ">\n        void SyncedSynic(string overwriterUserId){\n            //Deserialize p" +
                    "acket\n            using var decompressor = new BrotliDecompressor();\n           " +
                    " Span<byte> transmittedPaylaod = new Span<byte>(synicBuffer[overwriterUserId]);\n" +
                    "\n            var decompressedBuffer = decompressor.Decompress(transmittedPaylaod" +
                    ".Slice(0, synicPacketInfo[overwriterUserId].basis.currentSize));\n            Syn" +
                    "icContainer container = MemoryPackSerializer.Deserialize<SynicContainer>(decompr" +
                    "essedBuffer);\n#if SYNICSUGAR_LOG\n            Debug.Log($\"SyncedSynic: Deserializ" +
                    "e is Success for {overwriterUserId}\");\n    #endif\n\n            //Packet data\n   " +
                    "         int phase = synicPacketInfo[overwriterUserId].phase;\n            bool s" +
                    "yncSinglePhase = synicPacketInfo[overwriterUserId].isSinglePhase;\n\n            s" +
                    "witch(phase){");
            
            #line default
            #line hidden
            
            #line 401 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncedInvoker ));
            
            #line default
            #line hidden
            
            #line 401 ""
            this.Write("\n                default:\n                goto case 9;\n            }\n        }\n  " +
                    "      ");
            
            #line default
            #line hidden
            
            #line 406 ""
            this.Write(this.ToStringHelper.ToStringWithCulture( SyncedItems ));
            
            #line default
            #line hidden
            
            #line 406 ""
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
