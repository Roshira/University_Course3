using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TourismEcosystem.Model.NoSql
{
    public class TourDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("sql_tour_id")]
        public long SqlTourId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        // --- ДОДАЙТЕ ЦЮ ВЛАСТИВІСТЬ ---
        [BsonElement("price")]
        public decimal Price { get; set; }
        // -------------------------------

        [BsonElement("attributes")]
        public Dictionary<string, object> FlexibleAttributes { get; set; } = new();

        [BsonElement("reviews")]
        public List<ReviewDocument> Reviews { get; set; } = new();
    }

    public class ReviewDocument
    {
        [BsonElement("user_name")]
        public string UserName { get; set; }

        [BsonElement("rating")]
        public int Rating { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}