export interface NotificationsState {
    data: Notification;
}

export interface Notification {
    message: string;
    type: string;
    timestamp: Date;
}
