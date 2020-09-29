import { AxiosStatic } from "axios";

export class ProductService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getProductsList(params: any) {
        var self = this;
        return self.axios
            .get('/api/products/getproductslist', {
                params
            })
    }

    createProduct(body: any) {
        var self = this;
        return self.axios
            .post('/api/products/createproduct', body)
    }

    updateProduct(body: any) {
        var self = this;
        return self.axios
            .post('/api/products/updateproduct', body)
    }

    activateOrDeactivateProduct(id: any) {
        var self = this;
        return self.axios
            .post('/api/products/activateordeactivateproduct', {}, {
                params: { id }
            })
    }

    deleteProduct(id: any) {
        var self = this;
        return self.axios
            .post('/api/products/deleteproduct', {}, {
                params: { id }
            })
    }
}
