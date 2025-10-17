package main.java.model;

/**
 * Абстрактний базовий клас для всіх товарів у магазині.
 * Демонструє інкапсуляцію (private fields) та є основою
 * для поліморфізму.
 */
public abstract class ShopItem {
    private double price;
    private String description;

    public ShopItem(double price, String description) {
        this.price = price;
        this.description = description;
    }

    // Геттери для доступу до інкапсульованих полів
    public double getPrice() {
        return price;
    }

    public String getDescription() {
        return description;
    }
}
