private _pointer = ["examples\cs\httprequest.cs"] call ARMAEXT_fnc_load;
if (_pointer isEqualTo false) exitWith {diag_log 'Error';};

private _httprequestid = [_pointer, "send|http://google.com"] call ARMAEXT_fnc_run;
if (_httprequestid isEqualTo false) exitWith {diag_log 'Error';};

private _response = "wait";

while {_response isEqualTo "wait"} do {
	_response = [_pointer, format['response|%1', _httprequestid]] call ARMAEXT_fnc_run;
	uiSleep 1;
};
diag_log _response;