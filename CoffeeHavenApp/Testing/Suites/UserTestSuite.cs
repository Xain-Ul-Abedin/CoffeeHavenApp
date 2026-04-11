using System;
using System.Collections.Generic;
using System.Data;
using CoffeeHavenApp.Testing.Base;
using CoffeeHavenApp.Testing.Mocks;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Services;

namespace CoffeeHavenApp.Testing.Suites
{
    public class UserTestSuite : TestSuite
    {
        private IUserService _userService;
        private InMemoryUserDAL _userRepo;

        public UserTestSuite()
        {
            SuiteName = "User Management & Auth Suite (Customer & Admin)";
        }

        public override void GlobalSetup()
        {
            // Initialize services once
            _userRepo = new InMemoryUserDAL();
            _userService = new UserService(_userRepo);

            // Register test cases once
            _testCases.Clear();
            _testCases.Add(new RegistrationTestCase(_userService));
            _testCases.Add(new LoginTestCase(_userService));
            _testCases.Add(new ProfileUpdateTestCase(_userService));
            _testCases.Add(new SelfDeletionTestCase(_userService));
            _testCases.Add(new AdminManagementTestCase(_userService));
        }

        public override void Setup()
        {
            // Reset data before EVERY test case
            _userRepo.Reset();
        }

        private class RegistrationTestCase : ITestCase
        {
            private IUserService _service;
            public string Name => "Registration (Validation & Success)";
            public string ErrorMessage { get; private set; }

            public RegistrationTestCase(IUserService service) => _service = service;

            public bool Run()
            {
                if (!_service.Register("Test User", "test@coffee.com", "password123"))
                {
                    ErrorMessage = "Valid registration failed."; return false;
                }
                if (_service.Register("Fail User", "fail@coffee.com", "123"))
                {
                    ErrorMessage = "Short password was accepted."; return false;
                }
                if (_service.Register("Dup User", "test@coffee.com", "password123"))
                {
                    ErrorMessage = "Duplicate email was allowed."; return false;
                }
                return true;
            }
        }

        private class LoginTestCase : ITestCase
        {
            private IUserService _service;
            public string Name => "Login (Creds & Roles)";
            public string ErrorMessage { get; private set; }

            public LoginTestCase(IUserService service) => _service = service;

            public bool Run()
            {
                _service.Register("Admin User", "admin@test.com", "admin123", "Admin");
                int adminId = _service.Login("admin@test.com", "admin123", "Admin");
                if (adminId <= 0) { ErrorMessage = "Admin login failed."; return false; }

                int failId = _service.Login("admin@test.com", "admin123", "Customer");
                if (failId > 0) { ErrorMessage = "Admin logged in as Customer."; return false; }

                return true;
            }
        }

        private class ProfileUpdateTestCase : ITestCase
        {
            private IUserService _service;
            public string Name => "Profile Updates (Name/Email)";
            public string ErrorMessage { get; private set; }

            public ProfileUpdateTestCase(IUserService service) => _service = service;

            public bool Run()
            {
                _service.Register("Initial Name", "update@test.com", "password123");
                int id = _service.Login("update@test.com", "password123");

                if (!_service.UpdateFullName(id, "New Name"))
                {
                    ErrorMessage = "Name update returned false."; return false;
                }
                if (_service.GetUserFullName(id) != "New Name")
                {
                    ErrorMessage = "Name was not updated in storage."; return false;
                }

                return true;
            }
        }

        private class SelfDeletionTestCase : ITestCase
        {
            private IUserService _service;
            public string Name => "Account Self-Deletion";
            public string ErrorMessage { get; private set; }

            public SelfDeletionTestCase(IUserService service) => _service = service;

            public bool Run()
            {
                _service.Register("Gone User", "bye@test.com", "delete123");
                int id = _service.Login("bye@test.com", "delete123");

                if (!_service.DeleteAccount(id, "delete123"))
                {
                    ErrorMessage = "Deletion failed with valid password."; return false;
                }
                if (_service.Login("bye@test.com", "delete123") > 0)
                {
                    ErrorMessage = "User still exists after deletion."; return false;
                }

                return true;
            }
        }

        private class AdminManagementTestCase : ITestCase
        {
            private IUserService _service;
            public string Name => "Admin: Role & List Management";
            public string ErrorMessage { get; private set; }

            public AdminManagementTestCase(IUserService service) => _service = service;

            public bool Run()
            {
                _service.Register("User A", "a@test.com", "pass123");
                int id = _service.Login("a@test.com", "pass123");

                if (_service.GetUserRole(id) != "Customer") { ErrorMessage = "Default role not Customer."; return false; }

                if (!_service.UpdateUserRole(id, "Admin")) { ErrorMessage = "Role update failed."; return false; }
                if (_service.GetUserRole(id) != "Admin") { ErrorMessage = "Role not updated to Admin."; return false; }

                DataTable all = _service.GetAllUsers();
                if (all.Rows.Count == 0) { ErrorMessage = "GetAllUsers returned empty list."; return false; }

                return true;
            }
        }
    }
}
