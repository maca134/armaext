import urllib2

def HttpRequest(url):
	return urllib2.urlopen(url).read()

lambda x: HttpRequest(x)