import axios from "axios";
import Vue from 'vue';
import { AuthManager } from '../auth/app.auth.manager';
import { Notify } from '../common/notify';
import router from '../router';
import { UserService } from '../services/user.service';

// full config:  https://github.com/axios/axios#request-config
// axios.defaults.baseURL = process.env.baseURL || process.env.apiUrl || '';
// axios.defaults.headers.common['Authorization'] = AUTH_TOKEN;
// axios.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded';

let config = {
    // baseURL: process.env.baseURL || process.env.apiUrl || ""
    // timeout: 60 * 1000, // Timeout
    // withCredentials: true, // Check cross-site Access-Control
};

const _axios = axios.create(config);

_axios.interceptors.request.use(
    function (config) {
        var token = localStorage.getItem('token');
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`;
        }
        return config;
    },
    function (error) {
        return Promise.reject(error);
    }
);

// add a response interceptor
_axios.interceptors.response.use(
    function (response) {
        // do something with response data
        if (response.data.error === true) {
            setTimeout(() => Notify.pushErrorNotification(response.data.errorMessage), 250);
        }
        return response;
    },
    async function (error) {
        // do something with response error
        const status = error?.response?.status;
        const headers = error?.response?.headers;
        const config = error?.config;

        switch (status) {
            case 401:
                var authManager = new AuthManager(new UserService());
                var tokenExpired = JSON.parse(headers['token-expired']?.toLowerCase() || null);
                if (tokenExpired === true) {
                    await authManager.tryRefreshToken();
                }

                if (authManager.authenticated() === true) {
                    var token = localStorage.getItem('token');
                    if (token) {
                        config.headers['Authorization'] = `Bearer ${token}`;
                    }

                    //return original request 
                    return axios(config);
                }
                else {
                    router.push({ name: 'login' });
                }
                break;
            default:
                setTimeout(() => Notify.pushErrorNotification('Error. The operation cannot be completed.'), 250);
        }
    }
);

Plugin.install = function (Vue, options) {
    Vue.axios = _axios;
    window.axios = _axios;
    Object.defineProperties(Vue.prototype, {
        axios: {
            get() {
                return _axios;
            }
        },
        $axios: {
            get() {
                return _axios;
            }
        },
    });
};

Vue.use(Plugin)

export default Plugin;
