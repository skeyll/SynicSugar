using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SynicSugar.PostProcess {
    public class SyncVarProcessor {
        public bool WeavingSyncVar(ModuleDefinition module, IPostProcessLog logger){
            var sw = new Stopwatch();
            sw.Start();

            //Create dictionary
            Dictionary<string, MethodDefinition> syncvarDic = new Dictionary<string, MethodDefinition>(GenerateSyncvarDictionary(module, logger));
            int count = 0;
            //Process
            foreach (var typeDef in module.Types) {
                //Interface also has method, but the method has no Instructions.
                //So, need to skip here.
                if(typeDef.IsInterface){
                    continue;
                }
                foreach (var method in typeDef.Methods) {
                    Dictionary<string, List<Instruction> >instructions = new Dictionary<string, List<Instruction>>();
                    try {
                        var processor = method.Body.GetILProcessor();
                        instructions.Clear();
                        //Method use syncvars?
                        foreach(var instruction in processor.Body.Instructions){
                            if((instruction.Operand != null && instruction.OpCode == OpCodes.Stfld)){
                                if(instruction.Operand is not FieldDefinition){
                                    continue;
                                }
                                string syncVar = ((FieldDefinition)instruction.Operand).Name;
                                if(syncvarDic.ContainsKey(syncVar)){
                                    //Pass generated process for own.
                                    if(method.Name == $"SetLocal{syncVar}" || method.Name == ($"set_Sync{syncVar}")){
                                        continue;
                                    }
                                    if(!instructions.ContainsKey(syncVar)){
                                        instructions.Add(syncVar, new List<Instruction>());
                                    }
                                    count++;
                                    instructions[syncVar].Add(instruction);
                                }
                            }
                        }
                        foreach(var instruction in instructions){
                            foreach(var i in instruction.Value){
                                processor.Replace(i, processor.Create(OpCodes.Callvirt, syncvarDic[instruction.Key]));
                            }
                        }
                    }
                    catch (Exception e){
                        logger.Error($"SynicSugar failed to weave for syncVar in {typeDef.Name}. : {e} {e.Message}\n{e.StackTrace}");
                        return false;
                    }
                }
            }

            sw.Stop();
            if(count > 0){
                logger.Warning($"SynicSugar has overwritten IL in {module.Assembly.Name.Name} for {count} SyncVar.({sw.Elapsed.TotalMilliseconds}ms)");
            }
            return true;
        }

        Dictionary<string, MethodDefinition> GenerateSyncvarDictionary(ModuleDefinition module, IPostProcessLog logger){
            //Get all syncvar
            List<FieldDefinition> syncvars = new List<FieldDefinition>(FindSyncVars(module, logger));
            
            Dictionary<string, MethodDefinition> syncvarDic = new Dictionary<string, MethodDefinition>();
            //Get SyncProps
            foreach (var typeDef in module.Types) {
                foreach(var prop in typeDef.Properties){
                    if(!prop.Name.StartsWith("Sync")){
                        continue;
                    }
                    foreach(var syncvar in syncvars){
                        if(prop.Name != $"Sync{syncvar.Name}"){
                            continue;
                        }
                        if(prop.SetMethod == null){
                            continue;
                        }
                        syncvarDic.Add(syncvar.Name, prop.SetMethod);
                    }
                }
            }
            return syncvarDic; 
        }
        List<FieldDefinition> FindSyncVars(ModuleDefinition module, IPostProcessLog logger){
            List<FieldDefinition> syncvars = new List<FieldDefinition>();

            foreach (var typeDef in module.Types) {
                if(!typeDef.IsClass && !typeDef.CustomAttributes.Any(a => a.AttributeType.FullName == ConstData.NetworkPlayer || a.AttributeType.Name == ConstData.NetworkCommons)){
                    continue;
                }

                foreach(var field in typeDef.Fields){
                    if(!field.CustomAttributes.Any(a => a.AttributeType.FullName == ConstData.SyncVar)){
                        continue;
                    }
                    if(field.FieldType.IsArray || field.FieldType.IsGenericInstance){
                        logger.Error("SyncVar dosen't support List or Array now.", field);
                        continue;
                    }
                    syncvars.Add(field);
                }
            }
            return syncvars;
        }
    }
}