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
            string assignment = isPublic ? $"MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload, ref {name}[packet.UserID].{variable});" :
              $"{name}[packet.UserID].SetLocal{variable}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload));";
            return $@"
                case CHANNELLIST.{variable}:
                    {assignment}
                return;";
        }
        internal string CreateCommonsSyncVarPacketConvert(string name, string variable, string param, string nameSpace, bool isPublic){
            string assignment = isPublic ? $"MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload, ref {name}.{variable});" :
             $@"{name}.isLocalCall = false;
                    {name}.SetLocal{variable}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload));
                    {name}.isLocalCall = true;";
            return $@"
                case CHANNELLIST.{variable}:
                    {assignment}
                return;";
        }
        //TargetRPC
        internal string CreatePlayerTargetRpcPacketConvert(string rootName, string method, string param, string paramNs){
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $", MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(packet.payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}[packet.UserID].{method}(new UserId(packet.UserID){paramName});
                return;";
        }
        //RPC
        internal string CreatePlayerRpcPacketConvert(string rootName, string method, string param, string paramNs){
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(packet.payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}[packet.UserID].{method}({paramName});
                return;";
        }
        internal string CreateCommonsRpcPacketConvert(string rootName, string method, string param, string paramNs){
            //string fixedParam = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNameSpace, param)}>(packet.payload))";
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(packet.payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}.isLocalCall = false;
                    {rootName}.{method}({paramName});
                return;";
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
            string condition = isOnlyHost ? $"!p2pConfig.Instance.userIds.IsHost() || {intervalCondition}" : intervalCondition;
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

            EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.{name}, MemoryPack.MemoryPackSerializer.Serialize({name})).Forget();
            await UniTask.Delay({intervalTime});
            
            if(p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){{
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
        internal string CreatePlayerRpcMethod(string fnName, string paramNs, string paramType){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $"{GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            return $@"
        void SynicSugarRpc_{fnName}({arg}) {{
            if(isLocal){{
                EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.{fnName}, {serializer}).Forget();
            }}
        }}";
        }
        internal string CreatePlayerTargetRpcMethod(string fnName, string paramNs, string paramType){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $", {GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            return $@"
        void SynicSugarRpc_{fnName}(UserId id{arg}) {{
            if(isLocal){{
                EOSp2p.SendPacket((byte)ConnectHub.CHANNELLIST.{fnName}, {serializer}, id);
            }}
        }}";
        }
        internal string CreateCommonsRpcMethod(string fnName, string paramNs, string paramType){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $"{GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            return $@"
        void SynicSugarRpc_{fnName}({arg}) {{
            if(isLocalCall){{
                EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.{fnName}, {serializer}).Forget();
            }}
            isLocalCall = true;
        }}";
        }
        //Extenstions
        internal string GetFullName(string nameSpace, string name) {
            if (string.IsNullOrEmpty(nameSpace)) {
                return name;
            }
            return $"{nameSpace}.{name}";
        }
    }
}
