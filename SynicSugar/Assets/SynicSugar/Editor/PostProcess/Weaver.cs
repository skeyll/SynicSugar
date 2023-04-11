using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace SynicSugar.PostProcess {
    internal class Weaver {
        IPostProcessLog logger;
        AssemblyDefinition CurrentAssembly { get; set; }
        public Weaver(IPostProcessLog loger){
            this.logger = loger;
        }
        public AssemblyDefinition TryWeave(ICompiledAssembly compiledAssembly){
            try{
                CurrentAssembly = AssemblyDefinitionFor(compiledAssembly);
                ModuleDefinition module = CurrentAssembly.MainModule;
                //SyncVar
                SyncVarProcessor syncvarProcessr = new SyncVarProcessor();
                bool canWeaveSyncVar = syncvarProcessr.WeavingSyncVar(module, logger);
                if(!canWeaveSyncVar){
                    throw new Exception("SyncVar");
                }
                //Rpc
                RpcProcessor rpcProcessor = new RpcProcessor();
                bool canWeaveRpc = rpcProcessor.WeavingRpc(module, logger);
                if(!canWeaveRpc){
                    throw new Exception("Rpc IL");
                }
                //Create Error message
                return CurrentAssembly;
            }
            catch (Exception e){
                logger.Log($"can't weave on {e} in TryWeave.");
                return null;
            }
        }
        public AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly){
            var assemblyResolver = new ILPostProcessorAssemblyResolver(compiledAssembly);
            var readerParameters = new ReaderParameters {
                SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData),
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = assemblyResolver,
                ReflectionImporterProvider = new ILPostProcessorReflectionImporterProvider(),
                ReadingMode = ReadingMode.Immediate
            };

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(new MemoryStream(compiledAssembly.InMemoryAssembly.PeData), readerParameters);

            assemblyResolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

            return assemblyDefinition;
        }
        public TypeReference CreateParameterTypeReference(ModuleDefinition module, Type parameterType, TypeReference typeRef) {
            if (!parameterType.ContainsGenericParameters){
                return module.ImportReference(parameterType);
            }

            if (parameterType.IsGenericParameter){
                return typeRef.GenericParameters[parameterType.GenericParameterPosition];
            }

            if (parameterType.IsArray){
                return new ArrayType(
                    CreateParameterTypeReference(
                        module,
                        parameterType.GetElementType(),
                        typeRef),
                    parameterType.GetArrayRank());
            }

            var openGenericType = parameterType.GetGenericTypeDefinition();
            var genericInstance = new GenericInstanceType(module.ImportReference(openGenericType));

            foreach (var arg in parameterType.GenericTypeArguments){
                genericInstance.GenericArguments.Add(
                    CreateParameterTypeReference(module, arg, typeRef));
            }

            return genericInstance;
        }
    }
}