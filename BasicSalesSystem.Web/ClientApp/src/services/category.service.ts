import axios from "axios";

export class CategoryService {
    constructor() {
    }

    getCategory(id: any) {
        return axios
            .get('/api/categories/getcategory', {
                params: { id }
            })
    }

    getCategoriesList(params: any) {
        return axios
            .get('/api/categories/getcategorieslist', {
                params
            })
    }

    createCategory(body: any) {
        return axios
            .post('/api/categories/createcategory', body)
    }

    updateCategory(body: any) {
        return axios
            .post('/api/categories/updatecategory', body)
    }

    activateOrDeactivateCategory(id: any) {
        return axios
            .post('/api/categories/activateordeactivatecategory', {}, {
                params: { id }
            })
    }

    deleteCategory(id: any) {
        return axios
            .post('/api/categories/deletecategory', {}, {
                params: { id }
            })
    }
}
