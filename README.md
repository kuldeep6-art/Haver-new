# Haver & Boecker Machinery Schedules Integration Project

## Overview

This is a scheduling and operations integration system designed to unify machinery production tracking across multiple departments. It consolidates data from separate Excel-based workflows into a centralized system for tracking engineering, procurement, assembly, testing, and delivery milestones for vibrating screen machinery.

The system improves visibility, coordination, and scheduling accuracy across operational teams.

---

## Key Features

* **Excel Data Integration**

  * Merges multiple departmental Excel sheets into a unified scheduling system
  * Standardizes inconsistent operational data into a single data model

* **End-to-End Machine Tracking**

  * Tracks full lifecycle of machinery from order to delivery
  * Maintains records for sales orders, machine details, and production status

* **Gantt Chart Scheduling**

  * Visual representation of engineering, procurement, assembly, and delivery phases
  * Built using Frappe Gantt for timeline-based workflow tracking

* **Vendor & Procurement Tracking**

  * Tracks vendor assignments, purchase orders, and delivery timelines
  * Links procurement data to machine production schedules

* **Reporting & Print System**

  * Generates structured, printable schedules for operational use
  * Supports management-level reporting and planning

* **Notes & Status Tracking**

  * Stores machine-specific notes and operational updates
  * Enables communication across departments

---

## Tech Stack

* C#
* ASP.NET Core MVC
* Entity Framework Core
* SQLite
* Frappe Gantt (Visualization)
* HTML / CSS
* Visual Studio

---

## My Role & Contributions

* Designed and implemented a centralized scheduling system for machinery production workflows
* Developed data integration logic to merge and normalize Excel-based datasets
* Built backend structure using ASP.NET Core MVC and Entity Framework Core
* Implemented scheduling visualization using Gantt chart representation
* Created reporting and printable schedule generation system
* Structured database schema for tracking machines, vendors, and production stages

---

## Technical Challenges & Solutions

* Handled inconsistent and unstructured Excel data formats across departments
* Designed unified data model for multi-source operational tracking
* Resolved data synchronization issues between scheduling components
* Improved performance of scheduling queries for large datasets
* Managed relational complexity between machines, orders, and vendors

---

## Setup Instructions

1. Clone repository
2. Open solution in Visual Studio
3. Restore NuGet packages
4. Configure SQLite database connection
5. Run database migrations (if applicable)
6. Start application using IIS Express or Kestrel
