﻿using armaextconsole;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace armaextconsole
{
    public class ARMAExt
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int RVExtension_t(StringBuilder output, int outputSize, char[] function);

        private IntPtr _dllPtr;
        private RVExtension_t _rvextension;
        public bool Loaded = false;

        public void Load(String dllPath)
        {
            if (!File.Exists(dllPath))
            {
                throw new ARMAExtException();
            }
            _dllPtr = Win32.LoadLibrary(dllPath);
            _rvextension = loadFunction<RVExtension_t>("_RVExtension@12");
            Loaded = true;
        }

        public String Invoke(String input)
        {
            if (!Loaded)
            {
                throw new ARMAExtException();
            }
            StringBuilder output = new StringBuilder();
            char[] function = input.ToCharArray();
            int outputSize = 10000;
            output.Length = outputSize;
            _rvextension(output, outputSize, function);
            return output.ToString();
        }

        public void Unload()
        {
            if (!Loaded)
            {
                Win32.FreeLibrary(_dllPtr);
            }
            _rvextension = null;
            Loaded = false;
        }

        private T loadFunction<T>(string name) where T : class
        {
            IntPtr address = Win32.GetProcAddress(_dllPtr, name);
            System.Delegate fn_ptr = Marshal.GetDelegateForFunctionPointer(address, typeof(T));
            return fn_ptr as T;
        }
    }
}
