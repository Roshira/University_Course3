package lab.parser;

// Оновлено імпорти
import lab.model.Candy;
import lab.model.CandyType;
import lab.model.ChocolateType;
import lab.model.IngredientsType; // Змінено
import lab.model.ValueType;       // Змінено

import javax.xml.stream.XMLEventReader;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.events.Attribute;
import javax.xml.stream.events.EndElement;
import javax.xml.stream.events.StartElement;
import javax.xml.stream.events.XMLEvent;
import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class StaxParser implements CandyParser {

    @Override
    public List<Candy> parse(String xmlFilePath) throws Exception {
        List<Candy> candies = new ArrayList<>();
        Candy currentCandy = null;
        IngredientsType currentIngredients = null; 
        ValueType currentValue = null;      
        String currentTagName = null;

        XMLInputFactory factory = XMLInputFactory.newInstance();

        factory.setProperty(XMLInputFactory.IS_SUPPORTING_EXTERNAL_ENTITIES, false);

        factory.setProperty(XMLInputFactory.SUPPORT_DTD, false);

        XMLEventReader eventReader = factory.createXMLEventReader(new FileInputStream(xmlFilePath));

        while (eventReader.hasNext()) {
            XMLEvent event = eventReader.nextEvent();

            if (event.isStartElement()) {
                StartElement startElement = event.asStartElement();
                currentTagName = startElement.getName().getLocalPart();

                switch (currentTagName) {
                    case "Candy":
                        currentCandy = new Candy();

                        Iterator<Attribute> attributes = startElement.getAttributes();
                        while (attributes.hasNext()) {
                            Attribute attribute = attributes.next();
                            String attrName = attribute.getName().getLocalPart();
                            if ("id".equals(attrName)) {
                                currentCandy.setId(attribute.getValue());
                            } else if ("production".equals(attrName)) {
                                currentCandy.setProduction(attribute.getValue());
                            }

                        }
                        break;
                    case "Ingredients":
                        currentIngredients = new IngredientsType();
                        break;
                    case "Value":
                        currentValue = new ValueType(); 
                        break;

                }
            } else if (event.isCharacters() && currentTagName != null) {
                String content = event.asCharacters().getData().trim();
                if (content.isEmpty()) {
                    continue;
                }

                switch (currentTagName) {
                    case "Name": currentCandy.setName(content); break;
                    case "Energy": currentCandy.setEnergy(Integer.parseInt(content)); break;
                    case "Type": currentCandy.setType(CandyType.fromValue(content)); break;
                    
                    case "Water": currentIngredients.setWater(Integer.parseInt(content)); break;
                    case "Sugar": currentIngredients.setSugar(Integer.parseInt(content)); break;
                    case "Fructose": currentIngredients.setFructose(Integer.parseInt(content)); break;
                    case "ChocolateType": 

                        if (currentIngredients != null && content != null && !content.isEmpty()) {
                             currentIngredients.setChocolateType(ChocolateType.fromValue(content)); 
                        }
                        break;
                    case "Vanillin": currentIngredients.setVanillin(Integer.parseInt(content)); break;

                    case "Proteins": currentValue.setProteins(Integer.parseInt(content)); break;
                    case "Fats": currentValue.setFats(Integer.parseInt(content)); break;
                    case "Carbohydrates": currentValue.setCarbohydrates(Integer.parseInt(content)); break; // Змінено
                }
            } else if (event.isEndElement()) {
                EndElement endElement = event.asEndElement();
                String endTagName = endElement.getName().getLocalPart();

                switch (endTagName) {
                    case "Candy":
                        candies.add(currentCandy);
                        currentCandy = null;
                        break;
                    case "Ingredients":
                        currentCandy.setIngredients(currentIngredients);
                        currentIngredients = null;
                        break;
                    case "Value":
                        currentCandy.setValue(currentValue);
                        currentValue = null;
                        break;
                }
                currentTagName = null;
            }
        }
        eventReader.close();
        return candies;
    }
}

