Ext.namespace('Dox.control');

Ext.define('Dox.control.DoxExplorerPanel', {
	extend: 'Ext.tree.Panel',

	title: 'Books',
	width: 100,
	autoScroll: true,
	rootVisible: false,
	hiddenNodes: [],
		
	/**
	 * Session parameter
	 */
	session: null,
	
	/**
	 * Viewport parameter
	 */
	viewport: null,
	
	currentFilter: null,
	
	initComponent: function() {		
		Ext.apply(this, {
			listeners: {
				scope: this,
				'itemclick': function(view, record, item, index, e) {
					if (!record || !record.raw)
						return;
						
					e.stopEvent();
						
					switch (record.raw.type) {
						case "document":
							this.viewport.loadDocument(record.raw.id);
							break;
						default:
							break;
					}
					
					if (!record.isLeaf()) {
					    if (record.isExpanded()) {
						    record.collapse(false);
					    } else{
						    record.expand(false);
					    }
					}
				}				
			},
			
			tbar: [{
				xtype: 'trigger',
				emptyText: 'Search...',
				triggerCls: 'x-form-clear-trigger',
				enableKeyEvents: true,
				explorerPanel: this,
                flex: 1,
				
				onTriggerClick: function(e) {
					this.setValue('');
					this.explorerPanel.filterTree(this, e);
				},
				
				listeners: {
					'keydown': {
						fn: this.filterTree,
						buffer: 350,
						scope: this
					}
				}
			}]		
		});	
		
		this.callParent(arguments);
	},
	
	reloadBooks: function() {
		this.store.load();
	},
	
	
	/**
	 * Hacks the quirk in TreeNode where anim parameter of TreeNode.expand
	 * doesn't propagate to lower levels when called with deep == true
	 */
	expandAllQuick: function() {
		this.animate = false;
		this.expandAll();
		this.animate = true;
	},

	
	filterTree: function(t, e) {
		var text = t.getValue();

		this.collapseAll();
		
		delete this.store.getProxy().data;
		this.store.getProxy().data = this.session.getTreeData(text);
		this.store.load({
			callback: function() {
				if (text) // don't expand if the filter is empty
					this.expandAllQuick();
			},
			scope: this
		});
	},
	
	selectDocument: function(url) {
		var node = this.store.getNodeById(url);
		if (node) {
			this.getView().getSelectionModel().select(node);
			node.bubble(function(n) {
				n.expand();
			});
		}
	}
});
