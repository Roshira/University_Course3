package Model;


public abstract class Flower extends ShopItem {
    private int freshnessLevel;
    private double stemLength;

    public Flower(double price, String description, int freshnessLevel, double stemLength) {
        super(price, description);
        this.freshnessLevel = freshnessLevel;
        this.stemLength = stemLength;
    }

    public int getFreshnessLevel() {
        return this.freshnessLevel;
    }

    public double getStemLength() {
        return this.stemLength;
    }
}
