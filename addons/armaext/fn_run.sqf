/*
	ARMAEXT_fnc_run

	Description:
	Runs a script.

	Parameter(s):
	_this select 0: Pointer to the script to run (Number)

	_this select 1: Arguments to send to the script (String)
	
	Returns:
	The results of the invoked script (String)
*/

params [
	['_pointer', -1, [0, '']],
	['_args', '', ['']]
];
if (_pointer isEqualTo -1) exitWith {false};
private _response = "armaext" callExtension format['run%1%2%1%3%1', toString[10], _pointer, _args];
if (_pointer == 'ERROR') exitWith {
	diag_log 'armaext run error - check console';
	false
};
_response