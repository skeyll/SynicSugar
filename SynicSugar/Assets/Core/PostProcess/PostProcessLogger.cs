// From Mirage under the MIT
// https://github.com/MirageNet/Mirage/tree/47ec33710b69831ac9e0c69898040ebcc2dd3bc1/Assets/Mirage/Weaver
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace SynicSugar.PostProcess{
    public class PostProcessLogger : IPostProcessLog{
        public List<DiagnosticMessage> Diagnostics = new List<DiagnosticMessage>();
        
        public bool EnableTrace { get; }
        public PostProcessLogger(bool enableTrace){
            EnableTrace = enableTrace;
        }

        public void Log(string msg){
            Console.WriteLine($"SynicSugarILPostProcess: {msg}");
        }
        public void Error(string message){
            AddMessage(message, null, DiagnosticType.Error);
        }

        public void Error(string message, MemberReference mr){
            Error($"{message} (at {mr})");
        }

        public void Error(string message, MemberReference mr, SequencePoint sequencePoint){
            AddMessage($"{message} (at {mr})", sequencePoint, DiagnosticType.Error);
        }

        public void Error(string message, MethodDefinition md){
            Error(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
        }


        public void Warning(string message){
            AddMessage($"{message}", null, DiagnosticType.Warning);
        }

        public void Warning(string message, MemberReference mr){
            Warning($"{message} (at {mr})");
        }

        public void Warning(string message, MemberReference mr, SequencePoint sequencePoint){
            AddMessage($"{message} (at {mr})", sequencePoint, DiagnosticType.Warning);
        }

        public void Warning(string message, MethodDefinition md){
            Warning(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
        }


        private void AddMessage(string message, SequencePoint sequencePoint, DiagnosticType diagnosticType){
            Diagnostics.Add(new DiagnosticMessage
            {
                DiagnosticType = diagnosticType,
                File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
                Line = sequencePoint?.StartLine ?? 0,
                Column = sequencePoint?.StartColumn ?? 0,
                MessageData = message
            });
        }
    }
}