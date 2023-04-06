import _ from 'lodash';
import { InvalidOperation } from '../common/transgression';
import { Utils } from '../common/utils';

export class Purchase {
    public id?: string;
    public supplier?: any;
    public receiptType?: string;
    public tax?: number;
    public date?: Date;
    public approved?: boolean;
    public details?: Array<PurchaseDetail>;

    constructor(item?: any) {
        this.id = item ? item.id : null;
        this.supplier = item ? item.supplier : null;
        this.receiptType = item ? item.receiptType.toUpperCase() : null;
        this.tax = item ? item.tax : 20;
        this.date = item ? item.date : new Date();
        this.approved = item ? item.approved : false;
        this.details = item
            ? item.details.map((d) => new PurchaseDetail(d))
            : new Array<PurchaseDetail>();
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

            var purchaseDetail = Object.assign(new PurchaseDetail, item);
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

export class PurchaseDetail {
    public id?: string;
    public product?: any;
    public quantity?: number;
    public unitPrice?: number;

    constructor(item?: any) {
        this.id = item ? item.id : null;
        this.product = item ? item.product : null;
        this.quantity = item ? item.quantity : 0;
        this.unitPrice = item ? item.unitPrice : 0;
    }

    public calculateSubtotal() {
        var self = this;
        return Utils.tryGetNumber(() => (self.quantity! * self.unitPrice!).toFixed(2));
    }
}
