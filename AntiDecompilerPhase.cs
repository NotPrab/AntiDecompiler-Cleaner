using dnlib.DotNet;

namespace AntiDecompiler_Cleaner {
    internal static class AntiDecompilerPhase {
        public static void Execute(MethodDef method) {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
                // manually edit numbers, if file broken after used.
                switch (AntiDecompilerUtils.Rnd.Next(0, 3)) {
                    case 0:
                        AntiDecompilerUtils.CallsizeOfcalli(method); // thanks to cursedsheep
                        break;
                    case 1:
                        AntiDecompilerUtils.CallUnaligned(method);
                        break;
                    case 2:
                        AntiDecompilerUtils.CallBox(method);
                        break;
                }
        }
    }
}