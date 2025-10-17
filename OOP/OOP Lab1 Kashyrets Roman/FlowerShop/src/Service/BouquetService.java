package Service;

import java.util.List;
import Model.Bouquet;
import Model.Flower;

public interface BouquetService {
    double calculatePrice(Bouquet var1);

    List<Flower> sortFlowersByFreshness(Bouquet var1);

    List<Flower> findFlowersByStemLength(Bouquet var1, double var2, double var4);
}

