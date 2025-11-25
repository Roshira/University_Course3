import java.io.*;
import java.util.Scanner;
import java.util.Random;

public class Worker {
    public static void main(String[] args) {
        // Отримуємо ID воркера (1 або 2), щоб трохи змінити логіку
        int workerId = args.length > 0 ? Integer.parseInt(args[0]) : 1;

        Scanner scanner = new Scanner(System.in);
        Random random = new Random();

        // Нескінченний цикл прийому завдань від Manager
        while (scanner.hasNextLine()) {
            String line = scanner.nextLine();
            if ("END".equals(line)) break; // Команда на завершення

            try {
                double x = Double.parseDouble(line);

                // --- ЕМУЛЯЦІЯ ОБЧИСЛЕННЯ ---
                // Затримка від 1 до 10 секунд
                int sleepTime = 1000 + random.nextInt(9000);

                // Іноді "зависаємо" надовго (для тестування меню), якщо число велике
                if (x > 100) sleepTime = 15000;

                Thread.sleep(sleepTime);

                // --- ЕМУЛЯЦІЯ РЕЗУЛЬТАТУ ---
                // Шанс на критичну помилку (Soft fail)
                if (random.nextDouble() < 0.1) {
                    System.out.println("fail"); // Повідомляємо про помилку
                } else {
                    // Обчислення залежно від ID (просто різні формули)
                    double result = (workerId == 1) ? (x * 2) : (x * x);
                    System.out.println(result);
                }

                // Важливо: скидаємо буфер, щоб Manager одразу отримав дані
                System.out.flush();

            } catch (NumberFormatException e) {
                System.out.println("undefined");
            } catch (InterruptedException e) {
                // Якщо процес вбили або перервали
                return;
            }
        }
    }
}