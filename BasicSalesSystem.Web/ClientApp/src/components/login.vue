<template>
    <div id="login">
        <v-app id="inspire">
            <v-container class="fill-height" fluid>
                <v-row align="center"
                       justify="center">
                    <v-col cols="12"
                           sm="8"
                           md="4">
                        <v-card class="elevation-12">
                            <v-toolbar color="primary"
                                       dark
                                       flat>
                                <v-toolbar-title>Basic Sales System</v-toolbar-title>
                                <v-spacer></v-spacer>
                            </v-toolbar>
                            <v-card-text>
                                <v-form v-on:submit="onSubmit" ref="loginForm" id="loginForm">
                                    <v-text-field v-model.trim="credentials.email"
                                                  label="email"
                                                  name="email"
                                                  prepend-icon="mdi-account"
                                                  type="text"
                                                  :rules="[v => (!!v && utils.isValidEmail(v)) || 'This field is required']">
                                    </v-text-field>
                                    <v-text-field v-model.trim="credentials.password"
                                                  label="password"
                                                  name="password"
                                                  prepend-icon="mdi-lock"
                                                  type="password"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-form>
                            </v-card-text>
                            <v-progress-linear v-if="pendingRequest"
                                               :indeterminate="true"
                                               style="height:5px;margin:0px">
                            </v-progress-linear>
                            <v-divider></v-divider>
                            <v-card-actions>
                                <v-spacer></v-spacer>
                                <v-btn small color="primary" v-bind:disabled="pendingRequest" type="submit" form="loginForm">LOGIN</v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-col>
                </v-row>
            </v-container>
        </v-app>
    </div>
</template>

<script lang="ts">
    import { AuthManager } from '../auth/app.auth.manager';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { UserService } from '../services/user.service';
    import { Utils } from '../common/utils';

    @Component({ methods: {} })
    export default class LoginComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private userService = new UserService();
        private authManager = new AuthManager(new UserService());

        private credentials: any = {
            email: null,
            password: null
        };
        private async onSubmit(event) {
            var self = this;
            try {
                self.pendingRequest = true;
                event.preventDefault();

                var form = self.$refs.loginForm as any;
                if (!form.validate()) {
                    throw new Error();
                }

                var credentials = {
                    email: Utils.tryGet(() => self.credentials.email),
                    password: Utils.tryGet(() => self.credentials.password)
                };

                await self.authManager.login(credentials);
                if (self.authManager.authenticated() === false) {
                    throw new Error();
                }

                self.$router.push({ name: 'home' });
                return;
            }
            catch (error) {
                self.pendingRequest = false;
            }
        }
    }
</script>
