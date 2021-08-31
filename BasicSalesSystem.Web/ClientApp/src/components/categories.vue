<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-form ref="categoryForm">
                    <v-layout row wrap style="margin:10px;padding-top:10px;padding-bottom:10px;">
                        <v-flex flex sm4>
                            <v-text-field v-model.trim="categoriesTable.filters.searchQuery"
                                          label="Search"
                                          :hint="`${('Please enter your search term')}`"
                                          clearable>
                            </v-text-field>
                        </v-flex>
                        <v-spacer></v-spacer>
                        <v-flex flex style="text-align:right;padding-top:20px;">
                            <v-btn small color="primary" v-on:click.stop="showCategoryDialog()">NEW CATEGORY</v-btn>
                        </v-flex>
                    </v-layout>
                </v-form>
            </v-card>
        </v-col>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <v-data-table :headers="categoriesTable.headers"
                              :items="categoriesTable.itemsList"
                              :options.sync="categoriesTable.options"
                              :server-items-length="categoriesTable.totalItemCount"
                              :loading="categoriesTable.loading"
                              :footer-props="{itemsPerPageOptions:[5,10,15,20]}"
                              class="elevation-1">
                    <template v-slot:item.[actions]="{ item }">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showCategoryDialog({category: item, readonly: false})">
                                    mdi-pencil
                                </v-icon>
                            </template>
                            <span>Edit</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="showCategoryDialog({category: item, readonly: true, deletion: true})">
                                    mdi-delete
                                </v-icon>
                            </template>
                            <span>Delete</span>
                        </v-tooltip>
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon small class="mr-2" v-on="on"
                                        v-on:click.stop="activateOrDeactivateCategory({category: item})">
                                    {{ item.active? 'mdi-minus-circle' : 'mdi-checkbox-marked-circle' }}
                                </v-icon>
                            </template>
                            <span>{{ item.active? 'Deactivate' : 'Activate' }}</span>
                        </v-tooltip>
                    </template>
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
        <v-dialog v-model="categoryDialog.show" persistent max-width="500px">
            <v-card>
                <v-card-title>
                    <span v-if="!categoryDialog.data.id" class="headline">New category</span>
                    <span v-else-if="categoryDialog.data.id && categoryDialog.deletion" class="headline">Do you want to delete this category?</span>
                    <span v-else class="headline">Edit category</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="categoryDataForm">
                        <v-container grid-list-md>
                            <v-layout wrap>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model="categoryDialog.data.id"
                                                  label="Id"
                                                  dense
                                                  disabled
                                                  style="display:none;">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-text-field v-model.trim="categoryDialog.data.name"
                                                  label="Name"
                                                  dense
                                                  :disabled="categoryDialog.readonly"
                                                  :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-text-field>
                                </v-col>
                                <v-col cols="12" xs="12">
                                    <v-textarea v-model.trim="categoryDialog.data.description"
                                                label="Description"
                                                dense
                                                :rows="4"
                                                :disabled="categoryDialog.readonly"
                                                :rules="[v => (!!v && !utils.isNullOrEmpty(v)) || 'This field is required']">
                                    </v-textarea>
                                </v-col>
                            </v-layout>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="closeCategoryDialog()">CANCEL</v-btn>
                    <v-btn small text v-bind:disabled="pendingRequest" v-on:click.stop="executeCategoryDialogRequest()">ACCEPT</v-btn>
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
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class CategoriesComponent extends Vue {
        private pendingRequest: boolean = false;
        private utils: any = Utils;

        private categoryService = new CategoryService();

        private categoriesTable: any = {
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
            headers: [
                { text: 'Actions', value: '[actions]', width: '110px', sortable: false },
                { text: 'Id', value: '[id]', width: '70px', sortable: false },
                { text: 'Name', value: '[name]', width: '15%', sortable: true },
                { text: 'Description', value: '[description]', sortable: true },
                { text: 'State', value: '[active]', width: '80px', sortable: true }
            ]
        };
        private categoryDialog: any = {
            show: null,
            readonly: null,
            deletion: null,
            data: {
                id: null,
                name: null,
                description: null
            }
        };

        private async mounted() {
            var self = this;
            await self.getCategoriesList();
        }
        private getCategoriesList = _.debounce(async function () {
            var self = this;
            self.categoriesTable.loading = true;

            const { searchQuery } = self.categoriesTable.filters;
            const { sortBy, sortDesc, page, itemsPerPage } = self.categoriesTable.options;

            var parameters = {
                searchQuery: Utils.tryGet(() => searchQuery),
                sortBy: Utils.tryGet(() => sortBy ? `${[...sortBy].shift().replace(/[\[\]']+/g, '')}` : null),
                sortDesc: Utils.tryGet(() => [...sortDesc].shift()),
                page: Utils.tryGet(() => page),
                pageSize: Utils.tryGet(() => itemsPerPage)
            };

            var response = await self.categoryService.getCategoriesList(parameters);
            var error = response?.data?.error;
            if (error === false) {
                var resources = response.data.resources;
                self.categoriesTable.totalItemCount = resources.totalItemCount;
                self.categoriesTable.itemsList = resources.itemsList;
            }

            self.categoriesTable.loading = false;
        }, 300)
        private getItemIndex = function (item) {
            var self = this;
            var row = self.categoriesTable.itemsList.findIndex(u => u.id === item.id) + 1;
            return ((self.categoriesTable.options.page - 1) * self.categoriesTable.options.itemsPerPage + row).toString().padStart(3, '0');
        }
        private showCategoryDialog(options) {
            var self = this;
            if (options?.category) {
                self.categoryDialog.data.id = options.category.id;
                self.categoryDialog.data.name = options.category.name;
                self.categoryDialog.data.description = options.category.description;
            }

            self.categoryDialog.deletion = Utils.tryGet(() => options.deletion);
            self.categoryDialog.readonly = Utils.tryGet(() => options.readonly);
            self.categoryDialog.show = true;
        }
        private closeCategoryDialog() {
            var self = this;
            self.categoryDialog.show = null;
            self.categoryDialog.deletion = null;
            self.categoryDialog.readonly = null;
            self.categoryDialog.data = {};

            var form = self.$refs.categoryDataForm as any;
            form.reset();
        }
        private async executeCategoryDialogRequest() {
            var self = this;
            try {
                self.pendingRequest = true;
                var form = self.$refs.categoryDataForm as any;
                if (!form.validate()) {
                    return;
                }

                var bodyData = {
                    id: Utils.tryGet(() => self.categoryDialog.data.id),
                    name: Utils.tryGet(() => self.categoryDialog.data.name),
                    description: Utils.tryGet(() => self.categoryDialog.data.description)
                };

                var response: AxiosResponse;
                if (!self.categoryDialog.data.id) {
                    response = await self.categoryService.createCategory(bodyData);
                }
                else {
                    if (!self.categoryDialog.deletion) {
                        response = await self.categoryService.updateCategory(bodyData);
                    }
                    else {
                        var id = self.categoryDialog.data.id;
                        response = await self.categoryService.deleteCategory(id);
                    }
                }

                var error = response?.data?.error;
                if (error === false) {
                    self.closeCategoryDialog();
                    await self.getCategoriesList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
            finally {
                self.pendingRequest = false;
            }
        }
        private async activateOrDeactivateCategory(options) {
            var self = this;
            try {
                if (!options?.category) {
                    return;
                }

                var id = options.category.id;
                var response: AxiosResponse;
                response = await self.categoryService.activateOrDeactivateCategory(id);

                var error = response?.data?.error;
                if (error === false) {
                    await self.getCategoriesList();
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }

        @Watch('categoriesTable.filters', { immediate: true, deep: true })
        private async onFiltersChanged(value: any, oldValue: any) {
            var self = this;
            self.categoriesTable.options.page = 1;
            await self.getCategoriesList();
        }

        @Watch('categoriesTable.options', { immediate: true, deep: true })
        private async onOptionsChanged(value: any, oldValue: any) {
            var self = this;
            await self.getCategoriesList();
        }
    }
</script>
