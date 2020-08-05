﻿namespace BasicSalesSystem.Web.Domain.Entities
{
    using BasicSalesSystem.Web.Domain.Common;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class Customer : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRequired]
        public string FullName { get; set; }

        [BsonRequired]
        public string Address { get; set; }

        [BsonRequired]
        public string PhoneNumber { get; set; }

        [BsonRequired]
        public string Email { get; set; }

        [BsonRequired]
        public int DocumentType { get; set; }

        [BsonRequired]
        public int DocumentNumber { get; set; }

        [BsonRequired]
        public bool Active { get; set; }
    }
}
