"use strict";

import axios from "axios";
import Vue from 'vue';
import { Notify } from '../common/notify';

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
        // do something before request is sent
        //request.headers['RequestVerificationToken'] = aftoken;
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
            Notify.pushErrorNotification(response.data.errorMessage);
        }
        return response;
    },
    function (error) {
        // do something with response error
        return Promise.reject(error);
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
