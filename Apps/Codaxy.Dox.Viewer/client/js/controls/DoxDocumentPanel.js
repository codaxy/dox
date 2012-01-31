Ext.namespace('Dox.control');

Ext.define('Dox.control.DoxDocumentPanel', {
	extend: 'Ext.panel.Panel',
	alias: 'widget.dox-document-panel',
	
	url: '',
	
	initComponent: function() {
		this.on({
			'activate': this.onActivate,
			scope: this	
		});
		
		this.callParent(arguments);		
	},
	
	scrollToSection : function(id){
		var el = this.body.query('a.' + id);
		if(el && (el = el[0])) {
			var top = (Ext.fly(el).getOffsetsTo(this.body)[1]) + this.body.dom.scrollTop;
			this.body.scrollTo('top', top-25, true);
        }
	},
	
	onActivate: function() {
		window.location.hash = '#' + this.url;
	}
});

Ext.define('Dox.control.DoxClassDocumentPanel', {
	extend: 'Dox.control.DoxDocumentPanel',
	alias: 'widget.dox-class-document-panel',

	iconCls: 'document-node',
	fieldsText: 'Fields',
	methodsText: 'Methods',
	propertiesText: 'Properties',
	eventsText: 'Events',
	
	initComponent: function() {
		Ext.apply(this, {
			tbar: [{
				iconCls: 'icon-field',
				text: this.fieldsText,
				scope: this,
				handler: function() {
					this.scrollToSection('fields');
				}
			},{
				iconCls: 'icon-property',
				text: this.propertiesText,
				scope: this,
				handler: function() {
					this.scrollToSection('properties');
				}
			},{
				iconCls: 'icon-method',
				text: this.methodsText,
				scope: this,
				handler: function() {
					this.scrollToSection('methods');
				}
			},{
				iconCls: 'icon-event',
				text: this.eventsText,
				scope: this,
				handler: function(){
					this.scrollToSection('events');
				}
			}]
		});
		
		this.callParent(arguments);
	},
	
	scrollToSection : function(id){
		var el = this.body.query('a.'+id); // Ext.get(id);
		if(el && (el = el[0])) {
			var top = (Ext.fly(el).getOffsetsTo(this.body)[1]) + this.body.dom.scrollTop;
			this.body.scrollTo('top', top-25, {duration:0.5, callback: function(){
				var e = Ext.fly(el).up('tr');
				if (e) e.highlight('8DB2E3');
            }});
        }
	}		
});

Ext.define('Dox.control.DoxSqlDocumentPanel', {
	extend: 'Dox.control.DoxDocumentPanel',
	alias: 'widget.dox-sql-document-panel',
	
	iconCls: 'document-node',
	tablesText: 'Tables',
	viewsText: 'Views',
	spText: 'Stored Procedures',
	
	initComponent: function() {
		Ext.apply(this, {
			tbar: [{
				iconCls: 'icon-table',
				text: this.tablesText,
				scope: this,
				handler: function() {
					this.scrollToSection('tables');
				}
			},{
				iconCls: 'icon-view',
				text: this.viewsText,
				scope: this,
				handler: function() {
					this.scrollToSection('views');
				}
			},{
				iconCls: 'icon-stored-procedure',
				text: this.spText,
				scope: this,
				handler: function() {
					this.scrollToSection('stored-procedures');
				}
			}]
		});
		
		this.callParent(arguments);
	},
	
	scrollToSection : function(id){
		var el = this.body.query('a.'+id); // Ext.get(id);
		if(el && (el = el[0])) {
			var top = (Ext.fly(el).getOffsetsTo(this.body)[1]) + this.body.dom.scrollTop;
			this.body.scrollTo('top', top-25, {duration:0.5, callback: function(){
				var e = Ext.fly(el).up('tr');
				if (e) e.highlight('8DB2E3');
            }});
        }
	}		
});


Ext.define('Dox.control.DoxSearchPanel', {
	extend: 'Ext.panel.Panel',
	alias: 'widget.dox-search-panel',
	
	iconCls: 'book-node',
	
	session: null,
	bodyCssClass: 'search-results',
	
	initComponent: function() {
		this.resultTemplate = new Ext.XTemplate(
	        '<tpl for=".">',
	        '<div class="search-item">',
				'<img src="client/images/s.gif" class="item-icon icon-{sectionType}"/>',
	            '<a class="section" dox:doc="{url}" dox:section="{section}" href="#{url}">{section}</a>',
				'<a class="document" dox:doc="{url}" href="#{url}">{document}</a>',
	            '<p class="description">{description}</p>',
	        '</div></tpl>');
			
		this.tbSearch = new Ext.form.TwinTriggerField({
			width: 180,
		    trigger1Cls:'x-form-clear-trigger',
		    trigger2Cls:'x-form-search-trigger',
			
		    onTrigger1Click : Ext.bind(this.clearSearch, this),
		    onTrigger2Click : Ext.bind(this.doSearch, this),
			
			listeners: {
				'specialkey': function(f, e){
	            	if(e.getKey() == e.ENTER) {
	                	this.doSearch();
	            	}
	        	},
				scope: this				
			}
		});
		
		Ext.apply(this, {
			tbar: [
				'Search:',
				this.tbSearch
			]
		});
		
		this.callParent(arguments);
	},
	
	clearSearch: function() {
		this.tbSearch.setValue('');
		this.doSearch();
	},
	
	doSearch: function() {
		var pattern = this.tbSearch.getValue();
		if (pattern == '') {
			this.body.update('');
			return;
		}
			
		this.session.remote.Search(pattern, {
			scope: this,
			success: function(result) {
				this.body.update(this.resultTemplate.apply(result));
			}
		});
	}
});


