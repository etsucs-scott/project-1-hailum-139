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
    private Health HP; // made health its own class to make tracking HP easier
    public Inventory PlayerInventory { get;  }// made inventory its own class for easier handling of weapons

    public Player(string name)
    {
        Name = name;
        HP = new Health(100); // making sure the base health level is 100 for all players
        PlayerInventory = new Inventory();
    }

    public void Attack(Monster monster) 
    { 
        int attackpower = 10 + PlayerInventory.StrongestWeapon();
        monster.TakeDamage(attackpower);
    }

    public void Attack(ICharacter opponent) // to fulfill the contract made with the ICharacter interface
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
            CurrentHealth = 150;//this makes sure that if the HP of a player it returns to the max value of 150
    }
    public bool IsDead()
    {
        return CurrentHealth <= 0; // Duiring battles this is the method we use to find out who lost
    }
}

public class Inventory
{
    private List<Weapon> WeaponsList;// since the size of the inventory is going to increase as you pick up more weapons we use a list

    public Inventory()
    {
        WeaponsList = new List<Weapon>();
    }

    public void AddWeapon(Weapon weapon)// method we use to pick up the weapon and add it to the inventory
    {
        WeaponsList.Add(weapon);
    }
    public int StrongestWeapon()// since a player can only use one weapon from the inventory we use this method to find the weapon that causes the most damage
    {
        if (WeaponsList.Count == 0)
            return 0; // this is to ensure that when the list is empty the method still returns an integer value so it doesn't break the attack method which calls on this method

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
        if (hp < 30 || hp > 50)// this is to ensure that if a monster where to be created their hp wouldnt be too low and make the game really easy or too high causing the game to be impossible to finish
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

public abstract class Tile// we will be using this class to print the characters that we use for the maze
{
    public abstract char Symbol();// each subclass will return a character that will represent them in the maze
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
    public readonly Player ActualPlayer;//this field is added because we want an actual player object to be found in the playertile so we can call the player methods while alao referencing the player tile

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
    public readonly Weapon ActualWeapon;//same as player and so will the rest

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

public class Maze//this is the class that everything happens in
{
    private Tile[,] Size;// we make an two dimensional array with tile as the data type because we will be using the fields in the tile subclasses to represent the other classes
    private int Rows;
    private int Columns;

    public Maze() // constructor for when we want to create a maze
    {}

    public void StartMaze()// this method lets the user decide the size of the maze they want to play and also places empty space tiles that will be very useful for the placement methods
    {
        int rows; 

        while (true)
        {
            Console.WriteLine("Enter number of rows you want for the maze(must be atleast 10)");
            string input = Console.ReadLine();
            if (int.TryParse(input, out rows) && rows >= 10)// incase the user doesnt input the appropriate size for the maze or uses a wrong data type
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
            if (int.TryParse(input, out columns) && columns >= 10)// does the same thing as before
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
                Size[r, c] = new EmptySpaceTile();//this two for loops ensure that every tile is turned into an empty space tile
            }
        }
    }

    private Random random = new Random(); // declaring these variable for later
    private (int playerrow, int playercolumn) PlayerPosition;
    private (int exitrow, int exitcolumn) ExitPosition;

    public void PlacePlayer()//places our player in the maze
    {
        Console.WriteLine("Enter your player name:");
        string playername = Console.ReadLine();

        Player NewPlayer = new Player(playername);//new player object that will be stored in the tile
        int PlayerRow = 0;
        int PlayerColumn = 0;// i wanted the player to always start at the top left corner

        PlayerPosition = (PlayerRow, PlayerColumn);

        Size[PlayerRow, PlayerColumn] = new PlayerTile(NewPlayer);
        
    }

    public void PlaceExit()
    {

        int ExitRow = Rows - 1;
        int ExitColumn = Columns - 1;//i wanted the exit to be on the opposite side of the player's starting position so i put it on the bottom right corner

        ExitPosition = (ExitRow, ExitColumn);

        Size[ExitRow, ExitColumn] = new ExitTile();

    }

    public List<(int row, int col)> PathTiles = new List<(int row, int col)>();//we create a new list to store all the coordinates of the path 

    public void CarvePath()//this method is used to ensure that there will always be a path from the player to the exit ensuring that the game is winnable
    {
        PathTiles.Clear();
        int CurrentRow = 0;
        int CurrentColumn = 0;

        int ExitRow = Rows - 1;
        int ExitColumn = Columns - 1;

        
        PathTiles.Add((CurrentRow, CurrentColumn));

        
        while (CurrentRow != ExitRow || CurrentColumn != ExitColumn)// this is to stop the loop when the path reach the exit
        {
            
            bool moveVertical = random.Next(0, 2) == 0;// this randomly decides to go vertical or horizontal

            if (moveVertical && CurrentRow < ExitRow)
            {
                CurrentRow++;//this is when you get a vertical move it keeps the path going down until it reaches the last row
            }
            else if (CurrentColumn < ExitColumn)
            {
                CurrentColumn++;//this is to keep the going to the right until it reaches the last column
            }
            else if (CurrentRow < ExitRow) //this does the same thing as the first one but for horizontal moves
            {
                CurrentRow++;
            }// these are to ensure the path keeps going to the bottom right corner

            
            if (!PathTiles.Contains((CurrentRow, CurrentColumn)))// this is to ensure that already listed coordinates dont keep getting relisted
            {
                PathTiles.Add((CurrentRow, CurrentColumn));
            }
        }        
    }

    private int WallsUsed;//declaring variables we'll be using for placement of these classes
    private int MonstersUsed;
    private int WeaponsUsed;
    private int PotionsUsed;

    public void TileNumberCalculator()//method used to decide the number of the types of tiles
    {
        int TotalTiles = Rows * Columns;
        int NonPathTiles = TotalTiles - PathTiles.Count;
        int WantedWalls = TotalTiles / 2;//a good amount of wall tiles would be half of all tiles
        int AvailableWalls = NonPathTiles / 2;//but if the path takes up more than that we want to the number of walls to accomdate that
        WallsUsed = Math.Min(WantedWalls, AvailableWalls);// the smaller of those two values will be what is used for wall placement
        int TilesLeft = NonPathTiles - WallsUsed;// we will be using the remaining tiles to calculate how much of the other classes should be placed
        MonstersUsed = TilesLeft / 5;
        WeaponsUsed = TilesLeft / 10;
        PotionsUsed = TilesLeft / 10;
    }
    public void PlaceWalls()
    {
        int WallsPlaced = 0;

        while (WallsPlaced < WallsUsed)//this loop places wall tiles until the it reaches the calculated number of walls to be placed and the rest of the tile placement methods use this too
        {
            int r = random.Next(0, Rows);
            int c = random.Next(0, Columns);

            if (Size[r, c] is EmptySpaceTile && !PathTiles.Contains((r, c)))//making sure the pathtiles, player, exit tiles are not replaced by walls
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

            if (Size[r,c] is EmptySpaceTile)//the rest of the placements are allowed to be placed in path since they wont prevent the player from passing through
            {
                int health = random.Next(30, 51);
                int damage = random.Next(10, 31);//used random to generate monster of different levels randomly

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
                string[] type = { "sword", "hammer", "spear" };// random name generators

                string firstword = adjectives[random.Next(0, 3)];
                string secondword = type[random.Next(0, 3)];
                string name = $"{firstword} {secondword}";
                int damage = random.Next(10, 31);//weapons of different powers are randomly generated

                Weapon NewWeapon = new Weapon(name, damage);
                Size[r, c] = new WeaponTile(NewWeapon);

                WeaponsPlaced++;
            }
        }
    }

    public void PlacePotions()//same concept as the previous placetile methods
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

    public (int rowchange, int columnchange) GetMovement(ConsoleKey Key)//method to recieve the input from the user and to translate it to maze terms
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
    public bool GameOver { get; private set; } = false;// to be used when the game stops because the player died
    public bool MazeCompleted { get; private set; } = false;// to be used when the game stops because the player reached the exit

    public void PrintMaze()//shows the maze on the screen
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
        {//this statemtent is used when the player tries to go outside the walls or tries to walk into walss 
            Console.WriteLine("Invalid Movement."); 
            Console.ReadKey(true);
            Console.Clear();
            PrintMaze();
            return;
        }

        Player CurrentPlayer = ((PlayerTile)Size[PlayerPosition.playerrow, PlayerPosition.playercolumn]).ActualPlayer;

        if (DestinationTile is MonsterTile monstertile)//when a player enters a monster tile
        {
            Monster monster = monstertile.ActualMonster;
            Console.WriteLine("\n You have entered a battle with a monster.");
            
            while (!monster.IsDead() && !CurrentPlayer.IsDead())//this is to be shown when both the player and monster are alive
            {
                CurrentPlayer.Attack(monster);//first attack is the player's
                Console.WriteLine(" You have attacked the monster. ");

                if (!monster.IsDead())//if the monste survives it gets to hit back
                {
                    monster.Attack(CurrentPlayer);
                    Console.WriteLine("The monster has attacked you");
                }
                Console.ReadKey(true);//this will continue until one of the characters die
            }

            if (CurrentPlayer.IsDead())// if the player dies run the game over method
            {
                Console.WriteLine($"{CurrentPlayer.Name} has died. GAME OVER");
                Console.ReadKey(true);  
                 GameOver = true;
                return;
            }
            //if not the player takes over the monster's tile and the game continues
            Size[CurrentRowPosition, CurrentColumnPosition] = new PlayerTile(CurrentPlayer);
            Console.WriteLine("You won the battle against the monster.");
            Console.ReadKey();
            Console.Clear();
            PrintMaze();
            return;       
        }

        if (DestinationTile is WeaponTile weaponTile)//if player enters a weapon tile add weapon to inventory and show a message
        {
            CurrentPlayer.PlayerInventory.AddWeapon(weaponTile.ActualWeapon);
            weaponTile.ActualWeapon.PickupMessage();
            Console.ReadKey(true);
        }
        if (DestinationTile is PotionTile potionTile)//if player enters a potion tile add 20 HP and show pickup message
        {
            CurrentPlayer.Heal(20);
            potionTile.ActualPotion.PickupMessage();
            Console.ReadKey(true);
        }
        if (DestinationTile is ExitTile)//if player enters exit tile run the maze completed method
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
