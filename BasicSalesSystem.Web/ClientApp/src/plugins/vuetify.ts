import '@mdi/font/css/materialdesignicons.css';
import Vue from 'vue';
import Vuetify, { VAlert, VApp, VBtn, VDataTable, VFooter, VIcon, VList, VNavigationDrawer, VProgressLinear, VToolbar } from 'vuetify/lib';

// vue-cli a-la-carte installation
Vue.use(Vuetify, {
    components: {
        VAlert,
        VApp,
        VNavigationDrawer,
        VFooter,
        VList,
        VBtn,
        VIcon,
        VToolbar,
        VDataTable,
        VProgressLinear,
    },
});

const opts = {
    theme: {
        themes: {
            light: {
                primary: '#9E9E9E',
                accent: '#E91E63',
                secondary: '#212121',
                success: '#4CAF50',
                info: '#2196F3',
                warning: '#FB8C00',
                error: '#FF5252'
            }
        },
    },
};

export default new Vuetify(opts);
