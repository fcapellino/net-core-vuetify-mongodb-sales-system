import Vue from 'vue';
import Router from 'vue-router';
import { AuthManager } from './auth/app.auth.manager';
import { Notify } from './common/notify';
import CategoriesComponent from './components/categories.vue';
import LayoutComponent from './components/common/layout.vue';
import CustomersComponent from './components/customers.vue';
import HomeComponent from './components/home.vue';
import LoginComponent from './components/login.vue';
import NotFoundComponent from './components/not-found.vue';
import ProductsComponent from './components/products.vue';
import PurchasesReportComponent from './components/purchases-report.vue';
import PurchasesComponent from './components/purchases.vue';
import RolesComponent from './components/roles.vue';
import SalesReportComponent from './components/sales-report.vue';
import SalesComponent from './components/sales.vue';
import SuppliersComponent from './components/suppliers.vue';
import UsersComponent from './components/users.vue';
import { UserService } from './services/user.service';

Vue.use(Router);

const router = new Router({
    mode: 'history',
    base: process.env.BASE_URL,
    routes: [
        {
            path: '',
            name: 'login',
            component: LoginComponent,
            meta: {
                allowAnonymous: true
            }
        },
        {
            path: '/',
            component: LayoutComponent,
            children: [
                {
                    path: '/home',
                    name: 'home',
                    component: HomeComponent,
                    meta: {
                        roles: ['administrator', 'salesman', 'storekeeper']
                    }
                },
                {
                    path: '/categories',
                    name: 'categories',
                    component: CategoriesComponent,
                    meta: {
                        roles: ['administrator', 'storekeeper']
                    }
                },
                {
                    path: '/products',
                    name: 'products',
                    component: ProductsComponent,
                    meta: {
                        roles: ['administrator', 'storekeeper']
                    }
                },
                {
                    path: '/purchases',
                    name: 'purchases',
                    component: PurchasesComponent,
                    meta: {
                        roles: ['administrator', 'storekeeper']
                    }
                },
                {
                    path: '/suppliers',
                    name: 'suppliers',
                    component: SuppliersComponent,
                    meta: {
                        roles: ['administrator', 'storekeeper']
                    }
                },
                {
                    path: '/sales',
                    name: 'sales',
                    component: SalesComponent,
                    meta: {
                        roles: ['administrator', 'salesman']
                    }
                },
                {
                    path: '/customers',
                    name: 'customers',
                    component: CustomersComponent,
                    meta: {
                        roles: ['administrator', 'salesman']
                    }
                },
                {
                    path: '/users',
                    name: 'users',
                    component: UsersComponent,
                    meta: {
                        roles: ['administrator']
                    }
                },
                {
                    path: '/roles',
                    name: 'roles',
                    component: RolesComponent,
                    meta: {
                        roles: ['administrator']
                    }
                },
                {
                    path: '/purchases-report',
                    name: 'purchases-report',
                    component: PurchasesReportComponent,
                    meta: {
                        roles: ['administrator', 'storekeeper']
                    }
                },
                {
                    path: '/sales-report',
                    name: 'sales-report',
                    component: SalesReportComponent,
                    meta: {
                        roles: ['administrator', 'salesman']
                    }
                }
            ]
        },
        {
            path: '*',
            component: NotFoundComponent,
            meta: {
                allowAnonymous: true
            }
        }
    ]
});

router.beforeEach(async (to, from, next) => {
    var authManager = new AuthManager(new UserService());
    var roles = to.meta.roles as Array<string>;
    var authorized = roles?.some(r => authManager.isInRole(r));

    if (to.meta.allowAnonymous === true) {
        if (authManager.authenticated() === true && to.name === 'login') {
            next({ name: 'home' });
        }
        else {
            next();
        }
    }
    else {
        var authenticated = authManager.authenticated();
        if (authenticated === false) {
            await authManager.tryRefreshToken();
        }

        if (authManager.authenticated() === true) {
            if (authorized) {
                next();
            }
            else {
                next({ name: 'home' });
                setTimeout(() => Notify.pushErrorNotification('Unauthorized: Access is denied due to invalid credentials.'), 250);
            }
        }
        else {
            next({ name: 'login' });
        }
    }
});

export default router;
