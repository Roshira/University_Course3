package main.java.model;

/**
 * Абстрактний клас для квітки, що успадковує ShopItem.
 * Додає специфічні властивості: рівень свіжості та довжину стебла.
 */
public abstract class Flower extends ShopItem {
    private int freshnessLevel;
    private double stemLength;

    public Flower(double price, String description, int freshnessLevel, double stemLength) {
        super(price, description);
        this.freshnessLevel = freshnessLevel;
        this.stemLength = stemLength;
    }

    public int getFreshnessLevel() {
        return freshnessLevel;
    }

    public double getStemLength() {
        return stemLength;
    }
}
