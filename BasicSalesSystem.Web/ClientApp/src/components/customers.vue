<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="customerForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm4 class="mr-2">
                            <v-text-field v-model.trim="customersTable.filters.searchQuery"
                                          label="Search"
                                          :hint="`${('Please enter your search term')}`"
                                          clearable>
                            </v-text-field>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" v-on:click.stop="showCustomerDialog()">NEW CUSTOMER</v-btn>
                        </v-flex>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="customersTable.headers"
                              :items="customersTable.itemsList"
                              :options.sync="customersTable.options"
                              :server-items-length="customersTable.totalItemCount"
                              :loading="customersTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showCustomerDialog({customer: item, readonly: false})">
                                    mdi-pencil
                                </v-icon>
                            </template>
                            <span>Edit</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showCustomerDialog({customer: item, readonly: true, deletion: true})">
                                    mdi-delete
                                </v-icon>
                            </template>
                            <span>Delete</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="enableOrDisableCustomer({customer: item})">
                                    {{ item.active? 'mdi-minus-circle' : 'mdi-checkbox-marked-circle' }}
                                </v-icon>
                            </template>
                            <span>{{ item.active? 'Active' : 'Inactive' }}</span>
                        </v-tooltip>
                    </template>
                    <template v-slot:item.[id]="{ item }">
                        {{ getItemIndex(item) }}
                    </template>
                    <template v-slot:item.[fullname]="{ item }">
                        {{ item.fullName }}
                    </template>
                    <template v-slot:item.[documenttype]="{ item }">
                        <p style="margin:0px;">{{ item.documentType.toUpperCase() }} [{{ item.documentNumber }}]</p>
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
                    <template v-slot:item.[active]="{ item }">
                        <p style="margin-bottom:0px;font-weight:bold;" v-bind:class="[item.active? 'body-2 p-green' : 'body-2 p-red']">
                            {{ item.active? 'Active':'Inactive' }}
                        </p>
                    </template>
                </v-data-table>
            </v-card>
        </v-col>
        <v-dialog v-model="customerDialog.show" persistent max-width="580px">
            <v-card>
                <v-card-title>
                    <span v-if="!customerDialog.data.id" class="headline">New customer</span>
                    <span v-else-if="customerDialog.data.id && customerDialog.deletion" class="headline">Do you want to delete this customer?</span>
                    <span v-else class="headline">Edit customer</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="customerDataForm">
                        <v-container grid-list-md>
                            <v-layout wrap>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model="customerDialog.data.id"
                                                  label="Id"
                                                  dense
                                                  disabled
                                                  style="display:none;">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="customerDialog.data.fullName"
                                                  label="Name"
                                                  dense
                                                  :disabled="customerDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="customerDialog.data.address"
                                                  label="Address"
                                                  dense
                                                  :disabled="customerDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="6" xs="6">
                                    <v-select v-model.trim="customerDialog.data.documentType"
                                              :items="documentTypesList"
                                              label="Document type"
                                              dense
                                              :disabled="customerDialog.readonly"
                                              :rules="[v => v!=null || 'This field is required']">
                                    </v-select>
                                </v-col>
                                <v-col cols="6" xs="6">
                                    <v-text-field v-model.number="customerDialog.data.documentNumber"
                                                  label="Document number"
                                                  dense
                                                  :disabled="customerDialog.readonly"
                                                  :rules="[v => (!!v && Number(v)>0 && Number(v)<99999999 && Number.isInteger(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="customerDialog.data.phoneNumber"
                                                  label="Phone number"
                                                  dense
                                                  :disabled="customerDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="customerDialog.data.email"
                                                  label="Email"
                                                  dense
                                                  :disabled="customerDialog.readonly"
                                                  :rules="[v => (!!v && utils.isValidEmail(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                            </v-layout>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closeCustomerDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="executeCustomerDialogRequest()">ACCEPT</v-btn>
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
    import { CustomerService } from '../services/customer.service';
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class CustomersComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private customerService = new CustomerService();

        private customersTable: any = {
            filters: {
                searchQuery: null
            },
            loading: false,
            options: {
                sortBy: ['fullname'],
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
                { text: 'Document', value: '[documenttype]', sortable: true },
                { text: 'Address', value: '[address]', sortable: true },
                { text: 'Phone', value: '[phonenumber]', width: '15%', sortable: true },
                { text: 'Email', value: '[email]', sortable: true },
                { text: 'State', value: '[active]', width: '80px', sortable: true }
            ]
        };
        private customerDialog: any = {
            show: null,
            readonly: null,
            deletion: null,
            data: {
                id: null,
                fullName: null,
                address: null,
                phoneNumber: null,
                email: null,
                documentType: null,
                documentNumber: null
            }
        };
        private documentTypesList: Array<any> = [
            'DNI',
            'PASSPORT'
        ];

        private async mounted() {
            var self = this;
            await self.getCustomersList();
        }
        private getCustomersList = _.debounce(async function () {
            var self = this;
            self.customersTable.loading = true;

            const { searchQuery } = self.customersTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.customersTable.options;

            var parameters = {
                searchQuery: Utils.tryGet(() => searchQuery),
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage)
            };

            var response = await self.customerService.getCustomersList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.customersTable.totalItemCount = resources.totalItemCount;
                self.customersTable.itemsList = resources.itemsList;
            }

            self.customersTable.loading = false;
        }, 300)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.customersTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.customersTable.options.page - 1) * self.customersTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }
        private showCustomerDialog(options) {
            var self = this;
            if (options?.customer) {
                self.customerDialog.data.id = options.customer.id;
                self.customerDialog.data.fullName = options.customer.fullName;
                self.customerDialog.data.address = options.customer.address;
                self.customerDialog.data.phoneNumber = options.customer.phoneNumber;
                self.customerDialog.data.email = options.customer.email;
                self.customerDialog.data.documentType = options.customer.documentType.toUpperCase();
                self.customerDialog.data.documentNumber = options.customer.documentNumber;
            }

            self.customerDialog.deletion = Utils.tryGet(() => options.deletion);
            self.customerDialog.readonly = Utils.tryGet(() => options.readonly);
            self.customerDialog.show = true;
        }
        private closeCustomerDialog() {
            var self = this;
            self.customerDialog.show = null;
            self.customerDialog.deletion = null;
            self.customerDialog.readonly = null;
            self.customerDialog.data = {};

            var form = self.$refs.customerDataForm as any;
            form.reset();
        }
        private async executeCustomerDialogRequest() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.customerDataForm as any;
                if (!form.validate()) {
                    return;
                }

                var bodyData = {
                    id: Utils.tryGet(() => self.customerDialog.data.id),
                    fullName: Utils.tryGet(() => self.customerDialog.data.fullName),
                    address: Utils.tryGet(() => self.customerDialog.data.address),
                    phoneNumber: Utils.tryGet(() => self.customerDialog.data.phoneNumber),
                    email: Utils.tryGet(() => self.customerDialog.data.email),
                    documentType: Utils.tryGet(() => self.customerDialog.data.documentType),
                    documentNumber: Utils.tryGet(() => self.customerDialog.data.documentNumber)
                };

                var response: AxiosResponse;
                if (!self.customerDialog.data.id) {
                    response = await self.customerService.createCustomer(bodyData);
                }
                else {
                    if (!self.customerDialog.deletion) {
                        response = await self.customerService.updateCustomer(bodyData);
                    }
                    else {
                        var id = self.customerDialog.data.id;
                        response = await self.customerService.deleteCustomer(id);
                    }
                }

                var error = response?.data?.error;
                if (error === false) {
                    self.closeCustomerDialog();
                    await self.getCustomersList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.pendingRequest = false;
            }
        }
        private async enableOrDisableCustomer(options) {
            var self = this;
            try {
                if (!options?.customer) {
                    return;
                }

                var id = options.customer.id;
                var response: AxiosResponse;
                response = await self.customerService.activateOrDeactivateCustomer(id);

                var error = response?.data?.error;
                if (error === false) {
                    await self.getCustomersList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('customersTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.customersTable.options.page = 1;
            await self.getCustomersList();
        }

        @Watch('customersTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getCustomersList();
        }
    }
</script>
