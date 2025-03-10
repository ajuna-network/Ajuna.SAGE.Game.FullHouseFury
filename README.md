# Full House Fury: The Tower of Cards

Full House Fury: The Tower of Cards is a roguelike game that blends traditional dungeon-crawling with the strategic depth of poker. In this game, players ascend a mysterious tower, battling bosses and navigating levels by drawing and playing cards. Each combat round begins with a fresh draw from a standard 52-card deck. The player's poker hand is evaluated to determine the damage dealt to the enemy. The twist? The game state is managed in a compact, bit-level representation that tracks both the deck and the player's hand.

![image](https://github.com/user-attachments/assets/442b8d8e-35e4-4ac6-b684-c40f585ab038)
(ref. UI for testing)

## Gameplay Overview

- **Deck Management:**  
  The deck is represented as an 8-byte (64-bit) bitmask. The first 52 bits correspond to individual cards. A bit set to `1` means the card is still in the deck, while a `0` indicates it has been removed. As cards are drawn, their state is updated accordingly.

- **Hand Management:**  
  The player's hand can hold up to 10 cards, stored in an 8-byte (64-bit) region. Each card is encoded as a 6-bit value (allowing values from 0 to 51). A reserved value (63) is used to indicate an empty slot. This design enables efficient storage and quick lookups for hand operations.

- **Combat Mechanics:**  
  At the start of each combat round, the player is dealt 5 cards. They then choose a poker hand, which is evaluated to determine the damage inflicted on the enemy. If the enemy's health reaches zero, the level is cleared, and the player ascends further in the tower.

## Code Organization

The project is organized into several key components:

- **DeckAsset & Partial Classes:**  
  The core game state is managed by the `DeckAsset` class. To keep the code modular and maintainable, deck-related functions (e.g., card state, random draws) and hand-related functions (e.g., card assignment, slot checks) are split into partial classes:
  - `DeckAsset.Deck.cs` contains methods for deck manipulation.
  - `DeckAsset.Hand.cs` contains methods for hand management.
  - The main `DeckAsset.cs` file handles shared logic and data access.

- **Models:**  
  - **Card:** A structure that maps a card index (0-51) to its corresponding suit and rank.
  - **DeckAsset:** The primary asset class that encapsulates both the deck and hand using compact bit-level storage.

- **Tests:**  
  A comprehensive NUnit test suite covers deck functions, hand functions, and general asset behavior. These tests ensure that all mutations and state changes are accurately reflected.

## Running the Tests

To run the tests, make sure you have [NUnit](https://nunit.org/) installed along with a compatible test runner (e.g., Visual Studio Test Explorer, ReSharper, or the NUnit Console Runner). Then, execute the following command in your project directory:

```bash
dotnet test
```

This will run all tests in the `Ajuna.SAGE.Game.FullHouseFury.Test.Model` namespace and report any issues.

## Contributing

Contributions are welcome! If you have suggestions, feature requests, or bug fixes, please open an issue or submit a pull request. Ensure your code adheres to the project's coding standards and includes appropriate tests.

## License

[GNU Affero General Public License](https://www.gnu.org/licenses/agpl-3.0.html#license-text)

---

Enjoy playing Full House Fury: The Tower of Cards, and good luck on your ascent to the top of the tower!
