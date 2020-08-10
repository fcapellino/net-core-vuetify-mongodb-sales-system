<template>
    <v-app>
        <v-navigation-drawer persistent
                             :mini-variant="miniVariant"
                             :mini-variant-width="60"
                             :clipped="clipped"
                             v-model="drawer"
                             enable-resize-watcher fixed app>
            <v-list dense>
                <template v-for="item in items">
                    <v-tooltip right v-if="item.children">
                        <template v-slot:activator="{ on, attrs }">
                            <v-list-group v-if="item.children"
                                          :key="item.text"
                                          v-model="item.model"
                                          v-on="on"
                                          :prepend-icon="item.model ? item.icon : item['icon-alt']"
                                          append-icon="">
                                <template v-slot:activator>
                                    <v-list-item-content>
                                        <v-list-item-title>
                                            {{ item.text }}
                                        </v-list-item-title>
                                    </v-list-item-content>
                                </template>
                                <v-tooltip right v-for="(child, i) in item.children" :key="i">
                                    <template v-slot:activator="{ on, attrs }">
                                        <v-list-item :to="child.link" v-on="on">
                                            <v-list-item-action v-if="child.icon">
                                                <v-icon>{{ child.icon }}</v-icon>
                                            </v-list-item-action>
                                            <v-list-item-content>
                                                <v-list-item-title>
                                                    {{ child.text }}
                                                </v-list-item-title>
                                            </v-list-item-content>
                                        </v-list-item>
                                    </template>
                                    <span>{{ child.text }}</span>
                                </v-tooltip>
                            </v-list-group>
                        </template>
                        <span>{{ item.text }}</span>
                    </v-tooltip>
                    <v-tooltip v-else right :key="item.text">
                        <template v-slot:activator="{ on, attrs }">
                            <v-list-item :to="item.link" v-on="on">
                                <v-list-item-action>
                                    <v-icon>{{ item.icon }}</v-icon>
                                </v-list-item-action>
                                <v-list-item-content>
                                    <v-list-item-title>
                                        {{ item.text }}
                                    </v-list-item-title>
                                </v-list-item-content>
                            </v-list-item>
                        </template>
                        <span>{{ item.text }}</span>
                    </v-tooltip>
                </template>
            </v-list>
        </v-navigation-drawer>
        <v-app-bar app :clipped-left="clipped" color="primary" dark>
            <v-app-bar-nav-icon @click.stop=""></v-app-bar-nav-icon>
            <v-btn class="d-none d-lg-flex" icon @click.stop="miniVariant = !miniVariant">
                <v-icon v-html="miniVariant ? 'chevron_right' : 'chevron_left'"></v-icon>
            </v-btn>
            <v-toolbar-title v-text="title"></v-toolbar-title>
            <v-spacer></v-spacer>
        </v-app-bar>

        <v-main>
            <router-view />
        </v-main>

        <v-snackbars :objects.sync="messages" :timeout="40000" bottom right></v-snackbars>
        <v-footer app><span>&nbsp;Software &nbsp;&copy;&nbsp;2020</span></v-footer>
    </v-app>
</template>

<script lang="ts">
    import HelloWorld from '@/components/HelloWorld.vue';
    import VSnackbars from "@/components/v-snackbars.vue";
    import { Component, Vue, Watch } from 'vue-property-decorator';

    @Component({
        components: { HelloWorld, VSnackbars },
        methods: {
        }
    })
    export default class App extends Vue {
        private clipped: boolean = true;
        private drawer: boolean = true;
        private miniVariant: boolean = true;
        private right: boolean = true;
        private title: string = 'Basic Sales System';

        private items = [
            { icon: 'home', text: 'Home', link: '/' },
            { icon: 'mdi-drawing-box', text: 'Categories', link: '/categories' },
            { icon: 'mdi-cube', text: 'Products', link: '/products' },
            { icon: 'mdi-cart', text: 'Purchases', link: '/purchases' },
            { icon: 'mdi-package-down', text: 'Suppliers', link: '/suppliers' },
            { icon: 'mdi-cash', text: 'Sales', link: '/sales' },
            { icon: 'mdi-human-male-female', text: 'Customers', link: '/customers' },
            { icon: 'mdi-clipboard-account', text: 'Users', link: '/users' },
            { icon: 'mdi-clipboard-alert', text: 'Roles', link: '/roles' },
            {
                icon: 'mdi-chevron-up',
                'icon-alt': 'mdi-chevron-down',
                text: 'Reports',
                model: false,
                children: [
                    { icon: 'mdi-chart-bar', text: 'Purchases', link: '/purchases-report' },
                    { icon: 'mdi-chart-bar', text: 'Sales', link: '/sales-report' },
                ],
            }
        ];

        private messages: Array<any> = [];
        @Watch('$store.state.notifications', { immediate: true, deep: true })
        private async onNewNotification(value: any, oldValue: any) {
            var self = this;
            var notification = self.$store.getters['notifications/get_notification'];
            if (notification) {
                self.messages.push({
                    message: notification.message,
                    color: notification.type
                });
            }
        }
    }
</script>

<style>
    [class$="--disabled"],
    [class*="--disabled "] * {
        color: #616161 !important
    }

    .v-snack__wrapper{
        max-width:350px;
    }

    input[disabled] {
        color: #616161 !important
    }

    textarea[disabled] {
        color: #616161 !important
    }

    .p-green {
        color: #3cb371
    }

    .p-red {
        color: #cd5c5c
    }
</style>
