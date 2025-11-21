package lab.util;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

import java.io.File;
import java.nio.file.Files;

public class utilTest {

    private static final String VALID_XML = "src/test/resources/lab/test_candies.xml";
 
    private static final String INVALID_XML = "src/test/resources/lab/test_invalid.xml";
    private static final String XSD = "src/main/resources/lab/candies.xsd";
    private static final String XSL = "src/main/resources/lab/transform.xsl";

    @Test
    @DisplayName("XML Validator Test - Valid")
    void testValidatorValid() {
        boolean isValid = XMLValidator.validate(VALID_XML, XSD);
        assertTrue(isValid, "Validator incorrectly flagged a valid XML as invalid");
    }

    @Test
    @DisplayName("XML Validator Test - Invalid")
    void testValidatorInvalid() {
        boolean isValid = XMLValidator.validate(INVALID_XML, XSD);
        assertFalse(isValid, "Validator incorrectly flagged an invalid XML as valid");
    }

    @Test
    @DisplayName("XML Transformer Test")
    void testTransformer() {
        String outputHtml = "test_report.html";
        File outputFile = new File(outputHtml);
        
        try {
            XMLTransformer.transform(VALID_XML, XSL, outputHtml);
            
            assertTrue(outputFile.exists(), "Transformer did not create the output HTML file");
            assertTrue(outputFile.length() > 0, "Transformer created an empty HTML file");
            
            String content = new String(Files.readAllBytes(outputFile.toPath()));
            assertTrue(content.contains("Test Toffee"), "HTML output does not contain expected data");

        } catch (Exception e) {
            fail("Transformer threw an exception: " + e.getMessage(), e);
        } finally {
            if (outputFile.exists()) {
                outputFile.delete();
            }
        }
    }
}
