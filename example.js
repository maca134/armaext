var i = 0;

return function (data, complete) {
	console.log('This is from JS');
	i++;
	complete(null, 'js ' + i);
};