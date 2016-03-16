using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace armaext
{
    internal class CSScript : IScript
    {
        private readonly Regex _referenceRegex = new Regex(@"^[\ \t]*(?:\/{2})?\#r[\ \t]+""([^""]+)""", RegexOptions.Multiline);

        private object _instance = null;
        private MethodInfo _method = null;
        private string _script = "";
        private Dictionary<string, Assembly> _references = new Dictionary<string, Assembly>();

        public CSScript()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
            {
                Assembly result = null;
                _references.TryGetValue(args.Name, out result);
                return result;
            };
        }

        public void Load(string file)
        {
            _script = file;
            var basepath = Path.GetDirectoryName(file);
            var source = File.ReadAllText(file);

            List<string> references = new List<string>();
            Match match = _referenceRegex.Match(source);
            while (match.Success)
            {
                var dll = match.Groups[1].Value;
                if (!Path.IsPathRooted(dll))
                {
                    var dllpath = Path.Combine(basepath, dll);
                    if (File.Exists(dllpath))
                    {
                        dll = dllpath;
                    }
                }
                references.Add(dll);
                source = source.Substring(0, match.Index) + source.Substring(match.Index + match.Length);
                match = _referenceRegex.Match(source);
            }
            Dictionary<string, string> options = new Dictionary<string, string> {
                { "CompilerVersion", "v4.0" }
            };
            CSharpCodeProvider csc = new CSharpCodeProvider(options);
            CompilerParameters parameters = new CompilerParameters();
            parameters.CompilerOptions = "/platform:x86";
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.AddRange(references.ToArray());
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

            CompilerResults results = csc.CompileAssemblyFromSource(parameters, source);
            if (results.Errors.HasErrors)
            {
                throw new ScriptException(results.Errors[0].ToString());
            }
            var assembly = results.CompiledAssembly;

            foreach (var reference in references)
            {
                var dll = reference;
                try
                {
                    var referencedAssembly = Assembly.UnsafeLoadFrom(dll);
                    _references[referencedAssembly.FullName] = referencedAssembly;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Loading " + dll + " - " + ex.Message);
                }
            }

            Type startupType = assembly.GetType("Startup", true, true);
            _instance = Activator.CreateInstance(startupType, false);
            MethodInfo invokeMethod = startupType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Static);
            if (invokeMethod == null)
            {
                throw new ScriptException("Unable to access CLR method to wrap through reflection. Make sure it is a public instance method.");
            }
            _method = invokeMethod;
        }

        public string Invoke(string input)
        {
            if (_method == null)
                throw new NullReferenceException("Script has not been loaded");
            return _method.Invoke(_instance, new object[] { input }) as string;
        }
    }
}