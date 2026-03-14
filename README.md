# deathmatch match simulation
Modular Unity deathmatch match simulation demonstrating clean architecture, event-driven systems, and mobile-optimized gameplay logic.
------------------------------------------------------------------------------------------------------------------------------------------------------
# Unity Deathmatch Match Simulation

This project was created as part of a technical architecture.
The goal of the task was not visual polish, but to demonstrate clean architecture, system separation, and awareness of performance considerations.
The project simulates a small deathmatch match where players score kills over time and the system tracks the leaderboard and match state.

------------------------------------------------------------------------------------------------------------------------------------------------------
## Match Rules

At the beginning of the match, ten players are created automatically.
The simulation then runs continuously and every 1–2 seconds a random player eliminates another player.

When a kill happens:

* The killer gains one point.
* The victim becomes inactive and respawns after three seconds.

The match ends when either of the following conditions is met:

* A player reaches 10 kills
* Three minutes of match time have passed

The interface shows a simple leaderboard, the remaining match time, and a winner screen once the match finishes.

------------------------------------------------------------------------------------------------------------------------------------------------------

## Project Structure

I separated the project into small systems so that each part of the logic has a clear responsibility.

```
Scripts
 Core
 Match
 Player
 Score
 UI
 Utils
```

The goal was to avoid placing everything in a large GameManager and instead keep the systems modular and easier to maintain.

------------------------------------------------------------------------------------------------------------------------------------------------------

## Architecture Approach

The project follows a lightweight event-driven architecture.

Most gameplay logic is implemented in plain C# classes instead of MonoBehaviours.
MonoBehaviours are mainly used for scene setup and UI.

This keeps gameplay systems independent from Unity components and makes the logic easier to extend or reuse.

The main systems are:

* **MatchController** – coordinates the match and win conditions
* **KillSimulationSystem** – generates kill events
* **ScoreSystem** – updates player scores
* **PlayerRespawnSystem** – handles delayed respawns
* **LeaderboardSystem** – sorts players based on score

Communication between systems happens through a simple event bus so that systems do not depend on each other directly.

------------------------------------------------------------------------------------------------------------------------------------------------------
## Performance Considerations

Since the task targets mobile platforms, I tried to avoid patterns that typically generate unnecessary allocations.

Some examples:

* Gameplay logic does not run inside Update loops.
* Timed behaviour is handled through coroutines.
* LINQ is avoided in runtime code.
* Lists are created with predefined capacity where possible.
* UI updates only occur when scores actually change.

This helps keep garbage collection spikes low and avoids unnecessary CPU work.

------------------------------------------------------------------------------------------------------------------------------------------------------

## Extensibility

Match parameters such as player count, kill limit, and respawn delay are stored inside a ScriptableObject.
This allows the match configuration to be adjusted without touching the code.

The same structure could easily support features such as:

* team-based matches
* assist scoring
* different game modes
* dynamic rule sets

------------------------------------------------------------------------------------------------------------------------------------------------------

## Scaling the System

If this were extended into a production multiplayer game, the next steps would likely include:

* replacing the simulation system with server-driven events
* introducing object pooling for player entities
* batching leaderboard updates
* moving heavy simulation logic into ECS if player counts increase significantly

The current structure keeps gameplay systems separate enough that these changes could be introduced without major refactoring.

------------------------------------------------------------------------------------------------------------------------------------------------------

