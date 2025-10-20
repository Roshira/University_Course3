using System;
using System.Collections.Generic;

namespace NonlinearEquations
{
    class Program
    {
        // Метод релаксації
        static (double root, int iterations, double[] iterationsHistory) RelaxationMethod(
            Func<double, double> f, double x0, double epsilon, double tau, int maxIterations = 100)
        {
            double x = x0;
            int iterations = 0;
            double previousX;
            var history = new List<double>();

            do
            {
                previousX = x;
                x = x - tau * f(x);
                iterations++;
                history.Add(x);

                if (iterations >= maxIterations)
                    break;

            } while (Math.Abs(x - previousX) > epsilon);

            return (x, iterations, history.ToArray());
        }

        // Метод Ньютона
        static (double root, int iterations, double[] iterationsHistory) NewtonMethod(
            Func<double, double> f, Func<double, double> fprime, double x0, double epsilon, int maxIterations = 100)
        {
            double x = x0;
            int iterations = 0;
            double previousX;
            var history = new List<double>();

            do
            {
                previousX = x;
                double fx = f(x);
                double fpx = fprime(x);

                if (Math.Abs(fpx) < 1e-15)
                    break;

                x = x - fx / fpx;
                iterations++;
                history.Add(x);

                if (iterations >= maxIterations)
                    break;

            } while (Math.Abs(x - previousX) > epsilon);

            return (x, iterations, history.ToArray());
        }

        // Пошук всіх коренів
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

                if (prevFx * fx <= 0) // Сигнал зміни знака
                {
                    double left = a + (i - 1) * step;
                    double right = x;
                    Console.WriteLine($"Корінь на проміжку [{left:F4}, {right:F4}]");
                }

                prevFx = fx;
            }
        }

        // Оптимальний τ
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

        // Апріорна оцінка для релаксації
        static int AprioriEstimateRelaxation(Func<double, double> f, Func<double, double> fprime,
                                             double x0, double tau, double epsilon, double a, double b)
        {
            double m = double.MaxValue;
            double M = double.MinValue;
            int points = 1000;
            double step = (b - a) / points;

            for (int i = 0; i <= points; i++)
            {
                double x = a + i * step;
                double fpx = fprime(x);

                if (fpx < m) m = fpx;
                if (fpx > M) M = fpx;
            }

            double q = (M - m) / (M + m);
            double x1 = x0 - tau * f(x0);
            double delta0 = Math.Abs(x1 - x0);

            double n_apriori = Math.Log(epsilon * (1 - q) / delta0) / Math.Log(q);
            return (int)Math.Ceiling(Math.Abs(n_apriori));
        }

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 🔹 Приклад: функція x^4 - 5.74x^3 + 8.18x - 3.48
            Func<double, double> f = x => Math.Pow(x, 4) - 5.74 * Math.Pow(x, 3) + 8.18 * x - 3.48;
            Func<double, double> fprime = x => 4 * Math.Pow(x, 3) - 17.22 * Math.Pow(x, 2) + 8.18;

            double epsilon = 1e-4;
            double a = 5.0, b = 6.0;
            double x0 = 5.5;

            Console.WriteLine("Розв'язання рівняння f(x)=0");
            Console.WriteLine("============================\n");

            FindAllRoots(f, -2, 7);

            double tau = CalculateOptimalTau(fprime, a, b);
            Console.WriteLine($"\nОптимальний τ = {tau:F6}");

            int aprioriIterations = AprioriEstimateRelaxation(f, fprime, x0, tau, epsilon, a, b);
            Console.WriteLine($"Апріорна оцінка ітерацій: {aprioriIterations}\n");

            var (relaxRoot, relaxIter, _) = RelaxationMethod(f, x0, epsilon, tau);
            Console.WriteLine($"Метод релаксації: корінь = {relaxRoot:F8}, ітерацій = {relaxIter+1}");

            var (newtonRoot, newtonIter, _) = NewtonMethod(f, fprime, x0, epsilon);
            Console.WriteLine($"Метод Ньютона: корінь = {newtonRoot:F8}, ітерацій = {newtonIter+1}");
        }
    }
}
