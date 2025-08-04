# Shenkar Unity Finals 2025 – Systems Tour & Video Outline

## Introduction
- Brief introduction
- State the objective of the final exercise

---

## 1. **Game Overview**
- Explain the core gameplay loop:
    - Player movement: left, right, jump, attack.
    - Objective: reach end of each level.
    - Two distinct levels: Side-scrolling (Level 1), Maze (Level 2).
---

## 2. **Demo & Gameplay Showcase**
- Play through both levels:
  - Highlight all systems and features as described.
  - Demonstrate player abilities, pickups, mounts, all enemy types and their interactions.
  - Showcase edge cases (death, game reset, invincibility, item collection).

---

## 2. **Player Systems**
- **Movement & Controls**:
    - Showcase left/right movement, jumping, attacking (with/without weapons).
- **Lives & Health System**:
    - Starting with 3 lives, explain consequences of losing lives.
    - Health/power meter: how it decreases over time and how it is replenished.
    - What happens on death or running out of health.
- **Inventory & Power-Ups**:
    - Weapons: hammer and boomerang, how they are collected and used.
    - Mounts: blue, red, green animals; how to collect, switch, and their abilities.
    - Fairies: invincibility for 10 seconds.

---

## 3. **Level Design**
- **Level 1**: Linear side-scrolling, walking to the right.
- **Level 2**: Maze platformer, jumping challenges.
- Transition between levels.

---

## 4. **Objects & Pickups**
- **Fruits**:
    - Two types: +1 power, +2 power.
    - Collecting 30 fruits grants an extra life.
- **Eggs**:
    - How eggs are opened and what items they contain (animal, weapon, fairy).
- **Obstacles**:
    - Stones: interaction, destruction mechanics, impact on player/mount.
    - Fires: deadly, destroyed only by fairy, special mount interaction.

---

## 5. **Enemies & AI Behavior**
- **Enemy Types & Behaviors**:
    - Spider: moves up/down or static.
    - Bird: moves left, oscillates up/down.
    - Snake: jumps forward, some shoot fireballs.
    - Frog: jumps higher/further, reacts to proximity.
    - Ghost: invulnerable except to fairy.
- **Enemy Life Cycle**:
    - Destroying enemies and the timer for their respawn.
    - Dropping items (mount, weapon, fairy) on defeat.

---

## 6. **Weapons & Mounts**
- **Weapons System**:
    - Hammer and boomerang implementation (throwing mechanics, destruction of obstacles/enemies).
- **Mount System**:
    - Animal abilities (tail swing, fire breath, spin).
    - Collecting and switching mounts.
    - Interaction with obstacles and enemies.

---

## 7. **Power-Up & Invincibility System**
- **Fairy Mechanic**:
    - How fairy is acquired, effects on player, duration, what can/can't be destroyed.

---

## 8. **Game Flow & State Management**
- Starting, resetting levels, game over logic.
- Level transitions and what changes in each level.

---

## 9. **Design Patterns & Architecture**
- **SOLID Principles**:
    - Where and how they are applied (single responsibility, open/closed, etc.).
- **Dependency Injection (DI)**:
    - Example: managing game services and managers.
- **Pooling System**:
    - Example: bullets, enemies, pickups for performance.
- **Builder & Factory Patterns**:
    - Example: level creation, enemy instantiation.
- **MVC (Model-View-Controller)**:
    - Example: separating logic from UI, player/enemy controllers.
- **Async & Tasks**:
    - Example: timers, respawn logic, game events.
- **Template Pattern**:
    - Used for common behaviors in enemies, pickups, etc.

---


---

## 11. **Summary & Reflection**
- Recap what was built and learned.
- Challenges faced and how they were overcome.
- Mention adherence to requirements and design patterns.

