using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Text; // <--- ДОДАНО ДЛЯ ЗАПИСУ ФАЙЛІВ
using MathNet.Numerics.LinearAlgebra;

namespace Lab2Solver
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- Заголовок ---
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Лабораторна робота №2: Пошук лінійної моделі ---");
            Console.WriteLine("--- Формула: Y = R * X  =>  R = Y * X+ ---");
            Console.WriteLine(new string('-', 60));
            Console.ResetColor();

            try
            {
                // --- 1. Завантаження X ---
                Console.WriteLine("[КРОК 1/7] Завантаження вхідного зображення 'x1.bmp'...");
                Console.WriteLine("  > Читання файлу у FileStream...");
                Bitmap imgX_orig;
                using (FileStream fsX = new FileStream("x1.bmp", FileMode.Open, FileAccess.Read))
                using (Bitmap tempX = new Bitmap(fsX))
                {
                    Console.WriteLine($"  > Файл 'x1.bmp' завантажено (Оригінальний формат: {tempX.PixelFormat}).");
                    Console.WriteLine("  > Конвертація у стандартний формат 24bppRgb для сумісності...");
                    imgX_orig = new Bitmap(tempX.Width, tempX.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(imgX_orig))
                    {
                        g.DrawImage(tempX, 0, 0);
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[КРОК 1/7] Вхідне зображення 'x1.bmp' успішно завантажено.\n");
                Console.ResetColor();

                // --- 2. Завантаження Y ---
                Console.WriteLine("[КРОК 2/7] Завантаження вихідного зображення 'y4.bmp'...");
                Console.WriteLine("  > Читання файлу у FileStream...");
                Bitmap imgY;
                using (FileStream fsY = new FileStream("y4.bmp", FileMode.Open, FileAccess.Read))
                using (Bitmap tempY = new Bitmap(fsY))
                {
                    Console.WriteLine($"  > Файл 'y4.bmp' завантажено (Оригінальний формат: {tempY.PixelFormat}).");
                    Console.WriteLine("  > Конвертація у стандартний формат 24bppRgb для сумісності...");
                    imgY = new Bitmap(tempY.Width, tempY.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(imgY))
                    {
                        g.DrawImage(tempY, 0, 0);
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[КРОК 2/7] Вихідне зображення 'y4.bmp' успішно завантажено.\n");
                Console.ResetColor();


                using (imgX_orig)
                using (imgY)
                {
                    // --- 3. Аналіз та виправлення розмірів ---
                    Console.WriteLine("[КРОК 3/7] Аналіз та виправлення розмірів зображень...");
                    Console.WriteLine($"  > Оригінальний розмір X (x1.bmp): {imgX_orig.Width}x{imgX_orig.Height}");
                    Console.WriteLine($"  > Оригінальний розмір Y (y4.bmp): {imgY.Width}x{imgY.Height}");

                    Size targetSize = imgY.Size; // 256x256
                    bool isResizeNeeded = imgX_orig.Width != targetSize.Width || imgX_orig.Height != targetSize.Height;

                    if (isResizeNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  > УВАГА: Невідповідність розмірів! Масштабування X до розміру Y...");
                        Console.ResetColor();
                        Console.WriteLine($"  > Цільовий розмір: {targetSize.Width}x{targetSize.Height} пікселів.");
                    }
                    else
                    {
                        Console.WriteLine("  > Розміри збігаються. Масштабування не потрібне.");
                    }

                    using (Bitmap imgX_resized = isResizeNeeded ?
                                ResizeImage(imgX_orig, targetSize.Width, targetSize.Height) :
                                (Bitmap)imgX_orig.Clone())
                    {
                        if (isResizeNeeded) Console.WriteLine("  > Масштабування X завершено.");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[КРОК 3/7] Підготовка розмірів зображень завершена.\n");
                        Console.ResetColor();

                        // --- 4. Конвертація в матриці ---
                        Console.WriteLine("[КРОК 4/7] Конвертація зображень у числові матриці...");
                        Console.WriteLine($"  > Конвертація масштабованого X ({targetSize.Width}x{targetSize.Height}) у матрицю X...");
                        Matrix<double> X = BitmapToMatrix(imgX_resized);
                        Console.WriteLine("  > ...Успішно.");
                        Console.WriteLine($"  > Конвертація Y ({targetSize.Width}x{targetSize.Height}) у матрицю Y...");
                        Matrix<double> Y = BitmapToMatrix(imgY);
                        Console.WriteLine("  > ...Успішно.");
                        Console.WriteLine("\n  --- Характеристики матриць ---");
                        Console.WriteLine($"  > Розмір матриці X (вхід): {X.RowCount} рядків x {X.ColumnCount} стовпців");
                        Console.WriteLine($"  > Розмір матриці Y (вихід): {Y.RowCount} рядків x {Y.ColumnCount} стовпців");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[КРОК 4/7] Конвертація у матриці завершена.\n");
                        Console.ResetColor();

                        // --- 5. Обчислення X+ ---
                        Console.WriteLine("[КРОК 5/7] Обчислення псевдооберненої матриці X+ (Мур-Пенроуз)...");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  > Ця операція може зайняти деякий час (до хвилини)...");
                        Console.ResetColor();

                        Matrix<double> X_plus = X.PseudoInverse();

                        Console.WriteLine($"  > ...Успішно. Отримано матрицю X+ розміром: {X_plus.RowCount}x{X_plus.ColumnCount}");

                        // ---- НОВИЙ КОД: Вивід X+ ----
                        Console.WriteLine("  > Вивід прев'ю (5x5) матриці X+ у консоль:");
                        PrintMatrixPreview(X_plus, "X+ (Псевдообернена)");
                        Console.WriteLine("  > Збереження ПОВНОЇ матриці X+ у файл 'X_plus_matrix.txt'...");
                        SaveMatrixToFile(X_plus, "X_plus_matrix.txt", "Псевдообернена матриця X+ (Мур-Пенроуз)");
                        Console.WriteLine("  > ...Збережено.");
                        // ---- КІНЕЦЬ НОВОГО КОДУ ----

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[КРОК 5/7] Псевдообернену матрицю X+ обчислено та збережено.\n");
                        Console.ResetColor();

                        // --- 6. Обчислення R ---
                        Console.WriteLine("[КРОК 6/7] Обчислення оператора R за формулою R = Y * X+ ...");
                        Matrix<double> R = Y.Multiply(X_plus); // R = Y @ X+
                        Console.WriteLine($"  > ...Успішно. Отримано фінальну матрицю R розміром: {R.RowCount}x{R.ColumnCount}");

                        // ---- НОВИЙ КОД: Вивід R ----
                        Console.WriteLine("  > Вивід прев'ю (5x5) матриці R у консоль:");
                        PrintMatrixPreview(R, "R (Оператор)");
                        Console.WriteLine("  > Збереження ПОВНОЇ матриці R у файл 'R_matrix.txt'...");
                        SaveMatrixToFile(R, "R_matrix.txt", "Матриця оператора R");
                        Console.WriteLine("  > ...Збережено.");
                        // ---- КІНЕЦЬ НОВОГО КОДУ ----

                        Console.WriteLine("  > Збереження матриці R як зображення 'R_operator.bmp'...");
                        SaveMatrixAsImage(R, "R_operator.bmp");
                        Console.WriteLine("  > ...Збережено.");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[КРОК 6/7] Лінійну модель (матрицю R) успішно знайдено та збережено.\n");
                        Console.ResetColor();

                        // --- 7. Перевірка ---
                        Console.WriteLine("[КРОК 7/7] (Перевірка) Відновлення Y_model за формулою Y_model = R * X ...");
                        Matrix<double> Y_model = R.Multiply(X);
                        Console.WriteLine("  > ...Матрицю Y_model обчислено.");
                        Console.WriteLine("  > Збереження відновленого Y_model як зображення 'Y_model_output.bmp'...");
                        SaveMatrixAsImage(Y_model, "Y_model_output.bmp");
                        Console.WriteLine("  > ...Збережено.");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[КРОК 7/7] Перевірку завершено. Можете порівняти 'y4.bmp' та 'Y_model_output.bmp'.\n");
                        Console.ResetColor();
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(new string('-', 60));
                Console.WriteLine("✅ УСІ КРОКИ УСПІШНО ВИКОНАНО.");
                Console.WriteLine("Натисніть будь-яку клавішу для виходу...");
                Console.ResetColor();
            }
            catch (FileNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n--- 🛑 КРИТИЧНА ПОМИЛКА: Файл не знайдено ---");
                Console.WriteLine($"Помилка: Не вдалося знайти файл '{ex.FileName}'.");
                Console.WriteLine("Переконайтеся, що 'x1.bmp' та 'y4.bmp' знаходяться у папці 'bin/Debug/...'.");
                Console.ResetColor();
                Console.WriteLine($"Стек виклику: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n--- 🛑 КРИТИЧНА ПОМИЛКА ---");
                Console.WriteLine($"Виникла несподівана помилка: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine($"Стек виклику: {ex.StackTrace}");
            }

            Console.ReadKey();
        }

        // --- ДОПОМІЖНІ МЕТОДИ ---

        /// <summary>
        /// НОВИЙ МЕТОД: Виводить прев'ю матриці (5x5) у консоль.
        /// </summary>
        public static void PrintMatrixPreview(Matrix<double> matrix, string title)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    --- Прев'ю: {title} (перші 5x5 елементів) ---");
            int previewSize = Math.Min(5, matrix.RowCount); // Показуємо максимум 5x5
            for (int r = 0; r < previewSize; r++)
            {
                Console.Write("    [ ");
                for (int c = 0; c < previewSize; c++)
                {
                    // Форматуємо число: 8 знаків, 4 після коми
                    Console.Write($"{matrix[r, c],10:F4} ");
                }
                Console.WriteLine("... ]");
            }
            if (matrix.RowCount > previewSize)
            {
                Console.WriteLine("      ...");
            }
            Console.WriteLine($"    --- Кінець прев'ю (Повний розмір: {matrix.RowCount}x{matrix.ColumnCount}) ---");
            Console.ResetColor();
        }

        /// <summary>
        /// НОВИЙ МЕТОД: Зберігає повну матрицю у текстовий файл.
        /// </summary>
        public static void SaveMatrixToFile(Matrix<double> matrix, string filename, string title)
        {
            // Використовуємо StringBuilder для ефективного формування рядків
            // і StreamWriter для ефективного запису у файл
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.WriteLine($"--- {title} ---");
                writer.WriteLine($"Розмір: {matrix.RowCount} рядків x {matrix.ColumnCount} стовпців");
                writer.WriteLine(new string('-', 60));

                StringBuilder sb = new StringBuilder();
                for (int r = 0; r < matrix.RowCount; r++)
                {
                    sb.Clear();
                    for (int c = 0; c < matrix.ColumnCount; c++)
                    {
                        // Використовуємо крапку як десятковий роздільник
                        // і Tab (\t) як роздільник стовпців (для імпорту в Excel)
                        sb.Append(matrix[r, c].ToString(System.Globalization.CultureInfo.InvariantCulture));
                        sb.Append('\t');
                    }
                    writer.WriteLine(sb.ToString());
                }
            }
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, int width, int height)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, width, height);
            }
            return b;
        }

        public static Matrix<double> BitmapToMatrix(Bitmap bmp)
        {
            int height = bmp.Height;
            int width = bmp.Width;
            var matrix = Matrix<double>.Build.Dense(height, width);

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int stride = bmpData.Stride;
            byte[] pixels = new byte[stride * height];
            System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    matrix[r, c] = (double)pixels[r * stride + c * bytesPerPixel];
                }
            }

            bmp.UnlockBits(bmpData);
            return matrix;
        }

        public static void SaveMatrixAsImage(Matrix<double> matrix, string filename)
        {
            int height = matrix.RowCount;
            int width = matrix.ColumnCount;
            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                double min = matrix.Enumerate().Min();
                double max = matrix.Enumerate().Max();
                double range = max - min;

                if (range == 0) range = 1.0;

                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int stride = bmpData.Stride;
                byte[] pixels = new byte[stride * height];

                for (int r = 0; r < height; r++)
                {
                    for (int c = 0; c < width; c++)
                    {
                        double val = matrix[r, c];
                        byte normVal = (byte)(((val - min) / range) * 255.0);

                        int index = r * stride + c * bytesPerPixel;
                        pixels[index] = normVal;     // Blue
                        pixels[index + 1] = normVal; // Green
                        pixels[index + 2] = normVal; // Red
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);
                bmp.UnlockBits(bmpData);

                bmp.Save(filename, ImageFormat.Bmp);
            }
        }
    }
}