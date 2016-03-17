using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace armaext
{
    public class PYScript : IScript
    {
        private ObjectOperations _operations;
        private PythonFunction _pythonFunc;

        private static string _basePath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        static PYScript()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) => {
                string resourceName = "armaext.ironpython." + new AssemblyName(args.Name).Name + ".dll";
                Console.WriteLine(resourceName);
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            };
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (var name in names)
            {
                var parts = new List<string>(name.Split('.'));
                var ext = parts[parts.Count - 1];
                parts.RemoveAt(0);
                parts.RemoveAt(parts.Count - 1);
                if (parts[0] != "ironpython" || parts[1] != "lib")
                    continue;
                var path = string.Join("\\", parts) + "." + ext;
                path = Path.Combine(_basePath, path);
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    Stream resFilestream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                    if (resFilestream != null)
                    {
                        BinaryReader br = new BinaryReader(resFilestream);
                        FileStream fs = new FileStream(path, FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        byte[] ba = new byte[resFilestream.Length];
                        resFilestream.Read(ba, 0, ba.Length);
                        bw.Write(ba);
                        br.Close();
                        bw.Close();
                        resFilestream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error extracting embedded resources - {0}", ex.Message);
                }
            }
        }
        public void Load(string file)
        {
            var source = File.ReadAllText(file);
            ScriptEngine engine = Python.CreateEngine();
            engine.SetSearchPaths(new string[] {
                Path.Combine(_basePath, "ironpython", "lib")
            });
            ScriptSource script = engine.CreateScriptSourceFromString(source);

            _pythonFunc = script.Execute() as PythonFunction;
            if (_pythonFunc == null)
            {
                throw new InvalidOperationException("The Python code must evaluate to a Python lambda expression that takes one parameter, e.g. `lambda x: x + 1`.");
            }

            _operations = engine.CreateOperations();
        }

        public string Invoke(string input)
        {
            object ret = _operations.Invoke(_pythonFunc, new object[] { input });
            var task = Task.FromResult(ret);
            task.Wait();
            return task.Result as string;
        }

    }
}
