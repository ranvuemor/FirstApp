# FirstApp – Real-Time Activity Tracker

FirstApp is a Windows-focused .NET MAUI application that tracks the currently active application, records usage sessions, detects idle time, and visualizes activity through a timeline and dashboard.

---

## Features

### Active app tracking

- Detects the currently active Windows application
- Captures the active window title
- Tracks how long each app stays active
- Creates a new session when the active app changes

### Idle detection

- Detects when the user has not used the keyboard or mouse for a set period
- Adds idle time as its own activity state
- Shows idle periods in the timeline, summaries, and usage chart

---

## Technologies Used

- .NET MAUI
- C#
- Windows Win32 API
- `user32.dll`
- `ObservableCollection`
- `INotifyPropertyChanged`
- MAUI `Border`
- MAUI `CollectionView`
- MAUI layout controls

---

## Windows APIs Used

The app uses Windows APIs to read system activity:

- `GetForegroundWindow`
- `GetWindowText`
- `GetWindowTextLength`
- `GetWindowThreadProcessId`
- `GetLastInputInfo`

These APIs allow the app to detect the active window, process name, window title, and user idle time.

---

## Purpose

The goal of this project is to learn how to build a real desktop application that combines:

- system-level programming
- real-time tracking
- state management
- reactive UI
- data visualization
- productivity analytics

The long-term vision is to build a personal “App Replay” dashboard that helps users understand how their time is spent across apps and activities.
