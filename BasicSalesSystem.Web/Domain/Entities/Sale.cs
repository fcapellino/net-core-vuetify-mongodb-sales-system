namespace BasicSalesSystem.Web.Domain.Entities
{
    using BasicSalesSystem.Web.Domain.Common;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class Sale : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ClientId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRequired]
        public string ReceiptCode { get; set; }

        [BsonRequired]
        public string ReceiptNumber { get; set; }

        [BsonRequired]
        public string ReceiptType { get; set; }

        [BsonRequired]
        public string State { get; set; }

        [BsonRequired]
        public decimal Total { get; set; }

        [BsonRequired]
        public decimal Tribute { get; set; }

        [BsonRequired]
        public BsonDateTime Date { get; set; }
    }
}
