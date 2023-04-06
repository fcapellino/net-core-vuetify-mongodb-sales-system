namespace BasicSalesSystem.Web.Domain.Entities
{
    using AspNetCore.Identity.Mongo.Model;
    using MongoDB.Bson.Serialization.Attributes;

    public sealed class ApplicationUser : MongoUser
    {
        [BsonRequired]
        public string FullName { get; set; }

        [BsonRequired]
        public string Address { get; set; }

        [BsonRequired]
        public bool Enabled { get; set; }
    }
}
