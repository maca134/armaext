/*
	ARMAEXT_fnc_load

	Description:
	Loads a script.

	Parameter(s):
	_this select 0: Path to script to load (String)
	
	Returns:
	A pointer to use to invoke the loaded script (Number)
*/

params [
	['_script', '', ['']]
];

private _pointer = "armaext" callExtension format['load%1%2%1', toString[10], _script];
if (_pointer == 'ERROR') exitWith {
	diag_log 'armaext load error - check console';
	false
};
_pointer = parseNumber _pointer;
_pointer