// original code under MIT Copyright (c) 2021 Unity Technologies
// https://github.com/Unity-Technologies/com.unity.netcode.gameobjects
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace SynicSugar.PostProcess {
    internal class ILPostProcessorAssemblyResolver : IAssemblyResolver{
        private readonly string[] _assemblyReferences;
        private readonly string[] _assemblyReferencesFileName;
        private readonly Dictionary<string, AssemblyDefinition> _assemblyCache = new Dictionary<string, AssemblyDefinition>();
        private readonly ICompiledAssembly _compiledAssembly;
        private AssemblyDefinition _selfAssembly;

        public ILPostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly){
            _compiledAssembly = compiledAssembly;
            _assemblyReferences = compiledAssembly.References;
            // cache paths here so we dont need to call it each time we resolve
            _assemblyReferencesFileName = _assemblyReferences.Select(r => Path.GetFileName(r)).ToArray();
        }

        public void Dispose(){
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing){
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters(ReadingMode.Deferred));

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters){
            lock (_assemblyCache){
                if (name.Name == _compiledAssembly.Name)
                    return _selfAssembly;

                var fileName = FindFile(name);
                if (fileName == null)
                    return null;

                var lastWriteTime = File.GetLastWriteTime(fileName);

                var cacheKey = fileName + lastWriteTime;

                if (_assemblyCache.TryGetValue(cacheKey, out var result))
                    return result;

                parameters.AssemblyResolver = this;

                var ms = MemoryStreamFor(fileName);

                var pdb = fileName + ".pdb";
                if (File.Exists(pdb))
                    parameters.SymbolStream = MemoryStreamFor(pdb);

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
                _assemblyCache.Add(cacheKey, assemblyDefinition);
                return assemblyDefinition;
            }
        }
        // This method is called a lot, avoid linq
        private string FindFile(AssemblyNameReference name){

            // first pass, check if we can find dll or exe file
            var dllName = name.Name + ".dll";
            var exeName = name.Name + ".exe";
            for (var i = 0; i < _assemblyReferencesFileName.Length; i++){
                // if filename matches, return full path
                var fileName = _assemblyReferencesFileName[i];
                if (fileName == dllName || fileName == exeName)
                    return _assemblyReferences[i];
            }

            // second pass (only run if first fails), 

            //Unfortunately the current ICompiledAssembly API only provides direct references.
            //It is very much possible that a postprocessor ends up investigating a type in a directly
            //referenced assembly, that contains a field that is not in a directly referenced assembly.
            //if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            //in the ILPostProcessing API. As a workaround, we rely on the fact here that the indirect references
            //are always located next to direct references, so we search in all directories of direct references we
            //got passed, and if we find the file in there, we resolve to it.
            var allParentDirectories = _assemblyReferences.Select(Path.GetDirectoryName).Distinct();
            foreach (var parentDir in allParentDirectories){
                var candidate = Path.Combine(parentDir, name.Name + ".dll");
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        private static MemoryStream MemoryStreamFor(string fileName){
            return Retry(10, TimeSpan.FromSeconds(1), () => {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                    byteArray = new byte[fs.Length];
                    var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                    if (readLength != fs.Length)
                        throw new InvalidOperationException("File read length is not full length of file.");
                }

                return new MemoryStream(byteArray);
            });
        }

        private static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func){
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                    throw;
                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);
                return Retry(retryCount - 1, waitTime, func);
            }
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition){
            _selfAssembly = assemblyDefinition;
        }
    }
}