Ext.define('Codaxy.Dox.SimpleWindow', {
    extend: 'Dextop.Window',
    width: 400,
    height: 350,

    title: 'Simple Window',

    initComponent: function () {

        Ext.apply(this, {
            layout: 'fit'
        });

        this.callParent(arguments);
    }
});