package Service;


import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;
import Model.Bouquet;
import Model.Flower;
import Model.ShopItem;

public class BouquetServiceImpl implements BouquetService {
    public BouquetServiceImpl() {
    }

    public double calculatePrice(Bouquet bouquet) {
        return bouquet.getItems().stream().mapToDouble(ShopItem::getPrice).sum();
    }

    public List<Flower> sortFlowersByFreshness(Bouquet bouquet) {
        return (List)bouquet.getItems().stream().filter((item) -> {
            return item instanceof Flower;
        }).map((item) -> {
            return (Flower)item;
        }).sorted(Comparator.comparingInt(Flower::getFreshnessLevel)).collect(Collectors.toList());
    }

    public List<Flower> findFlowersByStemLength(Bouquet bouquet, double minLength, double maxLength) {
        return (List)bouquet.getItems().stream().filter((item) -> {
            return item instanceof Flower;
        }).map((item) -> {
            return (Flower)item;
        }).filter((flower) -> {
            return flower.getStemLength() >= minLength && flower.getStemLength() <= maxLength;
        }).collect(Collectors.toList());
    }
}
