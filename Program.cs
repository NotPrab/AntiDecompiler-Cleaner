using System;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.DotNet.Emit;
using System.Linq;

namespace AntiDecompiler_Cleaner {
    static class Program {
        static void Main(string[] args) {
            Console.Title = "Anti Decompiler Cleaner - Prab";
            ModuleDefMD module = null;
            try {
                module = ModuleDefMD.Load(args[0]);
                Console.WriteLine("[?] File Loaded: {0}", module);
            }
            catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[*] Error ! {0}", ex);
            }
            CrashDnSpy(module);
            CrashDnSpy2(module);
            CrashDnSpy3(module);
            var Path = module.Kind == ModuleKind.Dll ? args[0].Replace(".dll", "-Cleaned.dll") : args[0].Replace(".exe", "-Cleaned.exe");
            if (module.IsILOnly) {
                var opts = new ModuleWriterOptions(module);
                opts.MetadataOptions.Flags = MetadataFlags.PreserveAll;
                opts.Logger = DummyLogger.NoThrowInstance;
                module.Write(Path, opts);
            }
            else {
                var opts = new NativeModuleWriterOptions(module, true);
                opts.MetadataOptions.Flags = MetadataFlags.PreserveAll;
                opts.Logger = DummyLogger.NoThrowInstance;
                module.NativeWrite(Path, opts);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[+] Finished !\n[+] File Saved at : {0}", Path);
            Console.ReadLine();
        }
        // Thanks to MindSystem and Tiex
        // This support most of ConfuserEx (Hide method in every method)
        public static void CrashDnSpy(ModuleDefMD module) {
            foreach (var method in module.GetTypes().SelectMany(type => type.Methods)) {
                if (!method.HasBody)
                    continue;
                if (!method.Body.HasExceptionHandlers)
                    continue;
                var instr = method.Body.ExceptionHandlers;
                for (int i = 0; i < instr.Count; i++) {
                    var Start = instr[i].TryStart;
                    if (Start.OpCode != OpCodes.Calli || Start.Operand != null)
                        continue;
                    instr.Remove(instr[i]);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[+] Cleaning AntiDeompiler method : 0x{0:x2} {1} ", method.MDToken.Raw, method.Name);
                }
            }
        }
        // This works for DotNetSafer
        public static void CrashDnSpy2(ModuleDefMD module) {
            foreach (var method in module.GetTypes().SelectMany(type => type.Methods)) {
                if (!method.HasBody)
                    continue;
                if (!method.Body.HasInstructions)
                    continue;
                var instr = method.Body.Instructions;
                for (int i = 0; i < instr.Count; i++) {
                    try {
                        if (!instr[i].IsBr())
                            continue;
                        if (instr[i + 1].OpCode != OpCodes.Unaligned)
                            continue;
                        instr[i].OpCode = OpCodes.Nop;
                        instr[i + 1].OpCode = OpCodes.Nop;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[+] Cleaning AntiDeompiler method : 0x{0:x2} {1} ", method.MDToken.Raw, method.Name);
                    }
                    catch (Exception ex) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[+] Failed to clean method {0}\n     {1}", method.Name, ex.Message);
                    }
                }
            }
        }
        // This is very buggy
        // This may cause Unhandled Exeception 
        public static void CrashDnSpy3(ModuleDefMD module) {
            foreach (var method in module.GetTypes().SelectMany(type => type.Methods)) {
                if (!method.HasBody)
                    continue;
                if (!method.Body.HasInstructions)
                    continue;
                var instr = method.Body.Instructions;
                for (int i = 0; i < instr.Count; i++) {
                    try {
                        if (instr[i].OpCode != OpCodes.Box)
                            continue;
                        if (instr[i + 1].OpCode != OpCodes.Stelem_Ref)
                            continue;
                        instr[i].OpCode = OpCodes.Nop;
                        instr[i + 1].OpCode = OpCodes.Nop;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[+] Cleaning AntiDeompiler method : 0x{0:x2} {1} ", method.MDToken.Raw, method.Name);
                    }
                    catch (Exception ex) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[+] Failed to clean method {0}\n     {1}", method.Name, ex.Message);
                    }
                }
            }
        }
    }
}
