import dateFilter from '@/filters/date.filter';
import store from '@/store/index';
import 'core-js/stable';
import 'regenerator-runtime/runtime';
import Vue from 'vue';
import App from './App.vue';
import './plugins/axios';
import vuetify from './plugins/vuetify';
import './registerServiceWorker';
import router from './router';

Vue.config.productionTip = false;
Vue.filter('date', dateFilter);

new Vue({
    vuetify,
    router,
    store,
    render: (h) => h(App),
}).$mount('#app');
