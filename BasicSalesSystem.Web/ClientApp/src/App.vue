<template>
    <v-app id="app">
        <v-main>
            <router-view />
        </v-main>
        <v-snackbars :objects.sync="messages"
                     :timeout="3000"
                     bottom right>
        </v-snackbars>
    </v-app>
</template>

<script lang="ts">
    import VSnackbars from "@/components/common/v-snackbars.vue";
    import { Component, Vue, Watch } from 'vue-property-decorator';

    @Component({
        components: { VSnackbars },
        methods: {
        }
    })
    export default class App extends Vue {
        private messages: Array<any> = [];

        @Watch('$store.state.notifications', { immediate: true, deep: true })
        private async onNewNotification(value: any, oldValue: any) {
            var self = this;
            var notification = self.$store.getters['notifications/get_notification'];
            if (notification) {
                self.messages.push({ message: notification.message, color: notification.type });
            }
        }
    }
</script>
