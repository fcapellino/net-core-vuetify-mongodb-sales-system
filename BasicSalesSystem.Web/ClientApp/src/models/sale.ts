import _ from 'lodash';
import { InvalidOperation } from '../common/transgression';
import { Utils } from '../common/utils';

export class Sale {
    public id?: string;
    public customer?: any;
    public receiptType?: string;
    public tax?: number;
    public date?: Date;
    public approved?: boolean;
    public details?: Array<SaleDetail>;

    constructor(item?: any) {
        this.id = item ? item.id : null;
        this.customer = item ? item.customer : null;
        this.receiptType = item ? item.receiptType.toUpperCase() : null;
        this.tax = item ? item.tax : 20;
        this.date = item ? item.date : new Date();
        this.approved = item ? item.approved : false;
        this.details = item
            ? item.details.map((d) => new SaleDetail(d))
            : new Array<SaleDetail>();
    }

    public get formattedDate() {
        var self = this;
        return Utils.formatDate(self.date);
    }

    public set formattedDate(value) {
        var self = this;
        self.date = Utils.parseDate(value);
    }

    public addNewItem(item) {
        var self = this;
        if (item) {
            var duplicated = self.details?.some(x => x.product.id === item.product.id);
            if (duplicated) {
                throw new InvalidOperation('Error. The product has already been added to the list.');
            }

            if (item.product.stock === 0) {
                throw new InvalidOperation(`Error. There are no units available for this product.`);
            }

            if (item.quantity > item.product.stock) {
                throw new InvalidOperation(`Error. There are only ${item.product.stock} units available for this product.`);
            }

            var purchaseDetail = Object.assign(new SaleDetail, item);
            self.details?.push(purchaseDetail);
            self.details = self.details?.sort(function (a, b) {
                return a.product.name.localeCompare(b.product.name)
            });
        }
    }

    public removeItem(item) {
        var self = this;
        var index = self.details?.indexOf(item);
        if (index! > -1) {
            self.details?.splice(index!, 1);
        }
    }

    public calculateTotal() {
        var self = this;
        return Utils.tryGetNumber(() => {
            var detailsSum = _.sumBy(self.details, (d) => { return Number(d.calculateSubtotal()); });
            return (detailsSum * (1 + (self.tax! / 100))).toFixed(2);
        });
    }
}

export class SaleDetail {
    public id?: string;
    public product?: any;
    public quantity?: number;
    public unitPrice?: number;
    public discount?: number;

    constructor(item?: any) {
        this.id = item ? item.id : null;
        this.product = item ? item.product : null;
        this.quantity = item ? item.quantity : 0;
        this.unitPrice = item ? item.unitPrice : 0;
        this.discount = item ? item.discount : 0;
    }

    public calculateSubtotal() {
        var self = this;
        return Utils.tryGetNumber(() => (self.quantity! * self.unitPrice! * (1 - (self.discount! / 100)))).toFixed(2);
    }
}
