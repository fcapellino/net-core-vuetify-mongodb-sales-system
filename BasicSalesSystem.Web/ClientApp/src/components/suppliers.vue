<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="supplierForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm4 class="mr-2">
                            <v-text-field v-model.trim="suppliersTable.filters.searchQuery"
                                          label="Search"
                                          :hint="`${('Please enter your search term')}`"
                                          clearable>
                            </v-text-field>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" v-on:click.stop="showSupplierDialog()">NEW SUPPLIER</v-btn>
                        </v-flex>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="suppliersTable.headers"
                              :items="suppliersTable.itemsList"
                              :options.sync="suppliersTable.options"
                              :server-items-length="suppliersTable.totalItemCount"
                              :loading="suppliersTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showSupplierDialog({supplier: item, readonly: false})">
                                    mdi-pencil
                                </v-icon>
                            </template>
                            <span>Edit</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showSupplierDialog({supplier: item, readonly: true, deletion: true})">
                                    mdi-delete
                                </v-icon>
                            </template>
                            <span>Delete</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="enableOrDisableSupplier({supplier: item})">
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
        <v-dialog v-model="supplierDialog.show" persistent max-width="580px">
            <v-card>
                <v-card-title>
                    <span v-if="!supplierDialog.data.id" class="headline">New supplier</span>
                    <span v-else-if="supplierDialog.data.id && supplierDialog.deletion" class="headline">Do you want to delete this supplier?</span>
                    <span v-else class="headline">Edit supplier</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="supplierDataForm">
                        <v-container grid-list-md>
                            <v-layout wrap>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model="supplierDialog.data.id"
                                                  label="Id"
                                                  dense
                                                  disabled
                                                  style="display:none;">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="supplierDialog.data.fullName"
                                                  label="Name"
                                                  dense
                                                  :disabled="supplierDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="supplierDialog.data.address"
                                                  label="Address"
                                                  dense
                                                  :disabled="supplierDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="6" xs="6">
                                    <v-select v-model.trim="supplierDialog.data.documentType"
                                              :items="documentTypesList"
                                              label="Document type"
                                              dense
                                              :disabled="supplierDialog.readonly"
                                              :rules="[v => v!=null || 'This field is required']">
                                    </v-select>
                                </v-col>
                                <v-col cols="6" xs="6">
                                    <v-text-field v-model.number="supplierDialog.data.documentNumber"
                                                  label="Document number"
                                                  dense
                                                  :disabled="supplierDialog.readonly"
                                                  :rules="[v => (!!v && Number(v)>0 && Number(v)<99999999 && Number.isInteger(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="supplierDialog.data.phoneNumber"
                                                  label="Phone number"
                                                  dense
                                                  :disabled="supplierDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="supplierDialog.data.email"
                                                  label="Email"
                                                  dense
                                                  :disabled="supplierDialog.readonly"
                                                  :rules="[v => (!!v && utils.isValidEmail(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                            </v-layout>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closeSupplierDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="executeSupplierDialogRequest()">ACCEPT</v-btn>
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
    import { SupplierService } from '../services/supplier.service';
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class SuppliersComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private supplierService = new SupplierService();

        private suppliersTable: any = {
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
        private supplierDialog: any = {
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
            await self.getSuppliersList();
        }
        private getSuppliersList = _.debounce(async function () {
            var self = this;
            self.suppliersTable.loading = true;

            const { searchQuery } = self.suppliersTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.suppliersTable.options;

            var parameters = {
                searchQuery: Utils.tryGet(() => searchQuery),
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage)
            };

            var response = await self.supplierService.getSuppliersList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.suppliersTable.totalItemCount = resources.totalItemCount;
                self.suppliersTable.itemsList = resources.itemsList;
            }

            self.suppliersTable.loading = false;
        }, 300)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.suppliersTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.suppliersTable.options.page - 1) * self.suppliersTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }
        private showSupplierDialog(options) {
            var self = this;
            if (options?.supplier) {
                self.supplierDialog.data.id = options.supplier.id;
                self.supplierDialog.data.fullName = options.supplier.fullName;
                self.supplierDialog.data.address = options.supplier.address;
                self.supplierDialog.data.phoneNumber = options.supplier.phoneNumber;
                self.supplierDialog.data.email = options.supplier.email;
                self.supplierDialog.data.documentType = options.supplier.documentType.toUpperCase();
                self.supplierDialog.data.documentNumber = options.supplier.documentNumber;
            }

            self.supplierDialog.deletion = Utils.tryGet(() => options.deletion);
            self.supplierDialog.readonly = Utils.tryGet(() => options.readonly);
            self.supplierDialog.show = true;
        }
        private closeSupplierDialog() {
            var self = this;
            self.supplierDialog.show = null;
            self.supplierDialog.deletion = null;
            self.supplierDialog.readonly = null;
            self.supplierDialog.data = {};

            var form = self.$refs.supplierDataForm as any;
            form.reset();
        }
        private async executeSupplierDialogRequest() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.supplierDataForm as any;
                if (!form.validate()) {
                    return;
                }

                var bodyData = {
                    id: Utils.tryGet(() => self.supplierDialog.data.id),
                    fullName: Utils.tryGet(() => self.supplierDialog.data.fullName),
                    address: Utils.tryGet(() => self.supplierDialog.data.address),
                    phoneNumber: Utils.tryGet(() => self.supplierDialog.data.phoneNumber),
                    email: Utils.tryGet(() => self.supplierDialog.data.email),
                    documentType: Utils.tryGet(() => self.supplierDialog.data.documentType),
                    documentNumber: Utils.tryGet(() => self.supplierDialog.data.documentNumber)
                };

                var response: AxiosResponse;
                if (!self.supplierDialog.data.id) {
                    response = await self.supplierService.createSupplier(bodyData);
                }
                else {
                    if (!self.supplierDialog.deletion) {
                        response = await self.supplierService.updateSupplier(bodyData);
                    }
                    else {
                        var id = self.supplierDialog.data.id;
                        response = await self.supplierService.deleteSupplier(id);
                    }
                }

                var error = response?.data?.error;
                if (error === false) {
                    self.closeSupplierDialog();
                    await self.getSuppliersList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.pendingRequest = false;
            }
        }
        private async enableOrDisableSupplier(options) {
            var self = this;
            try {
                if (!options?.supplier) {
                    return;
                }

                var id = options.supplier.id;
                var response: AxiosResponse;
                response = await self.supplierService.activateOrDeactivateSupplier(id);

                var error = response?.data?.error;
                if (error === false) {
                    await self.getSuppliersList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('suppliersTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.suppliersTable.options.page = 1;
            await self.getSuppliersList();
        }

        @Watch('suppliersTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getSuppliersList();
        }
    }
</script>
