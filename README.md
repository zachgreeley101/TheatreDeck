# 🎬 TheatreDeck

*A VR-ready media controller for OBS and VLC that brings clarity to shared movie experiences.*

> **Disclaimer**
> This project was fully developed by **Zach Greeley** as a personal learning initiative.
> AI tools were used for assistance but not for "vibe coding."
> The goal of this project was to strengthen skills in **C#** and **.NET**.
> While the codebase is fully functional, it reflects an early-stage project and will be **refactored over time** for improved clarity and maintainability.

---

## 📖 Overview

**TheatreDeck** is a **Windows Forms application** designed to enhance the experience of watching movies in **VR** through **Bigscreen VR**.

The application acts as a **controller** that integrates **OBS** and **VLC**, managing movie playback from a playlist while displaying **real-time information** directly on the in-game VR theatre screen.

The core problem it solves is simple but important:

> In VR, users often enter a movie without knowing what’s currently playing or how far along it is.

**TheatreDeck bridges this gap** by providing live playback details and upcoming film information, giving users more control and awareness while enjoying shared VR screenings.

---

## 🛠️ Key Components

TheatreDeck is built around four main parts:

1. **Windows Forms Controller**

   * Acts as the central hub for the application.
   * Uses VLC’s HTTP APIs to send and receive **real-time playback data** (timecodes, status, etc.).

2. **Local Movie Database**

   * Manages locally stored films.
   * Syncs with VLC playback for accurate start/end tracking.

3. **Notion Database Integration**

   * Stores detailed metadata for each film, including:

     * Start and end times
     * Volume levels
     * Preferred display titles
   * Data is retrieved via the **Notion API (JSON)** and mapped into playlist items dynamically.
   * Enables playback refinements like **skipping intros/credits** for seamless back-to-back viewing.

4. **OBS Integration**

   * Connected via **WebSockets** to control theatre screen transitions and fades.
   * Custom connector scripts synchronize on-screen timers:

     * **Count-up timer** for the currently playing film.
     * **Countdown timer** for the upcoming film.
   * Provides users with a clear timeline, allowing them to decide whether to stay or return for the next showing.

---

## 🎥 Features

* 🎞 **Real-time Playlist Management**

  * Add or remove movies dynamically.
  * Updates reflect instantly on the VR theatre screen.

* ⏱ **Accurate Timecode Tracking**

  * Displays playback status and timestamps in real-time.
  * Seamless syncing between VLC and the on-screen UI.

* 📡 **Enhanced User Experience**

  * Gives VR viewers immediate insight into:

    * What’s playing now
    * How long it’s been playing
    * What’s coming next & when it starts

* 🧩 **Custom Playback Parameters**

  * Each film has its own metadata-driven configuration.
  * Parameters like start/end times and volume are automatically applied at runtime.

---

## 📚 Documentation

* **VLC HTTP API**
  [https://gist.github.com/fjenett/41fed52655dc4723eaf3d1c8cf06ac56](https://gist.github.com/fjenett/41fed52655dc4723eaf3d1c8cf06ac56)

* **Notion API**
  [https://developers.notion.com/](https://developers.notion.com/)

* **OBS WebSockets**
  [https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md](https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md)

---
 

View of the Form Controller 

<img width="672" height="405" alt="image" src="https://github.com/user-attachments/assets/b9aee852-964f-454b-8cc8-adb63353e7d7" />

View of the Theatre Screen
<img width="1620" height="910" alt="image" src="https://github.com/user-attachments/assets/573bb380-bf24-4715-9416-20a94563a781" />

View of the inside of the VR Theatre
<img width="2070" height="1009" alt="image" src="https://github.com/user-attachments/assets/c25eefa3-6f54-45f3-8792-0e3d8f26345f" />

View of the Notion Database
<img width="1451" height="661" alt="image" src="https://github.com/user-attachments/assets/2cd809e4-1ee2-46f0-a731-f51f9ec1d7f8" />

