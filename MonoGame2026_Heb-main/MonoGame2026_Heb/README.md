# Fantasy Brawlers

## Goal of the Game
The objective of Fantasy Brawlers is to strategically spend your mana to place a customized army of units on your side of the battlefield, and then watch them automatically fight the opponent's army until one side is completely defeated.

## Core Mechanics
- **Mana System**: Players use a limited pool of mana to purchase units.
- **Auto-Battler Combat**: Once the battle begins, units automatically find the nearest enemy and engage in combat using their unique weapons and abilities.
- **Unit Variety**: Different units have distinct stats (Health, Damage, Cost, Range, Speed) and unique abilities (e.g., Wizards shoot fireballs, Hypnotists temporarily convert enemies).

## Controls
- **Mouse**: The game is entirely controlled via the mouse.
  - **Left Click**: Select UI buttons, choose units from the spawn menu, and place units on the battlefield.
   - **Right Click**: When a unit is selected for placement, cancels placement and returns mana
- **Keyboard**: 
  - **Escape**: Exit the game.

## Gameplay
1. The game starts in the placement phase.
2. You click on unit buttons at the bottom of the screen to select a unit type (e.g., Knight, Ogre, Wizard).
3. Click anywhere on your side of the screen to place the selected unit. Each placement deducts from your mana pool.
4. Once you are satisfied with your army, click the "Play" button to begin the battle phase.
5. Units will automatically move and fight until only one team remains standing.

## Core Classes Responsibilities
- **Unit**: The foundational abstract class for all characters. It encapsulates properties like health, team, and state (Idle, Walking, Attacking, Dead), and provides polymorphic methods for taking damage and performing attacks.
- **BattleManager**: Tracks all alive units, determines when a battle starts, handles victory conditions, and manages the overall flow of combat.
- **UIManager**: Renders the placement UI, handles mana logic during the setup phase, and acts as the bridge between player mouse input and unit placement on the field.
- **Collider**: Handles physics and bounds-checking, triggering events when weapons, projectiles, or units intersect on the battlefield.
- **SceneManager**: Maintains a list of all active `IUpdatable` and `IDrawable` objects, ensuring the current game screen runs correctly.
