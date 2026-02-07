# 🌙 DreamKeeper

A cross-platform dream journal application built with **.NET MAUI** and **.NET 10**. Record, describe, and revisit your dreams with text entries and voice recordings — all stored locally on your device.

---

## ✨ Features

- **Dream Journal** — Log dreams with a title, description, and date in an intuitive card-based interface
- **Voice Recording** — Capture dream narrations with built-in audio recording and playback
- **Inline Editing** — Edit dream details directly from the journal view without navigating away
- **Search & Filter** — Find dreams by keyword, or filter by recording status
- **Pull-to-Refresh** — Quickly reload your dream list
- **Light & Dark Mode** — Dream-themed UI with full support for system appearance settings
- **Local Storage** — All data is stored on-device via SQLite for complete privacy

---

## 📸 Screenshots

| Dark Mode |
|:---------:|
| ![DreamKeeper Screenshot](https://raw.githubusercontent.com/DontDoThat21/DreamKeeper/master/Screenshots/screenshot.png) |

---

## 🏗️ Architecture

The solution follows the **MVVM** pattern with a clean separation between data and UI layers:

```
DreamKeeper.sln
├── DreamKeeper/                     (.NET MAUI app project)
│   ├── Views/                       XAML pages and code-behind
│   ├── ViewModels/                  Presentation logic (MVVM)
│   ├── Services/                    Value converters
│   ├── Data/                        Custom MediaElement helpers
│   └── Platforms/                   Platform-specific implementations
│       ├── Android/Services/        Android AudioRecorderService
│       ├── iOS/Services/            iOS AudioRecorderService
│       └── Windows/                 Windows manifest and config
│
└── DreamKeeper.Data/                (Class library)
    ├── Models/                      Dream, AudioRecording entities
    ├── Services/                    SQLiteDbService, DreamService
    └── Data/                        IAudioRecorderService interface
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022 17.14+](https://visualstudio.microsoft.com/) with the **.NET MAUI** workload installed
- **Windows**: Windows 10 version 1809 (build 17763) or later
- **Android**: API 21 (Android 5.0 Lollipop) or later
- **iOS**: iOS 15.0 or later
- **macOS (Catalyst)**: macOS 15.0 or later

### Build & Run

1. **Clone the repository**
   ```bash
   git clone https://github.com/DontDoThat21/DreamKeeper.git
   cd DreamKeeper
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Run the app** (Windows example)
   ```bash
   dotnet build -t:Run -f net10.0-windows10.0.19041.0
   ```
   Or open `DreamKeeper.sln` in Visual Studio and press **F5**.

---

## 📦 Dependencies

| Package | Purpose |
|---------|---------|
| [Plugin.Maui.Audio](https://www.nuget.org/packages/Plugin.Maui.Audio) | Cross-platform audio recording and playback |
| [CommunityToolkit.Maui.MediaElement](https://www.nuget.org/packages/CommunityToolkit.Maui.MediaElement) | Media playback control for audio in XAML |
| [Dapper](https://www.nuget.org/packages/Dapper) | Lightweight micro-ORM for parameterized SQL queries |
| [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite) | ADO.NET provider for SQLite |

---

## 🎯 Target Platforms

| Platform | Min OS Version | Status |
|----------|---------------|--------|
| Windows | 10.0.17763 (1809) | ✅ Primary |
| Android | API 21 (5.0) | ✅ Supported |
| iOS | 15.0 | ✅ Supported |
| macOS (Catalyst) | 15.0 | 🔨 Scaffold |

---

## 🤝 Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m "Add my feature"`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

---

## 📄 License

This project is open source. See the repository for license details.
