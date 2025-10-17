package main.java.model;


public class Rose extends Flower {
    // Можна додати унікальні властивості, напр. наявність шипів
    private boolean hasThorns;

    public Rose(double price, int freshnessLevel, double stemLength, boolean hasThorns) {
        super(price, "Rose", freshnessLevel, stemLength);
        this.hasThorns = hasThorns;
    }

    public boolean hasThorns() {
        return hasThorns;
    }
}