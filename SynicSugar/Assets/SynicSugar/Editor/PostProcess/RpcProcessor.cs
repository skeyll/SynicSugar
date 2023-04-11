using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SynicSugar.PostProcess {
    //For [Rpc] and [TargetRpc prcess]
    public class RpcProcessor {
        public bool WeavingRpc(ModuleDefinition module, IPostProcessLog logger){
            var sw = new Stopwatch();
            sw.Start();

            int count = 0;
            //Process
            foreach (var typeDef in module.Types) {
                if(!typeDef.IsClass){
                    continue;
                }
                if(typeDef.CustomAttributes.Count == 0){
                    continue;
                }
                //The class has a network attribute?
                bool hasNetworkPlayer = typeDef.CustomAttributes.Any(a => a.AttributeType.FullName == ConstData.NetworkPlayer);
                bool hasNetworkCommons = typeDef.CustomAttributes.Any(a => a.AttributeType.FullName == ConstData.NetworkCommons);
                if(!hasNetworkPlayer && !hasNetworkCommons){
                    continue;
                }
                Dictionary<string, MethodDefinition> rpcDic = new Dictionary<string, MethodDefinition>(GenerateRpcDictionary(typeDef));
                foreach (var method in typeDef.Methods) {
                    if(method.CustomAttributes.Count == 0){
                        continue;
                    }
                    //The method has a Rpc attribute?
                    bool hasRpc = method.CustomAttributes.Any(a => a.AttributeType.FullName == ConstData.Rpc);
                    bool hasTargetRpc = method.CustomAttributes.Any(a => a.AttributeType.FullName == ConstData.TargetRpc);
                    if(!hasRpc && !hasTargetRpc){
                        continue;
                    }
                    try {
                        count++;
                        //TODO: The SendProcess is currently created by SourceGenerator and inserted on below process.
                        // However, this way will cause unintended process by library user.
                        // So, if we can get MemoryPack's serializer from MemoryPack, we should insert the whole process as IL.
                        var processor = method.Body.GetILProcessor();
                        var begin = method.Body.Instructions[0];
                        processor.InsertBefore(begin, processor.Create(OpCodes.Ldarg_0));
                        if(hasRpc){
                            if(method.Parameters.Count >= 1){
                                processor.InsertBefore(begin, processor.Create(OpCodes.Ldarg_1));
                            }  
                        }else{
                            processor.InsertBefore(begin, processor.Create(OpCodes.Ldarg_1));
                            if(method.Parameters.Count >= 2){
                                processor.InsertBefore(begin, processor.Create(OpCodes.Ldarg_2));
                            }
                            if(method.Parameters[0].ParameterType.Name != "UserId"){
                                logger.Warning("TargetRpc need UserId on the 1st arg.", method);
                            }
                        }
                        processor.InsertBefore(begin, processor.Create(OpCodes.Call, rpcDic[method.Name]));
                    }
                    catch (Exception e){
                        logger.Error($"SynicSugar failed to weave for {typeDef.FullName} : {e} {e.Message}\n{e.StackTrace}");
                        return false;
                    }
                }
            }

            sw.Stop();
            logger.Warning($"SynicSugar has overwritten IL in {module.Assembly.Name.Name} for {count} Rpc.({sw.Elapsed.TotalMilliseconds}ms)");
            return true;
        }
        Dictionary<string, MethodDefinition> GenerateRpcDictionary(TypeDefinition module){
            Dictionary<string, MethodDefinition> methodDic = new Dictionary<string, MethodDefinition>();
            //Get RpcMethods
            foreach(var method in module.Methods){
                if(!method.Name.StartsWith("SynicSugarRpc_")){
                    continue;
                }
                string key = method.Name.Replace("SynicSugarRpc_", System.String.Empty);
                methodDic.Add(key, method);
            }
            return methodDic; 
        }
    }
}