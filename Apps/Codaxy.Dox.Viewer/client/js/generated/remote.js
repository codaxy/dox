Ext.define('Dox.Session.remoting.Proxy', {
	extend: 'Dextop.Session.remoting.Proxy',
	GetDoxDocumentHtml: function(url, callback, scope) { this.invokeRemoteMethod(callback, scope, 'GetDoxDocumentHtml', [url]);},
	Search: function(pattern, callback, scope) { this.invokeRemoteMethod(callback, scope, 'Search', [pattern]);}
});

Ext.define('Dox.window.SimpleWindow.remoting.Proxy', {
	extend: 'Dextop.Window.remoting.Proxy'
});

