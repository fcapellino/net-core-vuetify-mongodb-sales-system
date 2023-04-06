import { AxiosStatic } from "axios";

export class CustomerService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getCustomersList(params: any) {
        var self = this;
        return self.axios
            .get('/api/customers/getcustomerslist', {
                params
            })
    }

    createCustomer(body: any) {
        var self = this;
        return self.axios
            .post('/api/customers/createcustomer', body)
    }

    updateCustomer(body: any) {
        var self = this;
        return self.axios
            .post('/api/customers/updatecustomer', body)
    }

    activateOrDeactivateCustomer(id: any) {
        var self = this;
        return self.axios
            .post('/api/customers/activateordeactivatecustomer', {}, {
                params: { id }
            })
    }

    deleteCustomer(id: any) {
        var self = this;
        return self.axios
            .post('/api/customers/deletecustomer', {}, {
                params: { id }
            })
    }
}
