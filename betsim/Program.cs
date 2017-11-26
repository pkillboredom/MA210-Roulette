using System;
using System.IO;
using System.Security.Cryptography;

namespace betsim
{
    class Program
    {
        const decimal StartingAmount = 500;
        const decimal MinimumBettingAmount = 5;

        static void Main(string[] args)
        {
            bool mainLoopFlag = true;
            while (mainLoopFlag)
            {
                Console.WriteLine("Welcome to the Roulette Simulation System.\nThis System renders its outputs to a CSV file in the working directory, labeled with the current date and time, followed by the bet methodology.");
                Console.WriteLine("What do you want to simulate?");
                Console.WriteLine("1. Random Bets");
                Console.WriteLine("\n0. Exit");
                Console.Write("Enter your choice: ");
                string line = Console.ReadLine();
                uint opt;
                try
                {
                    opt = uint.Parse(line);
                    switch (opt)
                    {
                        case 0:
                            mainLoopFlag = false;
                            break;
                        case 1:
                            Console.Write("Performing RandomBets... ");
                            RandomBets();
                            Console.WriteLine("Done!");
                            break;
                        default:
                            Console.WriteLine("Invalid Entry");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid Entry");
                }
            }
        }
        
        static void RandomBets()
        {
            StreamWriter streamWriter = initializeCSVStream("RandomBets");
            decimal balance = StartingAmount;
            const decimal bettingAmount = MinimumBettingAmount; // we will bet the same amount every turn, so this is const.

            TableSpace betSpace;
            TableSpace rolledSpace;
            RouletteBet rouletteBet;
            decimal turnWinnings;
            string writeString;
            RouletteTable rouletteTable = new RouletteTable();

            for (int i = 0; i < 1500; i++)
            {
                if(balance - bettingAmount <= 0) // Minimum bet is $5.
                {
                    break;
                }
                // Write the initial balance to the writeString
                writeString = balance.ToString() + ", ";

                betSpace = rouletteTable.RandomBetSpace();

                // When implementing any of these betting systems, be diligent about the chain of custody.
                // That is to say, DONT LEAK MONEY!
                // In a real-world system, you would implement more rigorous structs to keep chain of custody.

                balance = balance - bettingAmount; // We've given our money to the system.
                rouletteBet = new RouletteBet(betSpace, bettingAmount); // The system takes our money.
                rolledSpace = rouletteTable.RollWheel();
                turnWinnings = rouletteBet.CheckWinnings(rolledSpace);
                balance = balance + turnWinnings; // Any winnings from the turn are added to the balance.

                // Now we output the results to the CSV.
                // Initial Balance, Amount Bet, Amount Won, Final Balance \n
                writeString += bettingAmount.ToString() + ", " + turnWinnings.ToString() + ", " + balance.ToString() + ",";
                streamWriter.WriteLine(writeString);
            }
            streamWriter.Close();
        }

        static StreamWriter initializeCSVStream(String betName)
        {
            string time = DateTime.Now.ToString("yyyy-MM-ddTHH.MM.ss");
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "\\" + betName + "-" + time + ".csv");
                return new StreamWriter(path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(0x1);
                return new StreamWriter("WILL NOT EXECUTE");
            }
        }
    }

    /// <summary>
    /// A singular roulette bet. This should be called for each space that is bet on.
    /// </summary>
    class RouletteBet
    {
        public TableSpace BetSpace { get; private set; }
        public Decimal BetAmount { get; private set; }

        public RouletteBet(TableSpace tableSpace, Decimal betAmount)
        {
            MakeBet(tableSpace, betAmount);
        }

        /// <summary>
        /// Makes a bet on a single space.
        /// </summary>
        /// <param name="tableSpace">The space from your RouletteTable object that you wish to bet on.</param>
        /// <param name="betAmount">The amount to be bet.</param>
        public void MakeBet(TableSpace tableSpace, Decimal betAmount)
        {
            BetSpace = tableSpace;
            BetAmount = betAmount;
        }

        /// <summary>
        /// Makes a bet on two or four adjacent *numbered* spaces at once, as though betting on an edge or corner.
        /// </summary>
        /// <param name="tableSpaces">The array of spaces to bet on. These should be references to TableSpaces from the RouletteTable object.
        /// This method assumes that if four spaces are passed, they will be passed in numerical order.</param>
        /// <param name="betAmount">The amount to be bet.</param>
        /// <returns>Returns true if the bet is valid.</returns>
        public bool MakeBet(TableSpace[] tableSpaces, Decimal betAmount)
        {
            if (tableSpaces.Length != 2 || tableSpaces.Length != 4)
                return false;
            else if (tableSpaces.Length == 2)
            {
                // Check that these spaces are normal numbered spaces.
                if (tableSpaces[0].Value > 0 || tableSpaces[1].Value > 0)
                    return false;
                // The % 3 operation ensures that its not a wraparound.
                if ((tableSpaces[0].Value + 1 == tableSpaces[1].Value && tableSpaces[0].Value % 3 != 0) || (tableSpaces[0].Value - 1 == tableSpaces[1].Value && tableSpaces[1].Value % 3 != 0))
                {
                    BetSpace = new TableSpace(tableSpaces, 17);
                    BetAmount = betAmount;
                    return true;
                }
                else return false;
            }
            else
            {
                if (tableSpaces[0].Value > 0 || tableSpaces[1].Value > 0 || tableSpaces[2].Value > 0 || tableSpaces[3].Value > 0)
                    return false;
                if ((tableSpaces[0].Value + 1 == tableSpaces[1].Value && tableSpaces[0].Value % 3 != 0) || (tableSpaces[0].Value - 1 == tableSpaces[1].Value && tableSpaces[1].Value % 3 != 0))
                {
                    if ((tableSpaces[2].Value + 1 == tableSpaces[3].Value && tableSpaces[2].Value % 3 != 0) || (tableSpaces[2].Value - 1 == tableSpaces[3].Value && tableSpaces[3].Value % 3 != 0))
                    {
                        BetSpace = new TableSpace(tableSpaces, 8);
                        BetAmount = betAmount;
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
        }

        /// <summary>
        /// Checks if the space rolled by the wheel was covered in this bet.
        /// </summary>
        /// <param name="rolledSpace">The space rolled by the wheel.</param>
        /// <returns>The total amount won in this bet.</returns>
        public decimal CheckWinnings(TableSpace rolledSpace)
        {
            if (BetSpace.Value == rolledSpace.Value)
            {
                return BetAmount * BetSpace.PayoutMultiplier;
            }
            else if (BetSpace.ContainsSpaces != null)
            {
                foreach (TableSpace x in BetSpace.ContainsSpaces)
                {
                    if (x.Value == rolledSpace.Value)
                    {
                        return BetAmount * BetSpace.PayoutMultiplier;
                    }
                }
            }
            return 0; // You get nothing. You lose. Etc etc.
        }
    }

    class RouletteTable
    {
        public freakcode.Cryptography.CryptoRandom rng = new freakcode.Cryptography.CryptoRandom();

        public TableSpace[] NormalSpaces { get; private set; }
        public TableSpace TwoToOneA { get; private set; } //1, 4, 7...
        public TableSpace TwoToOneB { get; private set; } //2, 5, 8...
        public TableSpace TwoToOneC { get; private set; } //3, 6, 9...
        public TableSpace FirstDozen { get; private set; }
        public TableSpace SecondDozen { get; private set; }
        public TableSpace ThirdDozen { get; private set; }
        public TableSpace OneThroughEighteen { get; private set; }
        public TableSpace NineteenThroughThirtySix { get; private set; }
        public TableSpace Even { get; private set; }
        public TableSpace Odd { get; private set; }
        public TableSpace Red { get; private set; }
        public TableSpace Black { get; private set; }

        /// <summary>
        /// Creates a European (no 00 space) roulette table.
        /// </summary>
        public RouletteTable()
        {
            NormalSpaces = new TableSpace[37];
            for (int i = 0; i < 37; i++) NormalSpaces[i] = new TableSpace(i, 35);

            TableSpace[] TwoToOneASpaces = new TableSpace[12];
            for (int i = 1; i < 12; i++) TwoToOneASpaces[i] = NormalSpaces[i * 3 - 2];
            TwoToOneA = new TableSpace(TwoToOneASpaces, 2);

            TableSpace[] TwoToOneBSpaces = new TableSpace[12];
            for (int i = 2; i < 12; i++) TwoToOneBSpaces[i] = NormalSpaces[i * 3 - 4];
            TwoToOneB = new TableSpace(TwoToOneBSpaces, 2);

            TableSpace[] TwoToOneCSpaces = new TableSpace[12];
            for (int i = 3; i < 12; i++) TwoToOneCSpaces[i] = NormalSpaces[i * 3 - 6];
            TwoToOneC = new TableSpace(TwoToOneCSpaces, 2);

            TableSpace[] FirstDozenSpaces = new TableSpace[12];
            for (int i = 1; i < 13; i++) FirstDozenSpaces[i-1] = NormalSpaces[i];
            FirstDozen = new TableSpace(FirstDozenSpaces, 2);

            TableSpace[] SecondDozenSpaces = new TableSpace[12];
            for (int i = 13; i < 25; i++) SecondDozenSpaces[i-13] = NormalSpaces[i];
            SecondDozen = new TableSpace(SecondDozenSpaces, 2);

            TableSpace[] ThirdDozenSpaces = new TableSpace[12];
            for (int i = 25; i < 37; i++) ThirdDozenSpaces[i-25] = NormalSpaces[i];
            ThirdDozen = new TableSpace(ThirdDozenSpaces, 2);

            TableSpace[] FirstHalfSpaces = new TableSpace[18];
            for (int i = 1; i < 19; i++) FirstHalfSpaces[i-1] = NormalSpaces[i];
            OneThroughEighteen = new TableSpace(FirstHalfSpaces, 1);

            TableSpace[] SecondHalfSpaces = new TableSpace[18];
            for (int i = 19; i < 37; i++) SecondHalfSpaces[i-19] = NormalSpaces[i];
            NineteenThroughThirtySix = new TableSpace(SecondHalfSpaces, 1);

            TableSpace[] EvenSpaces = new TableSpace[18];
            for (int i = 2; i < 37; i += 2) EvenSpaces[i/2-1] = NormalSpaces[i];
            Even = new TableSpace(EvenSpaces, 1);

            TableSpace[] OddSpaces = new TableSpace[18];
            for (int i = 1; i < 37; i += 2) OddSpaces[i/2] = NormalSpaces[i];
            Odd = new TableSpace(OddSpaces, 1);

            Red = new TableSpace(new TableSpace[]
            {
                NormalSpaces[1],
                NormalSpaces[3],
                NormalSpaces[5],
                NormalSpaces[7],
                NormalSpaces[9],
                NormalSpaces[12],
                NormalSpaces[14],
                NormalSpaces[16],
                NormalSpaces[18],
                NormalSpaces[19],
                NormalSpaces[21],
                NormalSpaces[23],
                NormalSpaces[25],
                NormalSpaces[27],
                NormalSpaces[30],
                NormalSpaces[32],
                NormalSpaces[34],
                NormalSpaces[36]
            }, 1);

            Black = new TableSpace(new TableSpace[]
            {
                NormalSpaces[2],
                NormalSpaces[4],
                NormalSpaces[6],
                NormalSpaces[8],
                NormalSpaces[10],
                NormalSpaces[11],
                NormalSpaces[13],
                NormalSpaces[15],
                NormalSpaces[17],
                NormalSpaces[20],
                NormalSpaces[22],
                NormalSpaces[24],
                NormalSpaces[26],
                NormalSpaces[28],
                NormalSpaces[29],
                NormalSpaces[31],
                NormalSpaces[33],
                NormalSpaces[35]
            }, 1);
        }

        /// <summary>
        /// Rolls numbers from the crytographically secure roulette wheel and returns them as spaces.
        /// </summary>
        /// <returns></returns>
        public TableSpace RollWheel()
        {
            return NormalSpaces[rng.Next(0, 37)];
        }

        /// <summary>
        /// Picks a random space from all availiable board spaces from the Secure Crypto Provider.
        /// </summary>
        /// <returns>The randomly chosen space.</returns>
        public TableSpace RandomBetSpace()
        {
            int spaceRandInt = rng.Next(0, 49);
            if (spaceRandInt >= 0 && spaceRandInt < 37)
            {
                return NormalSpaces[spaceRandInt];
            }
            else
            {
                switch(spaceRandInt)
                {
                    case 37:
                        return TwoToOneA;
                        break;
                    case 38:
                        return TwoToOneB;
                        break;
                    case 39:
                        return TwoToOneC;
                        break;
                    case 40:
                        return FirstDozen;
                        break;
                    case 41:
                        return SecondDozen;
                        break;
                    case 42:
                        return ThirdDozen;
                        break;
                    case 43:
                        return OneThroughEighteen;
                        break;
                    case 44:
                        return NineteenThroughThirtySix;
                        break;
                    case 45:
                        return Even;
                        break;
                    case 46:
                        return Odd;
                        break;
                    case 47:
                        return Red;
                        break;
                    case 48:
                        return Black;
                        break;
                    default:
                        Console.WriteLine("Unreachable Code Has Been Reached!!! Tell a Programmer Error Code 0x2");
                        Environment.Exit(0x2);
                        return new TableSpace(99, 0);
                        break;
                }
            }
        }
    }

    struct TableSpace
    {
        public int Value { get; private set; }
        public TableSpace[] ContainsSpaces { get; private set; }
        public Decimal PayoutMultiplier { get; private set; }
        public TableSpace(int value, Decimal payoutModifier)
        {
            Value = value;
            ContainsSpaces = null;
            PayoutMultiplier = payoutModifier;
        }
        public TableSpace(TableSpace[] containsSpaces, decimal payoutModifier)
        {
            Value = -1;
            ContainsSpaces = containsSpaces;
            PayoutMultiplier = payoutModifier;
        }
    }
}
