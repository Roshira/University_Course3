package lab.parser;

import lab.model.Candy;
import lab.model.CandyType;
// import lab.parser.DomParser;
// import lab.parser.JaxbParser;
// import lab.parser.SaxParser;
// import lab.parser.StaxParser;
// import lab.parser.CandyParser;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

import java.util.List;

public class parserTest {

    private static final String TEST_XML = "src/test/resources/lab/test_candies.xml";
    
    private static final int EXPECTED_COUNT = 2;
    private static final String EXPECTED_NAME_T1 = "Test Toffee";
    private static final int EXPECTED_ENERGY_T1 = 400;
    private static final CandyType EXPECTED_TYPE_T2 = CandyType.CHOCOLATE_WITH_FILLING; 
    private static final String EXPECTED_PROD_T2 = "TEST_FACTORY_B";


    @Test
    @DisplayName("DOM Parser Test")
    void testDomParser() {
        runParserTest(new DomParser(), "DOM");
    }

    @Test
    @DisplayName("SAX Parser Test")
    void testSaxParser() {
        runParserTest(new SaxParser(), "SAX");
    }

    @Test
    @DisplayName("StAX Parser Test")
    void testStaxParser() {
        runParserTest(new StaxParser(), "StAX");
    }

    @Test
    @DisplayName("JAXB Parser Test (User's original)")
    void testJaxbParser() {
        runParserTest(new JaxbParser(), "JAXB");
    }

    private void runParserTest(CandyParser parser, String type) {
        try {
            List<Candy> candies = parser.parse(TEST_XML);
            
            assertNotNull(candies, type + " parser returned null list");
            assertEquals(EXPECTED_COUNT, candies.size(), type + " parser found wrong number of candies");

            Candy candy1 = candies.get(0);
            assertEquals(EXPECTED_NAME_T1, candy1.getName(), type + " parser failed on Name");
            assertEquals(EXPECTED_ENERGY_T1, candy1.getEnergy(), type + " parser failed on Energy");
            assertEquals("t1", candy1.getId(), type + " parser failed on ID");

            Candy candy2 = candies.get(1);
            assertEquals(EXPECTED_TYPE_T2, candy2.getType(), type + " parser failed on Type");
            assertEquals(EXPECTED_PROD_T2, candy2.getProduction(), type + " parser failed on Production attribute");
            assertNotNull(candy2.getIngredients().getChocolateType(), type + " parser failed on optional ChocolateType");

        } catch (Exception e) {
            fail("Exception thrown during " + type + " parsing: " + e.getMessage(), e);
        }
    }
}

