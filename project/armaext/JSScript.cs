using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace armaext
{
    internal class JSScript : IScript
    {
        private static string _basePath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        static JSScript()
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (var name in names)
            {
                var parts = new List<string>(name.Split('.'));
                var ext = parts[parts.Count - 1];
                parts.RemoveAt(0);
                parts.RemoveAt(parts.Count - 1);
                if (parts[0] != "edge")
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
        
        private Func<object, Task<object>> _script;

        public void Load(string file)
        {
            _script = Edge.Func(file);
        }

        public string Invoke(string input)
        {
            var task = _script.Invoke(input);
            task.Wait();
            return task.Result as string;
        }
    }
}