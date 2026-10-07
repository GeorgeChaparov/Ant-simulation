# Project Recovery

## 1. Goal
Creating an ant simulation where ants follow pheremones layed by another ants for a food source. <br>
Ants search for food and when thay find it they head back to the nest while laying pheromons behind them. Then afther another ant finds them, they can follow the pheromones back to the food source and the cycle continues. <br>
In the end, the user should be able to inspect ants and there stats such as time lived, distance passed, food retrived and so one. They should also be able to place obsticles in real time to destrupt the ant's path.

## 2. What works

- [x] Visualizatios of ants
- [x] Ants spawning
- [ ] Logic for returning to home after finding food
- [ ] Food finding.

## 3. Architecture

## 4. Problems

- Ants can leave the pheremone grid - Fixed
- GameManager class is becoming a Monolith.

## 5. Last known task
I was tring to add the detection of the food.</br>
I wanted to add the food but then i saw i cant detect it.</br>
I tried to add the detection but the there was a problem with the sensors.</br>
I wanted to make the sensors a separet module that each ant have an instance to or something like that.</br>
I was about to make a method that checks if we are in bounds of the food and pheromone grid.</br>
I did not like the way the architecture was created because the GameManager class was becoming a Monolith.

## 6. Next action

> 1. [x] Implement the method that checks if we are in bounds of the food and pheromone grid. Its under Utils.IsOutOfBounds()
> 2. [x] Use the method to stop the ants from leaving the grid area.
> 3. [x] Add a separet sensor class for ants that can detect whatever needed.
> 4. [] Add food into the mix.
> 5. [x] Cleane the GameManager class because it haves too much cupling.