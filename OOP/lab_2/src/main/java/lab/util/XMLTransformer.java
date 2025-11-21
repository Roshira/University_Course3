package lab.util;

import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;

public class XMLTransformer {
    public static void transform(String xmlFilePath, String xslFilePath, String outputFilePath) throws Exception {
        TransformerFactory factory = TransformerFactory.newInstance();
        Transformer transformer = factory.newTransformer(new StreamSource(new File(xslFilePath)));
        transformer.transform(new StreamSource(new File(xmlFilePath)), new StreamResult(new File(outputFilePath)));
    }
}