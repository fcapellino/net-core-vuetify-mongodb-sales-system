import { AxiosStatic } from "axios";

export class CategoryService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getCategoriesList(params: any) {
        var self = this;
        return self.axios
            .get('/api/categories/getcategorieslist', {
                params
            })
    }

    getCompleteCategoriesList() {
        var self = this;
        return self.axios
            .get('/api/categories/getcompletecategorieslist', {})
    }

    createCategory(body: any) {
        var self = this;
        return self.axios
            .post('/api/categories/createcategory', body)
    }

    updateCategory(body: any) {
        var self = this;
        return self.axios
            .post('/api/categories/updatecategory', body)
    }

    activateOrDeactivateCategory(id: any) {
        var self = this;
        return self.axios
            .post('/api/categories/activateordeactivatecategory', {}, {
                params: { id }
            })
    }

    deleteCategory(id: any) {
        var self = this;
        return self.axios
            .post('/api/categories/deletecategory', {}, {
                params: { id }
            })
    }
}
