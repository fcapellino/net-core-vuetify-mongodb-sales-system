import Vue from 'vue';
import Router from 'vue-router';
import CategoriesComponent from './components/categories.vue';
import ProductsComponent from './components/products.vue';
import RolesComponent from './components/roles.vue';
import UsersComponent from './components/users.vue';
import Home from './views/Home.vue';

Vue.use(Router);

export default new Router({
    mode: 'history',
    base: process.env.BASE_URL,
    routes: [
        {
            path: '/',
            name: 'home',
            component: Home
        },
        {
            path: '/categories',
            name: 'categories',
            component: CategoriesComponent
        },
        {
            path: '/products',
            name: 'products',
            component: ProductsComponent
        },
        {
            path: '/users',
            name: 'users',
            component: UsersComponent
        },
        {
            path: '/roles',
            name: 'roles',
            component: RolesComponent
        }
        //{
        //    path: '/counter',
        //    name: 'counter',
        //    // route level code-splitting
        //    // this generates a separate chunk (about.[hash].js) for this route
        //    // which is lazy-loaded when the route is visited.
        //    component: () => import(/* webpackChunkName: "counter" */ './views/Counter.vue'),
        //},
        //{
        //    path: '/fetch-data',
        //    name: 'fetch-data',
        //    component: () => import(/* webpackChunkName: "fetch-data" */ './views/FetchData.vue'),
        //},
    ],
});
