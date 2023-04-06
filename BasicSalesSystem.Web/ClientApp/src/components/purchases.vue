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
                                <v-date-picker v-model="purchasesTable.filters.startDate" :show-current="false" no-title v-on:input="showStartDateMenu=false"></v-date-picker>
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
                                <v-date-picker v-model="purchasesTable.filters.endDate" :show-current="false" no-title v-on:input="showEndDateMenu=false"></v-date-picker>
                            </v-menu>
                        </v-flex>
                        <v-flex flex sm4 class="mr-2">
                            <v-autocomplete v-model="purchasesTable.filters.supplier"
                                            v-on:keyup="getSuppliersList()"
                                            :items="suppliersDropDown.searchResults"
                                            :search-input.sync.trim="suppliersDropDown.searchQuery"
                                            persistent-hint
                                            item-value="id"
                                            item-text="fullName"
                                            label="Supplier"
                                            return-object
                                            clearable>
                            </v-autocomplete>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" class="mr-2" v-on:click.stop="showPurchaseDialog()">NEW PURCHASE</v-btn>
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
                <v-data-table :headers="purchasesTable.headers"
                              :items="purchasesTable.itemsList"
                              :options.sync="purchasesTable.options"
                              :server-items-length="purchasesTable.totalItemCount"
                              :loading="purchasesTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showPurchaseDialog({purchase: item, readonly: true})">
                                    mdi-library-books
                                </v-icon>
                            </template>
                            <span>Details</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="printSelectedPurchase({purchase: item})">
                                    mdi-printer
                                </v-icon>
                            </template>
                            <span>Print</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showPurchaseDialog({purchase: item, readonly: true, cancellation: true})"
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
                    <template v-slot:item.[purchase.receipttype]="{ item }">
                        {{ item.receiptType.toUpperCase() }}
                    </template>
                    <template v-slot:item.[purchase.date]="{ item }">
                        {{ utils.formatDate(item.date) }}
                    </template>
                    <template v-slot:item.[supplier]="{ item }">
                        {{ item.supplier.fullName }}
                    </template>
                    <template v-slot:item.[purchase.tax]="{ item }">
                        {{ item.tax+'%' }}
                    </template>
                    <template v-slot:item.[purchase.total]="{ item }">
                        {{ '$'+item.total.toFixed(2).toString() }}
                    </template>
                    <template v-slot:item.[purchase.approved]="{ item }">
                        <p style="margin-bottom:0px;font-weight:bold;" v-bind:class="[item.approved? 'body-2 p-green' : 'body-2 p-red']">
                            {{ item.approved? 'Approved':'Cancelled' }}
                        </p>
                    </template>
                </v-data-table>
            </v-card>
        </v-col>
        <v-dialog v-model="purchaseDialog.show" persistent max-width="1000px">
            <v-card>
                <v-card-title>
                    <span v-if="!purchaseDialog.data.id" class="headline">New purchase</span>
                    <span v-else-if="purchaseDialog.data.id && purchaseDialog.cancellation" class="headline">Do you want to cancel this purchase?</span>
                    <span v-else class="headline">Details</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="purchaseDataForm">
                        <v-container grid-list-md>
                            <v-row>
                                <v-layout wrap>
                                    <v-col cols="2" xs="2">
                                        <v-select v-model.trim="purchaseDialog.data.receiptType"
                                                  :items="purchaseDialog.receiptTypesList"
                                                  label="Receipt type"
                                                  dense
                                                  :disabled="purchaseDialog.readonly"
                                                  :rules="[v => v!=null || 'This field is required']">
                                        </v-select>
                                    </v-col>
                                    <v-col cols="5" xs="5">
                                        <v-autocomplete v-model="purchaseDialog.data.supplier"
                                                        v-on:keyup="getSuppliersList()"
                                                        :items="purchaseDialog.suppliersDropDown.searchResults"
                                                        :search-input.sync.trim="purchaseDialog.suppliersDropDown.searchQuery"
                                                        item-value="id"
                                                        item-text="fullName"
                                                        return-object
                                                        label="Supplier"
                                                        dense
                                                        :disabled="purchaseDialog.readonly"
                                                        :rules="[v => v!=null || 'This field is required']">
                                        </v-autocomplete>
                                    </v-col>
                                    <v-col cols="2" xs="2">
                                        <v-text-field v-model.trim="purchaseDialog.data.formattedDate"
                                                      label="Date"
                                                      dense
                                                      :disabled="purchaseDialog.readonly"
                                                      readonly>
                                        </v-text-field>
                                    </v-col>
                                    <v-col cols="1" xs="1">
                                        <v-text-field v-model.number="purchaseDialog.data.tax"
                                                      type="number"
                                                      min="0"
                                                      step="1"
                                                      suffix="%"
                                                      label="Tax"
                                                      dense
                                                      :disabled="purchaseDialog.readonly"
                                                      :rules="[v => (Number(v)>=0 && Number.isInteger(v)) || 'Invalid']">
                                        </v-text-field>
                                    </v-col>
                                    <v-col cols="2" xs="2">
                                        <v-text-field v-bind:value="(() => { return utils.tryGetNumber(()=> purchaseDialog.data.calculateTotal()) }).call()"
                                                      prefix="$"
                                                      label="Total"
                                                      dense
                                                      :disabled="purchaseDialog.readonly"
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
                                        <th class="text-left">Subtotal</th>
                                        <th class="text-left" v-if="!purchaseDialog.data.id"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-if="!purchaseDialog.data.id">
                                        <td>
                                            <v-autocomplete v-model="purchaseDialog.newItem.product"
                                                            v-on:keyup="getProductsList()"
                                                            v-on:change="onProductSelected"
                                                            :items="purchaseDialog.productsDropDown.searchResults"
                                                            :search-input.sync.trim="purchaseDialog.productsDropDown.searchQuery"
                                                            item-value="id"
                                                            item-text="name"
                                                            return-object
                                                            clearable
                                                            dense>
                                            </v-autocomplete>
                                        </td>
                                        <td>
                                            <v-text-field v-model.number="purchaseDialog.newItem.unitPrice"
                                                          type="number"
                                                          min="0"
                                                          step="0.05"
                                                          prefix="$"
                                                          dense>
                                            </v-text-field>
                                        </td>
                                        <td>
                                            <v-text-field v-model.number="purchaseDialog.newItem.quantity"
                                                          type="number"
                                                          min="0"
                                                          step="1"
                                                          dense>
                                            </v-text-field>
                                        </td>
                                        <td>
                                            <v-text-field v-bind:value="(() => { return utils.tryGetNumber(()=> purchaseDialog.newItem.calculateSubtotal()) }).call()"
                                                          prefix="$"
                                                          dense
                                                          readonly>
                                            </v-text-field>
                                        </td>
                                        <td style="text-align:right;">
                                            <v-btn icon color="primary" v-if="!purchaseDialog.data.id" v-on:click="addNewItemToPurchase()">
                                                <v-icon>mdi-plus-circle</v-icon>
                                            </v-btn>
                                        </td>
                                    </tr>
                                    <tr v-for="(item, index) in purchaseDialog.data.details" :key="index">
                                        <td>{{ item.product.name }}</td>
                                        <td>{{ '$'+item.unitPrice.toFixed(2) }}</td>
                                        <td>{{ item.quantity }}</td>
                                        <td>{{ '$'+item.calculateSubtotal() }}</td>
                                        <td v-if="!purchaseDialog.data.id" style="text-align:right;">
                                            <v-btn icon color="primary" v-on:click="removeItemFromPurchase(item)">
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
                <v-card-actions v-if="purchaseDialog.show">
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closePurchaseDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-if="!purchaseDialog.readonly || purchaseDialog.cancellation" v-on:click.stop="executePurchaseDialogRequest()">ACCEPT</v-btn>
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
    import { InvalidOperation } from '../common/transgression';
    import { Notify } from '../common/notify';
    import { ProductService } from '../services/product.service';
    import { Purchase, PurchaseDetail } from '../models/purchase';
    import { PurchaseService } from '../services/purchase.service';
    import { SupplierService } from '../services/supplier.service';
    import { Utils } from '../common/utils';

    @Component({
        components: {},
    })
    export default class PurchasesComponent extends Vue {
        private pendingRequest: boolean = false;
        private exportingXLSX: boolean = false;
        private utils: any = Utils;

        private supplierService = new SupplierService();
        private productService = new ProductService();
        private purchaseService = new PurchaseService();

        private get formattedStartDate() {
            var self = this;
            return self.utils.formatDate(self.purchasesTable.filters.startDate);
        }
        private set formattedStartDate(value) {
            var self = this;
            self.purchasesTable.filters.startDate = value;
        }

        private get formattedEndDate() {
            var self = this;
            return self.utils.formatDate(self.purchasesTable.filters.endDate);
        }
        private set formattedEndDate(value) {
            var self = this;
            self.purchasesTable.filters.endDate = value;
        }

        private showStartDateMenu: any = null;
        private showEndDateMenu: any = null;
        private suppliersDropDown: any = {
            searchQuery: null,
            searchResults: []
        };

        private purchasesTable: any = {
            filters: {
                startDate: null,
                endDate: null,
                supplier: null
            },
            loading: false,
            options: {
                sortBy: ['purchase.date'],
                sortDesc: [true],
                page: 1,
                itemsPerPage: 5
            },
            totalItemCount: 0,
            itemsList: [],
            headers: [
                { text: 'Actions', value: '[actions]', width: '110px', sortable: false },
                { text: 'Id', value: '[id]', width: '70px', sortable: false },
                { text: 'Receipt', value: '[purchase.receipttype]', width: '15%', sortable: true },
                { text: 'Date', value: '[purchase.date]', width: '15%', sortable: true },
                { text: 'Supplier', value: '[supplier]', width: '25%', sortable: false },
                { text: 'Tax', value: '[purchase.tax]', width: '100px', sortable: true },
                { text: 'Total', value: '[purchase.total]', width: '100px', sortable: true },
                { text: 'Approved', value: '[purchase.approved]', width: '100px', sortable: true }
            ]
        };
        private purchaseDialog: any = {
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
            suppliersDropDown: {
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
            await self.getPurchasesList();
        }
        private getSuppliersList = _.debounce(async function () {
            var self = this;
            var searchQuery = self.purchaseDialog.show
                ? self.purchaseDialog.suppliersDropDown.searchQuery
                : self.suppliersDropDown.searchQuery;

            if (!Utils.isNullOrEmpty(searchQuery)) {
                var parameters = {
                    searchQuery: searchQuery,
                    sortBy: 'fullname',
                    sortDesc: false,
                    page: 1,
                    pageSize: 10
                };

                var response = await self.supplierService.getSuppliersList(parameters);
                var error = response?.data?.error;
                if (error === false) {
                    var resources = response.data.resources;
                    var itemsList = resources.itemsList;

                    if (self.purchaseDialog.show) {
                        self.purchaseDialog.suppliersDropDown.searchResults = itemsList.filter(s => {
                            return s.active === true
                        });
                    }
                    else {
                        self.suppliersDropDown.searchResults = itemsList;
                    }
                }
            }
        }, 300)
        private getProductsList = _.debounce(async function () {
            var self = this;
            var searchQuery = self.purchaseDialog.productsDropDown.searchQuery;

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

                    self.purchaseDialog.productsDropDown.searchResults = itemsList;
                }
            }
        }, 220)
        private getPurchasesList = _.debounce(async function () {
            var self = this;
            self.purchasesTable.loading = true;

            const { supplier, startDate, endDate } = self.purchasesTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.purchasesTable.options;

            var parameters = {
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage),
                supplierId: Utils.tryGet(() => supplier.id),
                startDate: Utils.tryGet(() => startDate),
                endDate: Utils.tryGet(() => endDate)
            };

            var response = await self.purchaseService.getPurchasesList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.purchasesTable.totalItemCount = resources.totalItemCount;
                self.purchasesTable.itemsList = resources.itemsList;
            }

            self.purchasesTable.loading = false;
        }, 220)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.purchasesTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.purchasesTable.options.page - 1) * self.purchasesTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }

        private showPurchaseDialog(options) {
            var self = this;
            var form = self.$refs.purchaseDataForm as any;
            form?.resetValidation();

            self.purchaseDialog.data = new Purchase(options?.purchase);
            self.purchaseDialog.newItem = new PurchaseDetail();

            if (options?.purchase) {
                self.purchaseDialog.suppliersDropDown.searchResults = [options?.purchase.supplier];
            }

            self.purchaseDialog.cancellation = Utils.tryGet(() => options.cancellation);
            self.purchaseDialog.readonly = Utils.tryGet(() => options.readonly);
            self.purchaseDialog.show = true;
        }
        private onProductSelected(product) {
            var self = this;
            if (product) {
                self.purchaseDialog.newItem.product = product;
                self.purchaseDialog.newItem.quantity = 1;
                self.purchaseDialog.newItem.unitPrice = 0;
            }
            else {
                self.purchaseDialog.newItem = new PurchaseDetail();
            }
        }
        private addNewItemToPurchase() {
            var self = this;
            try {
                var purchase = self.purchaseDialog.data as Purchase;
                var purchaseDetail = self.purchaseDialog.newItem as PurchaseDetail;

                if (purchaseDetail.quantity! > 0 && purchaseDetail.calculateSubtotal() > 0) {
                    purchase.addNewItem(purchaseDetail);
                    self.purchaseDialog.newItem = new PurchaseDetail();
                }
            }
            catch (error) {
                var errorMessage = (error instanceof InvalidOperation)
                    ? error.message
                    : 'Error. The operation cannot be completed.';

                Notify.pushErrorNotification(errorMessage);
            }
        }
        private removeItemFromPurchase(item) {
            var self = this;
            if (item) {
                var purchase = self.purchaseDialog.data as Purchase;
                purchase.removeItem(item);
            }
        }
        private closePurchaseDialog() {
            var self = this;
            self.purchaseDialog.show = null;
            self.purchaseDialog.cancellation = null;
            self.purchaseDialog.readonly = null;

            self.purchaseDialog.data = new Purchase();
            self.purchaseDialog.newItem = new PurchaseDetail();

            self.purchaseDialog.suppliersDropDown.searchQuery = null;
            self.purchaseDialog.suppliersDropDown.searchResults = [];

            self.purchaseDialog.productsDropDown.searchQuery = null;
            self.purchaseDialog.productsDropDown.searchResults = [];
        }
        private async executePurchaseDialogRequest() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.purchaseDataForm as any;
                var purchase = self.purchaseDialog.data as Purchase;

                if (!form.validate()) {
                    Notify.pushErrorNotification('Error. You must complete all the required fields.');
                    return;
                }

                if (!purchase.details?.length) {
                    Notify.pushErrorNotification('Error. You must add at least one product to the list.');
                    return;
                }

                var bodyData = {
                    id: Utils.tryGet(() => self.purchaseDialog.data.id),
                    supplierId: Utils.tryGet(() => self.purchaseDialog.data.supplier.id),
                    receiptType: Utils.tryGet(() => self.purchaseDialog.data.receiptType),
                    tax: Utils.tryGet(() => self.purchaseDialog.data.tax),
                    details: Utils.tryGet(() => {
                        return purchase.details?.map((d) => ({
                            productId: d.product.id, quantity: d.quantity, unitPrice: d.unitPrice
                        }));
                    })
                };

                var response: AxiosResponse | undefined;
                if (!self.purchaseDialog.data.id) {
                    response = await self.purchaseService.createPurchase(bodyData);
                }
                else if (self.purchaseDialog.cancellation) {
                    var id = self.purchaseDialog.data.id;
                    response = await self.purchaseService.cancelPurchase(id);
                }

                var error = response?.data?.error;
                if (error === false) {
                    self.closePurchaseDialog();
                    await self.getPurchasesList();
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
                const { supplier, startDate, endDate } = self.purchasesTable.filters;
                const { sortBy, sortDesc } = self.purchasesTable.options;

                var parameters = {
                    sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                    sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                    page: 1,
                    pageSize: Number(0X7FFFFFFF),
                    supplierId: Utils.tryGet(() => supplier.id),
                    startDate: Utils.tryGet(() => startDate),
                    endDate: Utils.tryGet(() => endDate)
                };

                var response = await self.purchaseService.getPurchasesList(parameters);
                var error = response?.data?.error;
                if (error === false) {
                    var resources = response.data.resources;
                    var data = resources.itemsList.map((item, index) => ({
                        ID: item.id,
                        RECEIPT_TYPE: item.receiptType.toUpperCase(),
                        DATE: item.date,
                        SUPPLIER: item.supplier.fullName,
                        TAX: item.tax,
                        TOTAL: item.total,
                        APPROVED: item.approved
                    }));

                    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
                    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
                    XLSX.utils.book_append_sheet(workbook, worksheet, 'sheet1');
                    XLSX.writeFile(workbook, 'purchases.xlsx');
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.exportingXLSX = false;
            }
        }
        private printSelectedPurchase(options) {
            var self = this;
            try {
                if (options?.purchase) {
                    var purchase = new Purchase(options?.purchase);
                    var doc = new jsPDF() as any;

                    doc.text(`PURCHASE ${purchase.id?.toUpperCase()}`, doc.internal.pageSize.getWidth() / 2, 14, null, null, 'center');
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
                                receiptType: purchase?.receiptType?.toUpperCase(),
                                supplier: purchase.supplier.fullName,
                                date: self.utils.formatDate(purchase.date),
                                tax: `${purchase.tax}%`,
                                total: `$${purchase.calculateTotal()}`
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
                            { dataKey: 'subTotal', header: 'SUBTOTAL' }
                        ],
                        body: purchase?.details?.map(d => ({
                            product: d.product.name,
                            unitPrice: `$${d.unitPrice?.toFixed(2)}`,
                            quantity: d.quantity,
                            subTotal: `$${d.calculateSubtotal()}`
                        }))
                    });

                    doc.save('purchase.pdf');
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('purchasesTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.purchasesTable.options.page = 1;
            await self.getPurchasesList();
        }

        @Watch('purchasesTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getPurchasesList();
        }
    }
</script>
