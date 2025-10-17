package main.java.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Клас, що представляє букет.
 * Використовує композицію (містить список ShopItem).
 * Демонструє інкапсуляцію, не даючи прямого доступу до списку.
 */
public class Bouquet {
    private final List<ShopItem> items;

    public Bouquet() {
        this.items = new ArrayList<>();
    }

    public void addItem(ShopItem item) {
        if (item != null) {
            this.items.add(item);
        }
    }

    /**
     * Повертає НЕМОДИФІКОВАНУ копію списку товарів,
     * щоб захистити внутрішній стан букета (інкапсуляція).
     */
    public List<ShopItem> getItems() {
        return Collections.unmodifiableList(items);
    }
}

