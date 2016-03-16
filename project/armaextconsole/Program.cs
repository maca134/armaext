using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace armaextconsole
{
    class Program
    {
        private static ARMAExt _ext = new ARMAExt();

        static void Main(string[] args)
        {
            _ext.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "armaext.dll"));
            while (true)
            {
                Console.Write("Enter Command: ");
                string cmd = Console.ReadLine();
                if (cmd == "")
                {
                    _ext.Unload();
                    break;
                }

                Stopwatch watch = Stopwatch.StartNew();
                cmd = cmd.Replace("\\n", "\n") + "\n";
                string result = _ext.Invoke(cmd);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine(String.Format("Command took {0} ms to run", elapsedMs));
                Console.WriteLine(result);
                Console.WriteLine("--------------");
            }

        }
    }
}
