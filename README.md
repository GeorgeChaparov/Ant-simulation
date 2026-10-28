# Ant Foraging Simulation

A Unity/C# **agent-based simulation** exploring how simple local rules can produce emergent colony-level behavior.

Ants continuously move through a 2D environment, search for food, return toward their nest using an internal directional estimate, and lay pheromone trails that other ants can detect and follow.

The project is primarily an exploration of **simulation systems, agent behavior, spatial algorithms, and performance-conscious data structures**.

> **Status:** Active development  
> **Engine:** Unity 6  
> **Unity Version:** 6000.4.11f1  
> **Language:** C#  
> **Focus:** Agent-based simulation · Emergent behavior · Spatial algorithms · State machines

---

## Overview

The simulation is built around a simple feedback loop:

```text
        ┌───────────────┐
        │     Nest      │
        └───────┬───────┘
                │
                ▼
        Search for food
                │
       ┌────────┴────────┐
       │                 │
   Food found       No information
       │                 │
       ▼                 ▼
  Move to food      Explore randomly
       │
       ▼
  Pick up food
       │
       ▼
  Return to nest
       │
  Deposit pheromone
       │
       ▼
      Nest
       │
       ▼
 Follow food pheromone
       │
       └──────────────► Food
```

No ant receives a predefined route to a food source. Instead, each ant uses information available through its sensors and its own internal state.

The colony's behavior therefore emerges from the interaction between:

- Individual exploration
- Local sensing
- Food discovery
- Pheromone deposition
- Pheromone decay
- Pheromone trail following
- Approximate nest localization
- Environmental changes

---

## What Makes the Simulation Interesting

The ants use **continuous movement** rather than moving from cell to cell.

The environment, however, uses a discrete spatial representation for things such as:

- Food
- Obstacles
- Pheromone intensity

This separation allows the agents to move freely while keeping spatial queries and pheromone storage relatively inexpensive.

```text
Continuous ant movement
          │
          ▼
   ┌───────────────┐
   │ Local sensors │
   └───────┬───────┘
           │
     ┌─────┴─────┐
     ▼           ▼
   Food      Pheromones
     │           │
     └─────┬─────┘
           ▼
      State decision
           │
           ▼
       New movement
```

---

# Ant Behavior

## Behavioral State Machine

Each ant has an explicit behavioral state:

```text
                    ┌─────────────────────┐
                    │  SearchingForFood   │
                    └──────────┬──────────┘
                               │
                         food detected
                               ▼
                    ┌─────────────────────┐
                    │ GoingTowardsFood    │
                    └──────────┬──────────┘
                               │
                         food collected
                               ▼
                    ┌─────────────────────┐
                    │ SearchingForNest    │
                    └──────────┬──────────┘
                               │
                       nest detected
                               ▼
                    ┌─────────────────────┐
                    │ GoingToTheNest      │
                    └──────────┬──────────┘
                               │
                         food delivered
                               ▼
                    ┌─────────────────────┐
                    │ FollowingFood       │
                    │ Pheromone           │
                    └──────────┬──────────┘
                               │
                         food detected
                               │
                               └────────────►
```

This keeps high-level behavior separate from the movement calculations used to execute it.

---

## Searching for Food

When an ant has no food and no usable pheromone information, it performs a **stochastic exploration pattern**.

Instead of choosing a completely new random direction every frame, the ant maintains its current direction and periodically applies a random rotation.

This produces more persistent exploration paths and avoids excessively jittery movement.

The rotation range and frequency are configurable per nest.

---

## Detecting Food

Each ant has three forward-facing sensors:

```text
                    Sensor
                       ●
                      /
                     /
               Ant ●────────● Sensor
                     \
                      \
                       ●
                    Sensor
```

The sensors are positioned relative to the ant's current orientation.

Each sensor has:

- A configurable distance from the ant
- A configurable detection radius
- A configurable angular offset

The current implementation uses three sensors at approximately:

```text
+35°     0°     -35°
  ●──────●──────●
```

The sensors are used to detect both **food sources and pheromone information**.

This gives each ant a limited local view of the environment instead of giving it direct access to global information about the simulation.

---

# Returning to the Nest

An important part of the simulation is that ants do not simply use the nest's world position as a navigation target.

When an ant leaves the nest, it starts with a perfect estimate of the nest's direction.

As it moves, the ant updates an internal `nestVector` using its accumulated movement and a small amount of uncertainty.

```text
Nest
  ●
   \
    \
     \       Ant
      \       ●
       \     ↙
        \   estimated direction
```

The ant also tracks:

- Distance traveled from the nest
- Confidence in its nest-direction estimate

Confidence decreases as the ant travels farther from the point where its estimate was established.

When confidence is low, the ant introduces more directional variation instead of blindly trusting its estimate.

This creates a simplified model of **navigation under uncertainty**.

---

## Nest Recovery

If the ant reaches the estimated nest location, but does not find it, it switches to an expanding spiral search.

```text
             ┌───────┐
          ┌──┘       │
       ┌──┘     ●────┘
       │      Ant
       │
       └───────────────
```

The search is controlled by:

- Search center
- Search angle
- Search radius
- Angular search speed
- Radius expansion speed

This allows an ant to recover from an inaccurate internal estimate without receiving the nest's global position as its target.

---

# Pheromone System

Pheromones are stored as a **numerical intensity field**.

Each spatial cell can contain a food pheromone intensity, or any other pheromone intensity.

```text
┌────┬────┬────┬────┬────┐
│    │    │  5 │ 12 │ 20 │
├────┼────┼────┼────┼────┤
│    │  2 │ 10 │ 25 │ 30 │
├────┼────┼────┼────┼────┤
│    │    │  8 │ 18 │ 22 │
└────┴────┴────┴────┴────┘
```

When an ant returns from a food source, it deposits pheromone along its path.

A second ant can then detect the trail and use it to bias its movement toward the food source.

---

## Pheromone Detection

The three ant sensors inspect their surrounding spatial regions for pheromone intensity.

The pheromone system calculates information from the detected cells, including:

- Average pheromone position
- Pheromone intensity
- Number of detected pheromone cells
- The strongest or weakest detected pheromone depending on configuration

The result is then combined across the sensors to produce a target position for the ant.

---

## Pheromone Decay

Pheromones gradually decay over time.

The current decay model also makes stronger pheromone concentrations decay more slowly:

```text
Intensity
   │
100│\
   │ \
   │  \
 50│   \__
   │      \___
   │          \____
  0└──────────────────► Time
```

Active pheromone cells are tracked in a `HashSet<int>`.

This avoids iterating over the entire spatial field every time pheromones need to be updated.

When a cell's intensity falls below the active threshold, it is removed from the active set.

---

# Spatial Data Structures

The environment uses flat arrays indexed from a 2D coordinate:

```text
index = y × width + x
```

For example:

```text
┌───┬───┬───┬───┐
│ 0 │ 1 │ 2 │ 3 │
├───┼───┼───┼───┤
│ 4 │ 5 │ 6 │ 7 │
├───┼───┼───┼───┤
│ 8 │ 9 │10 │11 │
└───┴───┴───┴───┘
```

Separate arrays are used for different types of environmental information.

For example:

- `foodAmount[]`
- `foodIntensity[]`
- `obstacles[]`

This avoids creating a Unity object for every environmental cell.

The same representation also makes local radius queries straightforward by converting a world position into a set of relevant cell indices.

---

# Dynamic Environment

The simulation supports modifying the environment while it is running.

The player can place and remove:

- Food
- Walls/obstacles
- Nests

The environment therefore does not have to be completely defined before the simulation starts.

### Controls

| Input | Action |
|---|---|
| `Q` | Select food placement |
| `W` | Select wall/obstacle placement |
| `E` | Select nest placement |
| Left mouse | Place selected item |
| Right mouse | Remove selected item |

Food and wall placement support brush-based interaction, allowing larger regions to be modified by dragging.

### Important limitation

Obstacles are currently represented and visualized by the environment system, but **ant movement does not yet perform obstacle avoidance/path planning**. The obstacle system is therefore currently an experimental foundation for future environmental-interaction behavior.

---

# Architecture

The project is divided into modules with focused responsibilities.

```text
                         Simulation
                             │
                     ┌───────┴───────┐
                     │  GameManager  │
                     └───────┬───────┘
                             │
       ┌─────────────┬───────┼────────┬──────────────┐
       ▼             ▼       ▼        ▼              ▼
 AntManager   FoodSource  Pheromone  Obstacle   Interaction
                  Manager   Manager    Manager     Manager
       │
       ▼
    AntNest
       │
       ▼
      Ant
       │
       ▼
    Sensors
```

### Main modules

| Module | Responsibility |
|---|---|
| `AntManager` | Updates ants, evaluates their states, and determines movement |
| `AntNest` | Owns ants belonging to a nest and controls spawning |
| `Ant` | Stores an individual ant's state, navigation estimates, sensors, and metrics |
| `AntSensor` | Represents local environmental sensing |
| `FoodSourceManager` | Stores food amounts and handles food detection/collection |
| `PheromoneManager` | Stores, queries, deposits, and decays pheromone intensity |
| `ObstacleManager` | Stores and modifies environmental obstacles |
| `InteractionManager` | Converts user input into environment modifications |
| `Visualization` | Visualizes ants, nests, food, pheromones, and obstacles |
| `GameManager` | Initializes the simulation systems and provides ant IDs |
| `Utils` | Provides shared spatial and mathematical operations |
---

# Ant Data Model

An ant is represented as a C# `struct`.

The ant stores its simulation state directly, including:

- Position and orientation
- Current behavioral state
- Food carrying state
- Nest estimate
- Nest confidence
- Sensor data
- Movement speed
- Pheromone configuration
- Search state
- Runtime metrics

This keeps the individual simulation agent independent from Unity's scene hierarchy.

Planned optimisation is to convert the data model from Array of Structures (AoS) to Structure of Arrays to further optimes the simulation.

---

# Runtime Metrics

Each ant currently tracks several metrics that can be used for future colony-level analysis:

- Unique ant ID
- Age
- Number of completed trips
- Total distance traveled

The project is intended to expose these metrics more directly in the future so that different simulation configurations can be compared quantitatively.

---

# Configuration

Many simulation parameters are exposed through the Unity Inspector.

Examples include:

### Ant behavior

- Minimum/maximum movement speed
- Random rotation range
- Random rotation frequency
- Sensor distance
- Sensor radius
- Pheromone preference
- Maximum reliable navigation distance

### Nest recovery

- Maximum estimate deviation
- Nest search turning speed
- Nest search expansion speed

### Pheromones

- Deposit strength
- Maximum intensity
- Decay rate
- Update interval

### Colony

- Maximum ants per nest
- Ant spawn rate

This makes it possible to experiment with different behavioral parameters without changing the core simulation code.

---

# Technical Highlights

### Agent-based modeling

The simulation models individual agents with their own state and limited perception while allowing shared environmental information to influence the colony.

### Emergent behavior

Colony-level trail formation is not explicitly scripted. It emerges from repeated interactions between ants and the pheromone field.

### State machines

Ant behavior is represented explicitly through a small state machine, making behavioral transitions easier to reason about and extend.

### Navigation under uncertainty

Ants estimate the direction of their nest using accumulated movement rather than receiving the nest position as their navigation target.

### Local sensing

Ant decisions are based on information gathered from three local sensors rather than global environmental knowledge.

### Spatial indexing

Food, pheromones, and obstacles use flat-array spatial representations, making local queries and environmental updates predictable.

### Lazy pheromone updates

Only cells containing active pheromones are considered for decay, reducing unnecessary work across empty parts of the environment.

### Interactive experimentation

The environment can be modified while the simulation is running, making it possible to experiment with how the colony reacts to changes.

---

# Project Structure

```text
Ant-simulator/
└── Assets/
    └── Modules/
        ├── Ant/
        │   ├── Ant.cs
        │   └── AntSensor.cs
        │
        ├── Ant Manager/
        │   └── AntManager.cs
        │
        ├── Ant Nest/
        │   └── AntNest.cs
        │
        ├── Food Source/
        │   ├── FoodSource.cs
        │   └── FoodSourceManager.cs
        │
        ├── Game Manager/
        │   └── GameManager.cs
        │
        ├── Interaction/
        │   └── InteractionManager.cs
        │
        ├── Navigation/
        │   └── Directions.cs
        │
        ├── Obstacle Manager/
        │   └── ObstacleManager.cs
        │
        ├── Pheromone/
        │   ├── Pheromone.cs
        │   └── PheromoneManager.cs
        │
        ├── Utils/
        │   └── Utils.cs
        │
        └── Visualization/
            └── Visualization.cs
```

---

# Running the Project

## Requirements

- Unity Hub
- Unity **6000.4.11f1**
- A desktop platform supported by Unity 6

## Running in the Unity Editor

1. Clone the repository.
2. Open the `Ant-simulator` directory in Unity Hub.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play**.

Simulation parameters can be adjusted from the relevant objects in the Unity Inspector.

---

# Current Limitations

This is an **experimental agent-based simulation**, not a biologically accurate model of ant colonies.

Current simplifications include:

- The environment uses a discrete spatial representation for food, pheromones, and obstacles.
- Ant movement is continuous within that environment.
- Pheromones are represented as intensity values rather than a physical chemical diffusion model.
- Navigation uses simplified directional memory rather than a biologically complete navigation mechanism.
- Obstacle placement exists, but ants do not currently perform obstacle avoidance.
- Colony-level statistics are tracked internally but are not yet presented through a dedicated analysis UI.
- Food-source direction memory is present in the data model but is not yet used as a complete fallback navigation strategy.

These limitations also define the next areas of development.

---

# Planned Development

The next stage of the project focuses on making the simulation more useful for experimentation and quantitative comparison.

- [ ] Improve pheromone-following using stronger weighting of sensor information.
- [ ] Use the last known food-source direction as a fallback when pheromone information disappears.
- [ ] Improve directional confidence for food-source memory.
- [ ] Implement obstacle avoidance and evaluate how trails adapt around obstacles.
- [ ] Add additional pheromone types.
- [ ] Add colony-level statistics and experiment summaries.
- [ ] Add repeatable simulation configurations.
- [ ] Measure food retrieval rate, travel distance, and trip completion time.
- [ ] Compare different exploration and pheromone-following strategies.
- [ ] Investigate scaling the simulation to larger colonies.

The longer-term goal is to turn the project from a behavioral prototype into a small platform for experimenting with **collective behavior and agent-based algorithms**.

---

# Why I Built This

I built this project to explore a programming problem where the final behavior is not directly scripted.

The central question is:

> **How can simple rules applied independently to many agents produce useful behavior at the system level?**

The emphasis is on the **behavioral and computational model**, rather than visual complexity.

---

# Technologies

- **C#**
- **Unity 6**
- **Unity 2D / URP**
- **Git**