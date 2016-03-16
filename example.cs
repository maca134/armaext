// Load some dll
// #r "System.Data.dll"
// #r "c:\path\to\System.Data.dll"
// Load relative dll
// #r ".\SomeDll.dll"

using System;
using System.Text;

public class Startup
{
	public static int _count = 0;
    public static string Invoke(string input)
    {
		_count++;
        Console.WriteLine("This is from .cs file");
        return "Hello " + input + " - " + _count.ToString();
    }
}