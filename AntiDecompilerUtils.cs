using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace AntiDecompiler_Cleaner {
    public static class AntiDecompilerUtils {
        private static readonly OpCode[] List = {OpCodes.Call, OpCodes.Sizeof, OpCodes.Calli};

        internal static bool DetectCallSizeOfCalli(MethodDef method) {
            var body = method.Body.ExceptionHandlers;
            foreach (var exceptionHandler in body.ToArray())
                if (List.Contains(exceptionHandler.TryStart.OpCode) && exceptionHandler.TryStart.Operand == null)
                    return true;
            return false;
        }

        internal static bool DetectCallUnaligned(MethodDef method) {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++) {
                if (!instr[i].IsBr())
                    continue;
                if (instr[i + 1].OpCode.Code != Code.Unaligned)
                    continue;
                return true;
            }

            return false;
        }

        internal static bool DetectCallConstrained(MethodDef method) {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
                if (instr[i].IsBr() &&
                    instr[i + 1].OpCode == OpCodes.Constrained)
                    return true;

            return false;
        }

        // This method works perfect for most Confuserex modded ( invalid cctor , anti dnSpy )
        internal static void CallSizeOfCalli(MethodDef method) {
            var hasprotection = DetectCallSizeOfCalli(method);
            if (!hasprotection)
                return;

            var body = method.Body.ExceptionHandlers;
            foreach (var exceptionHandler in body.ToArray())
                if (List.Contains(exceptionHandler.TryStart.OpCode) && exceptionHandler.TryStart.Operand == null) {
                    var instr = method.Body.Instructions;
                    var endIndex = instr.IndexOf(exceptionHandler.TryEnd);
                    for (var i = 0; i < endIndex; i++) instr[i].OpCode = OpCodes.Nop;

                    body.Remove(exceptionHandler);
                }
        }

        // This method solve Unaligned tricks ( mostly used on DotnetSafer )
        internal static void CallUnaligned(MethodDef method) {
            var hasprotection = DetectCallUnaligned(method);
            if (!hasprotection)
                return;

            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++) {
                if (!instr[i].IsBr())
                    continue;

                if (instr[i + 1].OpCode.Code != Code.Unaligned)
                    continue;

                instr.RemoveAt(i + 1);
            }
        }

        // This method solve constrained tricks ( mostly used on DotnetSafer )
        internal static void CallConstrained(MethodDef method) {
            var hasprotection = DetectCallConstrained(method);
            if (!hasprotection)
                return;

            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
                if (instr[i].IsBr() &&
                    instr[i + 1].OpCode == OpCodes.Constrained)
                    instr.RemoveAt(i + 1);
        }
    }
}