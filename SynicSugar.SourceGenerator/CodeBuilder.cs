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
                case CHANNELLIST.{variable}:
                    {assignment}
                return;";
        }
        internal string CreateCommonsSyncVarPacketConvert(string name, string variable, string param, string nameSpace, bool isPublic){
            string assignment = isPublic ? $"MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(payload, ref {name}.{variable});" :
             $@"{name}.isLocalCall = false;
                    {name}.SetLocal{variable}(MemoryPackSerializer.Deserialize<{GetFullName(nameSpace, param)}>(payload));
                    {name}.isLocalCall = true;";
            return $@"
                case CHANNELLIST.{variable}:
                    {assignment}
                return;";
        }
        //TargetRPC
        internal string CreatePlayerTargetRpcPacketConvert(string rootName, string method, string param, string paramNs){
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $", MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}[id].{method}(UserId.GetUserId(id){paramName});
                return;";
        }
        //RPC
        internal string CreatePlayerRpcPacketConvert(string rootName, string method, string param, string paramNs){
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}[id].{method}({paramName});
                return;";
        }
        internal string CreateCommonsRpcPacketConvert(string rootName, string method, string param, string paramNs){
            //string fixedParam = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNameSpace, param)}>(payload))";
            string paramName = string.IsNullOrEmpty(param) ? System.String.Empty : $"MemoryPackSerializer.Deserialize<{GetFullName(paramNs, param)}>(payload)";
            return $@"
                case CHANNELLIST.{method}:
                    {rootName}.isLocalCall = false;
                    {rootName}.{method}({paramName});
                return;";
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
        //SyncSynic
        internal string CreateSynicItemVariable(string variable, string nameSpace, string param) {
            return $@"
        public {GetFullName(nameSpace, param)} {variable};";
        }
        internal string CreateSyncSynicContent(string variableName, string className, bool isPlayerClass) {
            string id = isPlayerClass ? "[id.ToString()]" : System.String.Empty;
            return $@" {variableName} = {className}{id}.{variableName},";
        }
        internal (string text, bool needData) AddSyncSynicFrame(int index, string playerContent, string commonsContent) {
            string getPart = string.IsNullOrEmpty(playerContent) && string.IsNullOrEmpty(commonsContent) ? System.String.Empty :$@"
                    SynicItem{index} synicItem{index} = p2pInfo.Instance.IsHost() ? new SynicItem{index}(){{{ playerContent }{ commonsContent }}} : new SynicItem{index}(){{{ playerContent }}};
                    synicContainer.SynicItem{index} = JsonUtility.ToJson(synicItem{index});" ;
            string footer = index == 0 ? @"break;" : $@"if (syncSinglePhase) {{ break; }}
                else {{ goto case {index - 1}; }}";

            return ($@"
                case {index}: {getPart}
                {footer}", !string.IsNullOrEmpty(getPart));
        }
        internal string CreateGenerateSynicContainer(string content) {
            return $@"SynicContainer GenerateSynicContainer(UserId id, byte syncedPhase, bool syncSinglePhase){{
            SynicContainer synicContainer = new SynicContainer();
            switch(syncedPhase){{ {content}
                default:
                goto case 9;
            }}
            return synicContainer;
        }}
";
        }
        //SyncedSynic
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
            string id = isPlayerClass ? "[id]" : System.String.Empty;
            string indent = isPlayerClass ? System.String.Empty : "    ";
            return $@"
            {indent}{className}{id}.{variableName} = synicItem.{variableName};";
        }
        internal string CreateSyncedItem(int index, string playerContent, string commonsContent){
            return $@"
        void SyncedItem{index}(string id, SynicItem{index} synicItem){{ {playerContent}
            if(p2pInfo.Instance.IsHost(id)){{
                //Commons
                {commonsContent}
            }}
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
