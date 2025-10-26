using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TourismEcosystem.Model
{
    public class Tour
    {
        [JsonPropertyName("tour_id")]
        public long TourId { get; set; }
        [JsonPropertyName("provider_id")]
        public long ProviderId { get; set; }
        [JsonPropertyName("start_city_id")]
        public long StartCityId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("average_rating")]
        public decimal AverageRating { get; set; }
        // ... інші поля з v_active_tours_with_rating
    }
}
