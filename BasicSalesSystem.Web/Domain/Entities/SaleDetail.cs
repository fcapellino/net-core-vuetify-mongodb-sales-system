using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BasicSalesSystem.Web.Domain.Entities
{
    public class SaleDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SaleId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }

        [BsonRequired]
        public int Quantity { get; set; }

        [BsonRequired]
        public decimal UnitPrice { get; set; }

        [BsonRequired]
        public decimal Discount { get; set; }
    }
}
