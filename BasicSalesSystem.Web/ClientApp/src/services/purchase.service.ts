import { AxiosStatic } from "axios";

export class PurchaseService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getPurchasesList(params: any) {
        var self = this;
        return self.axios
            .get('/api/purchases/getpurchaseslist', {
                params
            })
    }

    getAnnualStatistics() {
        var self = this;
        return self.axios
            .get('/api/purchases/getannualstatistics')
    }

    createPurchase(body: any) {
        var self = this;
        return self.axios
            .post('/api/purchases/createpurchase', body)
    }

    cancelPurchase(id: any) {
        var self = this;
        return self.axios
            .post('/api/purchases/cancelpurchase', {}, {
                params: { id }
            })
    }
}
