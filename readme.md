# ARMA EXT - [Downloads](https://github.com/maca134/armaext/releases)
This is an extension/mod that will allow you to compile/run C#/Javascript (NodeJS)/Python on the fly.

More languages can/will be added if requested.

The mod has only 2 functions:

Load script, returns a pointer
```
_pointer = [_path_to_script] call ARMAEXT_fnc_load
```

Run the script and return the results
```
_result = [_pointer, _args] call ARMAEXT_fnc_run;
```

The c# has to implement the follow pattern:
```
class Startup {
    public static string Invoke(string input)
    {
        return "Hello World from C#";
    }
}
```

The Javascript has to return a function with 2 arguments:
```
module.exports = function (data, callback) {
	var err = null;
	callback(err, "Hello World from NodeJS");
};
```
You are free to use any nodejs modules

Python has to evaluate to a Python lambda expression that takes one parameter.
```
def example(input):
	return "Python welcomes " + input

lambda x: example(x)
```

- If you use this on clients, DISABLE BATTLEYE!
- Output is auto truncated, no way to get current output size yet

## Credits
Most of this was inspired by the work done by [tjanczuk](https://github.com/tjanczuk) on [Edge](https://github.com/tjanczuk/edge)

## Licence
This work is licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

[![Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](https://i.creativecommons.org/l/by-nc-sa/4.0/80x15.png)](http://creativecommons.org/licenses/by-nc-sa/4.0/)

If you want to use this commercially (or include it in "ARMA Samples", like my ARMA c# extension pattern) you must ask permission. *You know who you are!*