using Mono.Cecil;

namespace SynicSugar.PostProcess {
    internal class ILPostProcessorReflectionImporterProvider : IReflectionImporterProvider{
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module){
            return new PostProcessorReflectionImporter(module);
        }
    }
}