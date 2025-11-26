import java.io.*;
import java.util.Scanner;
import java.util.Random;

public class Worker {
    public static void main(String[] args) {
        int workerId = args.length > 0 ? Integer.parseInt(args[0]) : 1;
        Scanner scanner = new Scanner(System.in);
        Random random = new Random();

        while (scanner.hasNextLine()) {
            String line = scanner.nextLine();
            if ("END".equals(line)) break;

            try {
                double x = Double.parseDouble(line);

                // --- ЛОГІКА ЧАСУ ---
                // Якщо число > 100: "думаємо" 15 секунд (тест меню)
                // Якщо число <= 100: "думаємо" 0.5 - 1.5 секунди (швидкий результат)
                int sleepTime;
                if (x > 100) {
                    sleepTime = 15000;
                } else {
                    sleepTime = 500 + random.nextInt(1000);
                }

                Thread.sleep(sleepTime);

                // --- ОБЧИСЛЕННЯ (Без помилок) ---
                double result = (workerId == 1) ? (x * 2) : (x * x);
                System.out.println(result);
                System.out.flush();

            } catch (Exception e) {
                System.out.println("undefined");
            }
        }
    }
}