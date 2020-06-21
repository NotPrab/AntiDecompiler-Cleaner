using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace AntiDecompiler_Cleaner {
    public static class AntiDecompilerUtils {
        public static readonly Random Rnd = new Random();
        private static readonly OpCode[] List = {OpCodes.Call, OpCodes.Sizeof, OpCodes.Calli};

        // This method solve sizeof and calli tricks
        internal static void CallsizeOfcalli(MethodDef method) {
            var body = method.Body.ExceptionHandlers;
            foreach (var exceptionHandler in body.ToArray())
                if (List.Contains(exceptionHandler.TryStart.OpCode) && exceptionHandler.TryStart.Operand == null) {
                    var instr = method.Body.Instructions;
                    var endIndex = instr.IndexOf(exceptionHandler.TryEnd);
                    for (var i = 0; i < endIndex; i++) instr[i].OpCode = OpCodes.Nop;

                    body.Remove(exceptionHandler);
                }
        }

        // This method solve Unaligned tricks
        internal static void CallUnaligned(MethodDef method) {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
                if (instr[i].IsBr() && instr[i + 1].OpCode == OpCodes.Unaligned) {
                    instr[i].OpCode = OpCodes.Nop;
                    instr[i + 1].OpCode = OpCodes.Nop;
                }
        }

        // This method solve box tricks
        internal static void CallBox(MethodDef method) {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
                if (instr[i].OpCode ==
                    OpCodes.Box && instr[i].Operand.ToString().StartsWith("System") && instr[i + 1].OpCode ==
                    OpCodes.Stelem_Ref) {
                    instr[i].OpCode = OpCodes.Nop;
                    instr[i + 1].OpCode = OpCodes.Nop;
                }
        }
        
        // This method solve
        internal static void CallConstrained(MethodDef method) {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++) {
                if (instr[i].IsBr() && instr[i + 1].OpCode == OpCodes.Constrained) {
                    instr[i].OpCode = OpCodes.Nop;
                    instr[i + 1].OpCode = OpCodes.Nop;
                }
            }
        }
    }
}