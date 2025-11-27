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

        // Для XML виводу можна додати додаткові налаштування
        transformer.setOutputProperty("indent", "yes");
        transformer.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", "4");

        transformer.transform(
                new StreamSource(new File(xmlFilePath)),
                new StreamResult(new File(outputFilePath))
        );
    }

    // Додатковий метод для трансформації в XML
    public static void transformToXML(String xmlFilePath, String xslFilePath, String outputFilePath) throws Exception {
        transform(xmlFilePath, xslFilePath, outputFilePath);
    }
}