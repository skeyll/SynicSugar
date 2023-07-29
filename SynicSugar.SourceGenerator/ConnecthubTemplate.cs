﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン: 16.0.0.0
//  
//     このファイルへの変更は、正しくない動作の原因になる可能性があり、
//     コードが再生成されると失われます。
// </auto-generated>
// ------------------------------------------------------------------------------
namespace SynicSugarGenerator
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class ConnecthubTemplate : ConnecthubTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("// <auto-generated>\r\n// THIS (.cs) FILE IS GENERATED BY SynicSugarGenerator. DO N" +
                    "OT CHANGE IT.\r\n// </auto-generated>\r\n#pragma warning disable CS0164 // This labe" +
                    "l has not been referenced\r\n#pragma warning disable CS0436 // Type conflicts with" +
                    " the imported type\r\n\r\nusing UnityEngine;\r\nusing MemoryPack;\r\nusing MemoryPack.Co" +
                    "mpression;\r\nusing System;\r\nusing System.Collections.Generic;\r\nusing System.Threa" +
                    "ding;\r\nusing Cysharp.Threading.Tasks;\r\nnamespace SynicSugar.P2P {\r\n    internal " +
                    "sealed class ConnectHub {\r\n        //Singleton\r\n        private static Lazy<Conn" +
                    "ectHub> instance = new Lazy<ConnectHub>();\r\n        public static ConnectHub Ins" +
                    "tance => instance.Value;\r\n\r\n        public ConnectHub(){\r\n        }\r\n        //S" +
                    "tart\r\n        /// <summary>\r\n        /// Start the packet receiver. Call after c" +
                    "reating the Network Instance required for reception.\r\n        /// </summary>\r\n  " +
                    "      // Default Relay Setting is AllowRelay. To change this, get NatType.\r\n    " +
                    "    public void StartPacketReceiver(){\r\n            if(p2pConnectorForOtherAssem" +
                    "bly.Instance.p2pToken == null || p2pConnectorForOtherAssembly.Instance.p2pToken." +
                    "IsCancellationRequested){\r\n                p2pConnectorForOtherAssembly.Instance" +
                    ".p2pToken = new CancellationTokenSource();\r\n                RecivePacket().Forge" +
                    "t();\r\n            }\r\n        }\r\n        //Pause receiver\r\n        /// <summary>\r" +
                    "\n        /// Pause getting a packet from the buffer. To re-start, call StartPack" +
                    "etReceiver().<br />\r\n        /// *Packet receiving to the buffer is continue. If" +
                    " the packet is over the buffer, subsequent packets are discarded.\r\n        /// <" +
                    "/summary>\r\n        public void PausetPacketReceiver(){\r\n            if(p2pConnec" +
                    "torForOtherAssembly.Instance.p2pToken != null && !p2pConnectorForOtherAssembly.I" +
                    "nstance.p2pToken.IsCancellationRequested){\r\n                p2pConnectorForOther" +
                    "Assembly.Instance.p2pToken.Cancel();\r\n            }\r\n        }\r\n\r\n        //Paus" +
                    "e Reciving buffer\r\n        /// <summary>\r\n        /// Pause receiving a packet t" +
                    "o the receive buffer. To re-start, call RestartConnections(). <br />\r\n        //" +
                    "/ After call this, packets will have been discarded until connection will re-ope" +
                    "n.<br />\r\n        /// WARNING: This doesn\'t work as intended now. Can\'t stop rec" +
                    "eiving packets to buffer, so SynicSugar discard those packets before re-start.\r\n" +
                    "        /// </summary>\r\n        /// <param name=\"isForced\">If True, force to sto" +
                    "p and clear current packet queue. <br />\r\n        /// If false, process current " +
                    "queue, then stop it.</param>\r\n        public async UniTask PauseConnections(bool" +
                    " isForced = false, CancellationTokenSource cancelToken = default(CancellationTok" +
                    "enSource)){\r\n            await p2pConnectorForOtherAssembly.Instance.PauseConnec" +
                    "tions(isForced, cancelToken);\r\n        }\r\n        /// <summary>\r\n        /// Pre" +
                    "pare to receive packets in advance. If user sent a packet, it can also open conn" +
                    "ection to get packets without this.\r\n        /// </summary>\r\n        public void" +
                    " RestartConnections(){\r\n            p2pConnectorForOtherAssembly.Instance.Restar" +
                    "tConnections();\r\n            RecivePacket().Forget();\r\n        }\r\n        \r\n    " +
                    "    /// <summary>\r\n        /// Stop receiver, close all connections and remove t" +
                    "he notify events.\r\n        /// Then, the user leave the lobby.<br />\r\n        //" +
                    "/ To exit from lobby alone during a game(= not whole, only one battle). Usually " +
                    "use CloseSession().\r\n        /// </summary>\r\n        public async UniTask<bool> " +
                    "ExitSession(CancellationTokenSource cancelToken = default(CancellationTokenSourc" +
                    "e)){\r\n            bool isSuccess = await p2pConnectorForOtherAssembly.Instance.E" +
                    "xitSession(cancelToken.Token);\r\n            return isSuccess;\r\n        }\r\n      " +
                    "  /// <summary>\r\n        /// Stop receiver, close all connections and remove the" +
                    " notify events.<br />\r\n        /// Then, Host closees and Guest leaves the lobby" +
                    ".\r\n        /// </summary>\r\n        public async UniTask<bool> CloseSession(Cance" +
                    "llationTokenSource cancelToken = default(CancellationTokenSource)){\r\n           " +
                    " bool isSuccess = await p2pConnectorForOtherAssembly.Instance.CloseSession(cance" +
                    "lToken.Token);\r\n            return isSuccess;\r\n        }\r\n        async UniTask " +
                    "RecivePacket(){\r\n            while(!p2pConnectorForOtherAssembly.Instance.p2pTok" +
                    "en.IsCancellationRequested){\r\n                SugarPacket recivePacket = p2pConn" +
                    "ectorForOtherAssembly.Instance.GetPacketFromBuffer();\r\n\r\n                if(reci" +
                    "vePacket != null){\r\n                    ConnectHub.Instance.ConvertFromPacket(re" +
                    "civePacket);\r\n                }\r\n                await UniTask.Delay(p2pConnecto" +
                    "rForOtherAssembly.Instance.receiverInterval);\r\n\r\n                if(p2pConnector" +
                    "ForOtherAssembly.Instance.p2pToken.IsCancellationRequested){\r\n                  " +
                    "  break;\r\n                }\r\n            }\r\n        }\r\n\r\n        //(for elements" +
                    ")\r\n        public enum CHANNELLIST{\r\n            ");
            
            #line 105 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(SyncList));
            
            #line default
            #line hidden
            this.Write("\r\n        }\r\n        //For Synic\r\n        Dictionary<string, byte[]> buffer = new" +
                    " Dictionary<string, byte[]>();\r\n        Dictionary<string, LargePacketInfomation" +
                    "> packetInfo = new Dictionary<string, LargePacketInfomation>();\r\n\r\n        //Ref" +
                    "(for class)");
            
            #line 111 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Reference));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n        //Register(for class)");
            
            #line 113 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Register));
            
            #line default
            #line hidden
            this.Write(@"
        
        /// <summary>
        /// Get the NetworkPlayer instance registered with ConnectHub.
        /// </summary>
        /// <param name=""id"">UserID to get</param>
        /// <returns>T's instance</returns>
        public T GetUserInstance<T>(UserId id) where T : IGetPlayer {");
            
            #line 120 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(PlayeInstance));
            
            #line default
            #line hidden
            this.Write(@"
            return default(T);
        }
        
        /// <summary>
        /// Get the NetworkCommons instance registered with ConnectHub.
        /// </summary>
        /// <returns>T's instance</returns>
        public T GetInstance<T>() where T : IGetCommons {");
            
            #line 128 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(CommonsInstance));
            
            #line default
            #line hidden
            this.Write("\r\n            return default(T);\r\n        }\r\n\r\n        //SendPacket(for elements)" +
                    "\r\n        public void ConvertFromPacket(SugarPacket packet){\r\n            switch" +
                    "((CHANNELLIST)packet.ch){");
            
            #line 134 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(PacketConvert));
            
            #line default
            #line hidden
            this.Write(@"
                case CHANNELLIST.Synic:
                    bool restoredPacket = RestorePackets(ref packet);
                    if(!restoredPacket){
    #if SYNICSUGAR_LOG 
                        Debug.LogFormat(""ConvertFormPacket: Restore packet is in progress. for {0}"", packet.UserID);
    #endif
                        return;
                    }
                    SyncedSynic(packet.UserID);
                    //Change AcceptHostsSynic flag.
                    if(p2pInfo.Instance.IsLoaclUser(packet.UserID)){
                        p2pConnectorForOtherAssembly.Instance.CloseHostSynic();
                    }
                return;
            }
        }

        ");
            
            #line 152 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
 if (needSyncSynic) { 
            
            #line default
            #line hidden
            this.Write("\r\n        /// <summary>\r\n        /// Sync all Synic variables. This is very heavy" +
                    " because it handles multiple data and repeats compression and serialization.\r\n  " +
                    "      /// </summary>\r\n        /// <param name=\"targetId\">Target to be synced by " +
                    "this local user.</param>\r\n        /// <param name=\"syncedHierarchy\">Hierarchy to" +
                    " be synced. If syncSingleHierarchy is false, sync all variables in the hierarchy" +
                    " up to this point.</param>\r\n        /// <param name=\"syncSingleHierarchy\">If tru" +
                    "e, send only variables in syncedHierarchy.</param>\r\n        /// <param name=\"syn" +
                    "cTargetsData\">If true, sync target\'s data in Host local. When the target AllowHo" +
                    "stsSynic, can overwrite the target\'s data in that local only once.</param>\r\n    " +
                    "    public void SyncSynic(UserId targetId, byte syncedHierarchy = 9, bool syncSi" +
                    "ngleHierarchy = false, bool syncTargetsData = true){\r\n            //Sync local d" +
                    "ata to target local\r\n            SynicContainer synicContainer = GenerateSynicCo" +
                    "ntainer(p2pInfo.Instance.LocalUserId, syncedHierarchy, syncSingleHierarchy);\r\n\r\n" +
                    "            using var selfCompressor  = new BrotliCompressor();\r\n            Mem" +
                    "oryPackSerializer.Serialize(selfCompressor, synicContainer);\r\n\r\n            EOSp" +
                    "2p.SendLargePacket((byte)CHANNELLIST.Synic, selfCompressor.ToArray(), targetId, " +
                    "syncedHierarchy, syncSingleHierarchy);\r\n\r\n            if(!syncTargetsData || !p2" +
                    "pInfo.Instance.IsHost()){\r\n                return;\r\n            }\r\n            /" +
                    "/Sync target data in local to target local\r\n\r\n            synicContainer = Gener" +
                    "ateSynicContainer(targetId, syncedHierarchy, syncSingleHierarchy);\r\n\r\n          " +
                    "  using var targetCompressor  = new BrotliCompressor();\r\n            MemoryPackS" +
                    "erializer.Serialize(targetCompressor, synicContainer);\r\n\r\n            EOSp2p.Sen" +
                    "dLargePacket((byte)CHANNELLIST.Synic, targetCompressor.ToArray(), targetId, sync" +
                    "edHierarchy, syncSingleHierarchy);\r\n        }\r\n\r\n        ");
            
            #line 183 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GenerateSynicContainer));
            
            #line default
            #line hidden
            this.Write("\r\n        ");
            
            #line 184 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\n        //Synced 0 = index, 1 = chunk, 2 = hierarchy, 3 = syncSpecificHierarchy" +
                    ", 4 = isSelf\r\n        bool RestorePackets(ref SugarPacket packet){\r\n            " +
                    "if(packet.payload[4] == 0){\r\n                if(p2pInfo.Instance.IsHost(packet.U" +
                    "serID) && p2pInfo.Instance.AcceptHostSynic){\r\n                    packet.UserID " +
                    "= p2pInfo.Instance.LocalUserId.ToString();\r\n                }else{\r\n            " +
                    "        return false;\r\n                }\r\n            }\r\n\r\n            if(!buffe" +
                    "r.ContainsKey(packet.UserID)){\r\n                packetInfo.Add(packet.UserID, ne" +
                    "w LargePacketInfomation(){  chunk = packet.payload[1], \r\n                       " +
                    "                                                     hierarchy = packet.payload[" +
                    "2], \r\n                                                                          " +
                    "  syncSpecificHierarchy = packet.payload[3] == 1 ? true : false });\r\n           " +
                    "     //Prep enough byte[]\r\n                buffer.Add(packet.UserID, new byte[pa" +
                    "cket.payload[1] * 1100]);\r\n            }\r\n            int packetIndex = packet.p" +
                    "ayload[0];\r\n            int offset = packetIndex * 1100;\r\n\r\n    #if SYNICSUGAR_L" +
                    "OG\r\n            Debug.Log($\"RestorePackets: PacketInfo:: index {packet.payload[0" +
                    "]} / chunk {packet.payload[1]} / hierarchy {packet.payload[2]} / syncSpecificHie" +
                    "rarchy {packet.payload[3]}\");\r\n    #endif\r\n            //Remove header\r\n        " +
                    "    Span<byte> packetPayload = packet.payload.Slice(5);\r\n            packetInfo[" +
                    "packet.UserID].currentSize += packetPayload.Length;\r\n            //Copy Byte fro" +
                    "m what come in\r\n            Buffer.BlockCopy(packetPayload.ToArray(), 0, buffer[" +
                    "packet.UserID], offset, packetPayload.Length);\r\n            //Comming all?\r\n    " +
                    "        return packetInfo[packet.UserID].currentSize + 1100 > buffer[packet.User" +
                    "ID].Length ? true : false;\r\n        }\r\n\r\n        /// <summary>\r\n        /// Call" +
                    " from ConvertFormPacket.\r\n        /// </summary>\r\n        void SyncedSynic(strin" +
                    "g overwriterUserId){\r\n            //Deserialize packet\r\n            using var de" +
                    "compressor = new BrotliDecompressor();\r\n            Span<byte> transmittedPaylao" +
                    "d = new Span<byte>(buffer[overwriterUserId]);\r\n\r\n            var decompressedBuf" +
                    "fer = decompressor.Decompress(transmittedPaylaod.Slice(0, packetInfo[overwriterU" +
                    "serId].currentSize));\r\n            SynicContainer container = MemoryPackSerializ" +
                    "er.Deserialize<SynicContainer>(decompressedBuffer);\r\n#if SYNICSUGAR_LOG\r\n       " +
                    "     Debug.Log($\"SyncedSynic: Deserialize is Success for {overwriterUserId}\");\r\n" +
                    "    #endif\r\n\r\n            //Packet data\r\n            int hierarchy = packetInfo[" +
                    "overwriterUserId].hierarchy;\r\n            bool syncSingleHierarchy = packetInfo[" +
                    "overwriterUserId].syncSpecificHierarchy;\r\n\r\n            switch(hierarchy){");
            
            #line 236 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(SyncedInvoker));
            
            #line default
            #line hidden
            this.Write("\r\n                default:\r\n                goto case 9;\r\n            }\r\n        " +
                    "}\r\n        ");
            
            #line 241 "D:\SynicSugarGitTest\SynicSugar\SynicSugar.SourceGenerator\ConnecthubTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(SyncedItems));
            
            #line default
            #line hidden
            this.Write("\r\n    }\r\n}");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public class ConnecthubTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
