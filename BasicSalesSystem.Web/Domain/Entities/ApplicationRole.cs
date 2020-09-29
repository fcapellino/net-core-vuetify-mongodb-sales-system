namespace BasicSalesSystem.Web.Domain.Entities
{
    using AspNetCore.Identity.Mongo.Model;
    using MongoDB.Bson.Serialization.Attributes;

    public class ApplicationRole : MongoRole
    {
        [BsonRequired]
        public string Description { get; set; }

        [BsonRequired]
        public bool Active { get; set; }
    }
}
