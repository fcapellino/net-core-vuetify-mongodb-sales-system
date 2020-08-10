import { GetterTree } from 'vuex';
import { RootState } from '../types';
import { Notification, NotificationsState } from './types';

export const getters: GetterTree<NotificationsState, RootState> = {
    get_notification(state): Notification {
        return state.data;
    }
};
