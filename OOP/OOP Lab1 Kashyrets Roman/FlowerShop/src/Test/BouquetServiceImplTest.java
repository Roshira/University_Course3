package Test;

import Model.*;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import java.util.List;
import static org.junit.jupiter.api.Assertions.*;
import Service.BouquetService;
import Service.BouquetServiceImpl;

class BouquetServiceImplTest {

    private BouquetService bouquetService;
    private Bouquet bouquet;

    // Цей метод виконується перед кожним @Test
    @BeforeEach
    void setUp() {
        bouquetService = new BouquetServiceImpl();
        bouquet = new Bouquet();

        // Наповнюємо букет тестовими даними
        bouquet.addItem(new Rose(50.0, 8, 40.0, true));  // Свіжість 8, Довжина 40
        bouquet.addItem(new Tulip(30.0, 10, 30.0)); // Свіжість 10, Довжина 30
        bouquet.addItem(new Rose(20.0, 5, 35.0, false)); // Свіжість 5, Довжина 35
        bouquet.addItem(new Accessory(10.0, "Ribbon"));  // Не квітка
    }

    @Test
    void testCalculatePrice() {
        double expected = 50.0 + 30.0 + 20.0 + 10.0;
        double actual = bouquetService.calculatePrice(bouquet);
        assertEquals(expected, actual, "Price calculation is incorrect");
    }

    @Test
    void testSortFlowersByFreshness() {
        List<Flower> sortedList = bouquetService.sortFlowersByFreshness(bouquet);

        // Перевіряємо, що аксесуари не потрапили в список
        assertEquals(3, sortedList.size());

        // Перевіряємо порядок сортування (5, 8, 10)
        assertEquals(5, sortedList.get(0).getFreshnessLevel());
        assertEquals(8, sortedList.get(1).getFreshnessLevel());
        assertEquals(10, sortedList.get(2).getFreshnessLevel());
    }

    @Test
    void testFindFlowersByStemLength() {
        // Шукаємо квіти з довжиною стебла від 32 до 42 см
        List<Flower> foundList = bouquetService.findFlowersByStemLength(bouquet, 32.0, 42.0);

        // Має знайти Rose(40) і Rose(35)
        assertEquals(2, foundList.size());

        // Перевіряємо, що це саме ті квіти
        assertTrue(foundList.stream().anyMatch(f -> f.getStemLength() == 40.0));
        assertTrue(foundList.stream().anyMatch(f -> f.getStemLength() == 35.0));
    }

    @Test
    void testFindFlowersByStemLengthNoMatch() {
        // Шукаємо в діапазоні, де нічого немає
        List<Flower> foundList = bouquetService.findFlowersByStemLength(bouquet, 100.0, 200.0);
        assertTrue(foundList.isEmpty(), "Should find no flowers in this range");
    }

    @Test
    void testEmptyBouquet() {
        Bouquet emptyBouquet = new Bouquet();
        assertEquals(0.0, bouquetService.calculatePrice(emptyBouquet));
        assertTrue(bouquetService.sortFlowersByFreshness(emptyBouquet).isEmpty());
        assertTrue(bouquetService.findFlowersByStemLength(emptyBouquet, 0, 100).isEmpty());
    }
}