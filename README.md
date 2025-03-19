# Full House Fury

**Full House Fury** is a progression-based roguelike card game where you battle increasingly difficult enemies by playing poker-like hands each round. Along the way, you can purchase feats (positive effects or “boons”) from a shop and are also forced to take on negative effects (“banes”) to keep the game challenging and dynamic. Your goal is to progress through as many levels as possible before losing to an opponent or running out of cards.

## Table of Contents

1. [Overview](#overview)  
2. [Project Structure](#project-structure)  
3. [Core Assets and Their Roles](#core-assets-and-their-roles)  
4. [Game States and Transitions](#game-states-and-transitions)  
   - [1. Start](#1-start)  
   - [2. Play](#2-play)  
   - [3. Preparation](#3-preparation)  
   - [4. Battle](#4-battle)  
   - [5. Discard](#5-discard)  
   - [6. Score](#6-score)  
5. [Gameplay Flow](#gameplay-flow)  
6. [Boon and Bane Mechanics](#boon-and-bane-mechanics)  
7. [Running Tests](#running-tests)  
8. [Contributing](#contributing)  
9. [License](#license)

---

## Overview

In **Full House Fury**, you defeat enemies by assembling powerful poker hands and dealing damage. After each successful level, you gain tokens to purchase feats (boons) in the next “Preparation” phase. However, you must also select a new negative effect (bane) that will persist and shape the way you approach future battles. This interplay of increasing power offset by new drawbacks creates a dynamic, replayable experience.

**Key points**:
- Roguelike progression: Survive as many levels as you can.
- Card-based combat: Each round, you play up to five cards from a ten-card hand to form a poker hand; the better the hand, the more damage you deal.
- Stamina/fatigue system: Players have limited endurance. Repeated actions without rest lead to increasing self-damage.
- Boss progression: Each new level features a tougher enemy.

---

## Project Structure

The core code is split across several files that define assets, transitions, and tests:

- **Game Logic & Core Files**:
  - `FullHouseFuryGame.cs` – The main engine configuration and definition of transitions (Start, Play, Preparation, Battle, Discard, Score).
  - `FullHouseFuryIdentifier.cs` – Identifiers for the different transitions.
  - `FullHouseFuryRule.cs` – Defines rules used by the engine for validating transitions.
  - `FullHouseFuryUtil.cs` – Contains utility methods for evaluating poker hands, generating card combinations, and retrieving metadata about boons/banes.
  - `Enums.cs` – Various enums describing states, action types, suits, ranks, bonuses, and maluses.

- **Assets**:
  - `BaseAsset.cs` – The base class for all in-game assets (cards, deck, tower, etc.).
  - `GameAsset.cs` – Stores game-wide state (e.g., current level, health, endurance, whether the boss is alive, etc.).
  - `DeckAsset.cs` – Manages the card deck and ten-card hand. Includes logic for drawing and discarding cards.
  - `TowerAsset.cs` – Manages boons and banes available to the player, plus the boons/banes the player has already chosen.

- **Tests**:
  - `FullHouseFuryStartTests.cs`, `FullHouseFuryPlayTests.cs`, `FullHouseFuryPreparationTests.cs`, `FullHouseFuryBattleTests.cs`, `FullHouseFuryDiscardTests.cs`, `FullHouseFuryScoreTests.cs`
    - Unit tests for each transition in the game.
  - `FullHouseFuryBoonAndBaneTests.cs`
    - Tests logic related to applying boons and banes.
  - `FullHouseFuryFullGame1Tests.cs`, `FullHouseFuryFullGame2Tests.cs`
    - Tests for full end-to-end game loops.

---

## Core Assets and Their Roles

1. **GameAsset**  
   - Tracks the overall state of the game:
     - `GameState` / `LevelState`
     - Current level, round, boss health, player health/endurance, and so on.

2. **DeckAsset**  
   - Represents a 52-card deck (though only 62 bits are used internally).
   - Maintains the *hand* of up to 10 slots. Slots can be empty or contain card indices.
   - Provides methods to draw cards (based on pseudo-random hash bytes) and discard them.

3. **TowerAsset**  
   - Stores all potential boons (bonus effects) and banes (malus effects).
   - Holds the array of possible choices and the boons/banes the player has permanently gained.

4. **BaseAsset**  
   - The abstract parent class; other assets inherit from this.

---

## Game States and Transitions

The game is driven by **transitions**. Each transition can be considered an action a player can take to change the game’s state in some way. You’ll see these transitions referenced throughout the code in `FullHouseFuryGame.cs`, where each state has rules and a function that define how assets are created/updated.

1. ### **Start**  
   - **Identifier**: `FullHouseFuryIdentifier.Start()`  
   - **Purpose**: Creates the initial game, deck, and tower assets for a new player who has no existing assets.
   - **Rules**:
     - Must have zero assets (`AssetCount == 0`).
     - Must not already have a `Game` asset (`SameNotExist` with type = Game).
   - **On success**: Returns newly created `GameAsset`, `DeckAsset`, and `TowerAsset`.

2. ### **Play**  
   - **Identifier**: `FullHouseFuryIdentifier.Play()`  
   - **Purpose**: Initiates the game if the newly created assets are present.  
   - **Rules**:
     - Exactly 3 assets: `Game`, `Deck`, and `Tower`.
   - **On success**: Sets `GameState` to `Running` and other initial defaults in the deck/tower.

3. ### **Preparation**  
   - **Identifier**: `FullHouseFuryIdentifier.Preparation()`  
   - **Purpose**: Transition the game from “Running” to the readiness state for the next battle. This is where the player can shop for feats and choose new boons/banes (if it’s not the first level).  
   - **Rules**:
     - Exactly 3 assets: `Game`, `Deck`, and `Tower`.
     - The game must be `Running` and `LevelState` must be `Preparation`.
   - **On success**:
     - If not on level 1, the player picks a boon/bane combo.
     - Resets or adjusts player deck/hand for the battle.
     - Moves to `Battle` state.

4. ### **Battle**  
   - **Identifier**: `FullHouseFuryIdentifier.Battle()`  
   - **Purpose**: Conduct one “round” of the fight. The player chooses which cards (0–5) to play. The code evaluates that poker hand, inflicts damage on the boss, and handles player stamina/fatigue.  
   - **Rules**:
     - Game must be in `Running` state, `LevelState == Battle`.
   - **On success**:
     - Updates boss and player HP accordingly.
     - If both remain alive, the game continues in `Battle`.
     - If the boss or player dies (or you run out of cards), the game transitions to `Score` or `Finished`.

5. ### **Discard**  
   - **Identifier**: `FullHouseFuryIdentifier.Discard()`  
   - **Purpose**: Allows the player to discard some cards from the hand (if they have discard actions left) and immediately draw new ones.  
   - **Rules**:
     - The game is `Running` and the current state is `Battle`.
     - The `Discard` count on `GameAsset` is > 0.
   - **On success**:
     - Removes chosen cards from the hand.
     - Draws replacements from the deck.
     - Decrements the `Discard` count.

6. ### **Score**  
   - **Identifier**: `FullHouseFuryIdentifier.Score()`  
   - **Purpose**: Reached after the battle is over for that level (boss dead or player out of cards). This transition tallies up final round tokens, increments the level, and resets the deck for the next round.  
   - **Rules**:
     - Must be in `Running` state, `LevelState == Score`.
   - **On success**:
     - Sets up the new boss, re-initializes the deck, and transitions back to `Preparation` for the next level.
     - If the player died or has no cards left, the code might set `GameState = Finished`.

---

## Gameplay Flow

A typical flow for **Full House Fury** might look like this:

1. **Start**  
   - The user transitions from no game assets to having the `GameAsset`, `DeckAsset`, and `TowerAsset`.

2. **Play**  
   - The game initializes: sets `GameState = Running`, `Level = 1`, etc.

3. **Preparation** (Level 1)  
   - If the level is 1, no boons/banes are chosen yet. (In subsequent levels, you choose a boon/bane combo here.)  
   - Deck draws an initial hand.  
   - Moves to `Battle` state.

4. **Battle**  
   - Each *round* in the battle, the player can:
     1. Choose some cards from their 10-card hand to form a poker combination.
     2. Transition “Battle” to play them, dealing damage to the boss.  
     3. If they have discard actions left, they can do the **Discard** transition to draw replacements.  
   - Once the boss or the player is defeated (or the deck empties), the game goes to `Score`.

5. **Score**  
   - Tally up final damage, track tokens earned, increment the `Level`.  
   - The deck is reset/refilled, the boss is made stronger.  
   - The game returns to `Preparation`, and the cycle continues for the next level.  

6. **Repeat**  
   - This loop (Preparation → Battle → Score) continues until the player dies or the deck empties out, at which point `GameState` becomes `Finished`.

---

## Boon and Bane Mechanics

- **Boons** (`BonusType`)  
  - Grant positive effects like extra healing, improved endurance, or higher chance of drawing good cards.  
  - Often stored as single bits or multi-bit values in the `TowerAsset`.  
  - You unlock additional ranks/levels of a boon over time (some are single-level boons, others have multiple levels).

- **Banes** (`MalusType`)  
  - Impose negative effects like reduced damage, extra cost, or healing the opponent.  
  - Similarly tracked in the `TowerAsset` with single or multi-level states.  
  - Each new level of the game you must choose a new boon/bane pair, creating interesting strategic trade-offs.

---

## Running Tests

This project includes a suite of **NUnit** tests that verify each of the major transitions and full-game scenarios. You can find them in files like:

- `FullHouseFuryStartTests.cs`
- `FullHouseFuryPlayTests.cs`
- `FullHouseFuryPreparationTests.cs`
- `FullHouseFuryBattleTests.cs`
- `FullHouseFuryDiscardTests.cs`
- `FullHouseFuryScoreTests.cs`
- **and** the full integration tests `FullHouseFuryFullGame1Tests.cs`, `FullHouseFuryFullGame2Tests.cs`.

**To run the tests**:
1. Open the solution in your preferred C# IDE (e.g., Visual Studio, Rider, VS Code with C# extension).
2. Restore NuGet packages.
3. Build the project.
4. Run tests via the Test Explorer / `dotnet test` command, depending on your setup.

---

## Contributing

1. Fork and clone the repository.
2. Create a branch for your changes.
3. Commit and push to your fork.
4. Open a pull request, describing your changes in detail.

---

## License

This project is distributed under the GNU Affero General Public License. See `LICENSE.md` for details.

---

*Enjoy building and customizing **Full House Fury**! If you have any questions or issues, feel free to open an issue or pull request.*
