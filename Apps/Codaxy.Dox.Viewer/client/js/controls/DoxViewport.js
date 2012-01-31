Ext.namespace('Dox.control');

Ext.define('Dox.control.DoxViewport', {
	extend: 'Ext.container.Viewport',

	layout: 'border',
	
	/**
	 * Session parameter
	 */
	session: null,
	
	/**
	 *  Store parameter 
	 */
	store: null,
	
	initComponent: function() {		
		this.titleBar = new Ext.Component({
			cls: 'dox-viewer-title',
			frame: true,
			border: false,
			title: false,
			region: 'north',
			height: 40,
			html: 'Dox',
			padding: '0 0 5 5'
		});
		
		this.explorerPanel = new Dox.control.DoxExplorerPanel({
			region: 'west',
			split: true,
			width: 400,
			collapsible: true,
			margins: '3 0 6 6',
			cmargins: '3 3 6 6',
			
			session: this.session,
			viewport: this,
			store: this.store
		});
		
		this.viewerPanel = new Dox.control.DoxViewerPanel({
			region: 'center',
			margins: '3 6 6 0',
			session: this.session,
			viewport: this
		});
		
		Ext.apply(this, {
			items: [
				this.titleBar,
				this.explorerPanel,
				this.viewerPanel
			]
		});
		
		this.callParent(arguments);
	},
	
	loadDocument: function(url, section) {
		this.viewerPanel.loadDocument(url, section);		
		this.explorerPanel.selectDocument(url);
	}
});
