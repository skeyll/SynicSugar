using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using SynicSugarGenerator;

namespace SynicSugar.Generator {
    [Generator]
    public class SynicSugarGenerator : ISourceGenerator {
        internal const string NETWORKPLAYER = "NetworkPlayer";
        internal const string NETWORKCOMMONS = "NetworkCommons";
        internal const string RPC = "Rpc";
        internal const string TARGETRPC = "TargetRpc";
        internal const string SYNCVAR = "SyncVar";
        internal const string SYNIC = "Synic";

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            try {
                List<ClassInfo> classesInfo = new List<ClassInfo>();
                List<ContentInfo> contentsInfo = new List<ContentInfo>();
                //To set key on analysis phase.
                Dictionary<string, StringBuilder> syncvars = new Dictionary<string, StringBuilder>();
                Dictionary<string, StringBuilder> synics = new Dictionary<string, StringBuilder>();
                Dictionary<string, StringBuilder> rpcs = new Dictionary<string, StringBuilder>();

                int classI(){ return classesInfo.Count - 1; }
                int contentsI() { return contentsInfo.Count - 1; }
                CodeBuilder cb = new CodeBuilder();

                foreach (var target in receiver.Targets) {
                    //class
                    classesInfo.Add(new ClassInfo());
                    //Base Info
                    var networkAttributes = target.AttributeLists.SelectMany(al => al.Attributes).FirstOrDefault(a => a.Name.ToString() == NETWORKPLAYER || a.Name.ToString() == NETWORKCOMMONS);
                    classesInfo[classI()].isNetworkPlayer = (networkAttributes.Name.ToString() == NETWORKPLAYER);
                    // need GetInstance?
                    if ((networkAttributes.ArgumentList?.Arguments.Count ?? 0) == 1){
                        classesInfo[classI()].useGetInstance = (bool)(networkAttributes.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                    }

                    classesInfo[classI()].name = target.Identifier.ValueText;
                    classesInfo[classI()].nameSpace = GetNamespace(target);
                    if (!syncvars.ContainsKey($"{cb.GetFullName(classesInfo[classI()].nameSpace, classesInfo[classI()].name)}")){
                        syncvars.Add(cb.GetFullName(classesInfo[classI()].nameSpace, classesInfo[classI()].name), new StringBuilder());
                        rpcs.Add(cb.GetFullName(classesInfo[classI()].nameSpace, classesInfo[classI()].name), new StringBuilder());
                        synics.Add(cb.GetFullName(classesInfo[classI()].nameSpace, classesInfo[classI()].name), new StringBuilder());
                    }

                    foreach (var method in target.Members.OfType<MethodDeclarationSyntax>()){
                        //Is it Target? (class syntax has also no attributes method.)
                        var methodAttribute = method.AttributeLists.SelectMany(al => al.Attributes);
                        bool hasRpc = methodAttribute.Any(a => a.Name.ToString() == RPC);
                        bool hasTargetRpc = methodAttribute.Any(a => a.Name.ToString() == TARGETRPC);

                        if (!hasRpc && !hasTargetRpc){
                            continue;
                        }

                        contentsInfo.Add(new ContentInfo());
                        int ci = contentsI();
                        contentsInfo[ci].isNetworkPlayer = classesInfo[classI()].isNetworkPlayer;
                        contentsInfo[ci].type = hasRpc ? ContentInfo.Type.Rpc : ContentInfo.Type.TargetRpc;
                        contentsInfo[ci].rootNameSpace = classesInfo[classI()].nameSpace;
                        contentsInfo[ci].rootName = classesInfo[classI()].name;
                        contentsInfo[ci].contentName = method.Identifier.ValueText;


                        //Get attribute arg
                        var attributeArg = methodAttribute.FirstOrDefault(a => a.Name.ToString() is RPC or TARGETRPC);
                        if (attributeArg is not null && (attributeArg.ArgumentList?.Arguments.Count ?? 0) > 0){
                            contentsInfo[ci].isLargePacket = (bool)(attributeArg.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                            contentsInfo[ci].isRecord = (bool)(attributeArg.ArgumentList.Arguments[1].Expression as LiteralExpressionSyntax).Token.Value;
                        }
                        //Get method info
                        if (hasRpc){
                            if (method.ParameterList.Parameters.Count <= 0){
                                continue;
                            }
                            //Passsing param is only 1st.
                            var model = context.Compilation.GetSemanticModel(method.ParameterList.Parameters[0].SyntaxTree);
                            contentsInfo[ci].param = method.ParameterList.Parameters[0].Type.ToString();
                            contentsInfo[ci].paramNamespace = GetNamespace(method.ParameterList.Parameters[0].Type, model);
                        }else{
                            if (method.ParameterList.Parameters.Count <= 1){
                                continue;
                            }
                            //TargetRPC needs UserId by 1st args.
                            //And passing param is only 2nd.
                            var model = context.Compilation.GetSemanticModel(method.ParameterList.Parameters[1].SyntaxTree);
                            contentsInfo[ci].param = method.ParameterList.Parameters[1].Type.ToString();
                            contentsInfo[ci].paramNamespace = GetNamespace(method.ParameterList.Parameters[1].Type, model);
                        }
                    }

                    foreach (var field in target.Members.OfType<FieldDeclarationSyntax>()){
                        var fieldAttributes = field.AttributeLists.SelectMany(al => al.Attributes);
                        var syncvarSyntax = fieldAttributes.FirstOrDefault(a => a.Name.ToString() == SYNCVAR);
                        var synicSyntax = fieldAttributes.FirstOrDefault(a => a.Name.ToString() == SYNIC);

                        var model = context.Compilation.GetSemanticModel(field.SyntaxTree);

                        if (syncvarSyntax != null) {
                            //Get pre index. We need current target index after adding object.
                            int ci = contentsI() + 1;
                            AddInfoWithBasicData(ci, true);

                            //Set attribute data
                            var argsCount = syncvarSyntax.ArgumentList?.Arguments.Count ?? 0;
                            if (argsCount == 0){
                                //All elements are false
                            }else if(argsCount == 1){
                                var args = (syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                                if (args is bool){
                                    contentsInfo[ci].isOnlyHost = (bool)(syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                                }else if(args is int){
                                    contentsInfo[ci].intAttributeArg = (int)(syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                                }
                            }
                            else{
                                contentsInfo[ci].isOnlyHost = (bool)(syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                                contentsInfo[ci].intAttributeArg = (int)(syncvarSyntax.ArgumentList.Arguments[1].Expression as LiteralExpressionSyntax).Token.Value;
                            }
                        }
                        
                        if (synicSyntax != null && field.Modifiers.Any(SyntaxKind.PublicKeyword)){
                            int ci = contentsI() + 1;
                            AddInfoWithBasicData(ci, false);

                            //Set attribute data
                            var argsCount = synicSyntax.ArgumentList?.Arguments.Count ?? 0;
                            if (argsCount == 0){
                                contentsInfo[ci].intAttributeArg = 0;
                            }
                            else {
                                contentsInfo[ci].intAttributeArg = (int)(synicSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                            }
                        }
                        void AddInfoWithBasicData(int ci, bool isSyncVar){
                            contentsInfo.Add(new ContentInfo());

                            contentsInfo[ci].isNetworkPlayer = classesInfo[classI()].isNetworkPlayer;
                            contentsInfo[ci].type = isSyncVar ? ContentInfo.Type.SyncVar : ContentInfo.Type.Synic;
                            contentsInfo[ci].rootNameSpace = classesInfo[classI()].nameSpace;
                            contentsInfo[ci].rootName = classesInfo[classI()].name;
                            contentsInfo[ci].contentName = field.Declaration.Variables.FirstOrDefault().Identifier.ValueText;
                            contentsInfo[ci].isPublicVar = field.Modifiers.Any(SyntaxKind.PublicKeyword);
                            contentsInfo[ci].param = field.Declaration.Type.ToString();
                            contentsInfo[ci].paramNamespace = GetNamespace(field.Declaration.Type, model);
                        }
                    }
                }
                if (classesInfo.Count == 0){
                    return;
                }
                //Set each detail data
                StringBuilder SyncList = new StringBuilder();
                StringBuilder PacketConvert = new StringBuilder();
                Dictionary<int, StringBuilder> SynicItems = new Dictionary<int, StringBuilder>();
                Dictionary<int, StringBuilder> SyncPlayer = new Dictionary<int, StringBuilder>();
                Dictionary<int, StringBuilder> SyncCommons = new Dictionary<int, StringBuilder>();
                Dictionary<int, StringBuilder> SyncedPlayer = new Dictionary<int, StringBuilder>();
                Dictionary<int, StringBuilder> SyncedCommons = new Dictionary<int, StringBuilder>();
                StringBuilder SyncedInvoker = new StringBuilder();
                //Prep
                for (int i = 0; i <= 9; i++) {
                    SynicItems.Add(i, new StringBuilder());
                    SyncPlayer.Add(i, new StringBuilder());
                    SyncCommons.Add(i, new StringBuilder());
                    SyncedPlayer.Add(i, new StringBuilder());
                    SyncedCommons.Add(i, new StringBuilder());
                    SyncedInvoker.Append(cb.CreateSyncedInvoker(i));
                }
                // Namespaces for use with ConnectHub
                HashSet<string> connecthubNamespaces = new ();

                foreach (var info in contentsInfo) {
                    if (info.type == ContentInfo.Type.Synic) {
                        SynicItems[info.intAttributeArg].Append(cb.CreateSynicItemVariable(info.param, info.contentName));
                    }else{
                        SyncList.Append($"{info.contentName}, ");
                    }
                    connecthubNamespaces.Add(info.paramNamespace);

                    if (info.isNetworkPlayer) {
                        switch (info.type) {
                            case ContentInfo.Type.Rpc:
                                rpcs[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreatePlayerRpcMethod(info.contentName, info.paramNamespace, info.param, info.isRecord, info.isLargePacket));
                                PacketConvert.Append(cb.CreatePlayerRpcPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isLargePacket));
                           continue;
                            case ContentInfo.Type.TargetRpc:
                                rpcs[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreatePlayerTargetRpcMethod(info.contentName, info.paramNamespace, info.param, info.isRecord, info.isLargePacket));
                                PacketConvert.Append(cb.CreatePlayerTargetRpcPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isLargePacket));
                            continue;
                            case ContentInfo.Type.SyncVar:
                                syncvars[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreateSyncVarMethod(info.contentName, info.paramNamespace, info.param, info.intAttributeArg, info.isPublicVar, false, false));
                                PacketConvert.Append(cb.CreatePlayerSyncVarPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isPublicVar));
                            continue;
                            case ContentInfo.Type.Synic:
                                SyncPlayer[info.intAttributeArg].Append(cb.CreateSyncSynicContent(info.contentName, info.rootName, true));
                                SyncedPlayer[info.intAttributeArg].Append(cb.CreateSyncedContent(info.contentName, info.rootName, true));
                            continue;
                        }
                    }
                    //for Commons
                    switch (info.type){
                        case ContentInfo.Type.Rpc:
                            rpcs[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreateCommonsRpcMethod(info.contentName, info.paramNamespace, info.param, info.isRecord, info.isLargePacket));
                            PacketConvert.Append(cb.CreateCommonsRpcPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isLargePacket));
                        continue;
                        case ContentInfo.Type.SyncVar:
                            syncvars[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreateSyncVarMethod(info.contentName, info.paramNamespace, info.param, info.intAttributeArg, info.isPublicVar, info.isOnlyHost, true));
                            PacketConvert.Append(cb.CreateCommonsSyncVarPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isPublicVar));
                        continue;
                        case ContentInfo.Type.Synic:
                            SyncCommons[info.intAttributeArg].Append(cb.CreateSyncSynicContent(info.contentName, info.rootName, false));
                            SyncedCommons[info.intAttributeArg].Append(cb.CreateSyncedContent(info.contentName, info.rootName, false));
                        continue;
                    }
                }
                //For library api
                SyncList.Append("UserList = 252, ObtainPing = 253, ReturnPong = 254, Synic = 255");

                //Set base class data
                StringBuilder Reference = new StringBuilder();
                StringBuilder Register = new StringBuilder();
                StringBuilder ClearReference = new StringBuilder();
                StringBuilder GetInstance = new StringBuilder();
                StringBuilder PlayeInstance = new StringBuilder();
                StringBuilder CommonsInstance = new StringBuilder();
                StringBuilder AdditionalClass = new StringBuilder(AdditionalClassHeader);

                foreach (var info in classesInfo){
                    ClearReference.Append(cb.CreateClearReference(info.name, info.isNetworkPlayer));

                    if (info.isNetworkPlayer){
                        Reference.Append(cb.CreatePlayerReference(info.nameSpace, info.name));
                        Register.Append(cb.CreatePlayerRegisterInstance(info.nameSpace, info.name));
                        PlayeInstance.Append(cb.CreateGetPlayerInstance(info.nameSpace, info.name, info.useGetInstance));

                        var pt = new AdditionalPlayerTemplate();
                        pt.NameSpace = info.nameSpace;
                        pt.ClassName = info.name;
                        pt.SyncVar = syncvars[cb.GetFullName(info.nameSpace, info.name)].ToString();
                        pt.Rpcs = rpcs[cb.GetFullName(info.nameSpace, info.name)].ToString();
                        pt.useGetInstance = info.useGetInstance;
                        AdditionalClass.Append(pt.TransformText());
                        continue;
                    }
                    Reference.Append(cb.CreateCommonsReference(info.nameSpace, info.name));
                    Register.Append(cb.CreateCommonsRegisterInstance(info.nameSpace, info.name));
                    CommonsInstance.Append(cb.CreateGetCommonsInstance(info.nameSpace, info.name, info.useGetInstance));

                    var ct = new AdditionalCommonsTemplate();
                    ct.NameSpace = info.nameSpace;
                    ct.ClassName = info.name;
                    ct.SyncVar = syncvars[cb.GetFullName(info.nameSpace, info.name)].ToString();
                    ct.Rpcs = rpcs[cb.GetFullName(info.nameSpace, info.name)].ToString();
                    ct.useGetInstance = info.useGetInstance;
                    AdditionalClass.Append(ct.TransformText());
                }

                
                StringBuilder SyncSynic = new StringBuilder(string.Empty);
                bool needSyncSynic = false;
                foreach (var i in SyncPlayer) {
                    var result = cb.AddSyncSynicFrame(i.Key, i.Value.ToString(), SyncCommons[i.Key].ToString());

                    SyncSynic.Append(result.text);

                    if (!needSyncSynic) {
                        needSyncSynic = result.needData;
                    }
                }

                StringBuilder SyncedItem = new StringBuilder();
                
                foreach (var i in SyncedPlayer) {
                }
                for (int i = 0; i <= 9; i++){
                    bool playerContain = SyncedPlayer.ContainsKey(i);
                    bool commonsContain = SyncedCommons.ContainsKey(i);
                    if(!playerContain && !commonsContain) {
                        continue;
                    }
                    SyncedItem.Append(cb.CreateSyncedItem(i, playerContain ? SyncedPlayer[i].ToString() : string.Empty, commonsContain ? SyncedCommons[i].ToString() : string.Empty));
                }

                var connectTemplate = new ConnecthubTemplate() {
                    SyncList = SyncList.ToString(),
                    Register = Register.ToString(),
                    Reference = Reference.ToString(),
                    ClearReference = ClearReference.ToString(),
                    PlayeInstance = PlayeInstance.ToString(),
                    CommonsInstance = CommonsInstance.ToString(),
                    PacketConvert = PacketConvert.ToString(),
                    needSyncSynic = needSyncSynic,
                    GenerateSynicContainer = SyncSynic.ToString(),
                    SyncedInvoker = SyncedInvoker.ToString(),
                    SyncedItems = SyncedItem.ToString()
                }.TransformText();
                context.AddSource("ConnectHub.g.cs", connectTemplate);

                context.AddSource("SynicSugarAdditonalClass.g.cs", AdditionalClass.ToString());

                //Synic Container
                StringBuilder SynicItemsClass = new StringBuilder(SynicItemsHeader);
                StringBuilder UsingStatements = new StringBuilder();
                foreach (var ns in connecthubNamespaces) {
                    if(string.IsNullOrEmpty(ns)){ continue;}

                    UsingStatements.AppendLine($"using {ns};");
                }
                SynicItemsClass.Replace("USING_STATEMENTS", UsingStatements.ToString());

                foreach (var i in SynicItems){
                    var st = new SynicItemsTemplate(){
                        hierarchyIndex = i.Key,
                        items = i.Value.ToString()
                    };
                    SynicItemsClass.Append(st.TransformText());
                }
                SynicItemsClass.Append("}");
                context.AddSource("SynicSugarSynicContainer.g.cs", SynicItemsClass.ToString());
            }
            catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
        public class ClassInfo {
            public bool isNetworkPlayer, useGetInstance;
            public string nameSpace, name;
        }
        public class ContentInfo{
            public enum Type{
                Rpc, TargetRpc, SyncVar, Synic
            }
            public Type type;
            public string rootNameSpace, rootName, contentName, param, paramNamespace;
            public int intAttributeArg;
            //For RPC
            public bool isNetworkPlayer, isRecord, isLargePacket;
            //For SyncVar
            public bool isPublicVar, isOnlyHost;
        }

        string GetNamespace(TypeSyntax param, SemanticModel semanticModel){
            if(param == null){ 
                return System.String.Empty;
            }

            var typeSymbol = semanticModel.GetSymbolInfo(param).Symbol as INamedTypeSymbol;
            if (typeSymbol == null || typeSymbol?.ContainingNamespace.ToString() == "System") {
                return System.String.Empty;
            }

            return typeSymbol.ContainingNamespace.ToString();
        }
        string GetNamespace(ClassDeclarationSyntax param){
            if (param.Parent is NamespaceDeclarationSyntax namespaceDeclaration){
                return namespaceDeclaration.Name.ToString();
            }
            return System.String.Empty;
        }
        string AdditionalClassHeader = $@"// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY SynicSugarGenerator. DO NOT CHANGE IT.
// </auto-generated>
using UnityEngine;
using SynicSugar;
using SynicSugar.P2P;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MemoryPack;
using MemoryPack.Compression;
";

        string SynicItemsHeader = $@"// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY SynicSugarGenerator. DO NOT CHANGE IT.
// </auto-generated>
#pragma warning disable CS0164 // This label has not been referenced
#pragma warning disable CS0436 // Type conflicts with the imported type

USING_STATEMENTS

namespace SynicSugar.P2P {{
";

        class SyntaxReceiver : ISyntaxReceiver {
            internal List<ClassDeclarationSyntax> Targets { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode){
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count > 0
                    && classDeclarationSyntax.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() is NETWORKPLAYER or NETWORKCOMMONS)))
                    Targets.Add(classDeclarationSyntax);
                    //&& !Targets.Contains(classDeclarationSyntax)
            }
        }
    }
}