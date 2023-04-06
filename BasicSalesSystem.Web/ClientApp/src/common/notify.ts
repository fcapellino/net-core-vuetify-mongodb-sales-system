import store from '../store/index';

export abstract class Notify {

    public static pushSuccessNotification(message: string): void {
        store.dispatch('notifications/push_success_notification', message);
    }

    public static pushErrorNotification(errorMessage: string): void {
        store.dispatch('notifications/push_error_notification', errorMessage);
    }
}
