import { AxiosStatic } from "axios";

export class SaleService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getSalesList(params: any) {
        var self = this;
        return self.axios
            .get('/api/sales/getsaleslist', {
                params
            })
    }

    getAnnualStatistics() {
        var self = this;
        return self.axios
            .get('/api/sales/getannualstatistics')
    }

    createSale(body: any) {
        var self = this;
        return self.axios
            .post('/api/sales/createsale', body)
    }

    cancelSale(id: any) {
        var self = this;
        return self.axios
            .post('/api/sales/cancelSale', {}, {
                params: { id }
            })
    }
}
