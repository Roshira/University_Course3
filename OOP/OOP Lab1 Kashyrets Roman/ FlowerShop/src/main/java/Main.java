package main.java; // Using your package structure

import main.java.model.*;
import main.java.service.BouquetService;
import main.java.service.BouquetServiceImpl;

import java.util.ArrayList;
import java.util.InputMismatchException;
import java.util.List;
import java.util.Scanner;
import java.util.Locale; // Import for Locale settings

public class Main {

    // A static list to act as our "in-store" catalog of available items
    private static List<ShopItem> availableItems = new ArrayList<>();

    public static void main(String[] args) {
        // --- 1. Initialization ---
        
        // Initialize the service that contains all business logic
        BouquetService bouquetService = new BouquetServiceImpl();
       
        Scanner scanner = new Scanner(System.in).useLocale(Locale.US);
        
        // Create an empty bouquet for the user to fill
        Bouquet bouquet = new Bouquet();

        // Load our "in-store" catalog with predefined items
        fillStock();

        // --- 2. Main Application Loop ---
        boolean running = true;
        while (running) {
            printMenu(); // Display the menu options
            int choice = readInt(scanner, "Your choice: "); // Safely read user's choice

            // Handle the user's choice
            switch (choice) {
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
                case 0:
                    running = false; // Exit the loop
                    System.out.println("Thank you for visiting!");
                    break;
                default:
                    System.out.println("Invalid choice. Please try again.");
            }
        }
        scanner.close(); // Close the scanner when the application exits
    }

    /**
     * Prints the main menu to the console.
     */
    private static void printMenu() {
        System.out.println("\n--- FLOWER SHOP MENU ---");
        System.out.println("1. View available items");
        System.out.println("2. Add item to bouquet");
        System.out.println("3. View my bouquet and its cost");
        System.out.println("4. Sort flowers in bouquet by freshness");
        System.out.println("5. Find flowers by stem length");
        System.out.println("0. Exit");
    }

    /**
     * Fills the 'availableItems' list with hardcoded flowers and accessories.
     * This simulates a database of items in the shop.
     */
    private static void fillStock() {
        availableItems.add(new Rose(50.5, 8, 40.0, true));
        availableItems.add(new Tulip(30.0, 10, 35.0));
        availableItems.add(new Rose(25.0, 5, 38.0, false));
        availableItems.add(new Accessory(15.0, "Red Ribbon"));
        availableItems.add(new Accessory(10.0, "Wrapping Paper"));
    }

    /**
     * Displays all items from the 'availableItems' catalog.
     */
    private static void showAvailableItems() {
        System.out.println("\n--- Available Items ---");
        for (int i = 0; i < availableItems.size(); i++) {
            ShopItem item = availableItems.get(i);
            // Print item with a 1-based index for user-friendliness
            System.out.printf("%d. %s - %.2f UAH\n", (i + 1), item.getDescription(), item.getPrice());
        }
    }

    /**
     * Handles the logic for adding a selected item to the user's bouquet.
     */
    private static void addItemToBouquet(Scanner scanner, Bouquet bouquet) {
        showAvailableItems(); // Show the list first
        int itemNumber = readInt(scanner, "Enter item number to add (or 0 to go back): ");

        // Validate user input (must be within the list bounds)
        if (itemNumber > 0 && itemNumber <= availableItems.size()) {
            // Get the item from catalog (adjusting for 0-based index)
            ShopItem selected = availableItems.get(itemNumber - 1);
            bouquet.addItem(selected);
            System.out.printf("'%s' has been added to your bouquet.\n", selected.getDescription());
        } else if (itemNumber != 0) {
            System.out.println("Invalid item number.");
        }
    }

    /**
     * Displays the current contents of the bouquet and calculates the total price.
     */
    private static void showBouquetDetails(Bouquet bouquet, BouquetService service) {
        System.out.println("\n--- Your Bouquet ---");
        List<ShopItem> items = bouquet.getItems();
        
        if (items.isEmpty()) {
            System.out.println("Your bouquet is empty.");
            return;
        }

        // List all items in the bouquet
        for (ShopItem item : items) {
            System.out.printf("- %s (Price: %.2f UAH)\n", item.getDescription(), item.getPrice());
        }
        System.out.println("--------------------");
        // Calculate and display the total price using the service
        System.out.printf("Total price: %.2f UAH\n", service.calculatePrice(bouquet));
    }

    /**
     * Sorts only the flowers in the bouquet by their freshness level and prints them.
     */
    private static void sortBouquetByFreshness(Bouquet bouquet, BouquetService service) {
        // The service logic filters out accessories and sorts only flowers
        List<Flower> sortedFlowers = service.sortFlowersByFreshness(bouquet);
        
        if (sortedFlowers.isEmpty()) {
            System.out.println("\nThere are no flowers in your bouquet to sort.");
            return;
        }

        System.out.println("\n--- Flowers sorted by freshness (least to most fresh) ---");
        for (Flower flower : sortedFlowers) {
            System.out.printf("- %s (Freshness: %d/10)\n",
                    flower.getDescription(), flower.getFreshnessLevel());
        }
    }

    /**
     * Interactively asks for a stem length range and finds matching flowers.
     */
    private static void findFlowersByStemLengthInteractive(Scanner scanner, Bouquet bouquet, BouquetService service) {
        System.out.println("\n--- Find flowers by stem length ---");

        // --- FIX 2: Check if there are flowers in the bouquet ---
        // This prevents searching an empty bouquet or a bouquet with only accessories.
        boolean hasFlowers = bouquet.getItems().stream().anyMatch(item -> item instanceof Flower);
        if (!hasFlowers) {
            System.out.println("Error: There are no flowers in your bouquet yet.");
            System.out.println("Please add flowers to the bouquet first (option 2).");
            return;
        }
        // --- End of fix 2 ---

        // Read the min/max range from the user
        double min = readDouble(scanner, "Enter MINIMUM length (cm) (e.g., 35.5): ");
        double max = readDouble(scanner, "Enter MAXIMUM length (cm) (e.g., 40.5): ");

        if (min > max) {
            System.out.println("Error: Minimum length cannot be greater than maximum.");
            return;
        }

        // Use the service to find matching flowers
        List<Flower> foundFlowers = service.findFlowersByStemLength(bouquet, min, max);

        if (foundFlowers.isEmpty()) {
            System.out.printf("No flowers found with stem length between %.1f and %.1f cm.\n", min, max);
            return;
        }

        // Print the results
        System.out.printf("--- Found flowers (%.1f - %.1f cm) --- \n", min, max);
        for (Flower flower : foundFlowers) {
            System.out.printf("- %s (Length: %.1f cm)\n",
                    flower.getDescription(), flower.getStemLength());
        }
    }

    /**
     * Helper method for safely reading an integer from the console.
     * It will loop until a valid integer is entered.
     */
    private static int readInt(Scanner scanner, String prompt) {
        while (true) { // Loop indefinitely until valid input is received
            try {
                System.out.print(prompt);
                return scanner.nextInt(); // Try to read an integer
            } catch (InputMismatchException e) {
                // This block executes if the user types something other than an integer
                System.out.println("Error: Please enter a whole number.");
                scanner.next(); // Clear the invalid input from the scanner's buffer
            }
        }
    }

    /**
     * Helper method for safely reading a double from the console.
     * It will loop until a valid double is entered.
     */
    private static double readDouble(Scanner scanner, String prompt) {
        while (true) {
            try {
                System.out.print(prompt);
                return scanner.nextDouble(); // Try to read a double
            } catch (InputMismatchException e) {
                // This block executes if the input is not a valid double
                // Thanks to Locale.US, this expects a dot (e.g., 35.5)
                System.out.println("Error: Please enter a number using a DOT (e.g., 35.5).");
                scanner.next(); // Clear the invalid input
            }
        }
    }
}