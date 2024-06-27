# Snake Game in C#

![Snake Game](![image](https://github.com/ByPS128/SimpleAiShake/assets/44829883/e12832f2-48df-4156-a583-afc29a684bca)

## Overview

This is a console-based implementation of the classic Snake game, written in C#. The game features multiple difficulty levels, obstacles, and a dynamic speed increase as you progress.

Created by [Claude AI](https://www.anthropic.com), an AI assistant.

## Features

- Console-based graphics
- Multiple difficulty levels (1-5)
- Dynamically generated obstacles (for difficulty levels 2-5)
- Increasing speed as the snake grows
- Score tracking
- Pause and resume functionality

## How to Play

1. Use arrow keys to control the snake's direction:
   - ↑ : Move Up
   - ↓ : Move Down
   - ← : Move Left
   - → : Move Right
2. Eat food (@@ symbols) to grow and earn points
3. Avoid collisions with walls, obstacles, and the snake's own body
4. Press 'P' to pause/resume the game
5. Press 'Q' to quit the game

## Game Rules

1. The snake grows with each piece of food eaten
2. Each food item is worth 10 points
3. The snake's speed increases after every 5 food items eaten
4. The game ends if the snake collides with a wall, obstacle, or itself

## Difficulty Levels

- Level 1: No obstacles
- Levels 2-5: Increasing number of obstacles
- On levels 1-3, food does not generate adjacent to obstacles or walls

## License

This project is open source and available under the [MIT License](LICENSE).

## Acknowledgements

- Inspired by the classic Snake game
- Developed with the assistance of Claude AI
