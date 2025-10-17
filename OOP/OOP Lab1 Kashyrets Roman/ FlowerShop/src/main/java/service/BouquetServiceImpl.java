package main.java.service;

import main.java.model.Bouquet;
import main.java.model.Flower;
import main.java.model.ShopItem;
import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;

public class BouquetServiceImpl implements BouquetService {

    @Override
    public double calculatePrice(Bouquet bouquet) {
        // Демонстрація поліморфізму:
        // Ми викликаємо getPrice() у кожного ShopItem, не знаючи,
        // чи це Rose, Tulip, чи Accessory.
        return bouquet.getItems().stream()
                .mapToDouble(ShopItem::getPrice)
                .sum();
    }

    @Override
    public List<Flower> sortFlowersByFreshness(Bouquet bouquet) {
        return bouquet.getItems().stream()
                .filter(item -> item instanceof Flower) // Обираємо тільки квіти
                .map(item -> (Flower) item)            // Перетворюємо тип
                .sorted(Comparator.comparingInt(Flower::getFreshnessLevel))
                .collect(Collectors.toList());
    }

    @Override
    public List<Flower> findFlowersByStemLength(Bouquet bouquet, double minLength, double maxLength) {
        return bouquet.getItems().stream()
                .filter(item -> item instanceof Flower)
                .map(item -> (Flower) item)
                .filter(flower -> flower.getStemLength() >= minLength 
                                  && flower.getStemLength() <= maxLength)
                .collect(Collectors.toList());
    }
}