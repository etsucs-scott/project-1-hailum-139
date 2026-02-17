using AdventureGame.Core;

class Program
{
    static void Main(string[] args)
    { //you need to run all these methods in these specific order to make sure it runs properly
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