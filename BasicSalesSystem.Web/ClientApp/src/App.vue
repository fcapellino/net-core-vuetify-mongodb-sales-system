<template>
    <v-app>

        <v-navigation-drawer persistent :mini-variant="miniVariant" :clipped="clipped" v-model="drawer" enable-resize-watcher fixed app>
            <!--<v-list>
                <v-list-item value="true" v-for="(item, i) in items" :key="i" :to="item.link">
                    <v-list-item-action>
                        <v-icon v-html="item.icon"></v-icon>
                    </v-list-item-action>
                    <v-list-item-content>
                        <v-list-item-title v-text="item.title"></v-list-item-title>
                    </v-list-item-content>
                </v-list-item>
            </v-list>-->
            <v-list dense>
                <template v-for="item in items">
                    <v-list-group v-if="item.children"
                                  :key="item.text"
                                  v-model="item.model"
                                  :prepend-icon="item.model ? item.icon : item['icon-alt']"
                                  append-icon="">
                        <template v-slot:activator>
                            <v-list-item-content>
                                <v-list-item-title>
                                    {{ item.text }}
                                </v-list-item-title>
                            </v-list-item-content>
                        </template>
                        <v-list-item v-for="(child, i) in item.children"
                                     :key="i"
                                     :to="item.link">
                            <v-list-item-action v-if="child.icon">
                                <v-icon>{{ child.icon }}</v-icon>
                            </v-list-item-action>
                            <v-list-item-content>
                                <v-list-item-title>
                                    {{ child.text }}
                                </v-list-item-title>
                            </v-list-item-content>
                        </v-list-item>
                    </v-list-group>
                    <v-list-item v-else
                                 :key="item.text"
                                 :to="item.link">
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
            </v-list>
        </v-navigation-drawer>

        <v-app-bar app :clipped-left="clipped" color="primary" dark>
            <v-app-bar-nav-icon @click.stop=""></v-app-bar-nav-icon>
            <v-btn class="d-none d-lg-flex" icon @click.stop="miniVariant = !miniVariant">
                <v-icon v-html="miniVariant ? 'chevron_right' : 'chevron_left'"></v-icon>
            </v-btn>
            <!--<v-btn class="d-none d-lg-flex" icon @click.stop="clipped = !clipped">
                <v-icon>web</v-icon>
            </v-btn>-->
            <v-toolbar-title v-text="title"></v-toolbar-title>
            <v-spacer></v-spacer>
        </v-app-bar>

        <v-main>
            <router-view />
        </v-main>

        <v-footer app>
            <span>&nbsp;Software Ateliers&nbsp;&copy;&nbsp;2020</span>
        </v-footer>

    </v-app>
</template>

<script lang="ts">
    import HelloWorld from '@/components/HelloWorld.vue';
    import { Component, Vue } from 'vue-property-decorator';

    @Component({
        components: { HelloWorld },
    })
    export default class App extends Vue {
        private clipped: boolean = true;
        private drawer: boolean = true;
        private miniVariant: boolean = true;
        private right: boolean = true;
        private title: string = 'Basic Sales System';
        //private items = [
        //    { title: 'Home', icon: 'home', link: '/' },
        //    { title: 'Counter', icon: 'touch_app', link: '/counter' },
        //    { title: 'Fetch data', icon: 'get_app', link: '/fetch-data' },
        //];

        private items = [
            { icon: 'home', text: 'Home', link: '/' },
            { icon: 'mdi-contacts', text: 'Contacts', link: '/a' },
            { icon: 'mdi-history', text: 'Frequently contacted', link: '/b' },
            { icon: 'mdi-content-copy', text: 'Duplicates', link: '/c' },
            {
                icon: 'mdi-chevron-up',
                'icon-alt': 'mdi-chevron-down',
                text: 'Labels',
                model: false,
                children: [
                    { icon: 'mdi-plus', text: 'Create label', link: '/d' },
                ],
            },
            {
                icon: 'mdi-chevron-up',
                'icon-alt': 'mdi-chevron-down',
                text: 'More',
                model: false,
                children: [
                    { text: 'Import', link: '/e' },
                    { text: 'Export', link: '/f' },
                    { text: 'Print', link: '/g' },
                    { text: 'Undo changes', link: '/h' },
                    { text: 'Other contacts', link: '/i' },
                ],
            },
            { icon: 'mdi-message', text: 'Send feedback', link: '/k' },
            { icon: 'mdi-help-circle', text: 'Help', link: '/l' },
            { icon: 'mdi-cellphone-link', text: 'App downloads', link: '/m' },
            { icon: 'mdi-keyboard', text: 'Go to the old version', link: '/n' },
        ];
    }
</script>
