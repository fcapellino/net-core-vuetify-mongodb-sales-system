<template>
    <div id="notFound">
        <v-app id="inspire">
            <v-container class="fill-height" fluid>
                <v-row align="center"
                       justify="center">
                    <v-col cols="12" sm="8" md="4">
                        <v-card class="elevation-12">
                            <v-alert text
                                     outlined
                                     border="top"
                                     color="deep-orange"
                                     icon="mdi-alert-box">
                                <router-link to="/">Page not found. Take me home.</router-link>
                            </v-alert>
                        </v-card>
                    </v-col>
                </v-row>
            </v-container>
        </v-app>
    </div>
</template>

<script lang="ts">
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { Utils } from '../common/utils';

    @Component({})
    export default class NotFoundComponent extends Vue {
        private utils: any = Utils;
    }
</script>
