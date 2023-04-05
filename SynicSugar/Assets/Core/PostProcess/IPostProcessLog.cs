// From Mirage under the MIT
// https://github.com/MirageNet/Mirage/tree/47ec33710b69831ac9e0c69898040ebcc2dd3bc1/Assets/Mirage/Weaver
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SynicSugar.PostProcess {
    public interface IPostProcessLog {
        void Log(string message);

        void Error(string message);
        void Error(string message, MemberReference mr);
        void Error(string message, MemberReference mr, SequencePoint sequencePoint);
        void Error(string message, MethodDefinition md);

        void Warning(string message);
        void Warning(string message, MemberReference mr);
        void Warning(string message, MemberReference mr, SequencePoint sequencePoint);
        void Warning(string message, MethodDefinition md);
    }
}
