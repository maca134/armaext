// Modified/inspired from https://github.com/tjanczuk/edge-cs/blob/master/src/edge-cs/EdgeCompiler.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace armaext
{
    public class Edge
    {
        static object syncRoot = new object();
        static bool initialized;
        static Func<object, Task<object>> compileFunc;
        static ManualResetEvent waitHandle = new ManualResetEvent(false);

        static string AssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<object> InitializeInternal(object input)
        {
            compileFunc = (Func<object, Task<object>>)input;
            initialized = true;
            waitHandle.Set();

            return Task.FromResult((object)null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<object> ConsoleLog(object input)
        {
            Console.WriteLine(input as string);
            return Task.FromResult((object)null);
        }

        // Find the entry point with `dumpbin /exports node.exe`, look for Start@node
        [DllImport("node.dll", EntryPoint = "#928", CallingConvention = CallingConvention.Cdecl)]
        static extern int NodeStart(int argc, string[] argv);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        public static Func<object, Task<object>> Func(string file)
        {
            if (!initialized)
            {
                lock (syncRoot)
                {
                    if (!initialized)
                    {
                        if (IntPtr.Size == 4)
                        {
                            LoadLibrary(AssemblyDirectory + @"\edge\x86\node.dll");
                        }
                        else if (IntPtr.Size == 8)
                        {
                            LoadLibrary(AssemblyDirectory + @"\edge\x64\node.dll");
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported architecture. Only x86 and x64 are supported.");
                        }

                        Thread v8Thread = new Thread(() =>
                        {
                            List<string> argv = new List<string>();
                            argv.Add("node");
                            string node_params = Environment.GetEnvironmentVariable("EDGE_NODE_PARAMS");
                            if (!string.IsNullOrEmpty(node_params))
                            {
                                foreach (string p in node_params.Split(' '))
                                {
                                    argv.Add(p);
                                }
                            }
                            argv.Add(AssemblyDirectory + "\\edge\\double_edge.js");
                            NodeStart(argv.Count, argv.ToArray());
                            waitHandle.Set();
                        });

                        v8Thread.IsBackground = true;
                        v8Thread.Start();
                        waitHandle.WaitOne();

                        if (!initialized)
                        {
                            throw new InvalidOperationException("Unable to initialize Node.js runtime.");
                        }
                    }
                }
            }

            if (compileFunc == null)
            {
                throw new InvalidOperationException("Edge.Func cannot be used after Edge.Close had been called.");
            }
            var tmp = Path.Combine(Path.GetDirectoryName(file), Guid.NewGuid().ToString() + Path.GetExtension(file));
            File.WriteAllText(tmp, File.ReadAllText(file));
            var code = string.Format("return require('{0}');", tmp.Replace("\\", "\\\\"));
            var task = compileFunc(code);
            task.Wait();
            File.Delete(tmp);
            return (Func<object, Task<object>>)task.Result;
        }
    }
}
