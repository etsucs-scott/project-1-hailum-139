using AdventureGame.Core;

class Program
{
    static void Main(string[] args)
    { 
        Maze maze = new Maze();

        maze.StartMaze();
        maze.CarvePath();
        maze.TileNumberCalculator();
        maze.PlacePlayer();
        maze.PlaceExit();       
        maze.PlaceWalls();
        maze.PlaceMonsters();
        maze.PlaceWeapons();
        maze.PlacePotions();
        maze.PrintMaze();

        while (!maze.GameOver && !maze.MazeCompleted)
        { 
            ConsoleKey key = Console.ReadKey(true).Key;
            var (rowchange, columnchange) = maze.GetMovement(key);
            maze.MovePlayer(rowchange, columnchange);
        }
    }
}