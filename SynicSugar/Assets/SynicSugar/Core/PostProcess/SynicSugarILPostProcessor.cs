using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace SynicSugar.PostProcess {
    public sealed class SynicSugarILPostProcessor : ILPostProcessor {
        public override ILPostProcessor GetInstance() => this;
        public override bool WillProcess(ICompiledAssembly compiledAssembly) => 
            compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == ConstData.RuntimeAssemblyName);
        
        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly){
            if (!WillProcess(compiledAssembly)){
                Console.WriteLine($"SynicSugarILPostProcess: was not done.");
                return null;
            }
            PostProcessLogger logger = new PostProcessLogger(true);
            var weaver = new Weaver(logger);

            //Process
            using var assemblyDefinition = weaver.TryWeave(compiledAssembly);
            if (assemblyDefinition == null){
                logger.Log("can't weave.");
                return new ILPostProcessResult(null, GenerateDiagnostics(logger, compiledAssembly));
            }
            //Write
            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            var writerParameters = new WriterParameters{
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            assemblyDefinition.Write(pe, writerParameters);

            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), GenerateDiagnostics(logger, compiledAssembly));
        }
        private List<DiagnosticMessage> GenerateDiagnostics(PostProcessLogger logger, ICompiledAssembly compiledAssembly){
            var diag = logger.Diagnostics;
            var errorCount = diag.Where(x => x.DiagnosticType == DiagnosticType.Error).Count();
            if (errorCount > 0) {
                var defineMsg = ArrayMessage("Defines", compiledAssembly.Defines);
                var refMsg = ArrayMessage("References", compiledAssembly.References);
                var msg = $"SynicSugarILPostProcess Failed on {compiledAssembly.Name}. See Editor log for full details.\n{defineMsg}\n{refMsg}";

                // if fail
                // insert debug info for weaver as first message,
                diag.Insert(0, new DiagnosticMessage{
                    DiagnosticType = DiagnosticType.Error,
                    MessageData = msg
                });
            }

            return diag;

            string ArrayMessage(string prefix, string[] array){
                return array.Length == 0
                    ? $"{prefix}:[]"
                    : $"{prefix}:[\n  {string.Join("\n  ", array)}\n]";
            }
        }
    }
    
}