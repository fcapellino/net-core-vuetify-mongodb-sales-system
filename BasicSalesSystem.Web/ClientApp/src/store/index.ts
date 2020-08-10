import Vue from 'vue';
import Vuex, { StoreOptions } from 'vuex';
import { counter } from './counter/index';
import { notifications } from './notifications/index';
import { RootState } from './types';

Vue.use(Vuex);

// Vuex structure based on https://codeburst.io/vuex-and-typescript-3427ba78cfa8

const store: StoreOptions<RootState> = {
    state: {
        version: '1.0.0', // a simple property
    },
    modules: {
        counter,
        notifications
    },
};

export default new Vuex.Store<RootState>(store);
