Ext.define('Dox.Session', {
	extend: 'Dextop.Session',

	locationHashCache: window.location.hash,
	viewport: null,
	store: null,
	
	initSession: function () {
		this.callParent(arguments);
		
		this.setStore();
		this.setViewport();
		
		this.urlTrackerTaskRunner = new Ext.util.TaskRunner();
		this.urlTrackerTaskRunner.start(this.urlTrackerTask = {
			interval: 50,
			scope: this,
			run: function() {
				if (this.locationHashCache != window.location.hash)
					this.onLocationChanged();
			}
		});
		
		this.onLocationChanged();
	},
	
	onTreeReady: function() {
		// load document immediately if there is a # in url
		this.onLocationChanged();		
	},
	
	onLocationChanged: function() {
		this.locationHashCache = window.location.hash;
		var parts = window.location.hash.split('#');
		if (parts[1])
			this.viewport.loadDocument(parts[1], parts[2]);		
	},

	setStore: function() {
		this.store = Ext.create('Ext.data.TreeStore', {
            proxy: {
                type: 'memory',
				data: this.getTreeData() // Solve attach/reattach problem, don't use data directly
            },
            root: {
				id: 'books',	
				expanded: true
            }
		});		
	},
	
	getTreeData: function(filter) {
		var re = new RegExp('^' + Ext.escapeRe(filter || ''), 'i');		
		var data = [];
		var populate = function(d, nodes) {
			if (!nodes) return;
			
			for (var i = 0; i < nodes.length; i++) {
				var node = nodes[i];
				if (node.leaf && !re.test(node.text)) 
					continue;
				
				var n = Ext.apply({}, node);
				n.children = [];
				
				populate(n.children, node.children);
				
				if (node.leaf || n.children.length > 0)
					d.push(n);
			}
		};
		
		populate(data, Dox.Application.booksTree);
		return data;
	},
	
	setViewport: function() {
		this.viewport = new Dox.control.DoxViewport({
			session: this,
			store: this.store
		});
		this.viewport.show();
	}
});