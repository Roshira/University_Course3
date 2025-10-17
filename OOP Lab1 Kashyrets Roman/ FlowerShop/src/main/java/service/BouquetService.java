package main.java.service;

import main.java.model.Bouquet;
import main.java.model.Flower;
import java.util.List;

public interface BouquetService {

    /**
     * Розраховує загальну вартість букета.
     */
    double calculatePrice(Bouquet bouquet);

    /**
     * Повертає список квітів у букеті, відсортованих за рівнем свіжості.
     * (від найменш свіжих до найсвіжіших)
     */
    List<Flower> sortFlowersByFreshness(Bouquet bouquet);

    /**
     * Знаходить у букеті квіти, що відповідають заданому діапазону довжин стебел.
     */
    List<Flower> findFlowersByStemLength(Bouquet bouquet, double minLength, double maxLength);
}