using System;

namespace CoffeeHavenApp.UI.Helpers
{
    public static class ConsoleHelper
    {
        public static void DrawHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("   " + title);
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }

        public static string Prompt(string label)
        {
            Console.Write($"\n{label} > ");
            string input = Console.ReadLine();
            return input == null ? string.Empty : input.Trim();
        }

        public static string PromptDefault(string label, string defaultValue)
        {
            Console.Write($"{label} [{defaultValue}] > ");
            string input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input.Trim();
        }

        public static void SuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] " + message);
            Console.ResetColor();
        }

        public static void ErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[ERROR] " + message);
            Console.ResetColor();
        }

        public static void InfoMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[INFO] " + message);
            Console.ResetColor();
        }

        public static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static bool Confirm(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim().ToLower();
            return input == "y" || input == "yes";
        }

        public static int SafeReadInt(string label)
        {
            Console.Write($"{label} : ");
            return int.TryParse(Console.ReadLine(), out int val) ? val : 0;
        }

        public static int SafeReadIntDefault(string label, int defaultValue)
        {
            Console.Write($"{label} [{defaultValue}] : ");
            string input = Console.ReadLine();
            return int.TryParse(input, out int val) ? val : defaultValue;
        }

        public static decimal SafeReadDecimal(string label)
        {
            Console.Write($"{label} : ");
            return decimal.TryParse(Console.ReadLine(), out decimal val) ? val : 0m;
        }

        public static decimal SafeReadDecimalDefault(string label, decimal defaultValue)
        {
            Console.Write($"{label} [{defaultValue}] : ");
            string input = Console.ReadLine();
            return decimal.TryParse(input, out decimal val) ? val : defaultValue;
        }
    }
}
