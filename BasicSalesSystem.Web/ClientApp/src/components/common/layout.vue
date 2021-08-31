<template>
    <div id="layout">
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
                                <v-tooltip right v-for="(child, i) in item.children" :key="i" v-if="isUserAuthorized(child.link)">
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
                    <v-tooltip v-else-if="isUserAuthorized(item.link)" right :key="item.text">
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
            <v-app-bar-nav-icon v-on:click.stop=""></v-app-bar-nav-icon>
            <v-btn class="d-none d-lg-flex" icon v-on:click.stop="miniVariant = !miniVariant">
                <v-icon v-html="miniVariant ? 'chevron_right' : 'chevron_left'"></v-icon>
            </v-btn>
            <v-toolbar-title v-text="title"></v-toolbar-title>
            <v-spacer></v-spacer>
            <v-toolbar-title class="caption" style="text-align:end;">
                <div v-if="userClaims">
                    <div>{{ userClaims.fullname.toUpperCase() }}</div>
                    <div><small>{{ userClaims.role.toUpperCase() }}</small></div>
                </div>
            </v-toolbar-title>
            <v-menu bottom left>
                <template v-slot:activator="{ on }">
                    <v-btn dark
                           icon
                           v-on="on">
                        <v-icon>mdi-account-box</v-icon>
                    </v-btn>
                </template>
                <v-list>
                    <v-list-item link v-on:click.stop="changePasswordDialog.display=true">
                        <v-list-item-title>Change password</v-list-item-title>
                    </v-list-item>
                    <v-list-item link v-on:click.stop="logout()">
                        <v-list-item-title>Logout</v-list-item-title>
                    </v-list-item>
                </v-list>
            </v-menu>
        </v-app-bar>

        <router-view></router-view>

        <v-dialog v-model="changePasswordDialog.display" fullscreen hide-overlay transition="dialog-bottom-transition">
            <v-card>
                <v-toolbar dark color="primary">
                    <v-toolbar-title>Change password</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-toolbar-items>
                        <v-btn icon dark v-on:click.stop="closeChangePasswordDialog()">
                            <v-icon>mdi-close</v-icon>
                        </v-btn>
                    </v-toolbar-items>
                </v-toolbar>
                <v-card-text>
                    <v-form ref="changePasswordForm">
                        <v-container>
                            <v-layout row justify-center>
                                <v-flex xs3 style="margin:10px;">
                                    <v-text-field v-model.trim="changePasswordDialog.oldPassword"
                                                  label="current password"
                                                  prepend-inner-icon="mdi-lock"
                                                  filled
                                                  type="password"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-flex>
                                <v-flex xs3 style="margin:10px;">
                                    <v-text-field v-model.trim="changePasswordDialog.newPassword"
                                                  label="new password"
                                                  prepend-inner-icon="mdi-lock"
                                                  filled
                                                  type="password"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required', v => (v && v.length >= 8) || 'The password must have at least eight characters']">
                                    </v-text-field>
                                </v-flex>
                                <v-flex xs3 style="margin:10px;">
                                    <v-text-field v-model.trim="changePasswordDialog.confirmedPassword"
                                                  label="confirm new password"
                                                  prepend-inner-icon="mdi-lock"
                                                  filled
                                                  type="password"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required', changePasswordDialog.newPassword===changePasswordDialog.confirmedPassword || 'Passwords do not match']"></v-text-field>
                                </v-flex>
                            </v-layout>
                            <v-layout row justify-center>
                                <v-flex xs9 style="text-align:right;">
                                    <v-btn small class="mx-1" color="primary" v-bind:disabled="pendingRequest" v-on:click.stop="changePassword()">ACCEPT</v-btn>
                                    <v-btn small class="mx-1" color="primary" v-bind:disabled="pendingRequest" v-on:click.stop="closeChangePasswordDialog()">CANCEL</v-btn>
                                </v-flex>
                            </v-layout>
                        </v-container>
                        <v-divider></v-divider>
                    </v-form>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-footer app>
            <span>&nbsp;Software &nbsp;&copy;&nbsp;2020</span>
        </v-footer>
    </div>
</template>

<script lang="ts">
    import { AuthManager } from '../../auth/app.auth.manager';
    import { AxiosResponse } from 'axios';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { Notify } from '../../common/notify';
    import { UserService } from '../../services/user.service';
    import { Utils } from '../../common/utils';

    @Component({ methods: {} })
    export default class LayoutComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private userService = new UserService();
        private authManager = new AuthManager(new UserService());

        private clipped: boolean = true;
        private drawer: boolean = true;
        private miniVariant: boolean = true;
        private right: boolean = true;
        private title: string = 'Basic Sales System';

        private items = [
            { icon: 'home', text: 'Home', link: '/home' },
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
        private changePasswordDialog: any = {
            display: null,
            oldPassword: null,
            newPassword: null,
            confirmedPassword: null
        };

        private get userClaims() {
            var self = this;
            return self.authManager.readClaimsFromLocalStorage();
        }
        private isUserAuthorized(link) {
            var self = this;
            var router = self.$router as any;
            var root = router.options.routes.find(r => r.path === '/');
            var route = root.children.find(r => r.path === link);

            var roles = route?.meta.roles as Array<string>;
            var authorized = roles?.some(r => self.authManager.isInRole(r));
            return authorized;
        }
        private closeChangePasswordDialog() {
            var self = this;
            self.changePasswordDialog.display = false;
            var form = self.$refs.changePasswordForm as any;
            form.reset();
        }
        private async changePassword() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.changePasswordForm as any;
                if (!form.validate()) {
                    return;
                }

                var bodyData = {
                    oldPassword: Utils.tryGet(() => self.changePasswordDialog.oldPassword),
                    newPassword: Utils.tryGet(() => self.changePasswordDialog.newPassword)
                };

                var response: AxiosResponse;
                response = await self.userService.changePassword(bodyData);

                var error = response?.data?.error;
                if (error === false) {
                    self.closeChangePasswordDialog();
                }
            }
            catch (error) {
                self.closeChangePasswordDialog();
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.pendingRequest = false;
            }
        }
        private logout() {
            var self = this;
            self.authManager.logout();
            self.$router.push({ name: 'login' });
        }
    }
</script>

<style>
    [class$="--disabled"],
    [class*="--disabled "] * {
        color: #616161 !important
    }

    .v-snack__wrapper {
        max-width: 350px;
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
