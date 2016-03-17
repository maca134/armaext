using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace armaext
{
    public static class DllEntry
    {

        static DllEntry()
        {
            ConsoleHelper.CreateConsole();
        }

        [DllExport("_RVExtension@12", CallingConvention = System.Runtime.InteropServices.CallingConvention.Winapi)]
        public static void RVExtension(StringBuilder output, int outputSize, string function)
        {
            outputSize = outputSize - 10;
            var parts = function.Split('\n');
            var cmd = parts[0];
            var file = (parts.Length > 1) ? parts[1] : "";
            var args = (parts.Length > 2) ? parts[2] : "";

            try
            {
                switch (cmd)
                {
                    case "load":
                        output.Append(Load(file, args));
                        break;
                    case "run":
                        var response = Run(file, args);
                        if (response.Length >= outputSize)
                        {
                            response = response.Substring(0, outputSize);
                        }
                        output.Append(response);
                        break;
                    default:
                        Console.WriteLine("ERROR: Unknown command " + cmd);
                        output.Append("ERROR");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Exception caught while trying to " + cmd + " - " + file + " - " + args + " - " + ex.Message);
                output.Append("ERROR");
            }
        }

        private static Dictionary<int, IScript> scripts = new Dictionary<int, IScript>();
        private static int scriptsPointer = 0;
        private static string _basePath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        private static string Load(string file, string args)
        {
            var response = string.Empty;
    
            var scriptpath = file;
            if (!Path.IsPathRooted(file))
            {
                scriptpath = Path.Combine(_basePath, file);
            }
            if (!File.Exists(scriptpath))
            {
                Console.WriteLine("ERROR: Could not find the script " + scriptpath);
                return "ERROR";
            }
            var ext = Path.GetExtension(scriptpath);

            IScript script = null;
            switch (ext)
            {
                case ".cs":
                    script = new CSScript();
                    break;
                case ".js":
                    script = new JSScript();
                    break;
                case ".py":
                    script = new PYScript();
                    break;
                default:
                    Console.WriteLine("ERROR: Unknown file type");
                    return "ERROR";
            }
            try
            {
                script.Load(scriptpath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Exception caught trying to load " + scriptpath + " - " + ex.Message);
                return "ERROR";
            }
            var pointer = scriptsPointer++;
            scripts.Add(pointer, script);
            return pointer.ToString();
        }

        private static string Run(string file, string args)
        {
            var pointer = Convert.ToInt32(file);
            if (!scripts.ContainsKey(pointer))
            {
                Console.WriteLine("ERROR: Invalid pointer");
                return "ERROR";
            }
            try
            {
                return scripts[pointer].Invoke(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Exception caught trying to run a function - " + ex.Message);
                return "ERROR";
            }
        }
    }
}
