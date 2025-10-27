# WPF Dashboard MEF Plugins

This project is a **modular WPF dashboard application** using the **Managed Extensibility Framework (MEF)**. It allows you to build and run **plugins (widgets)** as independent DLLs that are loaded dynamically at runtime. You can extend or modify dashboard functionality without changing the core app.

---

## Build & Run

1. **Build Contracts first**
   The Contracts project contains the shared interfaces and events for both the app and all widgets.

2. **Build required widgets** (e.g., `ChartsWidget`, `TextWidget`)
   Building a widget copies its DLL into the `/Plugins` directory at the repo root.

3. **Build and run DashboardApp**
   The dashboard scans `/Plugins` and loads all found widget DLLs as tabs.

---

## Usage

* The dashboard will **hot-reload plugins** from the `Plugins` folder.
* To **add or update a widget**, build it and its DLL will be copied automatically (see: .csproj of existing widgets for dll copying configuration).
* Plugins implement a shared interface so they show up as **separate dashboard tabs**.
* Enter data in the dashboard to **broadcast it to all loaded widgets**.

---

## Folder Structure

```
/Contracts      # Shared contracts/interfaces
/Plugins        # Widget DLLs (auto-populated)
/ChartsWidget   # Example widget
/TextWidget     # Example widget
/DashboardApp   # Host app
```

**Build Order:** `Contracts → Widgets → DashboardApp`
Check `/Plugins` before starting the dashboard.
