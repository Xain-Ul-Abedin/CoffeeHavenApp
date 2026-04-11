# Coffee Haven: Detailed Module & Feature Guide

This document provides a comprehensive breakdown of every functional module within the Coffee Haven POS system, detailing the features, logic, and user workflows.

---

## 🔐 1. Authentication & Security (RBAuth & RBAC)
The core of the system's security is built on **Role-Based Access Control (RBAC)** and a unique **RBAuth** login pattern.

*   **Role Identification**: The system supports two distinct roles: `Admin` (Full access) and `Customer` (Shopping access).
*   **Role-Based Authentication (RBAuth)**: During login, the user must specify their role. The system validates the credentials against the specific role in the database, preventing cross-role login attempts.
*   **Secure Registration**: New users are registered with a default `Customer` role unless an "Admin Secret Key" is provided for administrative account creation.
*   **Session Persistence**: Captures User ID, Name, and Role in a `Session` object to track the user throughout their experience.

---

## 🛒 2. Shopping & Order Management
Handles the entire "Menu-to-Receipt" workflow for the cafe.

*   **Menu Exploration**: Real-time browsing of active products with price and availability updates.
*   **Single Item Checkout**: "Quick Buy" option for immediate purchase of a single item.
*   **Multi-Item Shopping Cart**: A robust cart system allowing users to add multiple items, adjust quantities, and review totals before buying.
*   **Transaction Integrity**: Uses SQL Transactions to ensure orders are processed "all-or-nothing"—preventing stock errors if a payment fails.
*   **Order History**: Customers can view their personal history of past orders, while Admins can oversee the entire shop's transaction log.
*   **Order Workflow**: Supports `Pending`, `Completed`, and `Cancelled` states for flexible business management.

---

## 📦 3. Menu & Inventory Control
Direct control over the shop's catalog and stock health.

*   **Catalog Management**: Admins can add new products, edit existing prices, and categorize items (e.g., Beverage, Pastry).
*   **Real-time Stock Tracking**: Quantity is automatically deducted when orders are placed and restored if an order is cancelled.
*   **Low Stock Alerts**: Automatic warning system in reports when an item falls below a set threshold (default: 5 units).
*   **Soft Item Deactivation**: Items can be marked as `Inactive` to prevent new sales without erasing their historical order data.

---

## 👥 4. Customer Management & Profiles
Empowering users to manage their identity and data.

*   **Profile Self-Service**: Users can update their Full Name and Email directly within the app.
*   **Secure Password Change**: Requires verification of the current password before allowed to set a new one.
*   **Loyalty Points System**: Tracks customer engagement by awarding points based on order activity.
*   **Account Deletion (Two-Way)**: 
    -   **User Self-Deletion**: Users can permanently delete their accounts after a final password confirmation.
    -   **Admin-Led Cleanup**: Administrators have the authority to remove accounts directly from the management dashboard.

---

## 📊 5. Business Intelligence & Reports
Aggregated data analytics for shop administrators.

1.  **Sales Summary**: Total revenue, total completed orders, and average order value.
2.  **Top Products**: Identifies which items are generating the most volume and revenue.
3.  **Inventory Health**: A focused report on low-stock items that requiring immediate restocking.
4.  **Customer Insights**: Identifies "VIP" customers based on spending and total orders.
5.  **Status Analytics**: Visualizes the flow of business (percentage of successful vs. cancelled orders).

---

## 🧪 6. System Verification (Testing Suite)
Ensures system stability without affecting production data.

*   **Custom Test Runner**: A built-in module that executes automated test suites from the main login screen.
*   **Mock/Sandbox Environment**: Tests use `InMemory` data storage (Mocks) to verify logic (Registration, Ordering, Stock deduction) without touching the actual SQL database.
*   **CI integration**: Includes a CLI hook (`--test`) for automated validation during code pushes.
