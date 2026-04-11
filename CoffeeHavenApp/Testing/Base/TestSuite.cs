using System;
using System.Collections.Generic;

namespace CoffeeHavenApp.Testing.Base
{
    public abstract class TestSuite
    {
        public string SuiteName { get; protected set; }
        protected List<ITestCase> _testCases = new List<ITestCase>();

        /// <summary>
        /// Called once before any test cases in the suite are run.
        /// Useful for initializing services that don't need reset.
        /// </summary>
        public virtual void GlobalSetup() { }

        /// <summary>
        /// Called before EVERY individual test case.
        /// Essential for resetting Mock DALs to ensure isolation.
        /// </summary>
        public abstract void Setup();
        
        public List<ITestCase> GetTestCases() => _testCases;

        public virtual void Cleanup() { }
    }
}
