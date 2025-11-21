package lab.sorter;

import lab.model.Candy;
import java.util.Comparator;

public class CandySorters {

    public static Comparator<Candy> byName() {
        return Comparator.comparing(Candy::getName);
    }

    public static Comparator<Candy> byEnergy() {
        return Comparator.comparingInt(Candy::getEnergy);
    }
    
    public static Comparator<Candy> byProduction() {
        return Comparator.comparing(Candy::getProduction);
    }
}