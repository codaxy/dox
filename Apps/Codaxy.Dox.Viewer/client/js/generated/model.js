Ext.define('Dox.Application.model.TreeNode',
{
	extend: 'Ext.data.Model',
	fields: [{
		name: 'id',
		type: 'string',
		useNull: true
	}, {
		name: 'text',
		type: 'string',
		useNull: true
	}, {
		name: 'iconCls',
		type: 'string',
		useNull: true
	}, {
		name: 'cls',
		type: 'string',
		useNull: true
	}, {
		name: 'leaf',
		type: 'boolean',
		useNull: true
	}, {
		name: 'expanded',
		type: 'boolean',
		useNull: true
	}, {
		name: 'type',
		type: 'string',
		useNull: true
	}, {
		name: 'qtip',
		type: 'string',
		useNull: true
	}],
	idProperty: 'id'
});
