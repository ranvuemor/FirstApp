# FirstApp – Real-Time Activity Tracker (MAUI .NET)

FirstApp is a .NET MAUI application that tracks which application is currently active on a Windows machine and logs how much time is spent in each app. It also visualizes this activity as a simple timeline.

This project is mainly for learning how to work with system-level APIs, real-time UI updates, and basic data visualization in MAUI.

---

## Features

- Tracks the currently active Windows application and window title
- Records time spent on each application automatically
- Creates sessions when the active window changes
- Displays live information about the current app and session duration
- Shows a session history list
- Visual timeline showing activity blocks sized by duration and colored by app

---

## How it works

- The app uses Windows API calls to detect the foreground window
- Every 500ms, it checks if the active window has changed
- When a change is detected, the previous session is saved and a new one begins
- The UI is updated continuously to reflect current activity
- A timeline is rebuilt from the session list to visualize usage

---

## Technologies used

- .NET MAUI
- C#
- Windows user32.dll (Win32 API)
- ObservableCollection for UI binding
- INotifyPropertyChanged for live updates

---

## Current limitations

- Data is stored only in memory (no persistence yet)
- Timeline is fully rebuilt on every update (not optimized)
- No merging of repeated sessions from the same app
- Works only on Windows

---

## Future improvements

- Save sessions to local storage or database
- Merge consecutive sessions from the same app
- Improve timeline performance with incremental updates
- Add analytics like total daily usage per app
- Add idle detection (keyboard/mouse inactivity)
- Build a full daily activity dashboard

---

## Purpose

This project is a learning exercise to understand:

- How to interact with Windows system APIs in C#
- How to build real-time tracking applications
- How to structure reactive UI applications in MAUI
- How to visualize time-based data
