package Model;

public abstract class ShopItem {
    private double price;
    private String description;

    public ShopItem(double price, String description) {
        this.price = price;
        this.description = description;
    }

    public double getPrice() {
        return this.price;
    }

    public String getDescription() {
        return this.description;
    }
}
