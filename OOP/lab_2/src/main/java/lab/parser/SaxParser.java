package lab.parser;

import java.io.File;
import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.List;

import javax.xml.parsers.SAXParser;
import javax.xml.parsers.SAXParserFactory;

import org.xml.sax.Attributes;
import org.xml.sax.helpers.DefaultHandler;

import lab.model.Candy;
import lab.model.CandyType;
import lab.model.ChocolateType;
import lab.model.IngredientsType;
import lab.model.ValueType;

public class SaxParser implements CandyParser {

    @Override
    public List<Candy> parse(String xmlFilePath) throws Exception {
        SAXParserFactory factory = SAXParserFactory.newInstance();
        factory.setNamespaceAware(true); 
        SAXParser saxParser = factory.newSAXParser();
        CandyHandler handler = new CandyHandler();
        saxParser.parse(new FileInputStream(new File(xmlFilePath)), handler);
        return handler.getCandies();
    }

    private static class CandyHandler extends DefaultHandler {
        private List<Candy> candies;
        private Candy currentCandy;
        private IngredientsType currentIngredients;
        private ValueType currentValue;
        private StringBuilder elementValue;

        public List<Candy> getCandies() {
            return candies;
        }

        @Override
        public void startDocument() {
            candies = new ArrayList<>();
        }

        @Override
        public void startElement(String uri, String localName, String qName, Attributes attributes) {
            elementValue = new StringBuilder();

            if ("Candy".equals(localName)) {
                currentCandy = new Candy();
                currentCandy.setId(attributes.getValue("id"));
                currentCandy.setProduction(attributes.getValue("production"));
            } else if ("Ingredients".equals(localName)) {
                currentIngredients = new IngredientsType();
            } else if ("Value".equals(localName)) {
                currentValue = new ValueType();
            }
        }

        @Override
        public void characters(char[] ch, int start, int length) {
            elementValue.append(ch, start, length);
        }

        @Override
        public void endElement(String uri, String localName, String qName) {
            String value = elementValue.toString().trim();
            if (currentCandy == null) return;

            switch (localName) {
                case "Name":
                    currentCandy.setName(value);
                    break;
                case "Energy":
                    currentCandy.setEnergy(Integer.parseInt(value));
                    break;
                case "Type":
                    currentCandy.setType(CandyType.fromValue(value));
                    break;
                
                case "Water":
                    currentIngredients.setWater(Integer.parseInt(value));
                    break;
                case "Sugar":
                    currentIngredients.setSugar(Integer.parseInt(value));
                    break;
                case "Fructose":
                    currentIngredients.setFructose(Integer.parseInt(value));
                    break;
                case "ChocolateType":
                    if (value != null && !value.isEmpty()) {
                         currentIngredients.setChocolateType(ChocolateType.fromValue(value));
                    }
                    break;
                case "Vanillin":
                    currentIngredients.setVanillin(Integer.parseInt(value));
                    break;
                case "Ingredients":
                    currentCandy.setIngredients(currentIngredients);
                    break;

                case "Proteins":
                    currentValue.setProteins(Integer.parseInt(value));
                    break;
                case "Fats":
                    currentValue.setFats(Integer.parseInt(value));
                    break;
                case "Carbohydrates":
                    currentValue.setCarbohydrates(Integer.parseInt(value));
                    break;
                case "Value":
                    currentCandy.setValue(currentValue);
                    break;

                case "Candy":
                    if (candies != null) { 
                        candies.add(currentCandy);
                    }
                    break;
            }
        }
    }
}

