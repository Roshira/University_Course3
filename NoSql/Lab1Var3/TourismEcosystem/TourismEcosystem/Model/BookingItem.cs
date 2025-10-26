using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TourismEcosystem.Model
{
    public class BookingItem
    {
        // "acc", "tour", "trans"
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("start_date")]
        public DateOnly? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateOnly? EndDate { get; set; }
    }
}
