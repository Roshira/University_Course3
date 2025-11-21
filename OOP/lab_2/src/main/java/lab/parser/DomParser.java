package lab.parser;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import lab.model.Candy;
import lab.model.CandyType;
import lab.model.ChocolateType;
import lab.model.IngredientsType;
import lab.model.ValueType;

public class DomParser implements CandyParser {
    private static final String CANDY_NAMESPACE_URI = "http://www.example.com/candies";

    @Override
    public List<Candy> parse(String xmlFilePath) throws Exception {
        List<Candy> candies = new ArrayList<>();
        
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        
        factory.setNamespaceAware(true); 
        
        DocumentBuilder builder = factory.newDocumentBuilder();
        Document doc = builder.parse(new File(xmlFilePath));
        doc.getDocumentElement().normalize();

        NodeList candyNodes = doc.getElementsByTagNameNS(CANDY_NAMESPACE_URI, "Candy");

        for (int i = 0; i < candyNodes.getLength(); i++) {
            Node node = candyNodes.item(i);
            if (node.getNodeType() == Node.ELEMENT_NODE) {
                Element candyElement = (Element) node;
                
                Candy candy = new Candy();
                candy.setId(candyElement.getAttribute("id"));
                candy.setProduction(candyElement.getAttribute("production"));


                candy.setName(getElementTextByTagName(candyElement, "Name"));
                candy.setEnergy(Integer.parseInt(getElementTextByTagName(candyElement, "Energy")));
                candy.setType(CandyType.fromValue(getElementTextByTagName(candyElement, "Type")));

                Element ingredientsEl = (Element) candyElement.getElementsByTagNameNS(CANDY_NAMESPACE_URI, "Ingredients").item(0);
                IngredientsType ingredients = new IngredientsType();
                ingredients.setWater(Integer.parseInt(getElementTextByTagName(ingredientsEl, "Water")));
                ingredients.setSugar(Integer.parseInt(getElementTextByTagName(ingredientsEl, "Sugar")));
                ingredients.setFructose(Integer.parseInt(getElementTextByTagName(ingredientsEl, "Fructose")));
                ingredients.setVanillin(Integer.parseInt(getElementTextByTagName(ingredientsEl, "Vanillin")));
                
                String chocoType = getElementTextByTagName(ingredientsEl, "ChocolateType");
                if (chocoType != null && !chocoType.isEmpty()) {
                    ingredients.setChocolateType(ChocolateType.fromValue(chocoType));
                }
                candy.setIngredients(ingredients);

                Element valueEl = (Element) candyElement.getElementsByTagNameNS(CANDY_NAMESPACE_URI, "Value").item(0);
                ValueType value = new ValueType();
                value.setProteins(Integer.parseInt(getElementTextByTagName(valueEl, "Proteins")));
                value.setFats(Integer.parseInt(getElementTextByTagName(valueEl, "Fats")));
                value.setCarbohydrates(Integer.parseInt(getElementTextByTagName(valueEl, "Carbohydrates")));
                candy.setValue(value);
                
                candies.add(candy);
            }
        }
        return candies;
    }

    private String getElementTextByTagName(Element parent, String localName) {
        NodeList nodeList = parent.getElementsByTagNameNS(parent.getNamespaceURI(), localName);
        if (nodeList.getLength() > 0) {
            return nodeList.item(0).getTextContent();
        }
        return ""; 
    }
}

