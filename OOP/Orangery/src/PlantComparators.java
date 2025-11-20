import java.util.Comparator;

public class PlantComparators {

    public static Comparator<Plant> byName() {
        return Comparator.comparing(Plant::getName);
    }

    public static Comparator<Plant> byOrigin() {
        return Comparator.comparing(Plant::getOrigin);
    }

    public static Comparator<Plant> byTemperature() {
        return Comparator.comparing(plant -> plant.getGrowingTips().getTemperature());
    }

    public static Comparator<Plant> byWatering() {
        return Comparator.comparing(plant -> plant.getGrowingTips().getWatering());
    }

    public static Comparator<Plant> byAverageSize() {
        return Comparator.comparing(plant -> plant.getVisualParameters().getAverageSize());
    }
}