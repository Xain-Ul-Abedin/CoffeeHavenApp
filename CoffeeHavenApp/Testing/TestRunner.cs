using System;
using System.Collections.Generic;
using CoffeeHavenApp.Testing.Base;
using CoffeeHavenApp.Testing.Models;
using CoffeeHavenApp.Testing.Suites;

namespace CoffeeHavenApp.Testing
{
    public static class TestRunner
    {
        public static bool RunAll(bool isCi = false)
        {
            if (!isCi) Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("      COFFEE HAVEN - SYSTEM VERIFICATION MODULE             ");
            Console.WriteLine("============================================================");
            Console.WriteLine($"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            List<TestSuite> suites = new List<TestSuite>
            {
                new UserTestSuite(),
                new ProductTestSuite(),
                new OrderTestSuite(),
                new InventoryTestSuite()
            };

            List<SuiteResult> results = new List<SuiteResult>();

            foreach (var suite in suites)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[SUITE] Running: {suite.SuiteName}...");
                Console.ResetColor();

                suite.GlobalSetup(); // Initialize once
                var suiteResult = new SuiteResult { SuiteName = suite.SuiteName };

                foreach (var testCase in suite.GetTestCases())
                {
                    suite.Setup(); // RESET MOCKS BEFORE EACH CASE
                    suiteResult.TotalTests++;
                    Console.Write($"  [TEST] {testCase.Name.PadRight(40)}");

                    try
                    {
                        if (testCase.Run())
                        {
                            suiteResult.PassCount++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("PASS");
                            Console.ResetColor();
                            suiteResult.CaseResults.Add(new CaseResult { Name = testCase.Name, Passed = true });
                        }
                        else
                        {
                            suiteResult.FailCount++;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("FAIL");
                            Console.ResetColor();
                            Console.WriteLine($"         Error: {testCase.ErrorMessage}");
                            suiteResult.CaseResults.Add(new CaseResult { Name = testCase.Name, Passed = false, ErrorMessage = testCase.ErrorMessage });
                        }
                    }
                    catch (Exception ex)
                    {
                        suiteResult.FailCount++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FATAL ERROR");
                        Console.ResetColor();
                        Console.WriteLine($"         Exception: {ex.Message}");
                        suiteResult.CaseResults.Add(new CaseResult { Name = testCase.Name, Passed = false, ErrorMessage = ex.Message });
                    }
                }

                suite.Cleanup();
                results.Add(suiteResult);
                Console.WriteLine();
            }

            int failures = PrintSummary(results);
            
            Console.WriteLine("\n[DEBUG] Verification Complete. Using Mock/Sandbox data environment.");
            
            if (isCi) return failures == 0;

            Console.WriteLine("Press any key to return to main menu...");
            Console.ReadKey();
            return failures == 0;
        }

        private static int PrintSummary(List<SuiteResult> results)
        {
            Console.WriteLine("============================================================");
            Console.WriteLine("                  VERIFICATION SUMMARY                      ");
            Console.WriteLine("============================================================");

            int totalTests = 0;
            int totalPassed = 0;
            int totalFailed = 0;

            foreach (var res in results)
            {
                totalTests += res.TotalTests;
                totalPassed += res.PassCount;
                totalFailed += res.FailCount;

                string status = res.FailCount == 0 ? "PASSED" : "FAILED";
                Console.ForegroundColor = res.FailCount == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"{res.SuiteName.PadRight(40)} {status} ({res.PassCount}/{res.TotalTests})");
                Console.ResetColor();
            }

            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine($"TOTAL TESTS : {totalTests}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PASSED      : {totalPassed}");
            Console.ForegroundColor = totalFailed > 0 ? ConsoleColor.Red : ConsoleColor.Gray;
            Console.WriteLine($"FAILED      : {totalFailed}");
            Console.ResetColor();
            Console.WriteLine("============================================================");
            return totalFailed;
        }
    }
}
