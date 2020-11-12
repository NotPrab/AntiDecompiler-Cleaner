using System;
using dnlib.DotNet;

namespace AntiDecompiler_Cleaner {
    internal static class AntiDecompilerPhase {
        public static void Execute(MethodDef method) {
            AntiDecompilerUtils.CallSizeOfCalli(method); // thanks to CursedSheep
            AntiDecompilerUtils.CallUnaligned(method);
            AntiDecompilerUtils.CallConstrained(method);
        }
    }
}