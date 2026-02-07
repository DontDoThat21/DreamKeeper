# DreamKeeper - Spec-Driven Development Document

**Version:** 1.0  
**Last Updated:** 2026-02-06  
**Platform:** .NET MAUI (.NET 8.0)  
**Local Storage:** SQLite  

---

## 1. Product Overview

DreamKeeper is a .NET MAUI mobile and desktop application that serves as a personal dream journal. Users log, describe, and voice-record their dreams, with all data stored locally on-device via SQLite. The app provides a single-screen card-based journal sorted in descending (latest-first) order by dream date, with inline editing, audio recording/playback, search, and filtering.

---

## 2. Target Platforms

| Platform | Min OS Version | Status |
|----------|---------------|--------|
| Windows  | 10.0.17763 (1809) | Primary target |
| Android  | API 21 (5.0 Lollipop) | Supported |
| iOS      | 11.0 | Supported |
| macOS (Catalyst) | 13.1 | Scaffold only |
| Tizen    | 6.5 | Scaffold only |

The `.csproj` currently builds for `net8.0-windows10.0.19041.0`. Android and iOS target frameworks should be uncommented for multi-platform builds.

---

## 3. Package Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Maui.Controls` | 8.0.80 | Core .NET MAUI framework |
| `Microsoft.Maui.Controls.Compatibility` | 8.0.80 | Xamarin.Forms compatibility layer |
| `Plugin.AudioRecorder` | 1.1.0 | Legacy audio recording plugin (Android/iOS fallback) |
| `Plugin.Maui.Audio` | 2.1.0 | Primary cross-platform audio recording and playback for MAUI |
| `sqlite-net-pcl` | 1.9.172 | SQLite ORM client library |
| `CommunityToolkit.Maui.MediaElement` | 3.1.1 | Media playback control for audio/video in XAML |
| `Dapper` | 2.1.35 | Lightweight micro-ORM for parameterized SQL queries |
| `Microsoft.Data.Sqlite` | 8.0.5 | ADO.NET provider for SQLite (used in Data project) |
| `Microsoft.Extensions.Logging.Debug` | 8.0.0 | Debug-mode logging |

---

## 4. Architecture

### 4.1 Solution Structure

```
DreamKeeper.sln
├── DreamKeeper/                    (.NET MAUI app project)
│   ├── Views/                      XAML pages and code-behind
│   ├── ViewModels/                 Presentation logic (MVVM)
│   ├── Services/                   Value converters
│   ├── Data/                       Custom MediaElement helpers
│   ├── Platforms/                  Platform-specific implementations
│   │   ├── Android/Services/       Android AudioRecorderService
│   │   ├── iOS/Services/           iOS AudioRecorderService
│   │   └── Windows/               Windows manifest and config
│   └── Resources/                  Fonts, images, icons, styles
│
└── DreamKeeper.Data/               (Class library)
    ├── Models/                     Domain entities (Dream, AudioRecording)
    ├── Services/                   Database and business logic services
    └── Data/                       Interfaces (IAudioRecorderService)
```

### 4.2 Pattern

**MVVM (Model-View-ViewModel)** using manual `INotifyPropertyChanged` and `ICommand` implementations. No MVVM framework dependency (no CommunityToolkit.MVVM). Services are instantiated directly in code-behind rather than resolved through DI for page-level dependencies.

### 4.3 Data Flow

```
View (XAML + Code-Behind)
    ↕ Data Binding (INotifyPropertyChanged)
ViewModel (DreamsViewModel)
    ↕ Method Calls
Service Layer (DreamService)
    ↕ SQL via Dapper
Database (SQLiteDbService → SQLite file: dream_database.db3)
```

---

## 5. Data Model

### 5.1 Database Schema

```sql
CREATE TABLE IF NOT EXISTS Dreams (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    DreamName         TEXT NOT NULL,
    DreamDescription  TEXT,
    DreamDate         TEXT NOT NULL,
    DreamRecording    BLOB
);
```

The database file is `dream_database.db3`, located according to the connection string in `appsettings.json`. Audio recordings are stored inline as BLOBs in the `DreamRecording` column.

### 5.2 Dream Entity

| Property | Type | Stored in DB | Default | Description |
|----------|------|:---:|---------|-------------|
| `Id` | `int` | Yes | Auto-increment | Primary key |
| `DreamName` | `string?` | Yes | `"Enter dream title here..."` | Dream title |
| `DreamDescription` | `string?` | Yes | `"Enter dream details here..."` | Full dream narrative |
| `DreamDate` | `DateTime` | Yes | `DateTime.Now` | Date the dream occurred |
| `DreamRecording` | `byte[]?` | Yes | `null` | Audio recording as raw bytes |
| `HasUnsavedChanges` | `bool` | No | `false` | Dirty-tracking flag for UI |
| `IsRecording` | `bool` | No | `false` | Active recording state flag |
| `IsEditingDate` | `bool` | No | `false` | Inline date-edit mode flag |

The `Dream` model implements `INotifyPropertyChanged`. Every persisted property setter sets `HasUnsavedChanges = true` and fires `PropertyChanged`. UI-only properties (`HasUnsavedChanges`, `IsRecording`, `IsEditingDate`) do not trigger dirty tracking.

### 5.3 AudioRecording Entity

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Primary key |
| `AudioData` | `byte[]` | Raw audio bytes |

Secondary DTO used by `SQLiteDbService` for standalone recording insert/query operations.

---

## 6. Features Specification

### 6.1 Dream Journal List (Main Screen)

**View:** `DreamsMainPage.xaml` (class: `MainPage`)  
**ViewModel:** `DreamsViewModel`

The main screen is a single-page, card-based journal. It consists of four vertical rows:

| Row | Content |
|-----|---------|
| 0 | Header: "Dream Journal" title + "Capture your dreams and insights" subtitle in a styled Frame |
| 1 | Search bar + filter controls |
| 2 | Scrollable, refreshable dream card list (`CollectionView`) |
| 3 | Floating "+" action button to add a new dream |

**Behavior:**
- Dreams are displayed as styled card frames within a `CollectionView` inside a `RefreshView` > `ScrollView`.
- The collection is bound to the `Dreams` `ObservableCollection<Dream>` on the ViewModel.
- **Sort order: descending by `DreamDate` (latest entry first).** The ViewModel maintains an internal `_allDreams` list and applies sort + filter to produce the displayed `Dreams` collection.
- An empty-state message ("No dreams found") is shown when the collection is empty.
- Pull-to-refresh is supported via `RefreshView`.

#### 6.1.1 Dream Card Layout

Each dream card (`Frame` with `DreamCardStyle`) contains:

1. **Title** — `Label` bound to `DreamName`, styled bold 18pt in primary color.
2. **Date** — Tappable `Label` bound to `DreamDate` (formatted). Tapping toggles to an inline `DatePicker` for editing. Selecting a new date reverts the view back to the label and marks the dream as changed.
3. **Description** — `Editor` bound to `DreamDescription`, auto-sizing, editable inline. Changes are tracked via `HasUnsavedChanges`.
4. **Audio Player** — `ByteArrayMediaElement` bound to `DreamRecording`. Only visible when a recording exists (`ByteArrayToVisibilityConverter`).
5. **Record Button** — Toggles between "Start Recording" / "Stop Recording" text. Turns red when actively recording (`BoolToRecordingColorConverter`, `BoolToRecordingTextConverter`).
6. **Play Button** — Emoji-based play icon. Purple when a recording exists, gray otherwise (`RecordingToPlayButtonColorConverter`). Double-tap (long press) triggers a "Delete Recording" action sheet.
7. **Delete Dream Button** — Backspace emoji icon. Removes the dream entirely after confirmation.
8. **Save Button** — Only visible when `HasUnsavedChanges` is `true` (`BoolToVisibilityConverter`). Persists all inline edits to the database.

### 6.2 Add New Dream (Modal)

**View:** `DreamEntryPage.xaml`

A modal page presented when the user taps the "+" floating action button.

**Fields:**
- Dream Title (`Entry`)
- Dream Description (`Editor`, 150px height)
- Dream Date (`DatePicker`, defaults to today)

**Actions:**
- **Save Dream** — Creates a new `Dream` object, raises the `DreamSaved` event, and dismisses the modal. The parent page handles persistence via `DreamService.AddDream()` and refreshes the ViewModel collection.
- **Cancel** — Toolbar item that dismisses the modal without saving.

### 6.3 Inline Dream Editing

Dreams are editable directly within the journal list without opening a separate page:

- **Title:** Displayed as a label (currently read-only in the list; editable on entry).
- **Description:** Rendered as an `Editor` control, directly editable. Any keystroke sets `HasUnsavedChanges = true`.
- **Date:** Tapping the date label toggles to a `DatePicker`. Selecting a date updates `DreamDate` and reverts to the label view.
- **Save:** The Save button appears only when `HasUnsavedChanges` is true. Tapping it calls `DreamService.UpsertDream()` and resets the dirty flag via `MarkAsSaved()`.

### 6.4 Audio Recording

**Dependencies:** `Plugin.Maui.Audio` (`IAudioManager` → `IAudioRecorder`)

**Behavior:**
1. User taps "Start Recording" on a dream card.
2. The ViewModel's `ToggleRecordingCommand` fires:
   - If no dream is currently recording, it creates an `IAudioRecorder` via `IAudioManager`, calls `StartAsync()`, sets `dream.IsRecording = true`, and tracks the dream as `_currentlyRecordingDream`.
   - If the same dream is already recording, it calls `StopAsync()`, captures the audio stream into a `byte[]`, persists it to the database via raw SQL (`UPDATE Dreams SET DreamRecording = @recording WHERE Id = @id`), and updates `dream.DreamRecording`.
3. Only one dream can record at a time. Starting a recording on a different dream stops the current one first.
4. The Record button text and color update dynamically via `BoolToRecordingTextConverter` and `BoolToRecordingColorConverter`.

**Platform-specific recording services** also exist (`IAudioRecorderService` with Android `MediaRecorder` and iOS `AVAudioRecorder` implementations) as a fallback mechanism, though the primary path uses `Plugin.Maui.Audio`.

**Audio formats by platform:**

| Platform | Container | Codec |
|----------|-----------|-------|
| Android  | `.mp4` | AAC (via MediaRecorder) / Plugin default |
| iOS      | `.m4a` | AAC at 44100 Hz |
| Windows  | `.mp3` | Plugin default |

### 6.5 Audio Playback

**Dependencies:** `CommunityToolkit.Maui.MediaElement`, `ByteArrayMediaElement`

**Behavior:**
1. User taps the Play button on a dream card.
2. The code-behind finds the `ByteArrayMediaElement` within the card's visual tree using `FindChildElement<T>`.
3. All other audio playback is stopped first (`StopAllAudioPlayback()`).
4. The `ByteArrayMediaElement.AudioData` property is set from `dream.DreamRecording`.
5. The `ByteArrayMediaElement` writes the byte array to a temporary file in `FileSystem.CacheDirectory` with a platform-appropriate extension (`.m4a` for iOS, `.mp4` for Android, `.mp3` for Windows), then sets `Source = MediaSource.FromFile(tempPath)`.
6. `Play()` is called on the media element.

**The `ByteArrayMediaElement` control** extends `CommunityToolkit.Maui.Views.MediaElement` with a bindable `AudioData` property of type `byte[]`, bridging the gap between in-memory BLOB data and the file-based `MediaSource` API.

A `MediaElementTemplateSelector` exists as a `DataTemplateSelector` that chooses between a `ByteArrayMediaElementTemplate` (when `DreamRecording != null`) and a `BaseMediaElementTemplate` (when null), though the main page currently uses `ByteArrayMediaElement` directly in the item template.

### 6.6 Recording Deletion

**Trigger:** Double-tap (long press) on the Play button, or via ViewModel's `DeleteRecordingCommand`.

**Behavior:**
1. An action sheet confirms the user's intent ("Delete Recording" / "Cancel").
2. On confirmation, raw SQL sets `DreamRecording = NULL` for the dream's row.
3. The in-memory `dream.DreamRecording` is set to `null`.
4. The Play button color reverts to gray, and the audio player hides.

### 6.7 Dream Deletion

**Trigger:** Tap the Delete (backspace) button on a dream card.

**Behavior:**
1. The `DeleteDreamCommand` fires on the ViewModel.
2. A confirmation dialog is shown.
3. On confirmation, `DreamService.DeleteDream(dream.Id)` removes the row from SQLite.
4. The dream is removed from both `_allDreams` and the displayed `Dreams` collection.

### 6.8 Search and Filter

**Controls (Row 1 of main page):**
- `SearchBar` bound to `SearchText`
- `CheckBox` "Has Recording" bound to `ShowOnlyWithRecordings`
- `CheckBox` "No Recording" bound to `ShowOnlyWithoutRecordings`
- `Button` "Clear Filters" bound to `ClearFiltersCommand`

**Behavior:**
- Changing `SearchText` triggers `ApplyFilters()` on the ViewModel.
- Search matches against `DreamName` and `DreamDescription` (case-insensitive contains).
- "Has Recording" and "No Recording" are mutually exclusive toggle filters. Enabling one disables the other.
- "Clear Filters" resets `SearchText` to empty, both checkboxes to unchecked, and restores the full dream list.
- Filtering operates on the in-memory `_allDreams` collection and produces a new `Dreams` `ObservableCollection` with the results, maintaining descending date sort order.

### 6.9 Dirty Tracking and Save

Every persisted property on the `Dream` model (`DreamName`, `DreamDescription`, `DreamDate`, `DreamRecording`) sets `HasUnsavedChanges = true` in its setter. The Save button on each card is bound to this flag via `BoolToVisibilityConverter` and only appears when edits exist.

**Save flow:**
1. `SaveDreamCommand` calls `DreamService.UpsertDream(dream)`.
2. `UpsertDream` performs an `INSERT` (if `Id <= 0`) or `UPDATE` (if `Id > 0`) via Dapper parameterized SQL.
3. On success, the dream's `Id` is updated (for new inserts) and `MarkAsSaved()` resets `HasUnsavedChanges` to `false`.
4. On failure, `Id` is set to `-1` and the exception message is stored in `DreamDescription` for debugging.

### 6.10 Seed Data

In `DEBUG` mode, `SQLiteDbService.InitializeDatabase()` creates the `Dreams` table and inserts 3 seed dream entries with pre-written titles, descriptions, and dates. This provides immediate content for development and demo purposes.

---

## 7. Value Converters

All converters reside in `DreamKeeper/Services/` under the `DreamKeeper.Services` namespace.

| Converter | Input → Output | Usage |
|-----------|---------------|-------|
| `BoolToRecordingTextConverter` | `bool` → `"Stop Recording"` / `"Start Recording"` | Record button text |
| `BoolToRecordingColorConverter` | `bool` → `Colors.Red` / `Colors.Gray` | Record button color |
| `RecordingToPlayButtonColorConverter` | `byte[]` → `Colors.Purple` / `Colors.Gray` | Play button color based on recording existence |
| `ByteArrayToVisibilityConverter` | `byte[]` → `bool` | Show/hide audio player (`true` if non-null and length > 0) |
| `BoolToVisibilityConverter` | `bool` → `bool` | Pass-through for `IsVisible` binding |
| `BoolToInvertedVisibilityConverter` | `bool` → `!bool` | Inverted visibility (e.g., show label when not editing date) |

---

## 8. Theming and Styling

### 8.1 Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| `Primary` | `#6200EE` | Headings, title text, primary actions |
| `PrimaryDark` | `#3700B3` | Darker accent variant |
| `Secondary` | `#BB86FC` | Secondary UI elements, subtitle text |
| `Tertiary` | `#03DAC6` | Teal accent for special actions |
| `NightSky` | `#121212` | Dark mode background |
| `MoonGlow` | `#F5F5F5` | Light mode background |
| `MidnightBlue` | `#121B50` | Deep accent color |
| `Starlight` | `#FFF9C4` | Warm highlight color |
| `DreamPurple` | `#9C64FB` | Brand purple for dream-themed elements |
| `Gray100`–`Gray950` | Various | Neutral scale for text, borders, backgrounds |

### 8.2 Key Styles

| Style Key | Target Type | Description |
|-----------|-------------|-------------|
| `DreamCardStyle` | `Frame` | Cards with 16px corner radius, shadow, themed border |
| `HeaderStyle` | `Label` | Bold 24pt header text |
| `SubHeaderStyle` | `Label` | Regular 18pt subheader |
| `DreamTitleStyle` | `Label` | Bold 18pt primary-color dream title |
| `DreamDateStyle` | `Label` | 14pt end-aligned underlined date |
| `ActionButtonStyle` | `Button` | General rounded action button |
| `AddButtonStyle` | `Button` | FAB-style circular "+" button |
| `RecordButtonStyle` | `Button` | Rounded record toggle button |
| `DeleteButtonStyle` | `Button` | Rounded delete action button |
| `PlayButtonStyle` | `Button` | Rounded play action button |

### 8.3 Light / Dark Mode

All styles use `AppThemeBinding` for automatic light/dark mode switching. Background colors swap between `MoonGlow` (light) and `NightSky` (dark). Text colors, border colors, and button backgrounds all respond to system theme changes.

---

## 9. Navigation

The app uses **Shell navigation** with a single `ShellContent` route pointing to the main page. The flyout is disabled—this is a single-screen app.

| Route | Page | Navigation Type |
|-------|------|----------------|
| (root) | `DreamsMainPage` | Shell root content |
| (modal) | `DreamEntryPage` | Pushed as modal via `Navigation.PushModalAsync` |

---

## 10. Platform Permissions

### Android (`AndroidManifest.xml`)

- `RECORD_AUDIO` — Microphone access for dream recordings
- `MODIFY_AUDIO_SETTINGS` — Audio configuration
- `READ_EXTERNAL_STORAGE` — Reading saved audio files
- `WRITE_EXTERNAL_STORAGE` — Writing audio to temp storage
- `INTERNET` — Network access (future use)
- `ACCESS_NETWORK_STATE` — Network state detection (future use)

### iOS (`Info.plist`)

- `NSMicrophoneUsageDescription` — Required for audio recording

### Windows (`Package.appxmanifest`)

- `<DeviceCapability Name="microphone"/>` — Microphone access for recording

---

## 11. Service Layer

### 11.1 SQLiteDbService

**Namespace:** `DreamKeeper.Data.Services`  
**Type:** Static class  

Manages the raw SQLite connection and database lifecycle.

| Method | Description |
|--------|-------------|
| `CreateConnection()` | Returns a new `SqliteConnection` using the connection string from `appsettings.json` |
| `InitializeDatabase()` | Creates the `Dreams` table (if not exists) and inserts seed data |
| `DisposeDatabase()` | Drops the `Dreams` table (available but commented out) |
| `SaveRecordingAsync(AudioRecording)` | Inserts an audio recording BLOB |
| `GetRecordingAsync(int id)` | Retrieves an audio recording by ID |

### 11.2 DreamService

**Namespace:** `DreamKeeper.Services`  

Business logic layer for Dream CRUD operations using Dapper over SQLite.

| Method | Description |
|--------|-------------|
| `GetDreams()` | Returns `ObservableCollection<Dream>` of all dreams, sorted by date |
| `AddDream(Dream)` | Inserts a new dream (resets `Id` to 0, calls `UpsertDream`) |
| `UpdateDream(Dream)` | Updates an existing dream (calls `UpsertDream`) |
| `DeleteDream(int dreamId)` | Deletes a dream by primary key |
| `UpsertDream(Dream)` | INSERT or UPDATE based on `Id`; returns dream with updated ID |

### 11.3 ConfigurationLoader

**Namespace:** `Muffle.Data.Services` (legacy namespace, to be renamed)  

Reads `appsettings.json` to load the SQLite connection string. Uses assembly location path traversal to locate the configuration file.

### 11.4 IAudioRecorderService

**Namespace:** `DreamKeeper.Data.Data`  

Platform-abstraction interface for native audio recording.

| Method | Description |
|--------|-------------|
| `StartRecordingAsync()` | Begins audio capture |
| `StopRecordingAsync()` | Stops capture, returns `byte[]` of recorded audio |

Platform implementations exist for Android (`MediaRecorder`) and iOS (`AVAudioRecorder`). Windows throws `NotImplementedException`. The primary recording path uses `Plugin.Maui.Audio` instead.

---

## 12. ViewModel Specification

### DreamsViewModel

**Namespace:** `DreamKeeper.ViewModels`  
**Dependencies:** `DreamService`, `IAudioManager` (Plugin.Maui.Audio)

#### Observable Properties

| Property | Type | Description |
|----------|------|-------------|
| `Dreams` | `ObservableCollection<Dream>` | Filtered, sorted collection bound to the UI |
| `AudioElements` | `ObservableCollection<ByteArrayMediaElement>` | Parallel audio element collection |
| `SelectedDream` | `Dream` | Currently selected dream |
| `IsRecording` | `bool` | Global recording-in-progress flag |
| `SearchText` | `string` | Search query; triggers `ApplyFilters()` on change |
| `ShowOnlyWithRecordings` | `bool` | Filter: show only dreams with audio |
| `ShowOnlyWithoutRecordings` | `bool` | Filter: show only dreams without audio |
| `AudioPlayer` | `MediaElement` | Reference to the active media element |

#### Commands

| Command | Parameter | Description |
|---------|-----------|-------------|
| `ToggleRecordingCommand` | `Dream` | Start or stop audio recording for a dream |
| `PlayRecordingCommand` | `Dream` | Play a dream's audio recording |
| `SaveDreamCommand` | `Dream` | Persist inline edits to database |
| `DeleteDreamCommand` | `Dream` | Delete a dream with confirmation |
| `DeleteRecordingCommand` | `Dream` | Remove only the audio recording from a dream |
| `ClearFiltersCommand` | (none) | Reset search and filter state |

#### Internal State

- `_allDreams`: Complete unfiltered list of dreams loaded from the database.
- `_filteredDreams`: Working list after applying search and filter criteria.
- `_currentlyRecordingDream`: Tracks which dream is actively being recorded to enforce single-recording-at-a-time.
- `_audioRecorder`: `IAudioRecorder` instance created from `IAudioManager`.

---

## 13. Custom Controls

### ByteArrayMediaElement

**Namespace:** `DreamKeeper.Data`  
**Extends:** `CommunityToolkit.Maui.Views.MediaElement`

Bridges in-memory `byte[]` audio data to the file-based `MediaSource` API.

| Bindable Property | Type | Description |
|-------------------|------|-------------|
| `AudioData` | `byte[]` | When set, writes bytes to a temp file and sets `Source = MediaSource.FromFile(path)` |

Temp file extensions: `.m4a` (iOS), `.mp4` (Android), `.mp3` (Windows/default).

### ByteArrayMediaSource

**Namespace:** `DreamKeeper.Data`  
**Extends:** `MediaSource`

Wraps a `byte[]` and provides `GetStream()` returning a `MemoryStream`. Used as a fallback approach for playback.

### MediaElementTemplateSelector

**Namespace:** `DreamKeeper.Data`  
**Extends:** `DataTemplateSelector`

Selects between `ByteArrayMediaElementTemplate` (recording present) and `BaseMediaElementTemplate` (no recording) based on `Dream.DreamRecording` null-check.

---

## 14. Dependency Injection Registration

Configured in `MauiProgram.cs`:

```csharp
// Media element support
builder.UseMauiApp<App>().UseMauiCommunityToolkitMediaElement();

// Fonts
builder.ConfigureFonts(fonts => {
    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
});

// ViewModel (singleton)
builder.Services.AddSingleton<DreamsViewModel>();

// Platform-specific audio recording service
builder.Services.AddSingleton<IAudioRecorderService>(sp => {
    // Android → AudioRecorderService (MediaRecorder)
    // iOS     → AudioRecorderService (AVAudioRecorder)
    // Other   → NotImplementedException
});

// Database initialization (DEBUG only)
SQLiteDbService.InitializeDatabase();
```

Note: The `DreamsViewModel` is registered in DI but the main page currently instantiates it manually with `new DreamsViewModel(new DreamService(), new AudioManager())`.

---

## 15. Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=dream_database.db3"
  }
}
```

The connection string is loaded by `ConfigurationLoader` and consumed by `SQLiteDbService.CreateConnection()`.

---

## 16. Feature Summary Matrix

| Feature | Status | Key Files |
|---------|--------|-----------|
| Dream list (descending date sort) | Implemented | `DreamsMainPage.xaml`, `DreamsViewModel.cs` |
| Add new dream (modal) | Implemented | `DreamEntryPage.xaml` |
| Inline description editing | Implemented | `DreamsMainPage.xaml` |
| Inline date editing | Implemented | `DreamsMainPage.xaml.cs` |
| Save dream (dirty tracking) | Implemented | `DreamsViewModel.cs`, `Dream.cs` |
| Delete dream | Implemented | `DreamsViewModel.cs` |
| Audio recording (per-dream) | Implemented | `DreamsViewModel.cs`, `Plugin.Maui.Audio` |
| Audio playback | Implemented | `ByteArrayMediaElement.cs`, `DreamsMainPage.xaml.cs` |
| Delete recording | Implemented | `DreamsViewModel.cs` |
| Search (name + description) | Implemented | `DreamsViewModel.cs` |
| Filter by recording presence | Implemented | `DreamsViewModel.cs` |
| Clear filters | Implemented | `DreamsViewModel.cs` |
| Dark / Light theme | Implemented | `Colors.xaml`, `Styles.xaml` |
| Seed data (debug) | Implemented | `SQLiteDbService.cs` |
| SQLite local storage | Implemented | `SQLiteDbService.cs`, `DreamService.cs` |
| Platform audio (Android) | Implemented | `Platforms/Android/Services/AudioRecorderService.cs` |
| Platform audio (iOS) | Implemented | `Platforms/iOS/Services/AudioRecorderService.cs` |
| Platform audio (Windows) | Partial | Uses `Plugin.Maui.Audio` only; no native fallback |
| Pull-to-refresh | Implemented | `DreamsMainPage.xaml` (RefreshView) |
| Empty state | Implemented | `DreamsMainPage.xaml` (EmptyView) |

---

## 17. Known Technical Debt

| Issue | Location | Severity |
|-------|----------|----------|
| `ConfigurationLoader` uses `Muffle.Data.Services` namespace | `DreamKeeper.Data/Services/ConfigurationLoader.cs` | Low |
| Android/iOS audio services use `YourNamespace.*` placeholder namespaces | `Platforms/Android/Services/`, `Platforms/iOS/Services/` | Low |
| `DreamsViewModel` registered in DI but manually instantiated in `MainPage` | `DreamsMainPage.xaml.cs`, `MauiProgram.cs` | Medium |
| Dual audio recording systems (`IAudioRecorderService` + `Plugin.Maui.Audio`) | Multiple files | Medium |
| Value converters in `Services/` folder; empty `Converters/` folder exists | `DreamKeeper/Services/`, `DreamKeeper/Converters/` | Low |
| Windows `IAudioRecorderService` throws `NotImplementedException` | `MauiProgram.cs` | Low (Plugin handles it) |
| Synchronous `.Wait()` / `.Result` calls in `DreamService` | `DreamKeeper.Data/Services/DreamService.cs` | Medium |
| `ConfigurationLoader` path traversal may fail in packaged MAUI apps | `DreamKeeper.Data/Services/ConfigurationLoader.cs` | Medium |
| Only `net8.0-windows` target framework is active in `.csproj` | `DreamKeeper.csproj` line 4 | Medium |
| MSIX output path is hardcoded to `C:\Users\Tylor\Desktop\dktest\` | `DreamKeeper.csproj` line 47 | Low |

---

## 18. Glossary

| Term | Definition |
|------|------------|
| **Dream** | A single journal entry containing a title, description, date, and optional audio recording |
| **Dream Card** | The visual representation of a Dream in the journal list, rendered as a styled Frame |
| **BLOB** | Binary Large Object; how audio recordings are stored in the SQLite `DreamRecording` column |
| **Dirty Tracking** | The mechanism by which `HasUnsavedChanges` detects unsaved inline edits |
| **ByteArrayMediaElement** | Custom control that converts in-memory byte arrays into playable media sources |
| **Seed Data** | Pre-populated dream entries inserted during database initialization in DEBUG mode |
| **Upsert** | Combined insert-or-update logic based on whether `Dream.Id` is 0 (insert) or positive (update) |
