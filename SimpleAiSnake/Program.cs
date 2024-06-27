using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

internal class Program
{
    private const int SPEED_EASIEST = 200;
    private const int SPEED_EASY = 180;
    private const int SPEED_MEDIUM = 160;
    private const int SPEED_HARD = 140;
    private const int SPEED_HARDEST = 120;
    private static readonly int width = 40;
    private static readonly int height = 20;
    private static int score;
    private static int foodEaten;
    private static bool gameOver;
    private static bool gamePaused;
    private static readonly Random random = new ();

    private static readonly List<Position> snake = new ();
    private static Position food = new ();
    private static readonly List<Position> obstacles = new ();

    private static int gameSpeed = 200;
    private static int difficulty = 1;

    private static Direction direction = Direction.Right;

    private static void Main(string[] args)
    {
        Console.CursorVisible = false;
        var playAgain = true;

        ShowWelcomeScreen();

        while (playAgain)
        {
            difficulty = ChooseDifficulty();

            InitializeGame();

            while (!gameOver)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    HandleInput(key);
                }

                if (!gamePaused)
                {
                    MoveSnake();
                    CheckCollision();
                    if (snake[0].X == food.X && snake[0].Y == food.Y)
                    {
                        EatFood();
                    }
                }

                Draw();
                Thread.Sleep(gameSpeed);
            }

            playAgain = ShowGameOverScreen();
        }
    }

    private static void ShowWelcomeScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Vítejte ve hře Had!");
        Console.WriteLine("Vytvořeno AI asistentem Claude");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Jak hrát:");
        Console.WriteLine("- Použijte šipky pro ovládání hada");
        Console.WriteLine("- Sbírejte jídlo (@@) pro růst a získávání bodů");
        Console.WriteLine("- Vyhněte se nárazům do zdi, překážek a do vlastního těla");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Ovládání:");
        Console.WriteLine("- Šipky: Pohyb hada");
        Console.WriteLine("- P: Pozastavení/Obnovení hry");
        Console.WriteLine("- Q: Ukončení hry");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Pravidla:");
        Console.WriteLine("1. Had roste s každým snězeným jídlem");
        Console.WriteLine("2. Každé jídlo je za 10 bodů");
        Console.WriteLine("3. Had zrychlí po každých 5 snězených jídlech");
        Console.WriteLine("4. Hra končí při nárazu do zdi, překážky nebo do vlastního těla");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Obtížnosti:");
        Console.WriteLine("1: Bez překážek");
        Console.WriteLine("2-5: Rostoucí počet překážek");
        Console.WriteLine("Na obtížnostech 1-3 se jídlo negeneruje těsně u překážek a stěn");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Hra se spustí po stisku klávesy Enter.");
        Console.ReadLine();
    }

    private static int ChooseDifficulty()
    {
        Draw(); // Nejprve vykreslíme herní plochu

        var boxHeight = 5;
        var boxWidth = 30;
        int boxTop = (height - boxHeight) / 2;
        int boxLeft = width - boxWidth / 2;

        // Vykreslení rámečku
        Console.ForegroundColor = ConsoleColor.White;
        for (var i = 0; i < boxHeight; i++)
        {
            Console.SetCursorPosition(boxLeft, boxTop + i);
            if (i == 0)
            {
                Console.Write("╔" + new string('═', boxWidth - 2) + "╗");
            }
            else if (i == boxHeight - 1)
            {
                Console.Write("╚" + new string('═', boxWidth - 2) + "╝");
            }
            else
            {
                Console.Write("║" + new string(' ', boxWidth - 2) + "║");
            }
        }

        // Vypsání textu výzvy
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteTextCentered("Zvolte obtížnost (1-5):", boxTop + 2);

        var chosenDifficulty = 0;
        while (chosenDifficulty == 0)
        {
            char key = Console.ReadKey(true).KeyChar;
            if (key >= '1' && key <= '5')
            {
                chosenDifficulty = int.Parse(key.ToString());
            }
        }

        return chosenDifficulty;
    }

    private static void InitializeGame()
    {
        snake.Clear();
        obstacles.Clear();
        GenerateObstacles();

        // Hledání vhodné startovní pozice
        Position startPosition = FindBestStartPosition();
        snake.Add(startPosition);

        GenerateFood();
        score = 0;
        foodEaten = 0;
        direction = DetermineInitialDirection(startPosition);
        gameOver = false;
        gamePaused = false;

        gameSpeed = difficulty switch
        {
            1 => SPEED_EASIEST,
            2 => SPEED_EASY,
            3 => SPEED_MEDIUM,
            4 => SPEED_HARD,
            5 => SPEED_HARDEST,
            _ => SPEED_MEDIUM
        };
    }

    private static Position FindBestStartPosition()
    {
        var maxSpace = 0;
        var bestPosition = new Position {X = width / 2, Y = height / 2};

        for (var x = 1; x < width - 1; x++)
        {
            for (var y = 1; y < height - 1; y++)
            {
                if (!obstacles.Any(o => o.X == x && o.Y == y))
                {
                    int space = CalculateFreeSpace(x, y);
                    if (space > maxSpace)
                    {
                        maxSpace = space;
                        bestPosition = new Position {X = x, Y = y};
                    }
                }
            }
        }

        return bestPosition;
    }

    private static int CalculateFreeSpace(int x, int y)
    {
        var space = 0;
        int checkDistance = difficulty switch
        {
            1 => 3,
            2 => 4,
            3 => 5,
            4 => 6,
            5 => 7,
            _ => 5
        };

        for (int dx = -1; dx <= 1; dx += 2)
        {
            for (var i = 1; i <= checkDistance; i++)
            {
                int newX = x + dx * i;
                if (newX < 0 || newX >= width || obstacles.Any(o => o.X == newX && o.Y == y))
                {
                    break;
                }

                space++;
            }
        }

        for (int dy = -1; dy <= 1; dy += 2)
        {
            for (var i = 1; i <= checkDistance; i++)
            {
                int newY = y + dy * i;
                if (newY < 0 || newY >= height || obstacles.Any(o => o.X == x && o.Y == newY))
                {
                    break;
                }

                space++;
            }
        }

        return space;
    }

    private static Direction DetermineInitialDirection(Position startPosition)
    {
        int rightSpace = 0, leftSpace = 0, upSpace = 0, downSpace = 0;

        for (int i = startPosition.X + 1; i < width && !obstacles.Any(o => o.X == i && o.Y == startPosition.Y); i++)
        {
            rightSpace++;
        }

        for (int i = startPosition.X - 1; i >= 0 && !obstacles.Any(o => o.X == i && o.Y == startPosition.Y); i--)
        {
            leftSpace++;
        }

        for (int i = startPosition.Y - 1; i >= 0 && !obstacles.Any(o => o.X == startPosition.X && o.Y == i); i--)
        {
            upSpace++;
        }

        for (int i = startPosition.Y + 1; i < height && !obstacles.Any(o => o.X == startPosition.X && o.Y == i); i++)
        {
            downSpace++;
        }

        int maxSpace = Math.Max(Math.Max(rightSpace, leftSpace), Math.Max(upSpace, downSpace));

        if (maxSpace == rightSpace)
        {
            return Direction.Right;
        }

        if (maxSpace == leftSpace)
        {
            return Direction.Left;
        }

        if (maxSpace == upSpace)
        {
            return Direction.Up;
        }

        return Direction.Down;
    }


    private static void GenerateObstacles()
    {
        if (difficulty == 1)
        {
            return;
        }

        int obstacleCount = (difficulty - 1) * 2;
        for (var i = 0; i < obstacleCount; i++)
        {
            bool isLShape = random.Next(2) == 0;
            var newObstacle = new List<Position>();

            if (isLShape)
            {
                int length1 = random.Next(2, 4);
                int length2 = random.Next(2, 4);
                int rotation = random.Next(4);

                for (var j = 0; j < length1; j++)
                {
                    newObstacle.Add(new Position {X = 0, Y = j});
                }

                for (var j = 1; j < length2; j++)
                {
                    newObstacle.Add(new Position {X = j, Y = length1 - 1});
                }

                for (var r = 0; r < rotation; r++)
                {
                    newObstacle = newObstacle.Select(p => new Position {X = p.Y, Y = -p.X}).ToList();
                }
            }
            else
            {
                int length = random.Next(3, 6);
                bool isHorizontal = random.Next(2) == 0;

                for (var j = 0; j < length; j++)
                {
                    newObstacle.Add(new Position {X = isHorizontal ? j : 0, Y = isHorizontal ? 0 : j});
                }
            }

            int offsetX, offsetY;
            do
            {
                offsetX = random.Next(width - newObstacle.Max(p => p.X));
                offsetY = random.Next(height - newObstacle.Max(p => p.Y));
            } while (newObstacle.Any(p =>
                         snake.Any(s => s.X == p.X + offsetX && s.Y == p.Y + offsetY) ||
                         obstacles.Any(o => o.X == p.X + offsetX && o.Y == p.Y + offsetY)));

            obstacles.AddRange(newObstacle.Select(p => new Position {X = p.X + offsetX, Y = p.Y + offsetY}));
        }
    }

    private static void GenerateFood()
    {
        bool validPosition;
        do
        {
            food = new Position {X = random.Next(0, width), Y = random.Next(0, height)};
            validPosition = !snake.Any(s => s.X == food.X && s.Y == food.Y) &&
                            !obstacles.Any(o => o.X == food.X && o.Y == food.Y);

            if (difficulty <= 3)
            {
                validPosition = validPosition &&
                                food.X > 0 && food.X < width - 1 &&
                                food.Y > 0 && food.Y < height - 1 &&
                                !obstacles.Any(o => Math.Abs(o.X - food.X) <= 1 && Math.Abs(o.Y - food.Y) <= 1);
            }
        } while (!validPosition);
    }

    private static void HandleInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                if (direction != Direction.Down)
                {
                    direction = Direction.Up;
                }

                break;
            case ConsoleKey.DownArrow:
                if (direction != Direction.Up)
                {
                    direction = Direction.Down;
                }

                break;
            case ConsoleKey.LeftArrow:
                if (direction != Direction.Right)
                {
                    direction = Direction.Left;
                }

                break;
            case ConsoleKey.RightArrow:
                if (direction != Direction.Left)
                {
                    direction = Direction.Right;
                }

                break;
            case ConsoleKey.P:
                gamePaused = !gamePaused;
                break;
            case ConsoleKey.Q:
                gameOver = true;
                break;
        }
    }

    private static void MoveSnake()
    {
        var newHead = new Position {X = snake.First().X, Y = snake.First().Y};

        switch (direction)
        {
            case Direction.Up:
                newHead.Y--;
                break;
            case Direction.Down:
                newHead.Y++;
                break;
            case Direction.Left:
                newHead.X--;
                break;
            case Direction.Right:
                newHead.X++;
                break;
        }

        snake.Insert(0, newHead);
        if (snake.Count > score / 10 + 1)
        {
            snake.RemoveAt(snake.Count - 1);
        }
    }

    private static void CheckCollision()
    {
        Position head = snake.First();

        if (head.X < 0 || head.Y < 0 || head.X >= width || head.Y >= height ||
            obstacles.Any(o => o.X == head.X && o.Y == head.Y) ||
            snake.Skip(1).Any(b => b.X == head.X && b.Y == head.Y))
        {
            gameOver = true;
        }
    }

    private static void EatFood()
    {
        score += 10;
        foodEaten++;

        if (foodEaten % 5 == 0 && gameSpeed > 50)
        {
            int speedIncrease = difficulty switch
            {
                1 => 5,
                2 => 7,
                3 => 10,
                4 => 12,
                5 => 15,
                _ => 10
            };
            gameSpeed = Math.Max(50, gameSpeed - speedIncrease);
        }

        GenerateFood();
    }

    private static void Draw()
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("╔" + new string('═', width * 2) + "╗");

        for (var y = 0; y < height; y++)
        {
            Console.Write("║");
            for (var x = 0; x < width; x++)
            {
                if (snake.Any(s => s.X == x && s.Y == y))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("██");
                }
                else if (obstacles.Any(o => o.X == x && o.Y == y))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("██");
                }
                else if (food.X == x && food.Y == y)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("@@");
                }
                else
                {
                    Console.Write("  ");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("║");
        }

        Console.WriteLine("╚" + new string('═', width * 2) + "╝");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Skóre: {score}  Rychlost: {(200 - gameSpeed) / 10 + 1}  Obtížnost: {difficulty}");
        if (gamePaused)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("HRA POZASTAVENA - Stiskněte P pro pokračování");
        }

        Console.ResetColor();
    }

    static bool ShowGameOverScreen()
    {
        int consoleHeight = Console.WindowHeight;
        int gameOverBoxHeight = 9;
        int gameOverBoxWidth = 40;
        int gameOverBoxTop = consoleHeight - gameOverBoxHeight - 1 < height + 2 
            ? (height - gameOverBoxHeight) / 2 
            : height + 3;

        Draw();

        Console.ForegroundColor = ConsoleColor.White;
        for (int i = 0; i < gameOverBoxHeight; i++)
        {
            Console.SetCursorPosition((width * 2 - gameOverBoxWidth) / 2, gameOverBoxTop + i);
            if (i == 0)
                Console.Write("╔" + new string('═', gameOverBoxWidth - 2) + "╗");
            else if (i == gameOverBoxHeight - 1)
                Console.Write("╚" + new string('═', gameOverBoxWidth - 2) + "╝");
            else
                Console.Write("║" + new string(' ', gameOverBoxWidth - 2) + "║");
        }

        Console.ForegroundColor = ConsoleColor.Red;
        WriteTextCentered("Konec hry!", gameOverBoxTop + 1);
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteTextCentered($"Vaše konečné skóre: {score}", gameOverBoxTop + 3);
        WriteTextCentered($"Snězeno jídla: {foodEaten}", gameOverBoxTop + 4);
        WriteTextCentered($"Konečná rychlost: {(200 - gameSpeed) / 10 + 1}", gameOverBoxTop + 5);
        WriteTextCentered($"Obtížnost: {difficulty}", gameOverBoxTop + 6);

        Console.ForegroundColor = ConsoleColor.Cyan;
        WriteTextCentered("Hrát další hru? (A/N)", gameOverBoxTop + 7);

        while (true)
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.A)
                return true;
            if (key == ConsoleKey.N)
                return false;
        }
    }

    private static void WriteTextCentered(string text, int top)
    {
        int left = (width * 2 - text.Length) / 2;
        Console.SetCursorPosition(left, top);
        Console.Write(text);
    }

    private class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}