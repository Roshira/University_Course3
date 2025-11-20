using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TourismEcosystem.Model
{
    public class Booking
    {
        [JsonPropertyName("booking_id")]
        public long BookingId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        // Це поле НЕ зберігається в БД, а використовується для передачі в SP
        [JsonPropertyName("items")]
        public List<BookingItem> Items { get; set; } = new();
    }
}
