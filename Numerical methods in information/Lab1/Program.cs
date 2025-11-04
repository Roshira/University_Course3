using System;
using System.Collections.Generic;
using System.Text;

namespace NonlinearEquations
{
    class Program
    {
        // --- Метод RunRelaxationAndAnalyze (Без змін) ---
        static (double finalRoot, int conditionMetAtIter) RunRelaxationAndAnalyze(
      Func<double, double> f, double x0, double epsilon, double tau, int fixedIterationsToRun)
        {
            double x = x0;
            double previousX;
            int conditionMetAtIter = 0; // 0 означає, що умова ще не виконана

            Console.WriteLine($"\n--- Запуск методу релаксації на {fixedIterationsToRun} ітерацій (згідно апріорної оцінки) ---");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"{"Ітер.",-6} | {"x_k",-20} | {"|x_k - x_{k-1}|",-20} | {"f(x_k)",-20}");
            Console.WriteLine("----------------------------------------------------------------------");

            for (int i = 1; i <= fixedIterationsToRun; i++)
            {
                previousX = x;
                x = x - tau * f(x);

                double diff = Math.Abs(x - previousX);
                Console.WriteLine($"{i,-6} | {x,-20:F12} | {diff,-20:F12} | {f(x),-20:F12}");

                // Перевіряємо, чи умова виконана, і фіксуємо номер ітерації (лише перший раз)
                if (diff <= epsilon && conditionMetAtIter == 0)
                {
                    conditionMetAtIter = i;
                }
            }

            Console.WriteLine("----------------------------------------------------------------------");
            return (x, conditionMetAtIter);
        }

        // --- Метод RunNewtonAndAnalyze (Без змін) ---
        static (double finalRoot, int conditionMetAtIter) RunNewtonAndAnalyze(
       Func<double, double> f, Func<double, double> fprime, double x0, double epsilon, int fixedIterationsToRun)
        {
            double x = x0;
            double previousX;
            int conditionMetAtIter = 0; // 0 означає, що умова ще не виконана

            Console.WriteLine($"\n--- Запуск методу Ньютона на {fixedIterationsToRun} ітерацій (згідно апріорної оцінки) ---");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"{"Ітер.",-6} | {"x_k",-20} | {"|x_k - x_{k-1}|",-20} | {"f(x_k)",-20}");
            Console.WriteLine("----------------------------------------------------------------------");

            for (int i = 1; i <= fixedIterationsToRun; i++)
            {
                previousX = x;
                double fx = f(x);
                double fpx = fprime(x);

                if (Math.Abs(fpx) < 1e-15)
                {
                    Console.WriteLine("Похідна занадто близька до нуля.");
                    break;
                }
                x = x - fx / fpx;

                double diff = Math.Abs(x - previousX);
                Console.WriteLine($"{i,-6} | {x,-20:F12} | {diff,-20:F12} | {f(x),-20:F12}");

                if (diff <= epsilon && conditionMetAtIter == 0)
                {
                    conditionMetAtIter = i;
                }
            }
            Console.WriteLine("----------------------------------------------------------------------");
            return (x, conditionMetAtIter);
        }

        // --- Метод PrintFinalAnalysis (Без змін) ---
        static void PrintFinalAnalysis(int aprioriIterations, int conditionMetAtIter, string methodName)
        {
            Console.WriteLine($"\n--- Фінальний аналіз для методу ({methodName}) ---");

            if (conditionMetAtIter > 0)
            {
                Console.WriteLine($"Фактично, умова зупинки (|x_k - x_k-1| < ε) була виконана ВЖЕ НА {conditionMetAtIter} ітерації.");
                if (conditionMetAtIter < aprioriIterations)
                {
                    Console.WriteLine("=> Висновок: практична збіжність виявилася швидшою за теоретичну гарантію.");
                }
                else
                {
                    Console.WriteLine("=> Висновок: практична збіжність відповідає або є повільнішою за теоретичну оцінку.");
                }
            }
            else
            {
                Console.WriteLine($"Увага: за {aprioriIterations} ітерацій практична умова зупинки ТАК І НЕ БУЛА виконана.");
            }
        }

        // --- *** НОВИЙ МЕТОД: Перевірка достатніх умов збіжності для методу Релаксації *** ---
        static bool CheckRelaxationConvergenceConditions(
            Func<double, double> fprime, double tau, double a, double b)
        {
            Console.WriteLine("\n--- Перевірка достатніх умов збіжності методу Релаксації ---");
            // Використовуємо m1 = min|f'| та M1 = max|f'| згідно теорії
            var (m1, M1) = Calculate_m1_M1(fprime, a, b);
            Console.WriteLine($"На інтервалі S = [{a}, {b}]:");
            Console.WriteLine($"  m1 = min|f'(x)| ≈ {m1:F8}");
            Console.WriteLine($"  M1 = max|f'(x)| ≈ {M1:F8}");
            Console.WriteLine($"  Використаний (оптимальний) τ = {tau:F8}");

            bool condition1Met = false;
            double tau_upper_bound = 2.0 / M1;
            Console.WriteLine($"\nПеревірка умови 1 (загальна): τ ∈ (0; 2/M1)");
            Console.WriteLine($"  2/M1 = 2 / {M1:F8} = {tau_upper_bound:F8}");
            if (tau > 0 && tau < tau_upper_bound)
            {
                Console.WriteLine("  => Умова 1 ВИКОНАНА.");
                condition1Met = true;
            }
            else
            {
                Console.WriteLine("  => Умова 1 НЕ ВИКОНАНА.");
            }

            bool condition2Met = false;
            // q0 для оптимального τ
            double q0 = (M1 - m1) / (M1 + m1);
            Console.WriteLine($"\nПеревірка умови 2 (для опт. τ): q0 = (M1 - m1) / (M1 + m1) < 1");
            Console.WriteLine($"  q0 = ({M1:F8} - {m1:F8}) / ({M1:F8} + {m1:F8}) = {q0:F8}");
            if (q0 < 1)
            {
                Console.WriteLine("  => Умова 2 ВИКОНАНА.");
                condition2Met = true;
            }
            else
            {
                Console.WriteLine("  => Умова 2 НЕ ВИКОНАНА.");
            }
            Console.WriteLine("---------------------------------------------------------");
            return condition1Met && condition2Met;
        }

        // --- Метод CheckNewtonConvergenceConditions (Без змін) ---
        static bool CheckNewtonConvergenceConditions(
            Func<double, double> f, Func<double, double> fprime, Func<double, double> fdoubleprime,
            double x0, double x_star, double a, double b)
        {
            Console.WriteLine("\n--- Перевірка достатніх умов збіжності методу Ньютона ---");
            bool allConditionsMet = true;

            // --- Розрахунок m1 та M2 ---
            var (m1, M2) = Calculate_m1_M2(fprime, fdoubleprime, a, b);
            Console.WriteLine($"На інтервалі S = [{a}, {b}]:");
            Console.WriteLine($"  m1 = min|f'(x)| ≈ {m1:F8}");
            Console.WriteLine($"  M2 = max|f''(x)| ≈ {M2:F8}");

            // --- Перевірка умови 1: f(x0) * f''(x0) > 0 ---
            Console.WriteLine("\nПеревірка умови 1: f(x0) * f''(x0) > 0");
            double fx0 = f(x0);
            double fdx0 = fdoubleprime(x0);
            double condition1_val = fx0 * fdx0;
            Console.WriteLine($"  f(x0) = {fx0:F8}");
            Console.WriteLine($"  f''(x0) = {fdx0:F8}");
            Console.WriteLine($"  Добуток = {condition1_val:F8}");

            if (condition1_val > 0)
            {
                Console.WriteLine("  => Умова 1 ВИКОНАНА.");
            }
            else
            {
                Console.WriteLine("  => Умова 1 НЕ ВИКОНАНА.");
                allConditionsMet = false;
            }

            // --- Перевірка умови 2: q = (M2 * |x0 - x*|) / (2 * m1) < 1 ---
            Console.WriteLine("\nПеревірка умови 2: q = (M2 * |x0 - x*|) / (2 * m1) < 1");
            double initialErrorAbs = Math.Abs(x0 - x_star);
            Console.WriteLine($"  |x0 - x*| = |{x0} - {x_star:F8}| = {initialErrorAbs:F8}");

            if (Math.Abs(m1) < 1e-15)
            {
                Console.WriteLine("  Помилка: m1 = 0. Неможливо розрахувати q.");
                allConditionsMet = false;
                return false;
            }

            double q = (M2 * initialErrorAbs) / (2 * m1);
            Console.WriteLine($"  q = ({M2:F8} * {initialErrorAbs:F8}) / (2 * {m1:F8}) = {q:F8}");

            if (q < 1)
            {
                Console.WriteLine("  => Умова 2 ВИКОНАНА.");
            }
            else
            {
                Console.WriteLine("  => Умова 2 НЕ ВИКОНАНА.");
                allConditionsMet = false;
            }

            Console.WriteLine("---------------------------------------------------------");
            return allConditionsMet;
        }

        #region Helper and Calculation Methods

        // --- Метод FindRootQuietly (Без змін) ---
        static double FindRootQuietly(Func<double, double> f, Func<double, double> fprime, double x0, double epsilon, bool useNewton = true, double tau = 0.1)
        {
            double x = x0;
            double prevX;
            for (int i = 0; i < 200; i++) // Обмеження на 200 ітерацій
            {
                prevX = x;
                if (useNewton)
                {
                    x = x - f(x) / fprime(x);
                }
                else
                {
                    x = x - tau * f(x);
                }
                if (Math.Abs(x - prevX) <= epsilon) break;
            }
            return x;
        }

        // --- Метод FindAllRoots (Без змін) ---
        static void FindAllRoots(Func<double, double> f, double a, double b, int points = 1000)
        {
            Console.WriteLine($"Пошук всіх коренів на інтервалі [{a}, {b}]");
            Console.WriteLine("=============================================");
            double step = (b - a) / points;
            double prevFx = f(a);
            for (int i = 1; i <= points; i++)
            {
                double x = a + i * step;
                double fx = f(x);
                if (prevFx * fx <= 0)
                {
                    double left = a + (i - 1) * step;
                    double right = x;
                    Console.WriteLine($"Знайдено інтервал з коренем: [{left:F4}, {right:F4}]");
                }
                prevFx = fx;
            }
        }

        // --- Метод CalculateOptimalTau (Без змін) ---
        // Цей метод коректний для знаходження тау, оскільки f'(x) > 0 на [5, 6]
        static double CalculateOptimalTau(Func<double, double> fprime, double a, double b, int points = 1000)
        {
            double m = double.MaxValue;
            double M = double.MinValue;
            double step = (b - a) / points;
            for (int i = 0; i <= points; i++)
            {
                double x = a + i * step;
                double fpx = fprime(x);
                if (fpx < m) m = fpx;
                if (fpx > M) M = fpx;
            }
            return 2.0 / (M + m);
        }

        // --- *** ЗМІНЕНО: AprioriEstimateRelaxation *** ---
        // Тепер використовує m1, M1 та q0 згідно теорії (image_13f6e1.png)
        static int AprioriEstimateRelaxation(Func<double, double> fprime,
                                  double x0, double x_star, double tau, double epsilon, double a, double b)
        {
            // Використовуємо m1 і M1 (з модулями) згідно теорії
            var (m1, M1) = Calculate_m1_M1(fprime, a, b);

            // Використовуємо q0 згідно теорії (image_13f6e1.png)
            // Це q для *оптимального* тау.
            double q0 = (M1 - m1) / (M1 + m1);

            if (q0 >= 1)
            {
                Console.WriteLine($"[Apriori Error] q0 = {q0:F8} >= 1. Збіжність не гарантована.");
                return -1;
            }

            double initialError = Math.Abs(x0 - x_star);
            if (initialError <= epsilon) return 0;

            double numerator = Math.Log(initialError / epsilon); // ln(|x0-x*|/eps)
            double denominator = Math.Log(1 / q0);           // ln(1/q0)

            if (Math.Abs(denominator) < 1e-15) return -2; // q0 = 1

            double n_apriori = numerator / denominator;

            // Використовуємо Floor + 1, згідно з теорією ([] + 1)
            return (int)Math.Floor(n_apriori) + 1;
        }

        // --- Метод AprioriEstimateNewton (Без змін, він вже коректний) ---
        static int AprioriEstimateNewton(Func<double, double> fprime, Func<double, double> fdoubleprime,
                               double x0, double x_star, double epsilon, double a, double b)
        {
            double initialError = Math.Abs(x0 - x_star);
            if (initialError <= epsilon) return 0;

            // 1. Отримуємо m1 = min|f'(x)| та M2 = max|f''(x)|
            var (m1, M2) = Calculate_m1_M2(fprime, fdoubleprime, a, b);

            // 2. Розраховуємо q за формулою з фото (image_1386a2.png)
            if (m1 < 1e-15)
            {
                Console.WriteLine("[Apriori Error] m1 = 0. Неможливо розрахувати q.");
                return -2; // Помилка, ділення на нуль
            }

            double q = (M2 * initialError) / (2 * m1);

            // 3. Перевіряємо достатню умову q < 1
            if (q >= 1)
            {
                Console.WriteLine($"[Apriori Warning] Умова q < 1 не виконана (q = {q:F8}). Апріорна оцінка неможлива.");
                return -1; // Помилка, умова збіжності не виконана
            }

            // 4. Розраховуємо n згідно теорії (image_13f6e9.png)
            double numerator = Math.Log(initialError / epsilon); // ln( |x0-x*| / eps )
            double denominator = Math.Log(1 / q);            // ln( 1/q )

            if (Math.Abs(denominator) < 1e-15) return -2;

            double term_inside_log2 = (numerator / denominator) + 1;

            if (term_inside_log2 <= 0)
            {
                Console.WriteLine($"[Apriori Error] Аргумент для Log2 не є додатнім (term = {term_inside_log2:F8}).");
                return -3;
            }

            double log2_result = Math.Log2(term_inside_log2);

            return (int)Math.Floor(log2_result) + 1;
        }

        // --- *** НОВИЙ ДОПОМІЖНИЙ МЕТОД: Розрахунок m1 та M1 *** ---
        static (double m1, double M1) Calculate_m1_M1(
            Func<double, double> fprime, double a, double b, int points = 1000)
        {
            double m1 = double.MaxValue; // min|f'|
            double M1 = double.MinValue; // max|f'|
            double step = (b - a) / points;

            for (int i = 0; i <= points; i++)
            {
                double x = a + i * step;
                double fpx_abs = Math.Abs(fprime(x));

                if (fpx_abs < m1)
                {
                    m1 = fpx_abs;
                }
                if (fpx_abs > M1)
                {
                    M1 = fpx_abs;
                }
            }
            if (m1 < 1e-15) m1 = 1e-15;

            return (m1, M1);
        }

        // --- Метод Calculate_m1_M2 (Без змін) ---
        static (double m1, double M2) Calculate_m1_M2(
            Func<double, double> fprime, Func<double, double> fdoubleprime,
            double a, double b, int points = 1000)
        {
            double m1 = double.MaxValue; // min|f'(x)|
            double M2 = double.MinValue; // max|f''(x)|
            double step = (b - a) / points;

            for (int i = 0; i <= points; i++)
            {
                double x = a + i * step;

                double fpx_abs = Math.Abs(fprime(x));
                if (fpx_abs < m1)
                {
                    m1 = fpx_abs;
                }

                double fdpx_abs = Math.Abs(fdoubleprime(x));
                if (fdpx_abs > M2)
                {
                    M2 = fdpx_abs;
                }
            }
            if (m1 < 1e-15) m1 = 1e-15;

            return (m1, M2);
        }

        #endregion

        // --- МЕТОД MAIN (Змінено) ---
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Func<double, double> f = x => Math.Pow(x, 4) - 5.74 * Math.Pow(x, 3) + 8.18 * x - 3.48;
            Func<double, double> fprime = x => 4 * Math.Pow(x, 3) - 17.22 * Math.Pow(x, 2) + 8.18;
            Func<double, double> fdoubleprime = x => 12 * Math.Pow(x, 2) - 34.44 * x;

            double epsilon = 1e-4;
            double a = 5.0, b = 6.0;
            double x0 = 6.0; // Згідно з аналізом (f*f'' > 0)

            Console.WriteLine("Розв'язання рівняння: x^4 - 5.74x^3 + 8.18x - 3.48 = 0");
            Console.WriteLine($"Точність ε = {epsilon}");
            Console.WriteLine($"Інтервал S = [{a}, {b}]");
            Console.WriteLine($"Початкове наближення x0 = {x0}\n");


            FindAllRoots(f, -2, 7);

            // --- ОБЧИСЛЕННЯ ДЛЯ МЕТОДУ РЕЛАКСАЦІЇ ---
            Console.WriteLine("\n\n==================== АНАЛІЗ МЕТОДУ РЕЛАКСАЦІЇ ====================");
            double tau = CalculateOptimalTau(fprime, a, b);
            Console.WriteLine($"Оптимальний τ на інтервалі [{a}, {b}] = {tau:F8}");

            // *** НОВИЙ БЛОК: Перевірка достатніх умов збіжності ***
            CheckRelaxationConvergenceConditions(fprime, tau, a, b);

            // Крок 1: "Тихо" знаходимо точний корінь
            double accurateRelaxRoot = FindRootQuietly(f, fprime, x0, 1e-12, false, tau);

            // Крок 2: Розраховуємо апріорну оцінку
            int aprioriRelax = AprioriEstimateRelaxation(fprime, x0, accurateRelaxRoot, tau, epsilon, a, b);
            Console.WriteLine($"Апріорна оцінка для методу РЕЛАКСАЦІЇ: {aprioriRelax} ітерацій.");

            // Крок 3: Запускаємо метод на фіксовану кількість ітерацій
            var (_, relaxConditionMet) = RunRelaxationAndAnalyze(f, x0, epsilon, tau, aprioriRelax);

            // Крок 4: Виводимо фінальний аналіз
            PrintFinalAnalysis(aprioriRelax, relaxConditionMet, "Релаксація");


            // --- ОБЧИСЛЕННЯ ДЛЯ МЕТОДУ НЬЮТОНА ---
            Console.WriteLine("\n\n====================== АНАЛІЗ МЕТОДУ НЬЮТОНА ======================");

            // Крок 1: "Тихо" знаходимо точний корінь
            double accurateNewtonRoot = FindRootQuietly(f, fprime, x0, 1e-12, true);
            Console.WriteLine($"Точний корінь (для розрахунків) x* ≈ {accurateNewtonRoot:F8}");

            // Крок 2: Перевірка достатніх умов збіжності
            CheckNewtonConvergenceConditions(f, fprime, fdoubleprime, x0, accurateNewtonRoot, a, b);

            // Крок 3: Розраховуємо апріорну оцінку
            int aprioriNewton = AprioriEstimateNewton(fprime, fdoubleprime, x0, accurateNewtonRoot, epsilon, a, b);
            Console.WriteLine($"Апріорна оцінка для методу НЬЮТОНА: {aprioriNewton} ітерацій.");

            // Крок 4: Запускаємо метод на фіксовану кількість ітерацій
            var (_, newtonConditionMet) = RunNewtonAndAnalyze(f, fprime, x0, epsilon, aprioriNewton);

            // Крок 5: Виводимо фінальний аналіз
            PrintFinalAnalysis(aprioriNewton, newtonConditionMet, "Ньютон");
        }
    }
}