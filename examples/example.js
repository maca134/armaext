var i = 0;

module.exports = function (data, complete) {
	console.log('This is from JS');
	i++;
	complete(null, 'js ' + i);
};