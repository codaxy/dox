Ext.namespace('Dox.control');

Ext.define('Dox.control.DoxViewerPanel', {
	extend: 'Ext.tab.Panel',

	title: false,
	enableTabScroll: true,
	plain: true,    
    ui: 'orange-tab',
	defaults: {
		autoScroll: true
	},
	
	docXTypes:{
		'default':	'dox-document-panel',
		'class':	'dox-class-document-panel',
		'sql':		'dox-sql-document-panel'
	},
	
	/**
	 * Session parameter
	 */
	session: null,
	
	/**
	 * Viewport parameter
	 */	
	viewport: null,
	
	loadingUrl: '',
	loadingSection: '',
	
	/**
	 * Shows loading mask on a window preventing any further user interactions until
	 * <em>hideLoadingMask</em> is called
	 */
	showLoadingMask: function() {
		if (!this.loadMaskEl)
			this.loadMaskEl = new Ext.LoadMask(this.getEl(), this.loadMaskText ? {msg: this.loadMaskText} : null );
		this.loadMaskEl.show();
	},
	
	/**
	 * Hides loading mask on a window enabling user interactions that were disabled by
	 * <em>showLoadingMask</em>
	 */
	hideLoadingMask: function() {
		if (!this.loadMaskEl)
			return;
		this.loadMaskEl.hide();
	},
		
	initComponent: function() {
		Ext.apply(this, {
			items: {
				title: 'Dox',
				xtype: 'dox-search-panel',
				session: this.session,
				closable: false	
			},
			listeners: {		
				'afterlayout': this.onAfterLayout,
				scope: this
			},
			activeTab: 0
		});
			
		this.callParent(arguments);
	},
	
	onAfterLayout: function() {
		if (this.cmInitialized)
			return;
			
		var tabBar = this.down('tabbar');
		
		this.mon(tabBar.el, {
			'contextmenu': function(e, t){
				if (!this.cm) {
					this.cm = new Ext.menu.Menu({
						cp: null,
						items: [{
							text: 'Close this tab',
							handler: function(){
								this.closePanel(this.cm.cp);
							},
							scope: this
						}, '-', {
							text: 'Close other tabs',
							handler: function(){
								this.closeOtherPanels(this.cm.cp);
							},
							scope: this
						}]
					});
				}
				
				var tab = tabBar.getChildByElement(t);
				var index = tabBar.items.indexOf(tab)
				var p = this.getComponent(index)
				
				this.cm.items.items[0].setDisabled(!p.closable);
				this.cm.cp = p;
				
				e.preventDefault();
				var xy = e.getXY();
				this.cm.showAt(xy);
			},
			
			scope: this,
			delegate: 'div.x-tab'			
		});
		
		this.cmInitialized = true;				
	},
	
	closePanel: function(p) {
		this.remove(p);
	},
	
	closeOtherPanels: function(p) {
		var n = 0;
		while (this.items.items.length > n) {
			var pp = this.items.items[n];
			if (pp.closable && pp != p) 
				this.remove(pp);
			else 
				n++;
		}
	},
	 
	initEvents: function() {
		Dox.control.DoxViewerPanel.superclass.initEvents.call(this);
		this.body.on('click', this.onClick, this)
	},
	 
	loadDocument: function(url, section) {
		var id = 'doc-' + url;
		var tab = this.getComponent(id);
		if (tab) {
			this.setActiveTab(tab);
			if (section)
				tab.scrollToSection(section);
		}
		else {
			if (this.loadingUrl == url && (!section || this.loadingSection == section))
				return;
				
			this.loadingUrl = url;
			this.loadingSection = section;
			
			this.setLoading(true);
			this.session.remote.GetDoxDocumentHtml(url, {
				type: 'alert',
				success: function(html) {
					var regex = /dox:format="([^"]+)"/
					var match = regex.exec(html);
					var xtype = (match && match[1])
								? this.docXTypes[match[1]]
								: this.docXTypes['default'];
						  
					var index = url.lastIndexOf('.');
					var title = (index == -1) ? '?' : url.substr(index+1);
					this.add({
					    xtype: xtype,                        
						url: url,
						id: id,
						html: html,
						closable: true,
						title: title
					});
					
					this.loadDocument(url, section);
				},
				cleanup: function(){
					this.setLoading(false);
				},
				scope: this
			});		
		}
	},
	
	onClick: function(e, target) {
        if(target = e.getTarget('a:not(.exi)', 3)){
            var doc = Ext.fly(target).getAttributeNS('dox', 'doc');
            if(doc) {
				/* 
				 * Don't stop event here, since we actually want 
				 * the window.location to get changed, so that the back
				 * button trick works
				 */
                var section = Ext.fly(target).getAttributeNS('dox', 'section');
                this.loadDocument(doc, section);
			}
			else if (target.className == 'inner-link') {
				e.stopEvent();
				this.getActiveTab().scrollToSection(target.href.split('#')[1]);
			}
			else {
				e.stopEvent();
                window.open(target.href, '_blank'); // ensure new tab/window
            }
        }
		else if(target = e.getTarget('.micon', 2)){
            e.stopEvent();
            var tr = Ext.fly(target.parentNode);
            if (tr.hasCls('expandable')) {
				tr.toggleCls('expanded');
			}
		}		
	}
});

