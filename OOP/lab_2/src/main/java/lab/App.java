package lab;

import lab.model.Candy;
import lab.parser.CandyParser;
import lab.parser.DomParser;
import lab.parser.JaxbParser;
import lab.parser.SaxParser;
import lab.parser.StaxParser;
import lab.sorter.CandySorters;
import lab.util.XMLTransformer;
import lab.util.XMLValidator;

import java.util.List;

public class App {

    private static final String XML_FILE = "src/main/resources/lab/candies.xml";
    private static final String XSD_FILE = "src/main/resources/lab/candies.xsd";
    private static final String XSL_FILE = "src/main/resources/lab/transform.xsl";
    private static final String HTML_OUTPUT = "candies_report.html";

    public static void main(String[] args) {
        System.out.println("--- XML Lab Application Start ---");

        System.out.println("\n--- 1. Validating XML ---");
        boolean isValid = XMLValidator.validate(XML_FILE, XSD_FILE);
        if (isValid) {
            System.out.println("Validation successful: " + XML_FILE + " conforms to " + XSD_FILE);
        } else {
            System.err.println("Validation failed. Check errors above.");
            return;
        }

        System.out.println("\n--- 2. Parsing XML ---");

        runParser(new DomParser(), "DOM");

        runParser(new SaxParser(), "SAX");

        List<Candy> staxResult = runParser(new StaxParser(), "StAX");

        runParser(new JaxbParser(), "JAXB");
   
        if (staxResult != null && !staxResult.isEmpty()) {
            System.out.println("\n--- 3. Sorting (using StAX result) ---");

            staxResult.sort(CandySorters.byName());
            System.out.println("\nSorted by Name (A-Z):");
            staxResult.forEach(c -> System.out.println("  " + c.getName() + " (Energy: " + c.getEnergy() + ")"));

            staxResult.sort(CandySorters.byEnergy());
            System.out.println("\nSorted by Energy (Low-High):");
            staxResult.forEach(c -> System.out.println("  " + c.getEnergy() + " kcal (" + c.getName() + ")"));
        }

        System.out.println("\n--- 4. Transforming XML to HTML ---");
        try {
            XMLTransformer.transform(XML_FILE, XSL_FILE, HTML_OUTPUT);
            System.out.println("Transformation complete. Output file: " + HTML_OUTPUT);
        } catch (Exception e) {
            System.err.println("Error during transformation: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println("\n--- XML Lab Application End ---");
    }

    private static List<Candy> runParser(CandyParser parser, String parserType) {
        System.out.println("\n--- Running " + parserType + " Parser ---");
        try {
            long startTime = System.currentTimeMillis();
            List<Candy> candies = parser.parse(XML_FILE);
            long endTime = System.currentTimeMillis();
            
            System.out.println(parserType + " parser finished in " + (endTime - startTime) + " ms. Found " + candies.size() + " candies.");
            candies.forEach(c -> System.out.println("  - Found: " + c.getName() + " (ID: " + c.getId() + ", Prod: " + c.getProduction() + ")"));
            return candies;
            
        } catch (Exception e) {
            System.err.println("Error running " + parserType + " parser: " + e.getMessage());
            e.printStackTrace();
            return null;
        }
    }
}

