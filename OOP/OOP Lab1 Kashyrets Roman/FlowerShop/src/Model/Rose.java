package Model;

public class Rose extends Flower {
    private boolean hasThorns;

    public Rose(double price, int freshnessLevel, double stemLength, boolean hasThorns) {
        super(price, "Rose", freshnessLevel, stemLength);
        this.hasThorns = hasThorns;
    }

    public boolean hasThorns() {
        return this.hasThorns;
    }
}

