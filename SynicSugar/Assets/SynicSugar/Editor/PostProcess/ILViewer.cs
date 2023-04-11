using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace SynicSugar.PostProcess {
    public class ILViewer {
        public string fullIL { get; private set;} = System.String.Empty;

        public void AddIl(List<Instruction> instructions){
            foreach(var instruction in instructions){
                fullIL += (instruction + Environment.NewLine);
            }
        }
        public void AddIl(Instruction instruction){
            fullIL += (instruction + Environment.NewLine);
        }
    }
}