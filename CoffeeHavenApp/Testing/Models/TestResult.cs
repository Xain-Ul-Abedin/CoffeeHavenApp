namespace CoffeeHavenApp.Testing.Models
{
    public class SuiteResult
    {
        public string SuiteName { get; set; }
        public int TotalTests { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public System.Collections.Generic.List<CaseResult> CaseResults { get; set; } = new System.Collections.Generic.List<CaseResult>();
    }

    public class CaseResult
    {
        public string Name { get; set; }
        public bool Passed { get; set; }
        public string ErrorMessage { get; set; }
    }
}
