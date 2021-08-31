namespace BasicSalesSystem.Web.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using BasicSalesSystem.Web.Domain.Common;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class Purchase : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SupplierId { get; set; }

        [BsonRequired]
        public string ReceiptType { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Tax { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Total { get; set; }

        [BsonRequired]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }

        [BsonRequired]
        public bool Approved { get; set; }

        [BsonRequired]
        public IList<PurchaseDetail> Details { get; set; }
    }
}
