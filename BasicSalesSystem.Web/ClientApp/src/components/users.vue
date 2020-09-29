<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="userForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm4 class="mr-2">
                            <v-select v-model="usersTable.filters.roleId"
                                      :items="rolesList"
                                      item-value="id"
                                      item-text="name"
                                      label="Role"
                                      clearable>
                            </v-select>
                        </v-flex>
                        <v-flex flex sm4 class="mr-2">
                            <v-text-field v-model.trim="usersTable.filters.searchQuery"
                                          label="Search"
                                          :hint="`${('Please enter your search term')}`"
                                          clearable>
                            </v-text-field>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" v-on:click.stop="showUserDialog()">NEW USER</v-btn>
                        </v-flex>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="usersTable.headers"
                              :items="usersTable.itemsList"
                              :options.sync="usersTable.options"
                              :server-items-length="usersTable.totalItemCount"
                              :loading="usersTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showUserDialog({user: item, readonly: true, deletion: true})">
                                    mdi-delete
                                </v-icon>
                            </template>
                            <span>Delete</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="enableOrDisableUser({user: item})">
                                    {{ item.enabled? 'mdi-minus-circle' : 'mdi-checkbox-marked-circle' }}
                                </v-icon>
                            </template>
                            <span>{{ item.enabled? 'Disable' : 'Enable' }}</span>
                        </v-tooltip>
                    </template>
                    <template v-slot:item.[id]="{ item }">
                        {{ getItemIndex(item) }}
                    </template>
                    <template v-slot:item.[fullname]="{ item }">
                        {{ item.fullName }}
                    </template>
                    <template v-slot:item.[address]="{ item }">
                        {{ item.address }}
                    </template>
                    <template v-slot:item.[phonenumber]="{ item }">
                        {{ item.phoneNumber }}
                    </template>
                    <template v-slot:item.[email]="{ item }">
                        {{ item.email }}
                    </template>
                    <template v-slot:item.[role]="{ item }">
                        {{ item.role.name }}
                    </template>
                    <template v-slot:item.[enabled]="{ item }">
                        <p style="margin-bottom:0px;font-weight:bold;" v-bind:class="[item.enabled? 'body-2 p-green' : 'body-2 p-red']">
                            {{ item.enabled? 'Enabled':'Disabled' }}
                        </p>
                    </template>
                </v-data-table>
            </v-card>
        </v-col>
        <v-dialog v-model="userDialog.show" persistent max-width="580px">
            <v-card>
                <v-card-title>
                    <span v-if="!userDialog.data.id" class="headline">New user</span>
                    <span v-else-if="userDialog.data.id && userDialog.deletion" class="headline">Do you want to delete this user?</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="userDataForm">
                        <v-container grid-list-md>
                            <v-layout wrap>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model="userDialog.data.id"
                                                  label="Id"
                                                  dense
                                                  disabled
                                                  style="display:none;">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="userDialog.data.fullName"
                                                  label="Name"
                                                  dense
                                                  :disabled="userDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="userDialog.data.address"
                                                  label="Address"
                                                  dense
                                                  :disabled="userDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-select v-model="userDialog.data.role"
                                              :items="rolesList"
                                              item-value="id"
                                              item-text="name"
                                              label="Role"
                                              dense
                                              :disabled="userDialog.readonly"
                                              :rules="[v => v!=null || 'This field is required']"
                                              return-object>
                                    </v-select>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="userDialog.data.phoneNumber"
                                                  label="Phone number"
                                                  dense
                                                  :disabled="userDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="userDialog.data.email"
                                                  label="Email"
                                                  dense
                                                  :disabled="userDialog.readonly"
                                                  :rules="[v => (!!v && utils.isValidEmail(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                            </v-layout>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closeUserDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="executeUserDialogRequest()">ACCEPT</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-container>
</template>

<script lang="ts">
    import * as _ from 'lodash';
    import { AxiosResponse } from 'axios';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { Notify } from '../common/notify';
    import { UserService } from '../services/user.service';
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class UsersComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private userService = new UserService();

        private usersTable: any = {
            filters: {
                searchQuery: null,
                roleId: null
            },
            loading: false,
            options: {
                sortBy: ['fullName'],
                sortDesc: [false],
                page: 1,
                itemsPerPage: 5
            },
            totalItemCount: 0,
            itemsList: [],
            headers: [
                { text: 'Actions', value: '[actions]', width: '110px', sortable: false },
                { text: 'Id', value: '[id]', width: '70px', sortable: false },
                { text: 'Name', value: '[fullname]', width: '15%', sortable: true },
                { text: 'Address', value: '[address]', sortable: true },
                { text: 'Phone', value: '[phonenumber]', width: '15%', sortable: true },
                { text: 'Email', value: '[email]', sortable: true },
                { text: 'Role', value: '[role]', sortable: false },
                { text: 'State', value: '[enabled]', width: '80px', sortable: true }
            ]
        };
        private userDialog: any = {
            show: null,
            readonly: null,
            deletion: null,
            data: {
                id: null,
                fullName: null,
                address: null,
                phoneNumber: null,
                email: null,
                role: null
            }
        };
        private rolesList: Array<any> = [];
        private documentTypesList: Array<any> = [
            'DNI',
            'PASSPORT'
        ];

        private async mounted() {
            var self = this;
            await self.getUsersRoleList();
            await self.getUsersList();
        }
        private getUsersRoleList = _.debounce(async function () {
            var self = this;
            var response = await self.userService.getUsersRoleList();
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.rolesList = resources.itemsList;
            }
        }, 300)
        private getUsersList = _.debounce(async function () {
            var self = this;
            self.usersTable.loading = true;

            const { searchQuery, roleId } = self.usersTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.usersTable.options;

            var parameters = {
                searchQuery: Utils.tryGet(() => searchQuery),
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage),
                roleId: Utils.tryGet(() => roleId)
            };

            var response = await self.userService.getUsersList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.usersTable.totalItemCount = resources.totalItemCount;
                self.usersTable.itemsList = resources.itemsList;
            }

            self.usersTable.loading = false;
        }, 300)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.usersTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.usersTable.options.page - 1) * self.usersTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }
        private showUserDialog(options) {
            var self = this;
            if (options?.user) {
                self.userDialog.data.id = options.user.id;
                self.userDialog.data.fullName = options.user.fullName;
                self.userDialog.data.address = options.user.address;
                self.userDialog.data.phoneNumber = options.user.phoneNumber;
                self.userDialog.data.email = options.user.email;
                self.userDialog.data.role = self.rolesList.find(x => x.id === options.user.role.id);
            }

            self.userDialog.deletion = Utils.tryGet(() => options.deletion);
            self.userDialog.readonly = Utils.tryGet(() => options.readonly);
            self.userDialog.show = true;
        }
        private closeUserDialog() {
            var self = this;
            self.userDialog.show = null;
            self.userDialog.deletion = null;
            self.userDialog.readonly = null;
            self.userDialog.data = {};

            var form = self.$refs.userDataForm as any;
            form.reset();
        }
        private async executeUserDialogRequest() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.userDataForm as any;
                if (!form.validate()) {
                    return;
                }

                var bodyData = {
                    id: Utils.tryGet(() => self.userDialog.data.id),
                    fullName: Utils.tryGet(() => self.userDialog.data.fullName),
                    address: Utils.tryGet(() => self.userDialog.data.address),
                    phoneNumber: Utils.tryGet(() => self.userDialog.data.phoneNumber),
                    email: Utils.tryGet(() => self.userDialog.data.email),
                    role: Utils.tryGet(() => self.userDialog.data.role.name)
                };

                var response: AxiosResponse;
                if (!self.userDialog.data.id) {
                    response = await self.userService.createUser(bodyData);
                }
                else {
                    var id = self.userDialog.data.id;
                    response = await self.userService.deleteUser(id);
                }

                var error = response?.data?.error;
                if (error === false) {
                    self.closeUserDialog();
                    await self.getUsersList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.pendingRequest = false;
            }
        }
        private async enableOrDisableUser(options) {
            var self = this;
            try {
                if (!options?.user) {
                    return;
                }

                var id = options.user.id;
                var response: AxiosResponse;
                response = await self.userService.enableOrDisableUser(id);

                var error = response?.data?.error;
                if (error === false) {
                    await self.getUsersList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('usersTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.usersTable.options.page = 1;
            await self.getUsersList();
        }

        @Watch('usersTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getUsersList();
        }
    }
</script>
