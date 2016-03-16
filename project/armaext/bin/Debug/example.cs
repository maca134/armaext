/*
private _pointer = ['.\example.cs'] call ARMACS_fnc_load;
if (_pointer isEqualTo false) exitWith {};

private _response = [_pointer, 'Hello world'] call ARMACS_fnc_run;
if (_response isEqualTo false) exitWith {};

diag_log format['Response: %1', _response];
*/

// Load some dll
// #r "System.Data.dll"
// Load relative dll
// #r ".\SomeDll.dll"

using System;
using System.Text;

public class Startup
{
	public static int _count = 0;
    public static void RVExtension(StringBuilder output, int outputSize, string function)
    {
		_count++;
        Console.WriteLine("This is from .cs file");
        output.Append("Hello " + function + " - " + _count.ToString());
    }
}