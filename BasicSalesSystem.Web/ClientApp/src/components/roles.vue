<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="categoryForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm4>
                            <v-text-field v-model.trim="rolesTable.filters.searchQuery"
                                          label="Search"
                                          :hint="`${('Please enter your search term')}`"
                                          clearable>
                            </v-text-field>
                        </v-flex>
                        <v-spacer></v-spacer>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="rolesTable.headers"
                              :items="rolesTable.pagedAndFilteredList"
                              :options.sync="rolesTable.options"
                              :server-items-length="rolesTable.totalItemCount"
                              :loading="rolesTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[id]="{ item }">
                        {{ getItemIndex(item) }}
                    </template>
                    <template v-slot:item.[name]="{ item }">
                        {{ item.name }}
                    </template>
                    <template v-slot:item.[description]="{ item }">
                        {{ item.description }}
                    </template>
                    <template v-slot:item.[active]="{ item }">
                        <p style="margin-bottom:0px;font-weight:bold;" v-bind:class="[item.active? 'body-2 p-green' : 'body-2 p-red']">
                            {{ item.active? 'Active':'Inactive' }}
                        </p>
                    </template>
                </v-data-table>
            </v-card>
        </v-col>
    </v-container>
</template>

<script lang="ts">
    import * as _ from 'lodash';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { UserService } from '../services/user.service';
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class RolesComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private userService = new UserService();
        private rolesTable: any = {
            filters: {
                searchQuery: null
            },
            loading: false,
            options: {
                sortBy: ['name'],
                sortDesc: [false],
                page: 1,
                itemsPerPage: 5
            },
            totalItemCount: 0,
            itemsList: [],
            pagedAndFilteredList: [],
            headers: [
                { text: 'Id', value: '[id]', width: '70px', sortable: false },
                { text: 'Name', value: '[name]', width: '15%', sortable: true },
                { text: 'Description', value: '[description]', sortable: true },
                { text: 'State', value: '[active]', width: '80px', sortable: true }
            ]
        };

        private async mounted() {
            var self = this;
            await self.getRolesList();
        }
        private getRolesList = async function () {
            var self = this;
            self.rolesTable.loading = true;

            var response = await self.userService.getUsersRoleList();
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.rolesTable.totalItemCount = resources.itemsList.length;
                self.rolesTable.itemsList = resources.itemsList;
            }

            self.getPagedAndFilteredRolesList();
            self.rolesTable.loading = false;
        }
        private getPagedAndFilteredRolesList = _.debounce(function () {
            var self = this;
            var itemsList = self.rolesTable.itemsList;

            var { searchQuery } = self.rolesTable.filters;
            var { sortBy, sortDesc, page, itemsPerPage } = self.rolesTable.options;
            sortBy = Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null);
            sortDesc = Utils.tryGet(() => [...sortDesc].shift());

            if (!Utils.isNullOrEmpty(searchQuery)) {
                itemsList = itemsList.filter(function (item) {
                    var text = JSON.stringify(item).toLowerCase();
                    return text.includes(searchQuery.toLowerCase());
                });
            }

            if (!Utils.isNullOrEmpty(sortBy)) {
                itemsList = _.orderBy(itemsList, [item => item[sortBy].toString().toLowerCase()], [sortDesc ? 'desc' : 'asc']);
            }

            var totalItemCount = itemsList.length;
            itemsList = _.take(_.drop(itemsList, (page - 1) * itemsPerPage), itemsPerPage);

            self.rolesTable.totalItemCount = totalItemCount;
            self.rolesTable.pagedAndFilteredList = itemsList;

        }, 300)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.rolesTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.rolesTable.options.page - 1) * self.rolesTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }

        @Watch('rolesTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.rolesTable.options.page = 1;
            await self.getPagedAndFilteredRolesList();
        }

        @Watch('rolesTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getPagedAndFilteredRolesList();
        }
    }
</script>
