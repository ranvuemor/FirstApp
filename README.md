# FirstApp – Real-Time Activity Tracker

FirstApp is a Windows-focused .NET MAUI desktop application that tracks the currently active application, records usage sessions, detects idle time, classifies activities, awards XP, and visualizes activity through a timeline-based productivity dashboard.

The project is an experimental productivity and self-tracking app inspired by the idea of an “App Replay” system, where users can review how their time was spent across applications, websites, and activity categories.

---

## Features

### Active App Tracking

- Detects the currently active Windows application
- Captures the active window title
- Tracks how long each app stays active
- Creates a new session when the active app or activity category changes
- Displays the current active app, window title, and session duration in real time

### Browser URL Detection

- Detects the active browser URL for supported browsers
- Uses URL information to classify browser activity more accurately
- Can distinguish between different activities inside the same browser

Example:

- YouTube in Edge → Entertainment
- GitHub in Edge → Programming

### Idle Detection

- Detects when the user has not used the keyboard or mouse for a set period
- Adds idle time as its own activity state
- Shows idle periods in the timeline, summaries, usage chart, heatmap, and session history

### Activity Classification

FirstApp classifies sessions into activity categories such as:

- Programming
- Learning
- Writing
- Browsing
- Entertainment
- Gaming
- System
- Idle
- Unknown

Classification is based on the active app name, window title, and browser URL.

### XP and Level System

- Awards XP based on activity category
- Productive categories such as Programming and Learning give more XP
- Idle time gives no XP
- Displays total XP
- Shows current level
- Shows XP progress toward the next level
- Updates progress based on the selected date range

### Date Filtering

The dashboard can filter activity data by:

- Today
- Yesterday
- This Week
- All Time

The selected filter affects the XP dashboard, activity summary, timeline, usage chart, heatmap, and session history.

### Timeline Visualization

- Shows recorded activity sessions as timeline blocks
- Timeline blocks are colored by activity category
- Supports zooming in and out
- Displays session category, app name, and duration when there is enough space
- Makes it easier to visually review the flow of activity over time

### Hourly Activity Heatmap

- Shows activity intensity across 24 hours
- Uses the selected date filter to calculate hourly activity
- For Today and Yesterday, shows actual activity per hour
- For This Week, shows average activity per hour across the week
- For All Time, shows average activity per hour across all tracked days
- Colors each hour by the dominant activity category
- Uses color intensity to represent how active that hour was

### Activity Summary

- Groups usage time by application
- Shows how much time was spent in each app
- Uses cleaned app names for better readability
- Stays visible beside the XP dashboard for quick review

Example:

- Visual Studio
- Edge
- Explorer
- Idle

### Usage Chart

- Shows usage time grouped by activity category
- Uses category-colored horizontal bars
- Makes it easier to understand how time was distributed across different types of activity
- Lives inside a collapsible details section with session history

### Session History

- Displays recent recorded sessions
- Shows app name, duration, and XP
- Updates based on the selected date filter
- Lives inside a collapsible details section with the usage chart

### Expandable Dashboard Sections

- Activity Summary remains visible for quick access
- Usage Chart and Session History are grouped inside a shared collapsible details card
- Collapsed cards show preview information such as top category and session count
- The expanded/collapsed state is preserved while switching date filters

### Local Persistence

- Saves completed sessions locally using SQLite
- Loads saved sessions when the app starts again
- Helps prevent losing completed session data after closing or restarting the app

---

## Technologies Used

- .NET MAUI
- C#
- Windows Win32 API
- user32.dll
- SQLite
- sqlite-net-pcl
- FlaUI
- UI Automation
- ObservableCollection
- INotifyPropertyChanged
- MAUI Border
- MAUI CollectionView
- MAUI layout controls
- MAUI Shapes

---

## Windows APIs Used

The app uses Windows APIs to read system activity:

- GetForegroundWindow
- GetWindowText
- GetWindowTextLength
- GetWindowThreadProcessId
- GetLastInputInfo

These APIs allow the app to detect the active window, process name, window title, and user idle time.

---

## Dashboard Layout

The current dashboard layout is designed for a desktop-style experience:

- Header and live current activity card
- Date filter buttons
- XP Dashboard and Activity Summary side by side
- Full-width activity timeline
- Full-width hourly heatmap
- Collapsible Activity Details card containing Usage Chart and Session History

---

## Project Structure

```text
FirstApp/
├── Models/
│   ├── ActivitySession.cs
│   ├── ActivityCategory.cs
│   ├── AppSummary.cs
│   ├── CategorySummary.cs
│   └── DateFilter.cs
│
├── Services/
│   ├── ActiveWindowService.cs
│   ├── IdleDetectionService.cs
│   ├── ActivityClassifier.cs
│   ├── ActivityDatabaseService.cs
│   ├── AppNameFormatter.cs
│   ├── LevelService.cs
│   └── BrowserUrlService.cs
│
├── Platforms/
│   └── Windows/
│       └── Services/
│           └── BrowserUrlService.cs
│
├── MainPage.xaml
├── MainPage.xaml.cs
└── MauiProgram.cs
```

---

## Purpose

The goal of this project is to learn how to build a real desktop application that combines:

- system-level programming
- real-time tracking
- Windows API integration
- browser automation
- state management
- reactive UI
- local database persistence
- data visualization
- productivity analytics
- gamification

The long-term vision is to build a personal “App Replay” dashboard that helps users understand how their time is spent across apps, websites, and activity types.

---

## Current Limitations

- The app is currently Windows-focused
- Browser URL detection may not work reliably on every browser
- FlaUI and UI Automation behavior can vary depending on browser version and UI structure
- The dashboard currently loads saved sessions directly from the local database
- Long-term use may require more efficient database queries, cleanup tools, and large-dataset optimization
- Timeline performance may need optimization when many sessions are recorded

---

## License

This project is licensed under the MIT License.

Copyright (c) 2026 Amish Kushwaha.
