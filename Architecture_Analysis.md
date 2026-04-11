# Coffee Haven: Technical Implementation Analysis

## 1. Professional Architectural Foundation
Coffee Haven is built upon a **3-Tier Architecture** (Presentation, Business Logic, and Data Access), which ensures a clean separation of concerns. This technical analysis deep-dives into the professional coding techniques utilized throughout the system.

---

## 2. Advanced Security & Access Patterns

### RBAC (Role-Based Access Control)
The application enforces strict permissions based on user roles. The system identifies whether a user is an **Admin** or a **Customer** and dynamically renders different dashboards.
-   **Files:** `DashboardUI.cs`, `Session.cs`.
-   **Logic:** When the application starts, it checks the `UserRole` bit in the session to determine which menus are available.

### RBAuth (Role-Based Authentication)
Unlike standard login systems, Coffee Haven uses **Role-Based Authentication**.
-   **The Technique:** During Login, the system requires the user to specify their role *before* entering credentials. The database query then validates the Email, Password, AND the Role simultaneously.
-   **Files:** `UserDAL.cs` (SQL logic), `AuthUI.cs` (Login flow).

---

## 3. Data Integrity & Management Techniques

### SQL Transactions (All-or-Nothing)
For complex operations like placing orders with multiple items, we use **SQL Transactions**.
-   **The Technique:** When a bulk order is placed, the system starts a transaction. It updates several tables (Orders, OrderItems, MenuItems) all at once. If any single step fails (e.g., an item runs out of stock halfway through), the entire process is rolled back, ensuring the database never ends up in a "half-finished" state.
-   **Files:** `OrderDAL.cs` (Inside `PlaceOrder`).

### Soft Deletion Pattern
To preserve audit trails and historical records, the system utilizes **Soft Deletion** for orders.
-   **The Technique:** Instead of using a SQL `DELETE` command which permanently erases data, the "Cancel Order" feature updates the `Status` column to 'Cancelled'. This keeps the order in the database for future reporting but removes it from active view.
-   **Files:** `OrderDAL.cs` (Inside `CancelOrder`).

---

## 4. Modern Logic Patterns

### Search & Keyword Filtering
The system implements a generic **Search/Filter Helper** that allows users to find data quickly within large tables.
-   **The Technique:** The `SearchHelper` utility scans `DataTables` in memory based on user keywords or specific column values (like searching for all "Admin" users or products containing the word "Latte").
-   **Files:** `SearchHelper.cs`, `CustomerManagementUI.cs`.

### CRUD Implementation
Standard **CRUD** (Create, Read, Update, Delete) patterns are followed for all major modules:
1.  **Users:** Adding users, updating profiles, deleting accounts.
2.  **Products:** Adding to the menu, updating prices, removing items.
3.  **Orders:** Placing new orders, viewing history, canceling transactions.

---

## 5. Development & Debugging Tools

### Integrated Debug Options
To facilitate easier testing during development, we included specialized debug features:
-   **Accounts List:** A secret option in the main menu to view all registered users and their credentials (role and password) for quick testing.
-   **Methodology:** `AuthUI.ShowDebugAccountsList` calls `UserService.GetDebugUserList`.

### Automated Test Runner
A built-in **Testing Suite** allows developers to verify the stability of the system with a single click.
-   **Files:** `Testing/TestRunner.cs`.
-   **Feature:** Located in the login menu, it runs a series of automated checks across the database and logic layers.

---

## 6. Architecture Mapping Summary

| Layer | Primary Responsibility | Technical Pattern Used |
| :--- | :--- | :--- |
| **Presentation (UI)** | Interaction & Menus | Dashboards, Search Filters, Debug Menus |
| **Business Logic (BLL)** | Rules & Validation | Input Sanitization, Security Guardrails |
| **Data Access (DAL)** | Storage & SQL | Transactions, Soft Deletes, CRUD Operations |
