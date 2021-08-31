namespace BasicSalesSystem.Web.Domain.Entities
{
    using BasicSalesSystem.Web.Domain.Common;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class PurchaseDetail : IEntity
    {
        public PurchaseDetail()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }

        [BsonRequired]
        public int Quantity { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal UnitPrice { get; set; }
    }
}
