using System;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

/*
 * This is very unstable, sometimes you have to disable / modify something by yourself
 * I'll fix it later. If i come up with good ideas
 */
namespace AntiDecompiler_Cleaner {
    internal static class Program {
        private static void Main(string[] args) {
            Console.Title = "Anti Decompiler Cleaner - Prab ";
            Console.ForegroundColor = ConsoleColor.Yellow;
            ModuleDefMD module = null;
            try {
                module = ModuleDefMD.Load(args[0]);
                Console.WriteLine("[?] File Loaded: {0}", module);
                foreach (var dependencies in module.GetAssemblyRefs())
                    Console.WriteLine($"[?] Dependencies : {dependencies.Name}");
            }
            catch {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[^] Drag n Drop ! ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            foreach (var type in module.GetTypes()) {
                foreach (var method in type.Methods)
                    if (method != null && method.HasBody && method.Body.HasInstructions)
                        try {
                            AntiDecompilerPhase.Execute(method);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("[*] Cleaning method 0x{0:X2} {1}", method.MDToken.Raw, method.Name);
                        }
                        catch (Exception ex) {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[x] Failed to clean method {0}\n     {1}", method.Name, ex.Message);
                        }
            }

            var savingPath = module.Kind == ModuleKind.Dll
                ? args[0].Replace(".dll", "-noAnti.dll")
                : args[0].Replace(".exe", "-noAnti.exe");
            if (module.IsILOnly) {
                var opts = new ModuleWriterOptions(module) {
                    MetadataOptions = {Flags = MetadataFlags.PreserveAll}, Logger = DummyLogger.NoThrowInstance
                };
                module.Write(savingPath, opts);
            }
            else {
                var opts = new NativeModuleWriterOptions(module, true) {
                    MetadataOptions = {Flags = MetadataFlags.PreserveAll}, Logger = DummyLogger.NoThrowInstance
                };
                module.NativeWrite(savingPath, opts);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[+] Finished !\n[+] File Saved at : {0}", savingPath);
            Console.ReadLine();
        }
    }
}