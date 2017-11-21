using System;

namespace betsim
{
    class Program
    {
        static void Main(string[] args)
        {
            
        }
    }

    class RouletteBet
    {
        public TableSpace BetSpace { get; private set; }
        public Decimal BetAmount { get; private set; }

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
                    BetSpace = new TableSpace(tableSpaces);
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
                        BetSpace = new TableSpace(tableSpaces);
                        BetAmount = betAmount;
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
        }


        // TODO: Merge this method into a method which also calculates winnings, so that we only have to do this once.
        /// <summary>
        /// Checks if the space rolled by the wheel is covered in this bet. Does not calculate winnings.
        /// </summary>
        /// <param name="rolledSpace">The space whose number was rolled by the wheel.</param>
        /// <returns>Returns true if the space rolled was covered in this bet.</returns>
        public bool CheckBet (TableSpace rolledSpace)
        {
            if (BetSpace.Value == rolledSpace.Value) return true;
            else
            {
                if (BetSpace.ContainsSpaces != null) {
                    foreach (TableSpace x in BetSpace.ContainsSpaces)
                    {
                        if (x.Value == rolledSpace.Value) return true;
                    }
                }
                return false;
            }
        }
    }

    class RouletteTable
    {
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
            for (int i = 0; i < 37; i++) NormalSpaces[i] = new TableSpace(i);

            TableSpace[] TwoToOneASpaces = new TableSpace[12];
            for (int i = 1; i < 12; i++) TwoToOneASpaces[i] = NormalSpaces[i * 3 - 2];
            TwoToOneA = new TableSpace(TwoToOneASpaces);

            TableSpace[] TwoToOneBSpaces = new TableSpace[12];
            for (int i = 2; i < 12; i++) TwoToOneBSpaces[i] = NormalSpaces[i * 3 - 4];
            TwoToOneB = new TableSpace(TwoToOneBSpaces);

            TableSpace[] TwoToOneCSpaces = new TableSpace[12];
            for (int i = 3; i < 12; i++) TwoToOneCSpaces[i] = NormalSpaces[i * 3 - 6];
            TwoToOneC = new TableSpace(TwoToOneCSpaces);

            TableSpace[] FirstDozenSpaces = new TableSpace[12];
            for (int i = 1; i < 13; i++) FirstDozenSpaces[i] = NormalSpaces[i];
            FirstDozen = new TableSpace(FirstDozenSpaces);

            TableSpace[] SecondDozenSpaces = new TableSpace[12];
            for (int i = 13; i < 25; i++) SecondDozenSpaces[i] = NormalSpaces[i];
            SecondDozen = new TableSpace(SecondDozenSpaces);

            TableSpace[] ThirdDozenSpaces = new TableSpace[12];
            for (int i = 25; i < 37; i++) ThirdDozenSpaces[i] = NormalSpaces[i];
            ThirdDozen = new TableSpace(ThirdDozenSpaces);

            TableSpace[] FirstHalfSpaces = new TableSpace[18];
            for (int i = 1; i < 19; i++) FirstHalfSpaces[i] = NormalSpaces[i];
            OneThroughEighteen = new TableSpace(FirstHalfSpaces);

            TableSpace[] SecondHalfSpaces = new TableSpace[18];
            for (int i = 19; i < 37; i++) SecondHalfSpaces[i] = NormalSpaces[i];
            NineteenThroughThirtySix = new TableSpace(SecondHalfSpaces);

            TableSpace[] EvenSpaces = new TableSpace[18];
            for (int i = 2; i < 37; i += 2) EvenSpaces[i] = NormalSpaces[i];
            Even = new TableSpace(EvenSpaces);

            TableSpace[] OddSpaces = new TableSpace[18];
            for (int i = 1; i < 37; i += 2) OddSpaces[i] = NormalSpaces[i];
            Odd = new TableSpace(OddSpaces);

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
            });

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
            });
        }
    }

    struct TableSpace
    {
        public int Value { get; private set; }
        public TableSpace[] ContainsSpaces { get; private set; }
        public TableSpace(int value)
        {
            Value = value;
            ContainsSpaces = null;
        }
        public TableSpace(TableSpace[] containsSpaces)
        {
            Value = -1;
            ContainsSpaces = containsSpaces;
        }
    }
}
