var request = require('request');

var responses = {};
var pointer = 0;

module.exports = function (data, complete) {
	var parts = data.split('|');
	var cmd = parts[0];
	var arg = parts[1] || '';
	pointer++;
	switch (cmd) {
		case 'send':
			var index = pointer;
			request(arg, function (error, response, body) {
				console.log('request complete ' + arg + '. ID: ' + index.toString());
				responses[index] = (error) ? 'error\n' + error : body;
			});
			console.log('sending request ' + arg + '. ID: ' + index.toString());
			complete(null, index.toString());
			break;
		case 'response':
			arg = parseInt(arg);
			if (!responses[arg]) 
				return complete(null, 'wait');
			var response = responses[arg];
			delete responses[arg];
			complete(null, response);
			break;
		default:
			complete(null, 'error\nunknown command');
	};
};