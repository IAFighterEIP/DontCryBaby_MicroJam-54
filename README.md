# 🍼 Don't Cry Baby

A small chaotic 2D Unity game where you must manage a baby's mood by
interacting with objects in the environment.

Heat the bottle. Calm the baby. Try not to make things worse.

------------------------------------------------------------------------

## 🎮 Concept

The baby's anger constantly increases over time.

To calm it down, the player must:

1.  🧊 Take a **cold baby bottle** from the fridge\
2.  🔥 Heat it in the microwave\
3.  👶 Give it to the baby

If done correctly, the baby calms down.\
If handled poorly... things escalate.

------------------------------------------------------------------------

## 🧠 Core Systems

### 🔹 Interaction System

-   Radius-based interaction using `Physics2D.OverlapCircle`
-   Closest-object detection using collider surface distance
-   Supports two interaction types:
    -   `InteractableObject` → task-based objects
    -   `ItemInteractable` → item-based interactions

### 🔹 Item System

-   `ItemSO` (ScriptableObject-based item definitions)
-   `PlayerHands` single-slot inventory
-   Event-driven UI updates
-   Clean separation between world objects and inventory

Items implemented: - Cold Baby Bottle - Harmful Baby Bottle

### 🔹 Baby System

-   Time-based anger increase
-   Anger multiplier system
-   Calm system via item interaction
-   Designed to support future reactive behaviors

### 🔹 HUD / UI

-   🖐 Default hand icon when empty
-   🍼 Displays item icon when holding something
-   Dynamic interaction prompt: `Press [E] to ...`
-   Event-driven updates

------------------------------------------------------------------------

## 🏗 Project Structure

    Scripts/
    │
    ├── Player/
    │   ├── PlayerController.cs
    │   ├── PlayerHands.cs
    │
    ├── Interaction/
    │   ├── InteractableObject.cs
    │   ├── ItemInteractable.cs
    │
    ├── Items/
    │   ├── ItemSO.cs
    │
    ├── World/
    │   ├── FridgeInteractable.cs
    │   ├── MicrowaveInteractable.cs
    │   ├── BabyItemReceiver.cs
    │
    ├── UI/
    │   ├── PlayerHUD.cs
    │
    └── Baby/
        ├── BabyController.cs

------------------------------------------------------------------------

## 🎮 Controls

  Key                 Action
  ------------------- ----------
  WASD / Arrow Keys   Move
  E                   Interact

------------------------------------------------------------------------

## ⚙️ Technical Highlights

-   Unity 2D physics-based interaction
-   ScriptableObject-driven item architecture
-   Decoupled UI through events
-   Modular and extensible design

------------------------------------------------------------------------

## 🚀 Possible Extensions

-   Multiple bottle states (cold → warm → too hot)
-   Baby mood states (calm, crying, enraged)
-   Random environmental events
-   Timer / scoring system
-   Sound and animation polish

------------------------------------------------------------------------

## 👨‍💻 Author

@Skyli3spro

Developed as a Unity prototype focusing on modular interaction systems,
inventory design, and game feel.
