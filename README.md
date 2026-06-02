# NutriBite

> **6G6Z0014 Mobile Development — Final Coursework**  
> .NET MAUI Cross-Platform Mobile Application | Android · Windows · iOS · macOS

[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/uM_GSLJS)

---

## Table of Contents

- [Project Overview](#project-overview)
- [Screencast Demonstration Guide](#screencast-demonstration-guide)
  - [Phase 1: App Launch & Theme Fit](#phase-1-app-launch--theme-fit)
  - [Phase 2: UI/UX, Nine-Grid Gesture & Accessibility](#phase-2-uiux-nine-grid-gesture--accessibility)
  - [Phase 3: Core Functionality, Validation & Error Handling](#phase-3-core-functionality-validation--error-handling)
  - [Phase 4: Mobile Hardware Capabilities](#phase-4-mobile-hardware-capabilities)
  - [Phase 5: Offline-First Architecture — MockAPI + SQLite](#phase-5-offline-first-architecture--mockapi--sqlite)
  - [Phase 6: Cross-Platform & Multi-Device Adaptation](#phase-6-cross-platform--multi-device-adaptation)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Scoring Matrix](#scoring-matrix)
- [Build & Run](#build--run)
- [Tech Stack](#tech-stack)
- [Git History](#git-history)

---

## Project Overview

**NutriBite** is a cross-platform mobile application built with **.NET MAUI**, designed to help users record, browse, and explore food and drink items with rich nutritional information. The app integrates **7 mobile hardware capabilities** and implements a **dual-source data architecture (MockAPI + SQLite)** for robust offline-first behaviour.

| Attribute | Detail |
|---|---|
| **App Name** | NutriBite |
| **Framework** | .NET MAUI (.NET 9) |
| **Platforms** | Android, Windows, iOS, macOS, Tizen |
| **Language** | C# 13, XAML |
| **Theme** | Food & Drink · Pink-Blue Pastel Palette |
| **Architecture** | MVVM-lite with Services layer |

---

## Screencast Demonstration Guide

The screencast is structured into **6 phases** covering every scoring criterion from the assessment rubric.

---

### Phase 1: App Launch & Theme Fit

| Step | Action | What to Show |
|---|---|---|
| **1.1** | Tap the app icon to launch | Splash screen with app branding; smooth cold-start transition into the main UI |
| **1.2** | Scroll through the food list on MainPage | Food cards with category icons (🥪 Breakfast, 🍛 Lunch, 🥙 Dinner, 🍬 Snack, 🍹 Drink), calorie counts, and macro summaries |
| **1.3** | Type a keyword (e.g. "Lunch") in the SearchBar | Real-time filtering across name, category, tags, and description; results update instantly as you type |

**Key files:** `MainPage.xaml`, `MainPage.xaml.cs`, `FoodCatalogService.cs`

---

### Phase 2: UI/UX, Nine-Grid Gesture & Accessibility

| Step | Action | What to Show |
|---|---|---|
| **2.1** | Navigate to the **Hardware** tab, scroll to the nine-grid (3×3 Pattern Lock) | Draw a pattern by dragging your finger across the 3×3 dot matrix; real-time line tracing with dashed trail; sequence displayed as "1 → 4 → 7 → 8"; tap **Reset** to clear |
| **2.2** | Go to **Settings** tab → switch between Light/Dark themes | Pull down Android notification shade or use the in-app theme picker; all pages adapt instantly — backgrounds, text, cards, and Shell chrome all respond to `AppThemeBinding` with high contrast maintained |
| **2.3** | In Android emulator settings, set **Font Size → Largest**, return to app | Toggle "Large Text" switch in Settings; all Labels, Buttons, Entries, Pickers, and SearchBars scale by 1.22× via `AccessibilityService` — no overlap, no clipping, no broken layouts |

**Key files:** `Views/PatternLockDrawable.cs`, `HardwarePage.xaml.cs`, `SettingsPage.xaml.cs`, `Services/AccessibilityService.cs`, `Resources/Styles/Colors.xaml`

---

### Phase 3: Core Functionality, Validation & Error Handling

| Step | Action | What to Show |
|---|---|---|
| **3.1** | Tap **+** button → AddItemPage; leave **Name** empty and tap Save | Red `ValidationPanel` appears with error message: *"Please enter a food or drink name."*; device vibrates (250ms); app does NOT crash — form stays open for correction |
| **3.2** | Fill in complete valid data (Name: "Cake", Category: "Snack", valid numbers for calories/protein/carbs/fat, description) → tap Save | Success alert: *"Saved to mockapi.io"*; navigate back to MainPage; new item appears at the bottom of the list with 🍬 Snack icon |

**Validation rules:** Name (required) · Category (must select) · Description (required) · Calories/Protein/Carbs/Fat (must be non-negative integers)

**Key files:** `AddItemPage.xaml`, `AddItemPage.xaml.cs`

---

### Phase 4: Mobile Hardware Capabilities

All hardware demos are consolidated on the **Hardware** tab for easy screencast recording.

| Hardware | API Used | Demonstration |
|---|---|---|
| **Camera** | `MediaPicker.Default.CapturePhotoAsync()` | Tap "Take Food Photo" → Android virtual camera opens → press shutter → photo appears in app UI with success status |
| **Location & Geocoding** | `Geolocation.Default.GetLocationAsync()` + `Geocoding.Default.GetPlacemarksAsync()` | Tap "Get Location" → displays Latitude/Longitude (e.g. 37.42200, -122.08400) → reverse-geocoded address: *"United States / California / Mountain View"* |
| **Text-to-Speech** | `TextToSpeech.Default.SpeakAsync()` via `SpeechService` | Tap "Read Summary" → system voice reads nutrition info aloud → mid-reading, tap **"Stop Reading"** → voice cuts off instantly (uses `CancellationToken`) |
| **Vibration** | `Vibration.Default.Vibrate()` | Triggered on form validation failure and from detail page "Vibrate" button |
| **Haptic Feedback** | `HapticFeedback.Default.Perform(HapticFeedbackType.Click)` | Each dot touched on the nine-grid gesture triggers a haptic click; counter increments on-screen for screencast verification |
| **Shake-to-Recommend** | `Accelerometer` via `ShakeService` | Shake the device → accelerometer detects magnitude > 1.6g → random food recommendation pops up as a detail page navigation |

**Key files:** `HardwarePage.xaml.cs`, `Services/SpeechService.cs`, `Services/ShakeService.cs`, `FoodDetailPage.xaml.cs`

---

### Phase 5: Offline-First Architecture — MockAPI + SQLite

| Step | Action | What to Show |
|---|---|---|
| **5.1** | Orally explain the dual-source architecture | Point to the on-screen label: *"Data source: MockAPI (remote) + SQLite (local)"* — explain that the app communicates with mockapi.io via RESTful HTTP for cloud data |
| **5.2** | **Disconnect network** — pull down notification shade, tap Wi-Fi/Data OFF (or enable Airplane Mode) | Show the toggle happening on-screen so it's captured in the recording |
| **5.3** | Pull-to-refresh on MainPage (or kill and relaunch the app) | **Critical demo moment:** No error messages, no spinning loaders, no crash — all food items, including the newly added "Cake", load instantly from **local SQLite database** (`fooddrink.db3`). On-screen label updates to *"Data source: SQLite (local database)"* |

**Architecture details:**

```
┌──────────────┐         ┌──────────────────┐
│  MockAPI.io   │ ◄─────► │ FoodCatalogService │
│  (Remote)     │  HTTP   │                    │
└──────────────┘         │  ┌──────────────┐ │
                         │  │   SQLite DB   │ │
                         │  │  (Local)      │ │
                         │  └──────────────┘ │
                         └──────────────────┘
```

- **Merge strategy:** MockAPI data is authoritative for matching IDs; locally-added items are preserved (never lost during sync)
- **Fallback chain:** MockAPI → SQLite → Built-in fallback data (6 recipes)
- **Category normalization:** "Drinks" / "Beverages" → "Drink" (handles MockAPI schema inconsistencies)

**Key files:** `Services/FoodCatalogService.cs`, `Services/FoodDatabase.cs`, `Services/MockApiConfig.cs`

---

### Phase 6: Cross-Platform & Multi-Device Adaptation

| Step | Action | What to Show |
|---|---|---|
| **6.1** | Switch to an **Android Tablet** emulator (or stretch the Windows window to a wide aspect ratio) | The card layout, SearchBar, food list, and nine-grid gesture all scale properly — **no stretching, no deformation, no text overflow** |
| **6.2** | Rotate screen (portrait ↔ landscape) on both phone and tablet | Layout reflows correctly; the 3×3 nine-grid uses `Math.Min(width, height)` to always render as a **perfect square**, centered on canvas — dots never squash or stack on wide screens |

**Key fix:** The nine-grid was refactored from `canvasWidth`-only padding calculation to `Math.Min(width, height)`-based square grid construction, ensuring correct proportions on tablets where width ≫ height. (Commit `8cc5cb5`)

---

## Architecture

```
FoodDrinkApp/
├── Models/
│   └── FoodItem.cs              # Data model (JSON + SQLite dual annotations)
├── Services/
│   ├── FoodCatalogService.cs    # Dual-source orchestrator (MockAPI + SQLite + fallback)
│   ├── FoodDatabase.cs          # SQLite CRUD (sqlite-net-pcl)
│   ├── MockApiConfig.cs         # MockAPI endpoint configuration
│   ├── SpeechService.cs         # TTS wrapper (speak, stop, locale selection)
│   ├── AccessibilityService.cs  # Dynamic font scaling (1.22×) across all control types
│   └── ShakeService.cs          # Accelerometer-based shake detection
├── Views/
│   └── PatternLockDrawable.cs   # 3×3 nine-grid gesture with haptic feedback
├── Converters/
│   └── CategoryToEmojiConverter.cs
├── Pages/
│   ├── MainPage                 # Food list, search, pull-to-refresh, shake-to-recommend
│   ├── AddItemPage              # Form with validation panel + vibration feedback
│   ├── FoodDetailPage           # Nutrition detail, TTS read/stop, vibration
│   ├── HardwarePage             # Camera, GPS, TTS, haptic, nine-grid gesture
│   └── SettingsPage             # Theme switcher, large text toggle
├── AppShell.xaml                # TabBar navigation + AppThemeBinding for Shell chrome
└── Resources/Styles/
    ├── Colors.xaml              # Pink-blue pastel palette
    └── Styles.xaml              # Global control styles
```

---

## Scoring Matrix

| Rubric Item | Weight | Implementation Status |
|---|---|---|
| **UI/UX Design & Accessibility** | 30% | Warm food-themed UI with category icons; 3-tab Shell navigation; Dark/Light theme with `AppThemeBinding`; Dynamic Type (1.22× font scaling); `SemanticScreenReader.Announce()` throughout |
| **Use of Mobile Hardware** | 20% | **7 hardware features:** Camera, GPS/Geolocation, Geocoding, TTS (play + stop), Vibration, Haptic Feedback, Accelerometer (shake). Exceeds the 4-hardware distinction threshold |
| **Functionality** | 20% | CRUD food list with search/filter; detail page; add with validation; nine-grid gesture with haptic feedback; shake-to-recommend; pull-to-refresh |
| **Validation & Error Handling** | 10% | Required field checks; non-negative number validation; `ValidationPanel` with vibration; try/catch on all hardware calls with user-facing status messages; graceful degradation on permission denial |
| **Code Quality** | 10% | Layered architecture (Models/Services/Pages/Views/Converters); `FoodDatabase` singleton; `SpeechService` with `CancellationToken`; `ShakeService` with cooldown/debounce; `AccessibilityService` with `ConditionalWeakTable`; XML documentation on all public APIs |
| **Deployment** | 5% | Android + Windows builds verified; `Directory.Build.props` for output path |
| **GitHub Usage** | 5% | 10+ meaningful commits with descriptive bilingual commit messages; each commit addresses a distinct feature or bug fix |

---

## Build & Run

### Prerequisites
- .NET 9 SDK
- Android SDK (for Android builds)
- Windows App SDK (for Windows builds)

### Build Commands

```powershell
# Android
dotnet build .\FoodDrinkApp\FoodDrinkApp.csproj -f net9.0-android --no-incremental

# Windows
dotnet build .\FoodDrinkApp\FoodDrinkApp.csproj -f net9.0-windows10.0.19041.0 --no-incremental

# Android (with deploy to emulator/device)
dotnet build .\FoodDrinkApp\FoodDrinkApp.csproj -f net9.0-android -t:Run
```

> **Note:** If Visual Studio reports "Cannot launch for Android", ensure an emulator is running or a device is connected via ADB.

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | .NET MAUI (.NET 9) |
| **UI** | XAML with `AppThemeBinding` |
| **Local Database** | SQLite via `sqlite-net-pcl` |
| **Remote API** | MockAPI.io (RESTful HTTP via `System.Net.Http.Json`) |
| **Hardware APIs** | `MediaPicker`, `Geolocation`, `Geocoding`, `TextToSpeech`, `Vibration`, `HapticFeedback`, `Accelerometer` |
| **Accessibility** | `SemanticScreenReader`, `ConditionalWeakTable` for font scaling |
| **Pattern** | MVVM-lite with service layer |

---

## Git History

| Commit | Description |
|---|---|
| `8cc5cb5` | Fix nine-grid disproportion on tablet — use `Math.Min(width, height)` for square grid |
| `106a755` | Add SQLite as local data source (dual-source architecture) |
| `b2af4e0` | Fix CollectionView not refreshing after add — return new `List<T>` reference |
| `a66a435` | Fix 3 bugs: phone-only 2 items visible, new food not showing, stale cache |
| `f97b020` | Add shake-to-randomly-recommend feature (Accelerometer) |
| `073f77b` | Add nine-grid gesture unlock (3×3 pattern lock) |
| `32156ea` | Add emoji to categories for intuitive browsing |
| `c295483` | Change theme to pink-blue pastel palette |
| `1e3effb` | Add TTS `<queries>` for Android 11+; document all permissions |
| `5aef4d0` | Integrate MockAPI with local fallback mechanism |
| `9c7b4c5` | Add deadline badge |
| `b93a388` | Initial commit |

---

<p align="center">
  <sub>Built for 6G6Z0014 Mobile Development · Manchester Metropolitan University</sub>
</p>
