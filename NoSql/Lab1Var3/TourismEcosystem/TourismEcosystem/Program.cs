using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json; // Add this using directive
using Microsoft.Extensions.FileProviders; // Add this if needed
using System.IO; // required for Directory and Path

using TourismEcosystem.Data.UnitOfWork;
using TourismEcosystem.Model;

// Створюємо конфігурацію
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()) // Use Directory.GetCurrentDirectory() instead of AppContext.BaseDirectory
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Отримуємо рядок підключення
string connectionString = config.GetConnectionString("DefaultConnection");
long currentUserId = 123; // ID користувача, що зайшов у систему

Console.WriteLine("Запуск комплексної транзакції...");

// 'await using' автоматично викличе DisposeAsync() в кінці (або при помилці),
// що закриє з'єднання.
await using var uow = new UnitOfWork(connectionString);

try
{
    // --- ОПЕРАЦІЯ 1: Отримати дані про тур (через View) ---
    Console.WriteLine("Шукаємо тур 'Київська спадщина'...");
    var kievTour = (await uow.Tours.GetAllAsync())
        .FirstOrDefault(t => t.Name == "Київська спадщина");

    if (kievTour == null)
    {
        Console.WriteLine("Тур не знайдено, створюємо новий...");

        // ВИПРАВЛЕНО:
        // Додайте обов'язкові поля, які очікує процедура
        kievTour = new Tour
        {
            Name = "Київська спадщина",
            Price = 150.00m,
            ProviderId = 1,    // <-- Вкажіть ID існуючого провайдера
            StartCityId = 1,   // <-- Вкажіть ID існуючого міста
            Description = "Історична екскурсія" // <-- Це поле також надсилається
        };

        // --- ОПЕРАЦІЯ 2: Створити тур (через SP) ---
        kievTour.TourId = await uow.Tours.CreateAsync(kievTour, currentUserId);
        Console.WriteLine($"Створено тур з ID: {kievTour.TourId}");
    }

    // --- ОПЕРАЦІЯ 3: Отримати дані про готель (через View) ---
    // (Припустимо, у вас є IAccommodationRepository)
    // var hotel = await uow.Accommodations.GetByIdAsync(42);
    // decimal hotelPrice = hotel.PricePerNight * 2;

    // (Поки що захардкодимо для прикладу)
    decimal hotelPrice = 200.00m;
    long hotelId = 42;


    // --- ОПЕРАЦІЯ 4: Створити бронювання (через SP, що інкапсулює UoW) ---
    Console.WriteLine("Створення бронювання...");
    var myBooking = new Booking
    {
        UserId = currentUserId,
        Items = new List<BookingItem>
        {
            // Елемент 1: Тур, який ми щойно знайшли/створили
            new()
            {
                Type = "tour",
                Id = kievTour.TourId,
                Price = kievTour.Price
            },
            // Елемент 2: Готель
            new()
            {
                Type = "acc",
                Id = hotelId,
                Price = hotelPrice,
                StartDate = new DateOnly(2025, 12, 20),
                EndDate = new DateOnly(2025, 12, 22)
            }
        }
    };

    long newBookingId = await uow.CreateBookingAsync(myBooking);
    Console.WriteLine($"Створено бронювання з ID: {newBookingId}");

    // --- ЗАВЕРШЕННЯ: Якщо все добре, комітимо транзакцію ---
    await uow.CommitAsync();

    Console.WriteLine("Транзакцію успішно завершено!");
}
catch (Exception ex)
{
    // --- ВІДКАТ: Якщо сталася помилка, відкочуємо всі зміни ---
    // (Створення туру І створення бронювання будуть відкочені)
    Console.WriteLine($"Сталася помилка: {ex.Message}");
    await uow.RollbackAsync();
    Console.WriteLine("Транзакцію відкочено.");
}