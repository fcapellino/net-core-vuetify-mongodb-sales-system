import { Module } from 'vuex';
import { RootState } from '../types';
import { actions } from './actions';
import { getters } from './getters';
import { mutations } from './mutations';
import { NotificationsState } from './types';

export const state: any = {
    data: undefined
};

const namespaced: boolean = true;

export const notifications: Module<NotificationsState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations
};
