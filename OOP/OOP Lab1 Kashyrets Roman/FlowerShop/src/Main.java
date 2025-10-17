import java.util.ArrayList;
import java.util.InputMismatchException;
import java.util.Iterator;
import java.util.List;
import java.util.Locale;
import java.util.Scanner;
import Model.Accessory;
import Model.Bouquet;
import Model.Flower;
import Model.Rose;
import Model.ShopItem;
import Model.Tulip;
import Service.BouquetService;
import Service.BouquetServiceImpl;

public class Main {
    private static List<ShopItem> availableItems = new ArrayList();

    public Main() {
    }

    public static void main(String[] args) {
        BouquetService bouquetService = new BouquetServiceImpl();
        Scanner scanner = (new Scanner(System.in)).useLocale(Locale.US);
        Bouquet bouquet = new Bouquet();
        fillStock();
        boolean running = true;

        while(running) {
            printMenu();
            int choice = readInt(scanner, "Your choice: ");
            switch (choice) {
                case 0:
                    running = false;
                    System.out.println("Thank you for visiting!");
                    break;
                case 1:
                    showAvailableItems();
                    break;
                case 2:
                    addItemToBouquet(scanner, bouquet);
                    break;
                case 3:
                    showBouquetDetails(bouquet, bouquetService);
                    break;
                case 4:
                    sortBouquetByFreshness(bouquet, bouquetService);
                    break;
                case 5:
                    findFlowersByStemLengthInteractive(scanner, bouquet, bouquetService);
                    break;
                default:
                    System.out.println("Invalid choice. Please try again.");
            }
        }

        scanner.close();
    }

    private static void printMenu() {
        System.out.println("\n--- FLOWER SHOP MENU ---");
        System.out.println("1. View available items");
        System.out.println("2. Add item to bouquet");
        System.out.println("3. View my bouquet and its cost");
        System.out.println("4. Sort flowers in bouquet by freshness");
        System.out.println("5. Find flowers by stem length");
        System.out.println("0. Exit");
    }

    private static void fillStock() {
        availableItems.add(new Rose(50.5, 8, 40.0, true));
        availableItems.add(new Tulip(30.0, 10, 35.0));
        availableItems.add(new Rose(25.0, 5, 38.0, false));
        availableItems.add(new Accessory(15.0, "Red Ribbon"));
        availableItems.add(new Accessory(10.0, "Wrapping Paper"));
    }

    private static void showAvailableItems() {
        System.out.println("\n--- Available Items ---");

        for(int i = 0; i < availableItems.size(); ++i) {
            ShopItem item = (ShopItem)availableItems.get(i);
            System.out.printf("%d. %s - %.2f UAH\n", i + 1, item.getDescription(), item.getPrice());
        }

    }

    private static void addItemToBouquet(Scanner scanner, Bouquet bouquet) {
        showAvailableItems();
        int itemNumber = readInt(scanner, "Enter item number to add (or 0 to go back): ");
        if (itemNumber > 0 && itemNumber <= availableItems.size()) {
            ShopItem selected = (ShopItem)availableItems.get(itemNumber - 1);
            bouquet.addItem(selected);
            System.out.printf("'%s' has been added to your bouquet.\n", selected.getDescription());
        } else if (itemNumber != 0) {
            System.out.println("Invalid item number.");
        }

    }

    private static void showBouquetDetails(Bouquet bouquet, BouquetService service) {
        System.out.println("\n--- Your Bouquet ---");
        List<ShopItem> items = bouquet.getItems();
        if (items.isEmpty()) {
            System.out.println("Your bouquet is empty.");
        } else {
            Iterator var4 = items.iterator();

            while(var4.hasNext()) {
                ShopItem item = (ShopItem)var4.next();
                System.out.printf("- %s (Price: %.2f UAH)\n", item.getDescription(), item.getPrice());
            }

            System.out.println("--------------------");
            System.out.printf("Total price: %.2f UAH\n", service.calculatePrice(bouquet));
        }
    }

    private static void sortBouquetByFreshness(Bouquet bouquet, BouquetService service) {
        List<Flower> sortedFlowers = service.sortFlowersByFreshness(bouquet);
        if (sortedFlowers.isEmpty()) {
            System.out.println("\nThere are no flowers in your bouquet to sort.");
        } else {
            System.out.println("\n--- Flowers sorted by freshness (least to most fresh) ---");
            Iterator var4 = sortedFlowers.iterator();

            while(var4.hasNext()) {
                Flower flower = (Flower)var4.next();
                System.out.printf("- %s (Freshness: %d/10)\n", flower.getDescription(), flower.getFreshnessLevel());
            }

        }
    }

    private static void findFlowersByStemLengthInteractive(Scanner scanner, Bouquet bouquet, BouquetService service) {
        System.out.println("\n--- Find flowers by stem length ---");
        boolean hasFlowers = bouquet.getItems().stream().anyMatch((item) -> {
            return item instanceof Flower;
        });
        if (!hasFlowers) {
            System.out.println("Error: There are no flowers in your bouquet yet.");
            System.out.println("Please add flowers to the bouquet first (option 2).");
        } else {
            double min = readDouble(scanner, "Enter MINIMUM length (cm) (e.g., 35.5): ");
            double max = readDouble(scanner, "Enter MAXIMUM length (cm) (e.g., 40.5): ");
            if (min > max) {
                System.out.println("Error: Minimum length cannot be greater than maximum.");
            } else {
                List<Flower> foundFlowers = service.findFlowersByStemLength(bouquet, min, max);
                if (foundFlowers.isEmpty()) {
                    System.out.printf("No flowers found with stem length between %.1f and %.1f cm.\n", min, max);
                } else {
                    System.out.printf("--- Found flowers (%.1f - %.1f cm) --- \n", min, max);
                    Iterator var10 = foundFlowers.iterator();

                    while(var10.hasNext()) {
                        Flower flower = (Flower)var10.next();
                        System.out.printf("- %s (Length: %.1f cm)\n", flower.getDescription(), flower.getStemLength());
                    }

                }
            }
        }
    }

    private static int readInt(Scanner scanner, String prompt) {
        while(true) {
            try {
                System.out.print(prompt);
                return scanner.nextInt();
            } catch (InputMismatchException var3) {
                System.out.println("Error: Please enter a whole number.");
                scanner.next();
            }
        }
    }

    private static double readDouble(Scanner scanner, String prompt) {
        while(true) {
            try {
                System.out.print(prompt);
                return scanner.nextDouble();
            } catch (InputMismatchException var3) {
                System.out.println("Error: Please enter a number using a DOT (e.g., 35.5).");
                scanner.next();
            }
        }
    }
}
