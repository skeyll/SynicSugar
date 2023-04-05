namespace SynicSugar.Generator {
    public class CodeBuilder {
        //Reflence
        internal string CreatePlayerReference(string nameSpace, string name){
            string fullname = string.IsNullOrEmpty(nameSpace) ? name : $"{nameSpace}.{name}";
            return $@"
        Dictionary<string, {fullname}> {name} = new Dictionary<string, {fullname}>();";
        }
        internal string CreateStateReference(string nameSpace, string name){
            string fullname = string.IsNullOrEmpty(nameSpace) ? name : $"{nameSpace}.{name}";
            return $@"
        {fullname} {name};";
        }
        //SyncVar
        internal string CreatePlayerSyncVarPacketConvert(string name, string variable, string param, string nameSpace, bool isPublic) {
              string assignment = isPublic ? $"Sync{variable} = MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload)" : 
                $"SetLocal{variable}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload))";
            return $@"
                case CHANNELLIST.{variable}:
                    {name}[packet.UserID].{assignment};
                return;";
        }
        internal string CreateStateSyncVarPacketConvert(string name, string varaible, string param, string nameSpace, bool isPublic){
            string assignment = isPublic ? $"Sync{varaible} = MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload)" :
              $"SetLocal{varaible}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(packet.payload))";
            return $@"
                case CHANNELLIST.{varaible}:
                    {name}.{assignment};
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
        internal string CreateStateRpcPacketConvert(string rootName, string method, string param, string paramNs){
            //string fixedParam = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNameSpace, param)}>(packet.payload))";
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(packet.payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}.isRemotoCall = true;
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
        internal string CreateStateRegisterInstance(string nameNamespace, string name) {
            return $@"
        public void RegisterInstance({GetFullName(nameNamespace, name)} classInstance) {{
            {name} = classInstance;
        }}";
        }
        //GetInstance
        internal string CreateGetInstance(string nameNamespace, string name) {
            return $@"
        public {GetFullName(nameNamespace, name)} GetUserInstance(UserId id, {GetFullName(nameNamespace, name)} instanceType) {{
            if (!{name}.ContainsKey(id.ToString())) {{
                return null;
            }}
            return {name}[id.ToString()];
        }}";
        }
        internal string CreateGetInstanceAsObject(string nameSpace, string name) {
            return $@"
            if(type == typeof({GetFullName(nameSpace, name)})){{
                if (!{name}.ContainsKey(id.ToString())) {{
                    return null;
                }}
                return {name}[id.ToString()];
            }}";
        }
        //Additional Part
        //SyncMetod
        internal string CreateSyncVarMethod(string name, string paramNamespace, string type, int time, bool isPublic, bool isOnlyHost){
            string intervalCondition = $"isWaiting{name}Interval";
            string condition = isOnlyHost ? $"!p2pManager.Instance.userIds.IsHost() || {intervalCondition}" : intervalCondition;
            string intervalTime = time > 0 ? time.ToString() : "p2pManager.Instance.AutoSyncInterval";
            string modifer = isPublic ? "public " : System.String.Empty;
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
                StartSynic{name}();
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
            await UniTask.Delay({intervalTime}, cancellationToken: p2pManager.Instance.p2pToken.Token);
            
            if(p2pManager.Instance.p2pToken.IsCancellationRequested){{
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
            if(OwnerUserID == p2pManager.Instance.userIds.LocalUserId){{
                EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.{fnName}, {serializer}).Forget();
            }}
        }}";
        }
        internal string CreatePlayerTargetRpcMethod(string fnName, string paramNs, string paramType){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $", {GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            return $@"
        void SynicSugarRpc_{fnName}(UserId id{arg}) {{
            if(OwnerUserID == p2pManager.Instance.userIds.LocalUserId){{
                EOSp2p.SendPacket((byte)ConnectHub.CHANNELLIST.{fnName}, {serializer}, id);
            }}
        }}";
        }
        internal string CreateCommonsRpcMethod(string fnName, string paramNs, string paramType){
            string arg = string.IsNullOrEmpty(paramType) ? "" : $"{GetFullName(paramNs, paramType)} value";
            string serializer = string.IsNullOrEmpty(arg) ? "null" : "MemoryPack.MemoryPackSerializer.Serialize(value)";
            return $@"
        void SynicSugarRpc_{fnName}({arg}) {{
            if(!isRemotoCall){{
                EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.{fnName}, {serializer}).Forget();
            }}
            isRemotoCall = false;
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
