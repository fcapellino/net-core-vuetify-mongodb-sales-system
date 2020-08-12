<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="productForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm4 class="mr-2">
                            <v-select v-model="productsTable.filters.categoryId"
                                      :items="categoriesList"
                                      item-value="id"
                                      item-text="name"
                                      label="Category"
                                      clearable>
                            </v-select>
                        </v-flex>
                        <v-flex flex sm4 class="mr-2">
                            <v-text-field v-model.trim="productsTable.filters.searchQuery"
                                          label="Search"
                                          :hint="`${('Please enter your search term')}`"
                                          clearable>
                            </v-text-field>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" v-on:click.stop="showProductDialog()">NEW PRODUCT</v-btn>
                        </v-flex>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="productsTable.headers"
                              :items="productsTable.itemsList"
                              :options.sync="productsTable.options"
                              :server-items-length="productsTable.totalItemCount"
                              :loading="productsTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showProductDialog({product: item, readonly: false})">
                                    mdi-pencil
                                </v-icon>
                            </template>
                            <span>Edit</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showProductDialog({product: item, readonly: true, deletion: true})">
                                    mdi-delete
                                </v-icon>
                            </template>
                            <span>Delete</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="activateOrDeactivateProduct({product: item})">
                                    {{ item.active? 'mdi-minus-circle' : 'mdi-checkbox-marked-circle' }}
                                </v-icon>
                            </template>
                            <span>{{ item.active? 'Deactivate' : 'Activate' }}</span>
                        </v-tooltip>
                    </template>
                    <template v-slot:item.[id]="{ item }">
                        {{ getItemIndex(item) }}
                    </template>
                    <template v-slot:item.[product.barcode]="{ item }">
                        {{ item.barCode }}
                    </template>
                    <template v-slot:item.[product.name]="{ item }">
                        {{ item.name }}
                    </template>
                    <template v-slot:item.[product.description]="{ item }">
                        {{ item.description }}
                    </template>
                    <template v-slot:item.[product.category]="{ item }">
                        {{ item.category.name }}
                    </template>
                    <template v-slot:item.[product.stock]="{ item }">
                        {{ item.stock }}
                    </template>
                    <template v-slot:item.[product.unitprice]="{ item }">
                        {{ '$'+item.unitPrice.toFixed(2).toString() }}
                    </template>
                    <template v-slot:item.[product.active]="{ item }">
                        <p style="margin-bottom:0px;font-weight:bold;" v-bind:class="[item.active? 'body-2 p-green' : 'body-2 p-red']">
                            {{ item.active? 'Active':'Inactive' }}
                        </p>
                    </template>
                </v-data-table>
            </v-card>
        </v-col>
        <v-dialog v-model="productDialog.show" persistent max-width="500px">
            <v-card>
                <v-card-title>
                    <span v-if="!productDialog.data.id" class="headline">New product</span>
                    <span v-else-if="productDialog.data.id && productDialog.deletion" class="headline">Do you want to delete this product?</span>
                    <span v-else class="headline">Edit product</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="productDataForm">
                        <v-container grid-list-md>
                            <v-layout wrap>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model="productDialog.data.id"
                                                  label="Id"
                                                  disabled
                                                  style="display:none;">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="6" xs="6">
                                    <v-select v-model="productDialog.data.category"
                                              :items="categoriesList"
                                              item-value="id"
                                              item-text="name"
                                              label="Category"
                                              :disabled="productDialog.readonly"
                                              :rules="[v => v!=null || 'This field is required']"
                                              return-object>
                                    </v-select>
                                </v-col>
                                <!--<v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="categoryDialog.data.name"
                                                  label="Name"
                                                  :disabled="categoryDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-textarea v-model.trim="categoryDialog.data.description"
                                                label="Description"
                                                :rows="4"
                                                :disabled="categoryDialog.readonly"
                                                :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-textarea>
                                </v-col>-->
                            </v-layout>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closeProductDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="executeProductDialogRequest()">ACCEPT</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-container>
</template>

<script lang="ts">
    import * as _ from 'lodash';
    import { AxiosResponse } from 'axios';
    import { CategoryService } from '../services/category.service';
    import { Component, Vue, Watch } from 'vue-property-decorator';
    import { Notify } from '../common/notify';
    import { ProductService } from '../services/product.service';
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class ProductsComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private categoryService = new CategoryService();
        private productService = new ProductService();

        private productsTable: any = {
            filters: {
                searchQuery: null,
                categoryId: null
            },
            loading: false,
            options: {
                page: 1,
                itemsPerPage: 5
            },
            totalItemCount: 0,
            itemsList: [],
            headers: [
                { text: 'Actions', value: '[actions]', width: '10%', sortable: false },
                { text: 'Id', value: '[id]', sortable: false },
                { text: 'BarCode', value: '[product.barcode]', sortable: true },
                { text: 'Name', value: '[product.name]', width: '15%', sortable: true },
                { text: 'Description', value: '[product.description]', sortable: true },
                { text: 'Category', value: '[product.category]', sortable: false },
                { text: 'Stock', value: '[product.stock]', sortable: true },
                { text: 'Unit price', value: '[product.unitprice]', sortable: true },
                { text: 'State', value: '[product.active]', sortable: true }
            ]
        };
        private productDialog: any = {
            show: null,
            readonly: null,
            deletion: null,
            data: {
                id: null,
                category: null,
                barCode: null,
                name: null,
                description: null,
                stock: null,
                unitPrice: null
            }
        };
        private categoriesList: [];

        private async mounted() {
            var self = this;
            await self.getCategoriesList();
            await self.getProductsList();
        }
        private getCategoriesList = _.debounce(async function () {
            var self = this;
            var response = await self.categoryService.getCompleteCategoriesList();
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.categoriesList = resources.itemsList;
            }
        }, 300)
        private getProductsList = _.debounce(async function () {
            var self = this;
            self.productsTable.loading = true;

            const { searchQuery, categoryId } = self.productsTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.productsTable.options;

            var parameters = {
                searchQuery: Utils.tryGet(() => searchQuery),
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage),
                categoryId: Utils.tryGet(() => categoryId)
            };

            var response = await self.productService.getProductsList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.productsTable.totalItemCount = resources.totalItemCount;
                self.productsTable.itemsList = resources.itemsList;
            }

            self.productsTable.loading = false;
        }, 300)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.productsTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.productsTable.options.page - 1) * self.productsTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }
        private showProductDialog(options) {
            var self = this;
            if (options?.product) {
                self.productDialog.data.id = options.product.id;
                self.productDialog.data.category = self.categoriesList.find(x => x.id === options.product.category.id);
                self.productDialog.data.barCode = options.product.barCode;
                self.productDialog.data.name = options.product.name;
                self.productDialog.data.description = options.product.description;
                self.productDialog.data.stock = options.product.stock;
                self.productDialog.data.unitPrice = options.product.unitPrice;
            }

            self.productDialog.deletion = Utils.tryGet(() => options.deletion);
            self.productDialog.readonly = Utils.tryGet(() => options.readonly);
            self.productDialog.show = true;
        }
        private closeProductDialog() {
            var self = this;
            self.productDialog.show = null;
            self.productDialog.deletion = null;
            self.productDialog.readonly = null;
            self.productDialog.data = {};

            var form = self.$refs.productDataForm as any;
            form.reset();
        }
        private async executeProductDialogRequest() {
            var self = this;
            try {
                var form = self.$refs.categoryDataForm as any;
                if (!form.validate()) {
                    return;
                }

                //var bodyData = {
                //    id: Utils.tryGet(() => self.categoryDialog.data.id),
                //    name: Utils.tryGet(() => self.categoryDialog.data.name),
                //    description: Utils.tryGet(() => self.categoryDialog.data.description)
                //};

                //var response: AxiosResponse;
                //if (!self.categoryDialog.data.id) {
                //    response = await self.categoryService.createCategory(bodyData);
                //}
                //else {
                //    if (!self.categoryDialog.deletion) {
                //        response = await self.categoryService.updateCategory(bodyData);
                //    }
                //    else {
                //        var id = self.categoryDialog.data.id;
                //        response = await self.categoryService.deleteCategory(id);
                //    }
                //}

                //var error = response?.data?.error;
                //if (error === false) {
                //    self.closeCategoryDialog();
                //    await self.getCategoriesList();
                //}
            }
            catch (error) {
                self.closeProductDialog();
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }
        private async activateOrDeactivateProduct(options) {
            var self = this;
            try {
                if (!options?.product) {
                    return;
                }

                var id = options.product.id;
                var response: AxiosResponse;
                response = await self.productService.activateOrDeactivateProduct(id);

                var error = response?.data?.error;
                if (error === false) {
                    await self.getProductsList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('productsTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            await self.getProductsList();
        }

        @Watch('productsTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getProductsList();
        }
    }
</script>
