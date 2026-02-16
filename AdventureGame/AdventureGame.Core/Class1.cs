using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace AdventureGame.Core;

public interface ICharacter
{
    public abstract void Attack(ICharacter opponent);
    public abstract void TakeDamage(int amount);
}

public class Player : ICharacter
{
    public string Name;
    private Health HP;
    public Inventory PlayerInventory { get;  }

    public Player(string name)
    {
        Name = name;
        HP = new Health(100);
        PlayerInventory = new Inventory();
    }

    public void Attack(Monster monster)
    { 
        int attackpower = 10 + PlayerInventory.StrongestWeapon();
        monster.TakeDamage(attackpower);
    }

    public void Attack(ICharacter opponent)
    {
        if (opponent is Monster monster)
        {
            Attack(monster);
        } 
    }

    public void TakeDamage(int amount)
    {
        HP.TakeDamage(amount);
    }
    public void Heal(int amount)
    {
        HP.Heal(amount);
    }
    public bool IsDead()
    {
        return HP.IsDead();
    }
}

public class Health
{
    private int CurrentHealth;

    public Health(int currenthealth)
    {
        CurrentHealth = currenthealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

    }
    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > 150)
            CurrentHealth = 150;
    }
    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }
}

public class Inventory
{
    private List<Weapon> WeaponsList;

    public Inventory()
    {
        WeaponsList = new List<Weapon>();
    }

    public void AddWeapon(Weapon weapon)
    {
        WeaponsList.Add(weapon);
    }
    public int StrongestWeapon()
    {
        if (WeaponsList.Count == 0)
            return 0;

        int currentstrongest = WeaponsList[0].Damage;

        foreach (Weapon weapon in WeaponsList)
        {
            if (weapon.Damage > currentstrongest)
                currentstrongest = weapon.Damage;
        }
        return currentstrongest;
    }

}

public class Monster : ICharacter
{
    private Health HP;
    private int Damage;

    public Monster(int hp, int damage)
    {
        if (hp < 30 || hp > 50)
        {
            throw new ArgumentException("Health points must be in between 30 and 50.");
        }
        if (damage < 10 || damage > 30)
        {
            throw new ArgumentException("Damage points must be in between 10 and 30.");
        }
        HP = new Health(hp);
        Damage = damage;
    }

    public void Attack(Player player)
    {
        player.TakeDamage(Damage);
    }

    public void Attack(ICharacter opponent)
    {
        if (opponent is Player player)
        {
            Attack(player);
        }
    }

    public void TakeDamage(int amount)
    {
        HP.TakeDamage(amount);
    }

    public bool IsDead()
    {
        return HP.IsDead();
    }
}

public abstract class Item
{
    public string Name;

    public abstract void PickupMessage();
}

public class Weapon : Item
{
    public int Damage { get; }

    public Weapon(string name, int damage)
    {
        Name = name;
        Damage = damage;
    }
    public override void PickupMessage()
    {
        Console.WriteLine($"You just picked up {Name}, it has {Damage} in damage points.");
    }
}

public class Potion : Item
{
    private int HealthPoints;

    public Potion(string name)
    {
        Name = name;
        HealthPoints = 20;
    }
    public override void PickupMessage()
    {
        Console.WriteLine($"You just picked up {Name}, it has {HealthPoints} in health points.");
    }
}

public abstract class Tile
{
    public abstract char Symbol();
}

public class WallTile : Tile
{
    public override char Symbol()
    {
        return '#';
    }
}

public class EmptySpaceTile : Tile
{
    public override char Symbol()
    {
        return '.';
    }
}

public class PlayerTile : Tile
{
    public readonly Player ActualPlayer;

    public PlayerTile(Player actualplayer)
    {
        ActualPlayer = actualplayer;
    }
    public override char Symbol()
    {
        return '@';
    }
}

public class MonsterTile : Tile
{
    public readonly Monster ActualMonster;

    public MonsterTile(Monster actualmonster)
    {
        ActualMonster = actualmonster;
    }
    public override char Symbol()
    {
        return 'M';
    }
}

public class WeaponTile : Tile
{
    public readonly Weapon ActualWeapon;

    public WeaponTile(Weapon actualweapon)
    {
        ActualWeapon = actualweapon;
    }
    public override char Symbol()
    {
        return 'W';
    }
}

public class PotionTile : Tile
{
    public readonly Potion ActualPotion;

    public PotionTile(Potion actualpotion)
    {
        ActualPotion = actualpotion;
    }
    public override char Symbol()
    {
        return 'P';
    }
}

public class ExitTile : Tile
{
    public override char Symbol()
    {
        return 'E';
    }
}

public class Maze
{
    private Tile[,] Size;
    private int Rows;
    private int Columns;

    public Maze()
    {}

    public void StartMaze()
    {
        int rows; 

        while (true)
        {
            Console.WriteLine("Enter number of rows you want for the maze(must be atleast 10)");
            string input = Console.ReadLine();
            if (int.TryParse(input, out rows) && rows >= 10)
            {
                break;
            }
            Console.WriteLine("Invalid input. Try again");
        }
        Rows = rows;

        int columns;

        while (true)
        {
            Console.WriteLine("Enter number of columns you want for the maze(must be atleast 10)");
            string input = Console.ReadLine();
            if (int.TryParse(input, out columns) && columns >= 10)
            {
                break;
            }
            Console.WriteLine("Invalid input. Try again");
        }
        Columns = columns;

        Size = new Tile[Rows, Columns];

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                Size[r, c] = new EmptySpaceTile();
            }
        }
    }

    private Random random = new Random();
    private (int playerrow, int playercolumn) PlayerPosition;
    private (int exitrow, int exitcolumn) ExitPosition;

    public void PlacePlayer()
    {
        Console.WriteLine("Enter your player name:");
        string playername = Console.ReadLine();

        Player NewPlayer = new Player(playername);
        int PlayerRow = 0;
        int PlayerColumn = 0;

        PlayerPosition = (PlayerRow, PlayerColumn);

        Size[PlayerRow, PlayerColumn] = new PlayerTile(NewPlayer);
        
    }

    public void PlaceExit()
    {

        int ExitRow = Rows - 1;
        int ExitColumn = Columns - 1;

        ExitPosition = (ExitRow, ExitColumn);

        Size[ExitRow, ExitColumn] = new ExitTile();

    }

    public List<(int row, int col)> PathTiles = new List<(int row, int col)>();

    public void CarvePath()
    {
        PathTiles.Clear();
        int CurrentRow = 0;
        int CurrentColumn = 0;

        int ExitRow = Rows - 1;
        int ExitColumn = Columns - 1;

        
        PathTiles.Add((CurrentRow, CurrentColumn));

        
        while (CurrentRow != ExitRow || CurrentColumn != ExitColumn)
        {
            
            bool moveVertical = random.Next(0, 2) == 0;

            if (moveVertical && CurrentRow < ExitRow)
            {
                CurrentRow++;
            }
            else if (CurrentColumn < ExitColumn)
            {
                CurrentColumn++;
            }
            else if (CurrentRow < ExitRow) 
            {
                CurrentRow++;
            }

            
            if (!PathTiles.Contains((CurrentRow, CurrentColumn)))
            {
                PathTiles.Add((CurrentRow, CurrentColumn));
            }
        }        
    }

    private int WallsUsed;
    private int MonstersUsed;
    private int WeaponsUsed;
    private int PotionsUsed;

    public void TileNumberCalculator()
    {
        int TotalTiles = Rows * Columns;
        int NonPathTiles = TotalTiles - PathTiles.Count;
        int WantedWalls = TotalTiles / 2;
        int AvailableWalls = NonPathTiles / 2;
        WallsUsed = Math.Min(WantedWalls, AvailableWalls);
        int TilesLeft = NonPathTiles - WallsUsed;
        MonstersUsed = TilesLeft / 5;
        WeaponsUsed = TilesLeft / 10;
        PotionsUsed = TilesLeft / 10;
    }
    public void PlaceWalls()
    {
        int WallsPlaced = 0;

        while (WallsPlaced < WallsUsed)
        {
            int r = random.Next(0, Rows);
            int c = random.Next(0, Columns);

            if (Size[r, c] is EmptySpaceTile && !PathTiles.Contains((r, c)))
            {
                Size[r, c] = new WallTile();
                WallsPlaced++;
            }
        }

    }

    public void PlaceMonsters()
    {
        int MonstersPlaced = 0;

        while (MonstersPlaced < MonstersUsed)
        {
            int r = random.Next(0, Rows);
            int c = random.Next(0, Columns);

            if (Size[r,c] is EmptySpaceTile)
            {
                int health = random.Next(30, 51);
                int damage = random.Next(10, 31);

                Monster NewMonster = new Monster(health, damage);
                Size[r, c] = new MonsterTile(NewMonster);

                MonstersPlaced++;
            }
        }
    }

    public void PlaceWeapons()
    {
        int WeaponsPlaced = 0;

        while (WeaponsPlaced < WeaponsUsed)
        {
            int r = random.Next(0, Rows);
            int c = random.Next(0, Columns);

            if (Size[r, c] is EmptySpaceTile)
            {
                string[] adjectives = { "ancient", "blazing", "mystic" };
                string[] type = { "sword", "hammer", "spear" };

                string firstword = adjectives[random.Next(0, 3)];
                string secondword = type[random.Next(0, 3)];
                string name = $"{firstword} {secondword}";
                int damage = random.Next(10, 31);

                Weapon NewWeapon = new Weapon(name, damage);
                Size[r, c] = new WeaponTile(NewWeapon);

                WeaponsPlaced++;
            }
        }
    }

    public void PlacePotions()
    {
        int PotionsPlaced = 0;

        while (PotionsPlaced < PotionsUsed)
        {
            int r = random.Next(0, Rows);
            int c = random.Next(0, Columns);

            if (Size[r, c] is EmptySpaceTile)
            {
                string[] type = { "gold", "dark", "silver" };

                string firstword = type[random.Next(0, 3)];
                string name = $"{firstword} elixir";

                Potion NewPotion = new Potion(name);
                Size[r, c] = new PotionTile(NewPotion);

                PotionsPlaced++;           
            }
        }
    }

    public (int rowchange, int columnchange) GetMovement(ConsoleKey Key)
    {
        int rowchange = 0; 
        int columnchange = 0;

        switch(Key)
        {
            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                rowchange = -1;
                break;
            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                rowchange = 1;
                break;
            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                columnchange = -1;
                break;
            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                columnchange = 1;
                break;
            default:
                break;
        }
        return (rowchange, columnchange);
    }
    public bool GameOver { get; private set; } = false;
    public bool MazeCompleted { get; private set; } = false;

    public void PrintMaze()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                Console.Write(Size[r, c].Symbol());
            }
            Console.WriteLine();
        }
    }

    public void MovePlayer(int rowchange, int columnchange)
    {
        int CurrentRowPosition = PlayerPosition.playerrow + rowchange;
        int CurrentColumnPosition = PlayerPosition.playercolumn + columnchange;

        Tile DestinationTile = Size[CurrentRowPosition, CurrentColumnPosition];

        if (CurrentRowPosition < 0 || CurrentRowPosition > Rows - 1 || CurrentColumnPosition < 0 || CurrentColumnPosition > Columns - 1 || DestinationTile is WallTile)
        {
            Console.WriteLine("Invalid Movement.");
            Console.ReadKey(true);
            Console.Clear();
            PrintMaze();
            return;
        }

        Player CurrentPlayer = ((PlayerTile)Size[PlayerPosition.playerrow, PlayerPosition.playercolumn]).ActualPlayer;

        if (DestinationTile is MonsterTile monstertile)
        {
            Monster monster = monstertile.ActualMonster;
            Console.WriteLine("\n You have entered a battle with a monster.");
            
            while (!monster.IsDead() && !CurrentPlayer.IsDead())
            {
                CurrentPlayer.Attack(monster);
                Console.WriteLine(" You have attacked the monster. ");

                if (!monster.IsDead())
                {
                    monster.Attack(CurrentPlayer);
                    Console.WriteLine("The monster has attacked you");
                }
                Console.ReadKey(true);
            }

            if (CurrentPlayer.IsDead())
            {
                Console.WriteLine($"{CurrentPlayer.Name} has died. GAME OVER");
                Console.ReadKey(true);  
                 GameOver = true;
                return;
            }

            Size[CurrentRowPosition, CurrentColumnPosition] = new PlayerTile(CurrentPlayer);
            Console.WriteLine("You won the battle against the monster.");
            Console.ReadKey();
            Console.Clear();
            PrintMaze();
            return;       
        }

        if (DestinationTile is WeaponTile weaponTile)
        {
            CurrentPlayer.PlayerInventory.AddWeapon(weaponTile.ActualWeapon);
            weaponTile.ActualWeapon.PickupMessage();
            Console.ReadKey(true);
        }
        if (DestinationTile is PotionTile potionTile)
        {
            CurrentPlayer.Heal(20);
            potionTile.ActualPotion.PickupMessage();
            Console.ReadKey(true);
        }
        if (DestinationTile is ExitTile)
        {
            Console.Clear();
            PrintMaze();
            Console.WriteLine("Maze completed. YOU WON");
            Console.ReadKey(true);
            MazeCompleted = true;
        }

        Size[PlayerPosition.playerrow, PlayerPosition.playercolumn] = new EmptySpaceTile();
        Size[CurrentRowPosition, CurrentColumnPosition] = new PlayerTile(CurrentPlayer);
        PlayerPosition = (CurrentRowPosition, CurrentColumnPosition);
        Console.Clear();
        PrintMaze();
    }
}
