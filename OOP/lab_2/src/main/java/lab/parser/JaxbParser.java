package lab.parser;

import lab.model.Candies;
import lab.model.Candy;
// Оновлено імпорти з jakarta на javax
import javax.xml.bind.JAXBContext;
import javax.xml.bind.Unmarshaller;
import java.io.File;
import java.util.List;

public class JaxbParser implements CandyParser {
    @Override
    public List<Candy> parse(String xmlFilePath) throws Exception {
        JAXBContext jaxbContext = JAXBContext.newInstance(Candies.class);
        Unmarshaller unmarshaller = jaxbContext.createUnmarshaller();
        File xmlFile = new File(xmlFilePath);
        if (!xmlFile.exists()) {
            throw new Exception("File not found: " + xmlFilePath);
        }
        Candies candies = (Candies) unmarshaller.unmarshal(xmlFile);
        return candies.getCandy();
    }
}

