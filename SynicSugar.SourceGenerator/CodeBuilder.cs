namespace SynicSugar.Generator {
    public class CodeBuilder {
        //Reflence
        internal string CreatePlayerReference(string nameSpace, string name){
            string fullname = string.IsNullOrEmpty(nameSpace) ? name : $"{nameSpace}.{name}";
            return $@"
        Dictionary<string, {fullname}> {name} = new Dictionary<string, {fullname}>();";
        }
        internal string CreateCommonsReference(string nameSpace, string name){
            string fullname = string.IsNullOrEmpty(nameSpace) ? name : $"{nameSpace}.{name}";
            return $@"
        {fullname} {name};";
        }
        //SyncVar
        internal string CreatePlayerSyncVarPacketConvert(string name, string variable, string param, string nameSpace, bool isPublic) {
            string assignment = isPublic ? $"MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(payload, ref {name}[id].{variable});" :
              $"{name}[id].SetLocal{variable}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(payload));";
            return $@"
                case Channels.{variable}:
                    {assignment}
                return;";
        }
        internal string CreateCommonsSyncVarPacketConvert(string name, string variable, string param, string nameSpace, bool isPublic){
            string assignment = isPublic ? $"MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(payload, ref {name}.{variable});" :
             $@"{name}.isLocalCall = false;
                    {name}.SetLocal{variable}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(payload));
                    {name}.isLocalCall = true;";
            return $@"
                case Channels.{variable}:
                    {assignment}
                return;";
        }
        //TargetRPC
        internal string CreatePlayerTargetRpcPacketConvert(string rootName, string method, string param, string paramNs, bool isLargePacket){
            string paramName = System.String.Empty;
            if (string.IsNullOrEmpty(param)){
                //Pass
            }else if (isLargePacket){
                paramName = $", MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(decompressed{method})";
            }else{ //For normal packet
                paramName = $", MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(payload)";
            }

            string largepacketHeader = isLargePacket ? $@"{{
                    if(!RestoreLargePackets(ref ch, id, ref payload)){{
                #if SYNICSUGAR_LOG 
                        Debug.LogFormat(""ConvertFormPacket: Restore Large packet is in progress. for {{0}} "", ch);
                #endif
                        return;
                    }}
                    using var decompressor{method} = new BrotliDecompressor();
                    Span<byte> {method}Paylaod = new Span<byte>(largeBuffer[id][ch]);

                    var decompressed{method} = decompressor{method}.Decompress({method}Paylaod.Slice(0, largePacketInfo[id][ch].currentSize));
                ": "";
            string footer = isLargePacket ? $@"
                    largeBuffer[id].Remove(ch);
                    largePacketInfo[id].Remove(ch);
                return; }}" : "return; ";

            return $@"
                case Channels.{method}:{largepacketHeader}
                    {rootName}[id].{method}(UserId.GetUserId(id){paramName});
                {footer}";
        }
        //RPC
        internal string CreatePlayerRpcPacketConvert(string rootName, string method, string param, string paramNs, bool isLargePacket){
            string paramName = System.String.Empty;
            
            if (string.IsNullOrEmpty(param)){
                //Pass
            }else if (isLargePacket){
                paramName = $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(decompressed{method})";
            }else{ //For normal packet
                paramName = $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(payload)";

            }

            string largepacketHeader = isLargePacket ? $@"{{
                    if(!RestoreLargePackets(ref ch, id, ref payload)){{
                #if SYNICSUGAR_LOG 
                        Debug.LogFormat(""ConvertFormPacket: Restore Large packet is in progress. for {{0}}"", ch);
                #endif
                        return;
                    }}
                    using var decompressor{method} = new BrotliDecompressor();
                    Span<byte> {method}Paylaod = new Span<byte>(largeBuffer[id][ch]);

                    var decompressed{method} = decompressor{method}.Decompress({method}Paylaod.Slice(0, largePacketInfo[id][ch].currentSize));
                " : "";
            string footer = isLargePacket ? $@"
                    largeBuffer[id].Remove(ch);
                    largePacketInfo[id].Remove(ch);
                return; }}" : "return;";

            return $@"
                case Channels.{method}:{largepacketHeader}
                    {rootName}[id].{method}({paramName});
                {footer}";
        }
        internal string CreateCommonsRpcPacketConvert(string rootName, string method, string param, string paramNs, bool isLargePacket){
            string paramName = System.String.Empty;
            
            if (string.IsNullOrEmpty(param)){
                //Pass
            }else if (isLargePacket){
                paramName = $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(decompressed{method})";
            }else{ //For normal packet
                paramName = $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(payload)";
            }

            string largepacketHeader = isLargePacket ? $@"{{
                    if(!RestoreLargePackets(ref ch, id, ref payload)){{
                #if SYNICSUGAR_LOG 
                        Debug.LogFormat(""ConvertFormPacket: Restore Large packet is in progress. for {{0}}"", ch);
                #endif
                        return;
                    }}
                    using var decompressor{method} = new BrotliDecompressor();
                    Span<byte> {method}Paylaod = new Span<byte>(largeBuffer[id][ch]);

                    var decompressed{method} = decompressor{method}.Decompress({method}Paylaod.Slice(0, largePacketInfo[id][ch].currentSize));
                " : "";

            string footer = isLargePacket ? $@"
                    largeBuffer[id].Remove(ch);
                    largePacketInfo[id].Remove(ch);
                return; }}" : "return;";

            return $@"
                case Channels.{method}:{largepacketHeader}
                    {rootName}.isLocalCall = false;
                    {rootName}.{method}({paramName});
                {footer}";
        }

        //ClearReference
        internal string CreateClearReference(string name, bool isNetworkPlayer){
            string resetWay = isNetworkPlayer ? ".Clear()" : " = null";
            return $@"
            {name}{resetWay};";
        }
        //RegisterInstance
        internal string CreatePlayerRegisterInstance(string nameNamespace, string name) {
            return $@"
        public void RegisterInstance(UserId id, {GetFullName(nameNamespace, name)} classInstance) {{
            if ({name}.ContainsKey(id.ToString())) {{
                {name}[id.ToString()] = classInstance;
            }}else{{
                {name}.Add(id.ToString(), classInstance);
            }}
        }}";
        }
        internal string CreateCommonsRegisterInstance(string nameNamespace, string name) {
            return $@"
        public void RegisterInstance({GetFullName(nameNamespace, name)} classInstance) {{
            {name} = classInstance;
        }}";
        }
        //GetInstance
        internal string CreateGetPlayerInstance(string nameNamespace, string name, bool useGetInstance) {
            if (!useGetInstance) {
                return System.String.Empty;
            }
            return $@"
            if(typeof(T) == typeof({GetFullName(nameNamespace, name)})){{
                if (!{name}.ContainsKey(id.ToString())) {{
                    return default(T);
                }}
                return (T)(object){name}[id.ToString()];
            }}";
        }
        internal string CreateGetCommonsInstance(string nameNamespace, string name, bool useGetInstance) {
            if (!useGetInstance) {
                return System.String.Empty;
            }
            return $@"
            if(typeof(T) == typeof({GetFullName(nameNamespace, name)})){{
                return (T)(object){name};
            }}";
        }
        //Additional Part
        //SyncMetod
        internal string CreateSyncVarMethod(string name, string paramNamespace, string type, int time, bool isPublic, bool isOnlyHost, bool isCommons){
            string intervalCondition = $"isWaiting{name}Interval";
            string condition = isOnlyHost ? $"!p2pInfo.Instance.IsHost() || {intervalCondition}" : intervalCondition;
            string intervalTime = time > 0 ? time.ToString() : "p2pConfig.Instance.autoSyncInterval";
            string modifer = isPublic ? "public " : System.String.Empty;
            string localCondition = isCommons ? "isLocalCall" : "isLocal";
            string setterMethod = isPublic ? System.String.Empty : $@"
        internal void SetLocal{name}({GetFullName(paramNamespace, type)} value) {{
            {name} = value;
        }}"
        ;
            return $@"
        bool {intervalCondition};
        {setterMethod}
        {modifer}{GetFullName(paramNamespace, type)} Sync{name} {{
            set {{
                {name} = value;
                if({localCondition}){{
                    StartSynic{name}();
                }}
            }}
        }}
        
        void StartSynic{name}() {{
            if({condition}){{
                return;
            }}
            {intervalCondition} = true;
            Synic{name}().Forget();
        }}

        async UniTask Synic{name}(){{
            var preValue = {name};

            EOSp2p.SendPacketToAll((byte)ConnectHub.Channels.{name}, MemoryPack.MemoryPackSerializer.Serialize({name})).Forget();
            await UniTask.Delay({intervalTime}, cancellationToken: ConnectHub.Instance.GetSyncToken());
            
            if(ConnectHub.Instance.GetSyncToken().IsCancellationRequested){{
                return;
            }}
            if(preValue == {name}){{
                {intervalCondition} = false;
                return;
            }}
            Synic{name}().Forget();
        }}
";
        }
        //Rpc Method
        internal string CreatePlayerRpcMethod(string fnName, string paramNs, string paramType, bool recordLastInfo, bool isLargePacket){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $"{GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            string largepacketCompresser = isLargePacket ? @"
                using var compressor  = new BrotliCompressor();
                MemoryPackSerializer.Serialize(compressor, value);" : "";
            if (isLargePacket && !string.IsNullOrEmpty(arg)){
                serializer = "compressor.ToArray()";
            }

            string recordOption = recordLastInfo ? ", true" : "";
            string methodType = isLargePacket ? "SendLargePacketsToAll" : "SendPacketToAll";
            return $@"
        void SynicSugarRpc_{fnName}({arg}) {{
            if(isLocal){{{largepacketCompresser}
                EOSp2p.{methodType}((byte)ConnectHub.Channels.{fnName}, {serializer}{recordOption}).Forget();
            }}
        }}";
        }
        internal string CreatePlayerTargetRpcMethod(string fnName, string paramNs, string paramType, bool recordLastInfo, bool isLargePacket){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $", {GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            string largepacketCompresser = isLargePacket ? @"
                using var compressor  = new BrotliCompressor();
                MemoryPackSerializer.Serialize(compressor, value);" : "";
            if (isLargePacket && !string.IsNullOrEmpty(arg)){
                serializer = "compressor.ToArray()";
            }
            string recordOption = recordLastInfo ? ", true" : "";
            string methodType = isLargePacket ? "SendLargePackets" : "SendPacket";
            string forget = isLargePacket ? ".Forget()" : "";

            return $@"
        void SynicSugarRpc_{fnName}(UserId id{arg}) {{
            if(isLocal){{{largepacketCompresser}
                EOSp2p.{methodType}((byte)ConnectHub.Channels.{fnName}, {serializer}, id{recordOption}){forget};
            }}
        }}";
        }
        internal string CreateCommonsRpcMethod(string fnName, string paramNs, string paramType, bool recordLastInfo, bool isLargePacket){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $"{GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            string largepacketCompresser = isLargePacket ? @"
                using var compressor  = new BrotliCompressor();
                MemoryPackSerializer.Serialize(compressor, value);" : "";

            if (isLargePacket && !string.IsNullOrEmpty(arg)){
                serializer = "compressor.ToArray()";
            }
            string recordOption = recordLastInfo ? ", true" : "";
            string methodType = isLargePacket ? "SendLargePacketsToAll" : "SendPacketToAll";
            return $@"
        void SynicSugarRpc_{fnName}({arg}) {{
            if(isLocalCall){{{largepacketCompresser}
                EOSp2p.{methodType}((byte)ConnectHub.Channels.{fnName}, {serializer}{recordOption}).Forget();
            }}
            isLocalCall = true;
        }}";
        }
        //SyncSynicWithLocalData
        internal string CreateSynicItemVariable(string param, string variable) {
            //If the namespace is resolved by using directive, set automatically namespace before parameter.
            return $@"
        public {param} {variable};";
        }
        internal string CreateSyncSynicContent(string variableName, string className, bool isPlayerClass) {
            string assignment = isPlayerClass ? $"{className}.ContainsKey(id) ? {className}[id]?.{variableName} ?? default : default" : $"{className} != null ? {className}?.{variableName} ?? default : default";
            return $@" {variableName} = {assignment},";
        }
        internal (string text, bool needData) AddSyncSynicFrame(int index, string playerContent, string commonsContent) {
            string getPart = string.IsNullOrEmpty(playerContent) && string.IsNullOrEmpty(commonsContent) ? System.String.Empty :$@"
                    SynicItem{index} synicItem{index} = p2pInfo.Instance.IsHost(id) ? new SynicItem{index}(){{{ playerContent }{ commonsContent }}} : new SynicItem{index}(){{{ playerContent }}};
                    synicContainer.SynicItem{index} = JsonUtility.ToJson(synicItem{index});" ;
            string footer = index == 0 ? @"break;" : $@"if (syncSinglePhase) {{ break; }}
                else {{ goto case {index - 1}; }}";

            return ($@"
                case {index}: {getPart}
                {footer}", !string.IsNullOrEmpty(getPart));
        }
        //OverwrittenSynicWithRemoteData
        internal string CreateSyncedInvoker(int index) {
            string footer = index == 0 ? @"
                break;" : $@"
                if (syncSinglePhase) {{ break; }}
                else {{ goto case {index - 1}; }}";
            return $@"
                case {index}:
                    if(container.SynicItem{index} != null){{
                        SynicItem{index} synicItem{index} = JsonUtility.FromJson<SynicItem{index}>(container.SynicItem{index});
                        SyncedItem{index}(overwriterUserId, synicItem{index});
                    }}{footer}";
        }

        internal string CreateSyncedContent(string variableName, string className, bool isPlayerClass) {
            string id = isPlayerClass ? "[id]" : string.Empty;
            string indent = isPlayerClass ? string.Empty : "    ";
            string condition = isPlayerClass ? $"{className}.ContainsKey(id)" : $"{className} != null";

            string itemName = $"\"{className}.{variableName}, \"";

            return $@"
            {indent}if({condition}) {{
            #if SYNICSUGAR_LOG
                {indent}items += {itemName};
                {indent}itemCount++;
            #endif
                {indent}{className}{id}.{variableName} = synicItem.{variableName}; 
            }}";
        }

        internal string CreateSyncedItem(int index, string playerContent, string commonsContent){
            string logContent = $"$\"SyncedItem{index}: {{itemCount}} Synics is overwritten by {{id}}. The List: ({{items}}) \"";

            return $@"
        void SyncedItem{index}(string id, SynicItem{index} synicItem){{
            #if SYNICSUGAR_LOG
                string items = string.Empty;
                int itemCount = 0;
            #endif
            //Player
            {playerContent}
            if(p2pInfo.Instance.IsHost(id)){{
                //Commons
                {commonsContent}
            }}
        #if SYNICSUGAR_LOG
            if(itemCount > 0){{
                Debug.Log({logContent});
            }}
        #endif
        }}";
        }

        //Extenstions
        internal string GetFullName(string nameSpace, string name) {
            if (string.IsNullOrEmpty(nameSpace) || nameSpace  == "<global namespace>") {
                return name;
            }
            return $"{nameSpace}.{name}";
        }
    }
}
