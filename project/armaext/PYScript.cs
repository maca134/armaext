using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace armaext
{
    public class PYScript : IScript
    {
        private ObjectOperations _operations;
        private PythonFunction _pythonFunc;

        static PYScript()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) => {
                string resourceName = "armaext.ironpython." + new AssemblyName(args.Name).Name + ".dll";
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
        }
        public void Load(string file)
        {
            var source = File.ReadAllText(file);
            ScriptEngine engine = Python.CreateEngine();
            ScriptSource script = engine.CreateScriptSourceFromString(source, "path-to-py");
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
