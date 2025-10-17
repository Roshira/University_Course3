package Model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class Bouquet {
    private final List<ShopItem> items = new ArrayList();

    public Bouquet() {
    }

    public void addItem(ShopItem item) {
        if (item != null) {
            this.items.add(item);
        }

    }

    public List<ShopItem> getItems() {
        return Collections.unmodifiableList(this.items);
    }
}

