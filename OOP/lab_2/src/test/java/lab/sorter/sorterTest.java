package lab.sorter;

import lab.model.Candy;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

import java.util.ArrayList;
import java.util.List;


public class sorterTest {

    private List<Candy> candies;
    private Candy candyA; // Name: Alpha, Energy: 300
    private Candy candyB; // Name: Beta,  Energy: 100
    private Candy candyC; // Name: Gamma, Energy: 200

    @BeforeEach
    void setUp() {

        candyA = new Candy();
        candyA.setName("Alpha");
        candyA.setEnergy(300);
        candyA.setProduction("Prod_A");

        candyB = new Candy();
        candyB.setName("Beta");
        candyB.setEnergy(100);
        candyB.setProduction("Prod_C");

        candyC = new Candy();
        candyC.setName("Gamma");
        candyC.setEnergy(200);
        candyC.setProduction("Prod_B");

        candies = new ArrayList<>();
        candies.add(candyA); // Alpha
        candies.add(candyB); // Beta
        candies.add(candyC); // Gamma
    }

    @Test
    @DisplayName("Sort by Name (A-Z)")
    void testSortByName() {
        candies.sort(CandySorters.byName());
        
        assertAll(
            () -> assertEquals("Alpha", candies.get(0).getName()),
            () -> assertEquals("Beta", candies.get(1).getName()),
            () -> assertEquals("Gamma", candies.get(2).getName())
        );
    }
    
    @Test
    @DisplayName("Sort by Energy (Low-High)")
    void testSortByEnergy() {
        candies.sort(CandySorters.byEnergy());
        
        assertAll(
            () -> assertEquals(100, candies.get(0).getEnergy()), // Beta
            () -> assertEquals(200, candies.get(1).getEnergy()), // Gamma
            () -> assertEquals(300, candies.get(2).getEnergy())  // Alpha
        );
    }

    @Test
    @DisplayName("Sort by Production (A-Z)")
    void testSortByProduction() {
        candies.sort(CandySorters.byProduction());
        
        assertAll(
            () -> assertEquals("Prod_A", candies.get(0).getProduction()), // Alpha
            () -> assertEquals("Prod_B", candies.get(1).getProduction()), // Gamma
            () -> assertEquals("Prod_C", candies.get(2).getProduction())  // Beta
        );
    }

    @Test
    @DisplayName("Sort by Name (Reversed Z-A)")
    void testSortByNameReversed() {
        candies.sort(CandySorters.byName().reversed());
        
        assertAll(
            () -> assertEquals("Gamma", candies.get(0).getName()), // Очікуємо Gamma
            () -> assertEquals("Beta", candies.get(1).getName()),
            () -> assertEquals("Alpha", candies.get(2).getName())  // Очікуємо Alpha
        );
    }
}

