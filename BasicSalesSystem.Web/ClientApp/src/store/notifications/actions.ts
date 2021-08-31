import { ActionTree } from 'vuex';
import { RootState } from '../types';
import { NotificationsState } from './types';

export const actions: ActionTree<NotificationsState, RootState> = {
    push_success_notification({ commit }, message: string): any {
        if (message?.trim()) {
            commit('push_success_notification', { message: message, type: 'success', timestamp: new Date() });
        }
    },
    push_error_notification({ commit }, message: string): any {
        if (message?.trim()) {
            commit('push_error_notification', { message: message, type: 'error', timestamp: new Date() });
        }
    }
};
