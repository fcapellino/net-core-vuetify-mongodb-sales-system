<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="productForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm2 class="mr-2">
                            <v-menu ref="startDateMenu"
                                    v-model="showStartDateMenu"
                                    :close-on-content-click="false"
                                    :nudge-right="40"
                                    transition="scale-transition"
                                    offset-y
                                    max-width="290px"
                                    min-width="290px">
                                <template v-slot:activator="{ on }">
                                    <v-text-field v-model="formattedStartDate"
                                                  label="Start date"
                                                  clearable
                                                  prepend-inner-icon="event"
                                                  readonly
                                                  v-on="on">
                                    </v-text-field>
                                </template>
                                <v-date-picker v-model="salesTable.filters.startDate" :show-current="false" no-title v-on:input="showStartDateMenu=false"></v-date-picker>
                            </v-menu>
                        </v-flex>
                        <v-flex flex sm2 class="mr-2">
                            <v-menu ref="endDateMenu"
                                    v-model="showEndDateMenu"
                                    :close-on-content-click="false"
                                    :nudge-right="40"
                                    transition="scale-transition"
                                    offset-y
                                    max-width="290px"
                                    min-width="290px">
                                <template v-slot:activator="{ on }">
                                    <v-text-field v-model="formattedEndDate"
                                                  label="End date"
                                                  clearable
                                                  prepend-inner-icon="event"
                                                  readonly
                                                  v-on="on">
                                    </v-text-field>
                                </template>
                                <v-date-picker v-model="salesTable.filters.endDate" :show-current="false" no-title v-on:input="showEndDateMenu=false"></v-date-picker>
                            </v-menu>
                        </v-flex>
                        <v-flex flex sm4 class="mr-2">
                            <v-autocomplete v-model="salesTable.filters.customer"
                                            v-on:keyup="getCustomersList()"
                                            :items="customersDropDown.searchResults"
                                            :search-input.sync.trim="customersDropDown.searchQuery"
                                            persistent-hint
                                            item-value="id"
                                            item-text="fullName"
                                            label="Customer"
                                            return-object
                                            clearable>
                            </v-autocomplete>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" class="mr-2" v-on:click.stop="showSaleDialog()">NEW SALE</v-btn>
                            <v-btn small color="primary" v-bind:loading="exportingXLSX" v-bind:disabled="exportingXLSX" v-on:click.stop="exportToXLSX()">
                                EXPORT
                            </v-btn>
                        </v-flex>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="salesTable.headers"
                              :items="salesTable.itemsList"
                              :options.sync="salesTable.options"
                              :server-items-length="salesTable.totalItemCount"
                              :loading="salesTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showSaleDialog({sale: item, readonly: true})">
                                    mdi-library-books
                                </v-icon>
                            </template>
                            <span>Details</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="printSelectedSale({sale: item})">
                                    mdi-printer
                                </v-icon>
                            </template>
                            <span>Print</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showSaleDialog({sale: item, readonly: true, cancellation: true})"
                                        v-bind:disabled="!item.approved">
                                    mdi-close-circle
                                </v-icon>
                            </template>
                            <span>Cancel</span>
                        </v-tooltip>
                    </template>
                    <template v-slot:item.[id]="{ item }">
                        {{ getItemIndex(item) }}
                    </template>
                    <template v-slot:item.[sale.receipttype]="{ item }">
                        {{ item.receiptType.toUpperCase() }}
                    </template>
                    <template v-slot:item.[sale.date]="{ item }">
                        {{ utils.formatDate(item.date) }}
                    </template>
                    <template v-slot:item.[customer]="{ item }">
                        {{ item.customer.fullName }}
                    </template>
                    <template v-slot:item.[sale.tax]="{ item }">
                        {{ item.tax+'%' }}
                    </template>
                    <template v-slot:item.[sale.total]="{ item }">
                        {{ '$'+item.total.toFixed(2).toString() }}
                    </template>
                    <template v-slot:item.[sale.approved]="{ item }">
                        <p style="margin-bottom:0px;font-weight:bold;" v-bind:class="[item.approved? 'body-2 p-green' : 'body-2 p-red']">
                            {{ item.approved? 'Approved':'Cancelled' }}
                        </p>
                    </template>
                </v-data-table>
            </v-card>
        </v-col>
        <v-dialog v-model="saleDialog.show" persistent max-width="1000px">
            <v-card>
                <v-card-title>
                    <span v-if="!saleDialog.data.id" class="headline">New sale</span>
                    <span v-else-if="saleDialog.data.id && saleDialog.cancellation" class="headline">Do you want to cancel this sale?</span>
                    <span v-else class="headline">Details</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="saleDataForm">
                        <v-container grid-list-md>
                            <v-row>
                                <v-layout wrap>
                                    <v-col cols="2" xs="2">
                                        <v-select v-model.trim="saleDialog.data.receiptType"
                                                  :items="saleDialog.receiptTypesList"
                                                  label="Receipt type"
                                                  dense
                                                  :disabled="saleDialog.readonly"
                                                  :rules="[v => v!=null || 'This field is required']">
                                        </v-select>
                                    </v-col>
                                    <v-col cols="5" xs="5">
                                        <v-autocomplete v-model="saleDialog.data.customer"
                                                        v-on:keyup="getCustomersList()"
                                                        :items="saleDialog.customersDropDown.searchResults"
                                                        :search-input.sync.trim="saleDialog.customersDropDown.searchQuery"
                                                        item-value="id"
                                                        item-text="fullName"
                                                        return-object
                                                        label="Customer"
                                                        dense
                                                        :disabled="saleDialog.readonly"
                                                        :rules="[v => v!=null || 'This field is required']">
                                        </v-autocomplete>
                                    </v-col>
                                    <v-col cols="2" xs="2">
                                        <v-text-field v-model.trim="saleDialog.data.formattedDate"
                                                      label="Date"
                                                      dense
                                                      :disabled="saleDialog.readonly"
                                                      readonly>
                                        </v-text-field>
                                    </v-col>
                                    <v-col cols="1" xs="1">
                                        <v-text-field v-model.number="saleDialog.data.tax"
                                                      type="number"
                                                      min="0"
                                                      step="1"
                                                      suffix="%"
                                                      label="Tax"
                                                      dense
                                                      :disabled="saleDialog.readonly"
                                                      :rules="[v => (Number(v)>=0 && Number.isInteger(v)) || 'Invalid']">
                                        </v-text-field>
                                    </v-col>
                                    <v-col cols="2" xs="2">
                                        <v-text-field v-bind:value="(() => { return utils.tryGetNumber(()=> saleDialog.data.calculateTotal()) }).call()"
                                                      prefix="$"
                                                      label="Total"
                                                      dense
                                                      :disabled="saleDialog.readonly"
                                                      readonly>
                                        </v-text-field>
                                    </v-col>
                                </v-layout>
                            </v-row>
                        </v-container>
                        <v-divider style="margin-left:8px;margin-right:8px;"></v-divider>
                        <v-simple-table dense style="margin-left:8px;margin-right:8px;height:40vh;overflow:auto;">
                            <template v-slot:default>
                                <thead>
                                    <tr>
                                        <th class="text-left" style="width:35%;">Product</th>
                                        <th class="text-left">Unit price</th>
                                        <th class="text-left">Quantity</th>
                                        <th class="text-left">Discount</th>
                                        <th class="text-left">Subtotal</th>
                                        <th class="text-left" v-if="!saleDialog.data.id"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-if="!saleDialog.data.id">
                                        <td>
                                            <v-autocomplete v-model="saleDialog.newItem.product"
                                                            v-on:keyup="getProductsList()"
                                                            v-on:change="onProductSelected"
                                                            :items="saleDialog.productsDropDown.searchResults"
                                                            :search-input.sync.trim="saleDialog.productsDropDown.searchQuery"
                                                            item-value="id"
                                                            item-text="name"
                                                            return-object
                                                            clearable
                                                            dense>
                                            </v-autocomplete>
                                        </td>
                                        <td>
                                            <v-text-field v-bind:value="(() => { return utils.tryGetNumber(()=> saleDialog.newItem.product.unitPrice.toFixed(2)) }).call()"
                                                          prefix="$"
                                                          dense
                                                          readonly>
                                            </v-text-field>
                                        </td>
                                        <td>
                                            <v-text-field v-model.number="saleDialog.newItem.quantity"
                                                          type="number"
                                                          min="0"
                                                          step="1"
                                                          dense>
                                            </v-text-field>
                                        </td>
                                        <td>
                                            <v-text-field v-model.number="saleDialog.newItem.discount"
                                                          type="number"
                                                          min="0"
                                                          step="1"
                                                          suffix="%"
                                                          dense>
                                            </v-text-field>
                                        </td>
                                        <td>
                                            <v-text-field v-bind:value="(() => { return utils.tryGetNumber(()=> saleDialog.newItem.calculateSubtotal()) }).call()"
                                                          prefix="$"
                                                          dense
                                                          readonly>
                                            </v-text-field>
                                        </td>
                                        <td style="text-align:right;">
                                            <v-btn icon color="primary" v-if="!saleDialog.data.id" v-on:click="addNewItemToSale()">
                                                <v-icon>mdi-plus-circle</v-icon>
                                            </v-btn>
                                        </td>
                                    </tr>
                                    <tr v-for="(item, index) in saleDialog.data.details" :key="index">
                                        <td>{{ item.product.name }}</td>
                                        <td>{{ '$'+item.unitPrice.toFixed(2) }}</td>
                                        <td>{{ item.quantity }}</td>
                                        <td>{{ item.discount+'%' }}</td>
                                        <td>{{ '$'+item.calculateSubtotal() }}</td>
                                        <td v-if="!saleDialog.data.id" style="text-align:right;">
                                            <v-btn icon color="primary" v-on:click="removeItemFromSale(item)">
                                                <v-icon>mdi-close-circle</v-icon>
                                            </v-btn>
                                        </td>
                                    </tr>
                                </tbody>
                            </template>
                        </v-simple-table>
                    </v-form>
                </v-card-text>
                <v-progress-linear style="height:3px;margin:0px;"
                                   :indeterminate="pendingRequest">
                </v-progress-linear>
                <v-card-actions v-if="saleDialog.show">
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closeSaleDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-if="!saleDialog.readonly || saleDialog.cancellation" v-on:click.stop="executeSaleDialogRequest()">ACCEPT</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-container>
</template>

<script lang="ts">
    import 'jspdf-autotable'
    import * as XLSX from 'xlsx';
    import * as _ from 'lodash';
    import jsPDF from 'jspdf'
    import { AxiosResponse } from 'axios';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { CustomerService } from '../services/customer.service';
    import { InvalidOperation } from '../common/transgression';
    import { Notify } from '../common/notify';
    import { ProductService } from '../services/product.service';
    import { Sale, SaleDetail } from '../models/sale';
    import { SaleService } from '../services/sale.service';
    import { Utils } from '../common/utils';

    @Component({
        components: {},
    })
    export default class SalesComponent extends Vue {
        private pendingRequest: boolean = false;
        private exportingXLSX: boolean = false;
        private utils: any = Utils;

        private customerService = new CustomerService();
        private productService = new ProductService();
        private saleService = new SaleService();

        private get formattedStartDate() {
            var self = this;
            return self.utils.formatDate(self.salesTable.filters.startDate);
        }
        private set formattedStartDate(value) {
            var self = this;
            self.salesTable.filters.startDate = value;
        }

        private get formattedEndDate() {
            var self = this;
            return self.utils.formatDate(self.salesTable.filters.endDate);
        }
        private set formattedEndDate(value) {
            var self = this;
            self.salesTable.filters.endDate = value;
        }

        private showStartDateMenu: any = null;
        private showEndDateMenu: any = null;
        private customersDropDown: any = {
            searchQuery: null,
            searchResults: []
        };

        private salesTable: any = {
            filters: {
                startDate: null,
                endDate: null,
                customer: null
            },
            loading: false,
            options: {
                sortBy: ['sale.date'],
                sortDesc: [true],
                page: 1,
                itemsPerPage: 5
            },
            totalItemCount: 0,
            itemsList: [],
            headers: [
                { text: 'Actions', value: '[actions]', width: '110px', sortable: false },
                { text: 'Id', value: '[id]', width: '70px', sortable: false },
                { text: 'Receipt', value: '[sale.receipttype]', width: '15%', sortable: true },
                { text: 'Date', value: '[sale.date]', width: '15%', sortable: true },
                { text: 'Customer', value: '[customer]', width: '25%', sortable: false },
                { text: 'Tax', value: '[sale.tax]', width: '100px', sortable: true },
                { text: 'Total', value: '[sale.total]', width: '100px', sortable: true },
                { text: 'Approved', value: '[sale.approved]', width: '100px', sortable: true }
            ]
        };
        private saleDialog: any = {
            show: null,
            readonly: null,
            cancellation: null,
            data: {},
            newItem: {},
            receiptTypesList: [
                'CERTIFICATE',
                'INVOICE',
                'TICKET'
            ],
            customersDropDown: {
                searchQuery: null,
                searchResults: []
            },
            productsDropDown: {
                searchQuery: null,
                searchResults: []
            }
        };

        private async mounted() {
            var self = this;
            await self.getSalesList();
        }
        private getCustomersList = _.debounce(async function () {
            var self = this;
            var searchQuery = self.saleDialog.show
                ? self.saleDialog.customersDropDown.searchQuery
                : self.customersDropDown.searchQuery;

            if (!Utils.isNullOrEmpty(searchQuery)) {
                var parameters = {
                    searchQuery: searchQuery,
                    sortBy: 'fullname',
                    sortDesc: false,
                    page: 1,
                    pageSize: 10
                };

                var response = await self.customerService.getCustomersList(parameters);
                var error = response?.data?.error;
                if (error === false) {
                    var resources = response.data.resources;
                    var itemsList = resources.itemsList;

                    if (self.saleDialog.show) {
                        self.saleDialog.customersDropDown.searchResults = itemsList.filter(c => {
                            return c.active === true
                        });
                    }
                    else {
                        self.customersDropDown.searchResults = itemsList;
                    }
                }
            }
        }, 300)
        private getProductsList = _.debounce(async function () {
            var self = this;
            var searchQuery = self.saleDialog.productsDropDown.searchQuery;

            if (!Utils.isNullOrEmpty(searchQuery)) {
                var parameters = {
                    searchQuery: searchQuery,
                    sortBy: 'product.name',
                    sortDesc: false,
                    page: 1,
                    pageSize: 10
                };

                var response = await self.productService.getProductsList(parameters);
                var error = response?.data?.error;
                if (error === false) {
                    var resources = response.data.resources;
                    var itemsList = resources.itemsList.filter(p => {
                        return p.category.active === true && p.active === true
                    });

                    self.saleDialog.productsDropDown.searchResults = itemsList;
                }
            }
        }, 220)
        private getSalesList = _.debounce(async function () {
            var self = this;
            self.salesTable.loading = true;

            const { customer, startDate, endDate } = self.salesTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.salesTable.options;

            var parameters = {
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage),
                customerId: Utils.tryGet(() => customer.id),
                startDate: Utils.tryGet(() => startDate),
                endDate: Utils.tryGet(() => endDate)
            };

            var response = await self.saleService.getSalesList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.salesTable.totalItemCount = resources.totalItemCount;
                self.salesTable.itemsList = resources.itemsList;
            }

            self.salesTable.loading = false;
        }, 220)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.salesTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.salesTable.options.page - 1) * self.salesTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }

        private showSaleDialog(options) {
            var self = this;
            var form = self.$refs.saleDataForm as any;
            form?.resetValidation();

            self.saleDialog.data = new Sale(options?.sale);
            self.saleDialog.newItem = new SaleDetail();

            if (options?.sale) {
                self.saleDialog.customersDropDown.searchResults = [options?.sale.customer];
            }

            self.saleDialog.cancellation = Utils.tryGet(() => options.cancellation);
            self.saleDialog.readonly = Utils.tryGet(() => options.readonly);
            self.saleDialog.show = true;
        }
        private onProductSelected(product) {
            var self = this;
            if (product) {
                self.saleDialog.newItem.product = product;
                self.saleDialog.newItem.quantity = 1;
                self.saleDialog.newItem.unitPrice = product.unitPrice;
                self.saleDialog.newItem.discount = 0;
            }
            else {
                self.saleDialog.newItem = new SaleDetail();
            }
        }
        private addNewItemToSale() {
            var self = this;
            try {
                var sale = self.saleDialog.data as Sale;
                var saleDetail = self.saleDialog.newItem as SaleDetail;

                if (saleDetail.quantity! > 0 && saleDetail.discount! >= 0 && saleDetail.calculateSubtotal() > 0) {
                    sale.addNewItem(saleDetail);
                    self.saleDialog.newItem = new SaleDetail();
                }
            }
            catch (error) {
                var errorMessage = (error instanceof InvalidOperation)
                    ? error.message
                    : 'Error. The operation cannot be completed.';

                Notify.pushErrorNotification(errorMessage);
            }
        }
        private removeItemFromSale(item) {
            var self = this;
            if (item) {
                var sale = self.saleDialog.data as Sale;
                sale.removeItem(item);
            }
        }
        private closeSaleDialog() {
            var self = this;
            self.saleDialog.show = null;
            self.saleDialog.cancellation = null;
            self.saleDialog.readonly = null;

            self.saleDialog.data = new Sale();
            self.saleDialog.newItem = new SaleDetail();

            self.saleDialog.customersDropDown.searchQuery = null;
            self.saleDialog.customersDropDown.searchResults = [];

            self.saleDialog.productsDropDown.searchQuery = null;
            self.saleDialog.productsDropDown.searchResults = [];
        }
        private async executeSaleDialogRequest() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.saleDataForm as any;
                var sale = self.saleDialog.data as Sale;

                if (!form.validate()) {
                    Notify.pushErrorNotification('Error. You must complete all the required fields.');
                    return;
                }

                if (!sale.details?.length) {
                    Notify.pushErrorNotification('Error. You must add at least one product to the list.');
                    return;
                }

                var bodyData = {
                    id: Utils.tryGet(() => self.saleDialog.data.id),
                    customerId: Utils.tryGet(() => self.saleDialog.data.customer.id),
                    receiptType: Utils.tryGet(() => self.saleDialog.data.receiptType),
                    tax: Utils.tryGet(() => self.saleDialog.data.tax),
                    details: Utils.tryGet(() => {
                        return sale.details?.map((d) => ({
                            productId: d.product.id, quantity: d.quantity, unitPrice: d.unitPrice, discount: d.discount
                        }));
                    })
                };

                var response: AxiosResponse | undefined;
                if (!self.saleDialog.data.id) {
                    response = await self.saleService.createSale(bodyData);
                }
                else if (self.saleDialog.cancellation) {
                    var id = self.saleDialog.data.id;
                    response = await self.saleService.cancelSale(id);
                }

                var error = response?.data?.error;
                if (error === false) {
                    self.closeSaleDialog();
                    await self.getSalesList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.pendingRequest = false;
            }
        }
        private async exportToXLSX() {
            var self = this;
            try {
                self.exportingXLSX = true;
                const { customer, startDate, endDate } = self.salesTable.filters;
                const { sortBy, sortDesc } = self.salesTable.options;

                var parameters = {
                    sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                    sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                    page: 1,
                    pageSize: Number(0X7FFFFFFF),
                    customerId: Utils.tryGet(() => customer.id),
                    startDate: Utils.tryGet(() => startDate),
                    endDate: Utils.tryGet(() => endDate)
                };

                var response = await self.saleService.getSalesList(parameters);
                var error = response?.data?.error;
                if (error === false) {
                    var resources = response.data.resources;
                    var data = resources.itemsList.map((item, index) => ({
                        ID: item.id,
                        RECEIPT_TYPE: item.receiptType.toUpperCase(),
                        DATE: item.date,
                        CUSTOMER: item.customer.fullName,
                        TAX: item.tax,
                        TOTAL: item.total,
                        APPROVED: item.approved
                    }));

                    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
                    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
                    XLSX.utils.book_append_sheet(workbook, worksheet, 'sheet1');
                    XLSX.writeFile(workbook, 'sales.xlsx');
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.exportingXLSX = false;
            }
        }
        private printSelectedSale(options) {
            var self = this;
            try {
                if (options?.sale) {
                    var sale = new Sale(options?.sale);
                    var doc = new jsPDF() as any;

                    doc.text(`SALE ${sale.id?.toUpperCase()}`, doc.internal.pageSize.getWidth() / 2, 14, null, null, 'center');
                    doc.autoTable({
                        startY: 30,
                        theme: 'plain',
                        columns: [
                            { dataKey: 'receiptType', header: 'RECEIPT' },
                            { dataKey: 'supplier', header: 'SUPPLIER' },
                            { dataKey: 'date', header: 'DATE' },
                            { dataKey: 'tax', header: 'TAX' },
                            { dataKey: 'total', header: 'TOTAL' },
                        ],
                        body: [
                            {
                                receiptType: sale?.receiptType?.toUpperCase(),
                                supplier: sale.customer.fullName,
                                date: self.utils.formatDate(sale.date),
                                tax: `${sale.tax}%`,
                                total: `$${sale.calculateTotal()}`
                            }
                        ]
                    });
                    doc.autoTableSetDefaults({
                        headStyles: { fillColor: [158, 158, 158] },
                    });
                    doc.autoTable({
                        startY: 50,
                        showHead: 'firstPage',
                        columns: [
                            { dataKey: 'product', header: 'PRODUCT' },
                            { dataKey: 'unitPrice', header: 'UNIT PRICE' },
                            { dataKey: 'quantity', header: 'QUANTITY' },
                            { dataKey: 'discount', header: 'DISCOUNT' },
                            { dataKey: 'subTotal', header: 'SUBTOTAL' }
                        ],
                        body: sale?.details?.map(d => ({
                            product: d.product.name,
                            unitPrice: `$${d.unitPrice?.toFixed(2)}`,
                            quantity: d.quantity,
                            discount: `${d.discount}%`,
                            subTotal: `$${d.calculateSubtotal()}`
                        }))
                    });

                    doc.save('sale.pdf');
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('salesTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.salesTable.options.page = 1;
            await self.getSalesList();
        }

        @Watch('salesTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getSalesList();
        }
    }
</script>
