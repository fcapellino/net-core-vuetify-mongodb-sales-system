<template>
    <v-container fluid>
        <v-layout column align-center class="mt-3">
            <img src="@/assets/logo.png" alt="Vuetify.js" class="mb-5">
            <h3>Basic Sales and Inventory Management System built with ASP.NET Core, MongoDB and Vuetify.</h3>
        </v-layout>
        <v-slide-y-transition mode="out-in">
            <v-row justify="center">
                <v-col cols="8">
                    <v-row class="fill-height mt-3" align="center" justify="center">
                        <template v-for="(item, index) in menuItems">
                            <router-link :to="item.path" v-slot="{ href, route, navigate }" :key="index">
                                <v-hover v-slot:default="{ hover }" :href="href">
                                    <v-card v-on:click="navigate"
                                            v-bind:class="{ 'on-hover': hover }"
                                            class="ma-3 pa-5"
                                            :elevation="hover ? 10 : 3">
                                        {{ item.name.toUpperCase() }}
                                    </v-card>
                                </v-hover>
                            </router-link>
                        </template>
                    </v-row>
                </v-col>
            </v-row>
        </v-slide-y-transition>
    </v-container>
</template>

<script lang="ts">
    import { AuthManager } from '../auth/app.auth.manager';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { UserService } from '../services/user.service';
    import { Utils } from '../common/utils';

    @Component({
        components: {},
    })
    export default class HomeComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private authManager = new AuthManager(new UserService());
        private get menuItems() {
            var self = this;
            var router = self.$router as any;
            var root = router.options.routes.find(r => r.path === '/');
            var routes = root.children;

            var authorizedRoutes = routes
                .filter(r => {

                    var roles = r?.meta.roles as Array<string>;
                    return r.name !== 'home' && roles?.some(r => self.authManager.isInRole(r));
                })
                .sort(function (a, b) {
                    return a.name.localeCompare(b.name);
                });

            return authorizedRoutes;
        }
    }
</script>

<style scoped>
    .v-card {
        transition: opacity .4s ease-in-out;
        text-align: center;
    }

        .v-card:not(.on-hover) {
            opacity: 0.6;
        }

        .v-card.on-hover {
            cursor: pointer;
        }
</style>
