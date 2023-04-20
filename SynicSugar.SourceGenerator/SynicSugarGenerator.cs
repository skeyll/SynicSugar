using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
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

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            try {
                List<ClassInfo> classesInfo = new List<ClassInfo>();
                List<ContentInfo> contentsInfo = new List<ContentInfo>();
                Dictionary<string, StringBuilder> syncvars = new Dictionary<string, StringBuilder>();
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
                        var contentAttributes = field.AttributeLists.SelectMany(al => al.Attributes);
                        var syncvarSyntax = contentAttributes.FirstOrDefault(a => a.Name.ToString() == SYNCVAR);

                        if (syncvarSyntax == null){
                            continue;
                        }
                        var model = context.Compilation.GetSemanticModel(field.SyntaxTree);

                        contentsInfo.Add(new ContentInfo());
                        int ci = contentsI();
                        contentsInfo[ci].isNetworkPlayer = classesInfo[classI()].isNetworkPlayer;
                        contentsInfo[ci].type = ContentInfo.Type.SyncVar;
                        contentsInfo[ci].rootNameSpace = classesInfo[classI()].nameSpace;
                        contentsInfo[ci].rootName = classesInfo[classI()].name;
                        contentsInfo[ci].contentName = field.Declaration.Variables.FirstOrDefault().Identifier.ValueText;
                        contentsInfo[ci].isPublicVar = field.Modifiers.Any(SyntaxKind.PublicKeyword);
                        contentsInfo[ci].param = field.Declaration.Type.ToString();
                        contentsInfo[ci].paramNamespace = GetNamespace(field.Declaration.Type, model);

                        var fieldSymbol = model.GetDeclaredSymbol(field) as IFieldSymbol;

                        //Set attribute data
                        var argsCount = syncvarSyntax.ArgumentList?.Arguments.Count ?? 0;
                        if (argsCount == 0){
                            continue;
                        }

                        if(argsCount == 1){
                            var args = (syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                            if (args is bool){
                                contentsInfo[ci].isOnlyHost = (bool)(syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                            }else if(args is int){
                                contentsInfo[ci].syncInterval = (int)(syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                            }
                        }
                        else{
                            contentsInfo[ci].isOnlyHost  = (bool)(syncvarSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.Value;
                            contentsInfo[ci].syncInterval = (int)(syncvarSyntax.ArgumentList.Arguments[1].Expression as LiteralExpressionSyntax).Token.Value;
                        }
                    }
                }
                if (classesInfo.Count == 0){
                    return;
                }
                //Set each detail data
                StringBuilder SyncList = new StringBuilder();
                StringBuilder PacketConvert = new StringBuilder();
                foreach (var info in contentsInfo) {
                    SyncList.Append($"{info.contentName}, ");

                    if (info.isNetworkPlayer) {
                        switch (info.type) {
                            case ContentInfo.Type.Rpc:
                                rpcs[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreatePlayerRpcMethod(info.contentName, info.paramNamespace, info.param));
                                PacketConvert.Append(cb.CreatePlayerRpcPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace));
                                continue;
                            case ContentInfo.Type.TargetRpc:
                                rpcs[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreatePlayerTargetRpcMethod(info.contentName, info.paramNamespace, info.param));
                                PacketConvert.Append(cb.CreatePlayerTargetRpcPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace));
                                continue;
                            case ContentInfo.Type.SyncVar:
                                syncvars[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreateSyncVarMethod(info.contentName, info.paramNamespace, info.param, info.syncInterval, info.isPublicVar, false));
                                PacketConvert.Append(cb.CreatePlayerSyncVarPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isPublicVar));
                            continue;
                        }
                    }

                    switch (info.type){
                        case ContentInfo.Type.Rpc:
                            rpcs[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreateCommonsRpcMethod(info.contentName, info.paramNamespace, info.param));
                            PacketConvert.Append(cb.CreateStateRpcPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace));
                        continue;
                        case ContentInfo.Type.SyncVar:
                            syncvars[cb.GetFullName(info.rootNameSpace, info.rootName)].Append(cb.CreateSyncVarMethod(info.contentName, info.paramNamespace, info.param, info.syncInterval, info.isPublicVar, info.isOnlyHost));
                            PacketConvert.Append(cb.CreateStateSyncVarPacketConvert(info.rootName, info.contentName, info.param, info.paramNamespace, info.isPublicVar));
                        continue;
                    }
                }
                if (string.IsNullOrEmpty(SyncList.ToString())){
                    SyncList.Append("None");
                }

                //Set base class data
                //StringBuilder CommonsList = new StringBuilder();
                //StringBuilder PlayerList = new StringBuilder();
                StringBuilder Reference = new StringBuilder();
                StringBuilder Register = new StringBuilder();
                StringBuilder GetInstance = new StringBuilder();
                StringBuilder PlayeInstance = new StringBuilder();
                StringBuilder CommonsInstance = new StringBuilder();
                StringBuilder GetInstanceAsObject = new StringBuilder();
                StringBuilder AdditionalClass = new StringBuilder(HeaderNotes);
                foreach (var info in classesInfo){
                    if (info.isNetworkPlayer){
                        Reference.Append(cb.CreatePlayerReference(info.nameSpace, info.name));
                        Register.Append(cb.CreatePlayerRegisterInstance(info.nameSpace, info.name));
                        GetInstance.Append(cb.CreateGetInstance(info.nameSpace, info.name));
                        PlayeInstance.Append(cb.CreateGetPlayerInstance(info.nameSpace, info.name, info.useGetInstance));

                        GetInstanceAsObject.Append(cb.CreateGetInstanceAsObject(info.nameSpace, info.name));

                        var pt = new AdditionalPlayerTemplate();
                        pt.NameSpace = info.nameSpace;
                        pt.ClassName = info.name;
                        pt.SyncVar = syncvars[cb.GetFullName(info.nameSpace, info.name)].ToString();
                        pt.Rpcs = rpcs[cb.GetFullName(info.nameSpace, info.name)].ToString();
                        pt.useGetInstance = info.useGetInstance;
                        AdditionalClass.Append(pt.TransformText());
                        continue;
                    }
                    Reference.Append(cb.CreateStateReference(info.nameSpace, info.name));
                    Register.Append(cb.CreateStateRegisterInstance(info.nameSpace, info.name));
                    CommonsInstance.Append(cb.CreateGetCommonsInstance(info.nameSpace, info.name, info.useGetInstance));

                    var ct = new AdditionalCommonsTemplate();
                    ct.NameSpace = info.nameSpace;
                    ct.ClassName = info.name;
                    ct.SyncVar = syncvars[cb.GetFullName(info.nameSpace, info.name)].ToString();
                    ct.Rpcs = rpcs[cb.GetFullName(info.nameSpace, info.name)].ToString();
                    ct.useGetInstance = info.useGetInstance;
                    AdditionalClass.Append(ct.TransformText());
                }

                var connectTemplate = new ConnecthubTemplate() {
                    SyncList = SyncList.ToString(),
                    Register = Register.ToString(),
                    Reference = Reference.ToString(),
                    GetInstance = GetInstance.ToString(),
                    PlayeInstance = PlayeInstance.ToString(),
                    CommonsInstance = CommonsInstance.ToString(),
                    PacketConvert = PacketConvert.ToString(),
                    GetInstanceAsObject = GetInstanceAsObject.ToString()
                }.TransformText();
                context.AddSource("ConnectController.g.cs", connectTemplate);

                context.AddSource("SynicSugarAdditonalClass.g.cs", AdditionalClass.ToString());
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
            public bool isNetworkPlayer, isOnlyHost;
            public string rootNameSpace, rootName, contentName, param, paramNamespace;
            public int syncInterval;
            public bool isPublicVar;
            public Type type;
            public enum Type{
                Rpc, TargetRpc, SyncVar
            }
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
        string HeaderNotes = $@"// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY SynicSugarGenerator. DO NOT CHANGE IT.
// </auto-generated>
using UnityEngine;
using SynicSugar.P2P;
using System.Threading;
using Cysharp.Threading.Tasks;
";
        class SyntaxReceiver : ISyntaxReceiver {
            internal List<ClassDeclarationSyntax> Targets { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode){
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count > 0
                    && classDeclarationSyntax.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == NETWORKPLAYER || a.Name.ToString() == NETWORKCOMMONS)))
                    Targets.Add(classDeclarationSyntax);
                    //&& !Targets.Contains(classDeclarationSyntax)
            }
        }
    }
}