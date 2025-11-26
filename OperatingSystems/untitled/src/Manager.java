import java.io.*;
import java.util.Scanner;
import java.util.concurrent.*;

public class Manager {

    static class ProcessResult {
        Double value;
        String errorStatus;
        boolean isReady = false;
    }

    public static void main(String[] args) throws IOException {
        System.out.println("=== Manager System Started ===");

        ProcessBuilder pb1 = new ProcessBuilder("java", "Worker", "1");
        ProcessBuilder pb2 = new ProcessBuilder("java", "Worker", "2");

        Process p1 = pb1.start();
        Process p2 = pb2.start();

        PrintWriter out1 = new PrintWriter(new BufferedWriter(new OutputStreamWriter(p1.getOutputStream())), true);
        PrintWriter out2 = new PrintWriter(new BufferedWriter(new OutputStreamWriter(p2.getOutputStream())), true);

        Scanner in1 = new Scanner(p1.getInputStream());
        Scanner in2 = new Scanner(p2.getInputStream());

        Scanner console = new Scanner(System.in);
        ExecutorService executor = Executors.newCachedThreadPool();

        try {
            while (true) {
                System.out.print("\nEnter x (or 'q' to quit): ");
                String input = console.nextLine();
                if (input.equalsIgnoreCase("q")) break;

                double x;
                try {
                    x = Double.parseDouble(input);
                } catch (NumberFormatException e) {
                    System.out.println("Invalid number.");
                    continue;
                }

                out1.println(x);
                out2.println(x);

                final ProcessResult res1 = new ProcessResult();
                final ProcessResult res2 = new ProcessResult();

                Future<?> future1 = executor.submit(() -> readResult(in1, res1));
                Future<?> future2 = executor.submit(() -> readResult(in2, res2));

                // --- ЗМІННІ ДЛЯ СЕКУНДОМІРА ---
                long totalActiveTime = 0; // Накопичений час роботи (мс)
                long lastStartTime = System.currentTimeMillis(); // Час початку поточного циклу очікування

                boolean calculationFinished = false;
                long timeoutMillis = 2000; // Початковий час до появи меню

                while (!calculationFinished) {
                    try {
                        // Чекаємо завершення або таймауту
                        long waitStart = System.currentTimeMillis();
                        while (System.currentTimeMillis() - waitStart < timeoutMillis) {
                            if (res1.isReady && res2.isReady) {
                                calculationFinished = true;
                                break;
                            }
                            Thread.sleep(10);
                        }

                        // Оновлюємо таймер: додаємо час, який ми щойно прочекали
                        long now = System.currentTimeMillis();
                        totalActiveTime += (now - lastStartTime);

                        if (calculationFinished) break;

                        // Якщо ми тут — значить спрацював таймаут, показуємо меню.
                        // ТАЙМЕР ТУТ ФАКТИЧНО ЗУПИНЯЄТЬСЯ, бо ми не оновлюємо lastStartTime,
                        // поки користувач думає.

                        System.out.printf("\n--- Calculation is running for %.2f sec ---\n", totalActiveTime / 1000.0);
                        System.out.println("1. Continue (wait more)");
                        System.out.println("2. Continue until keypress (long wait)");
                        System.out.println("3. Show Status");
                        System.out.println("4. Cancel");
                        System.out.print("Choice: ");

                        String choice = console.nextLine(); // Блокування (час не йде в залік)

                        // Коли користувач щось ввів, ми "запускаємо" таймер знову
                        lastStartTime = System.currentTimeMillis();

                        switch (choice) {
                            case "1":
                                System.out.print("Add seconds: ");
                                try {
                                    int sec = Integer.parseInt(console.nextLine());
                                    timeoutMillis = sec * 1000L;
                                } catch (Exception e) { timeoutMillis = 5000; }
                                // Оновлюємо час старту ще раз, бо введення секунд теж зайняло час
                                lastStartTime = System.currentTimeMillis();
                                break;
                            case "2":
                                System.out.println("Waiting (keypress simulation)...");
                                timeoutMillis = 60000;
                                break;
                            case "3":
                                printStatus("fn1", res1);
                                printStatus("fn2", res2);
                                timeoutMillis = 1000; // Швидко повернутися в меню
                                break;
                            case "4":
                                future1.cancel(true);
                                future2.cancel(true);
                                System.out.println("Cancelled.");
                                calculationFinished = true;
                                break;
                            default:
                                System.out.println("Unknown choice. Continuing...");
                                timeoutMillis = 2000;
                        }

                    } catch (InterruptedException e) {
                        Thread.currentThread().interrupt();
                        break;
                    }
                }

                if (!future1.isCancelled() && !future2.isCancelled()) {
                    processFinalResults(x, res1, res2, totalActiveTime);
                }
            }

        } finally {
            out1.println("END");
            out2.println("END");
            p1.destroy();
            p2.destroy();
            executor.shutdownNow();
            System.out.println("System stopped.");
        }
    }

    private static void readResult(Scanner processScanner, ProcessResult target) {
        if (processScanner.hasNextLine()) {
            String line = processScanner.nextLine();
            if ("fail".equals(line)) target.errorStatus = "fail";
            else if ("undefined".equals(line)) target.errorStatus = "undefined";
            else {
                try { target.value = Double.parseDouble(line); }
                catch (Exception e) { target.errorStatus = "undefined"; }
            }
            target.isReady = true;
        }
    }

    private static void printStatus(String name, ProcessResult res) {
        System.out.println(name + ": " + (res.isReady ? "Finished" : "Running..."));
    }

    private static void processFinalResults(double x, ProcessResult r1, ProcessResult r2, long timeMs) {
        System.out.println("\n>>> Final Report for x = " + x);
        System.out.printf("Total Computation Time: %.2f seconds\n", timeMs / 1000.0);

        if (r1.errorStatus != null || r2.errorStatus != null) {
            System.out.println("Operation Failed (Soft Fail).");
            System.out.println("fn1: " + (r1.errorStatus != null ? r1.errorStatus : "OK"));
            System.out.println("fn2: " + (r2.errorStatus != null ? r2.errorStatus : "OK"));
        } else {
            double sum = r1.value + r2.value;
            System.out.printf("Result: %.4f + %.4f = %.4f\n", r1.value, r2.value, sum);
        }
        System.out.println("-------------------------------------------");
    }
}