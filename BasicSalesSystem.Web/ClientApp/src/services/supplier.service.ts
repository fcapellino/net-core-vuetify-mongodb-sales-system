import { AxiosStatic } from "axios";

export class SupplierService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getSuppliersList(params: any) {
        var self = this;
        return self.axios
            .get('/api/suppliers/getsupplierslist', {
                params
            })
    }

    createSupplier(body: any) {
        var self = this;
        return self.axios
            .post('/api/suppliers/createsupplier', body)
    }

    updateSupplier(body: any) {
        var self = this;
        return self.axios
            .post('/api/suppliers/updatesupplier', body)
    }

    activateOrDeactivateSupplier(id: any) {
        var self = this;
        return self.axios
            .post('/api/suppliers/activateordeactivatesupplier', {}, {
                params: { id }
            })
    }

    deleteSupplier(id: any) {
        var self = this;
        return self.axios
            .post('/api/suppliers/deletesupplier', {}, {
                params: { id }
            })
    }
}
