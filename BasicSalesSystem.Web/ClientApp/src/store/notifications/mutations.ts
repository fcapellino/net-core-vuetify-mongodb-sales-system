import * as _ from 'lodash';
import { MutationTree } from 'vuex';
import { Notification, NotificationsState } from './types';

export const mutations: MutationTree<NotificationsState> = {
    push_success_notification(state, notification) {
        var n: Notification = notification;
        if (!_.isEqual(state.data, n)) {
            state.data = n;
        }
    },
    push_error_notification(state, notification) {
        var n: Notification = notification;
        if (!_.isEqual(state.data, n)) {
            state.data = n;
        }
    }
};
