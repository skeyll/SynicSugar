using System;
using Mono.Collections.Generic;
using Mono.Cecil.Cil;

namespace SynicSugar.PostProcess {
    public class ILViewer {
        public string il { get; private set;} = System.String.Empty;
        public void ExpandIL(Collection<Instruction> instructions){
            foreach(var instruction in instructions){
                il += $"{instruction} //";
            }
        }
    }
}