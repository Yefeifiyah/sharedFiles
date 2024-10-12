// I changed how the game app accesses the word list to get the codeword,
// instead of creating a copy it now only makes a local list of the 
// words that are the size of the current codeword, then when a player
// guesses correctly it performs a count of the matches in the local
// word list. Then in display(), when it needs to display the total
// number of words, as well as to set the main codeword it accesses
// the other namespace, rather than making a 2000+ word local copy.
//
// the main() in words will be commented out in case you just run
// the file the way it is it can only have one main method. Or you
// can create the two console apps with 'dotnet new console'
// which you will need to create a solution file and then
// add each of the projects to it using 'dotnet sln add' then
// you specify which if you do a 'dotnet run' with --project,
// e.g. 'dotnet run --project game' or 'dotnet run --project words'.

namespace game
{
    internal class Program
    {
    /*  Extraterrestrials are abducting humans through tractor beams. 
    *  Players must guess the password before the 7th miss, otherwise 
    *  by then the guy has been sucked through the tube entirely.
    *
    *   Each wrong guess leaves one less part of the body. Player gets 
    *  choice to start new game or exit, as well as exit any time with 
    *  Control+C. A flag gets strategically set at forced-exit time to 
    *  prevent lines from getting executed and altering the colors. */

        public static bool isExiting = false;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            if (Console.BackgroundColor != ConsoleColor.White)
                Console.BackgroundColor = ConsoleColor.White;

            Console.WriteLine("\n\n\n\n\n");
            Game.Intro();

            bool keepAlive;
            do
            {
                Random rand = new();
                Game g = new(rand.Next(words.Program.wordsArray.Count));
       //         Game g = new(0);  //<-- for debugging

                while (!g.DidWin() && !g.DidLose())
                {
                    if (isExiting)
                        return;
                    g.Display();
                    g.Start();
                }

                if (isExiting)
                     return;

                g.Display();

            exit:

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(" PLAY AGAIN?(y/n):");

                Console.ForegroundColor = ConsoleColor.Black;
                string answer = Console.ReadLine() ?? "possible null assignment";

                if (!string.IsNullOrEmpty(answer))
                {
                    if (answer == answer.ToUpper())
                    {
                        answer = answer.ToLower();
                    }

                    if (answer == "n")
                    {
                        keepAlive = false;
                    }
                    else if (answer == "y")
                    {
                        keepAlive = true;
                    }
                    else
                    {
                        if (isExiting)
                            return;
                        g.Display();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(" need Y or N |");
                        goto exit;
                    }
                }
                else
                {
                    if (isExiting)
                        return;
                    g.Display();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(" NEED y OR n |");
                    goto exit;
                }
            } while (keepAlive);

            Console.ResetColor();
            Console.WriteLine("Exiting normally...");
        }

        protected static void OnExit(object? sender, ConsoleCancelEventArgs k)
        {
            isExiting = true;
            Console.ResetColor();
            Console.WriteLine("Force-exiting...");
            Environment.Exit(0);
        }
    }

    internal class Game
    {
        public UFO alienCraft = new UFO();

        private readonly string symbolsNumbersAndSpace = "`~!@#$%^&*()-_=+,.<>/?;:'\"[]{}\\| 0123456789";
        private readonly string alphasNumbersAndSpace = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz 0123456789";
        private readonly string symbolsAndNumbers = "`~!@#$%^&*()-_=+,.<>/?;:'\"[]{}\\|0123456789";
        private readonly string symbolsAndSpace = "`~!@#$%^&*()-_=+,.< >/?;:'\"[]{}\\|";
        private readonly string symbols = "`~!@#$%^&*()-_=+,.<>/?;:'\"[]{}\\|";
        private readonly string alphas = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
        private readonly string numbers = "0123456789";

    //  public readonly string[] codewordOptions; <-- comment out for testing

        public Game(int x)
        {
            this.MaxNumberOfGuesses = 7;
            this.CurrentWrongGuesses = 0;
            this.CurrentRightGuesses = 0;

    //  this.codewordOptions = [.. words.Program.wordsArray]; <- unnecessary

            this.Codeword = words.Program.wordsArray[x];
            int y = (this.Codeword.Length * 2) - 1;
            char[] emptyWordWithSpaces = new char[y];
            for (int z = 0; z < y; z++)
            {
                if (z % 2 == 0)
                    emptyWordWithSpaces[z] = '_';
                else
                    emptyWordWithSpaces[z] = ' ';
            }
            this.CurrentWord = new string(emptyWordWithSpaces);

            this.CodewordAsArray = new char[this.CurrentWord.Length];
            int superIndex = 0;
            for (int i = 0; i < this.CurrentWord.Length; i++)
            {
                if (i % 2 == 0)
                {
                    this.CodewordAsArray[i] = this.Codeword[superIndex];
                    ++superIndex;
                }
                else
                {
                    this.CodewordAsArray[i] = ' ';
                }
            }
            this.CodewordWithSpaces = new string(this.CodewordAsArray);

            this.MatchList = words.Program.wordsArray
                         .Where(u => u.Length == this.Codeword.Length)
                         .ToList();

            // fresh calculation for length counts on most current collection
            words.Program.GetLengthCounts();

            this.WordChoicesCount = 0;
            if (this.Codeword.Length == 13)
                this.WordChoicesCount = words.Program.ThirteensCount;
            else if (this.Codeword.Length == 12)
                this.WordChoicesCount = words.Program.TwelvesCount;
            else if (this.Codeword.Length == 11)
                this.WordChoicesCount = words.Program.ElevensCount;
            else if (this.Codeword.Length == 10)
                this.WordChoicesCount = words.Program.TensCount;
            else if (this.Codeword.Length == 9)
                this.WordChoicesCount = words.Program.NinesCount;
            else if (this.Codeword.Length == 8)
                this.WordChoicesCount = words.Program.EightsCount;
            else if (this.Codeword.Length == 7)
                this.WordChoicesCount = words.Program.SevensCount;
            else if (this.Codeword.Length == 6)
                this.WordChoicesCount = words.Program.SixesCount;
            else if (this.Codeword.Length == 5)
                this.WordChoicesCount = words.Program.FivesCount;
            else if (this.Codeword.Length == 4)
                this.WordChoicesCount = words.Program.FoursCount;
            else if (this.Codeword.Length == 3)
                this.WordChoicesCount = words.Program.ThreesCount;
            else
                this.WordChoicesCount = words.Program.TwosCount;

            this.InputAsChar = ' ';
            this.ErrorAsString = string.Empty;
            this.LttrsAlreadyUsed = [];

            this.WasWrong = false;
            this.ThereWasAnError = false;
            this.ItWasHitOrMiss = false;
        }

        public int MaxNumberOfGuesses { get; private set; }
        public int CurrentWrongGuesses { get; private set; }
        public int WordChoicesCount { get; }
        public int CurrentRightGuesses { get; private set; }
        public char InputAsChar { get; set; }
        public string Codeword { get; }
        public string CurrentWord { get; private set; }
        public string CodewordWithSpaces { get; }
        public string ErrorAsString { get; set; }
        public List<string> MatchList { get; }
        public char[] CodewordAsArray { get; }
        public List<char> LttrsAlreadyUsed { get; set; }
        public bool WasWrong { get; set; }
        public bool ThereWasAnError { get; set; }
        public bool ItWasHitOrMiss { get; set; }

        public static void Intro()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("            CRACK the CODEWORD");
            Console.WriteLine("          and STOP the ABDUCTION!");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("             ~~~*~~*~~*~~*~~~");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("    maximize panel to display images->");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("      set color filters to inverted->");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("  press Ctrl+C to exit|<Enter> to start=>");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.ReadLine();
        }

        public void Inform()
        {
            if (Program.isExiting)
                return;

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            if (DidLose())
                Console.WriteLine("       ~~*~*~  STUFF HAPPENS! ~*~*~~");
            else if (DidWin())
                Console.WriteLine("\n         ~~*~*~  RUN HOME!  ~*~*~~\n");
            else if (WasWrong && CurrentWrongGuesses > 0)
                Console.WriteLine("      ~~*~*~  SWING & A MISS!  ~*~*~~");
            else if (!WasWrong && CurrentRightGuesses > 0)
                Console.WriteLine("         ~~*~*~  BASE HIT!  ~*~*~~");
            else
                Console.WriteLine("          ~~*~*~ BATTER UP! ~*~*~~");
        }

        public void Display()
        {
            // necessary so color won't change on forced exit
            if (Program.isExiting)
                return;
            // loos ugly but it's working...
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            if (Program.isExiting)
                return;
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            if (Program.isExiting)
                return;
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            if (Program.isExiting)
                return;
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            if (Program.isExiting)
                return;
            Console.WriteLine("\n\n\n\n\n");
            if (Program.isExiting)
                return;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("    //===============================\\\\");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(alienCraft.Showcase());

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("    \\\\===============================//");

            Inform();

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            if (!DidWin() && !DidLose())
            {
                int guessesRemaining = MaxNumberOfGuesses - CurrentWrongGuesses;
                if (ThereWasAnError)
                {
                    Console.WriteLine(ErrorAsString);
                    ThereWasAnError = false;
                }
                else
                {
                    switch (guessesRemaining)
                    {
                        case 1:
                            Console.WriteLine("   ~~*~ game ends if you get a miss ~*~~");
                            break;
                        case 2:
                            Console.WriteLine("     ~~*~ you can get 1 more miss ~*~~");
                            break;
                        case 7:
                            if (CurrentRightGuesses == 0)
                                Console.WriteLine("   ~~*~  you can get up to 6 misses ~*~~");
                            else
                                Console.WriteLine("   ~~*~  you can still get 6 misses ~*~~");

                            break;
                        default:
                            Console.WriteLine("    ~~*~ you can get "
                                            + (guessesRemaining - 1) + " more misses ~*~~");
                            break;
                    }
                }

                int count = words.Program.wordsArray.Count;

                Console.ForegroundColor = ConsoleColor.Green;
                if (Codeword.Length > 9)
                {
                    Console.Write($"\n    length-{Codeword.Length}: {WordChoicesCount} of {count}");
                }
                else
                {
                    Console.Write($"\n     length-{Codeword.Length}: {WordChoicesCount} of {count}");
                }

                decimal perCent = (WordChoicesCount / (decimal) count) * (decimal) 100;
                Console.WriteLine($" ({perCent:F1}%)");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (CurrentRightGuesses < 1)
                {
                    Console.WriteLine($"      matches: 0");
                }
                else
                {
                    int matches = GetMatches(CurrentWord);
                    Console.WriteLine($"      matches: {matches}");
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("     codeword: ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(CurrentWord);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("     HIT");
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("miss");
                Console.Write(": ");

                var ordered = LttrsAlreadyUsed.OrderBy(l => l);

                foreach (char k in ordered)
                {
                    if (CurrentWord.Contains(k))
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(Convert.ToString(k + " ").ToUpper());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write(k + " ");
                    }
                }

                Console.WriteLine();
            }
            else if (!DidWin())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("\n\n\n the codeword was: ");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(Codeword + "\n\n");
            }
            else
            {
                Console.WriteLine("\n\n\n");
            }
        }

        public int GetMatches(string input)
        {
            int index = 0;
            int spaceCount = 0;

            foreach (char c in input)
            {
                if (c == ' ') 
                {
                    spaceCount++;
                }
            }

        // get rid of spaces for example 'g_ab' 
        // not 'g _ a b', then turn it into a string
            char[] reducedInput = new char[input.Length - spaceCount];
            foreach (char h in input)
            {
                if (h == ' ')
                {
                    continue;
                }
                else
                {
                    reducedInput[index] = h;
                    index++;
                }
            }
            string redInput = new(reducedInput);

            int numMatch = CountMatches(MatchList, redInput);
            return numMatch;
        }

        static int CountMatches(List<string> matchList, string constructedWord)
        {
            int count = 0;

            foreach (var word in matchList)
            {
                if (IsMatch(word, constructedWord))
                {
                    count++;
                }
            }
            return count;
        }

        static bool IsMatch(string word, string constructedWord)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (constructedWord[i] != '_' && constructedWord[i] != word[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void AssignValues(out IEnumerable<char> c1, out IEnumerable<char> c2,
            out IEnumerable<char> c3, out IEnumerable<char> c4, out IEnumerable<char> c5,
            out IEnumerable<char> c6, out IEnumerable<char> c7, string s)
        {
            c1 = s.Where(a => symbolsNumbersAndSpace.Contains(a));
            c2 = s.Where(b => symbolsAndNumbers.Contains(b));
            c3 = s.Where(c => alphas.Contains(c));
            c4 = s.Where(d => numbers.Contains(d));
            c5 = s.Where(e => symbols.Contains(e));
            c6 = s.Where(f => symbolsAndSpace.Contains(f));
            c7 = s.Where(g => alphasNumbersAndSpace.Contains(g));
        }

        public void Start()
        {
            if (Program.isExiting)
                return;

            beginning:

            if (ItWasHitOrMiss)
            {
                if (Program.isExiting) 
                    return;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("  try another: ");
                ItWasHitOrMiss = false;
            }
            else
            {
                if (Program.isExiting) 
                    return;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(" try a letter: ");
            }

            Console.ForegroundColor = ConsoleColor.Black;

            string input = Console.ReadLine() ?? "possible null assignment";

            IEnumerable<char> symNumSpa, symNum, alph, numb, symb, symSpa, alpNumSpa;
            AssignValues(out symNumSpa, out symNum, out alph, out numb, out symb, out symSpa, out alpNumSpa, input);

            if (!String.IsNullOrEmpty(input))
            {
                bool isANumber = int.TryParse(input, out _);

                if (isANumber)
                {
                    if (!symSpa.Any())
                        ErrorAsString = "        ~~*~ ERROR: no numbers ~*~~";
                    else
                        ErrorAsString = "   ~~*~ ERROR: no spaces & numbers  ~*~~";

                    ThereWasAnError = true;
                    Display();
                    goto beginning;
                }
                else if (symNumSpa.Any())
                {
                    if (!symNum.Any() && !alph.Any())
                        ErrorAsString = "        ~~*~ ERROR: no spaces  ~*~~";
                    else if (!alpNumSpa.Any())
                        ErrorAsString = "        ~~*~ ERROR: no symbols ~*~~";
                    else if (!alph.Any() && !numb.Any())
                        ErrorAsString = "   ~~*~ ERROR: no spaces & symbols  ~*~~";
                    else if (alph.Any())
                        if (!symb.Any() && !numb.Any())
                            ErrorAsString = "     ~~*~ ERROR: no extra spaces  ~*~~";
                        else if (numb.Any())
                            ErrorAsString = "     ~~*~ ERROR: no extra numbers ~*~~";
                        else
                            ErrorAsString = "     ~~*~ ERROR: no extra symbols ~*~~";
                    else
                        ErrorAsString = "   ~~*~ ERROR: no numbers & symbols ~*~~";

                    ThereWasAnError = true;
                    Display();
                    goto beginning;
                }
                else if (input.Length != 1)
                {
                    ErrorAsString = "    ~~*~ ERROR: more than 1 letter ~*~~";
                    ThereWasAnError = true;
                    Display();
                    goto beginning;
                }
                else
                {
                    if (input == input.ToUpper())
                    {
                        input = input.ToLower();
                    }

                    if (WasARepeat(input))
                        goto beginning;
                    else if (Codeword.Contains(input))
                        UpdateCurrentWord();
                    else
                        NextBadScene();

                    ItWasHitOrMiss = true;
                }
            }
            else
            {
                ErrorAsString = "   ~~*~ ERROR: nothing was entered  ~*~~";
                ThereWasAnError = true;
                Display();
                goto beginning;
            }
        }

        public void UpdateCurrentWord()
        {
            WasWrong = false;
            CurrentRightGuesses++;
            char[] newTry = CurrentWord.ToCharArray();

            for (int idx = 0; idx < CurrentWord.Length; idx++)
            {
                if (CodewordAsArray[idx] == InputAsChar)
                {
                    newTry[idx] = InputAsChar;
                }
            }

            LttrsAlreadyUsed.Add(InputAsChar);
            CurrentWord = new string(newTry);
            if (DidWin())
            {
                alienCraft.SetFinal();
            }
        }

        public void NextBadScene()
        {
            WasWrong = true;
            CurrentWrongGuesses++;
            LttrsAlreadyUsed.Add(InputAsChar);
            alienCraft.AddPart();
        }

        public bool WasARepeat(string x)
        {
            InputAsChar = Convert.ToChar(x);

            if (LttrsAlreadyUsed.Contains(InputAsChar))
            {
                ErrorAsString = "      ~~*~ ERROR: already in use  ~*~";
                ThereWasAnError = true;
                Display();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DidLose() => (CurrentWrongGuesses == MaxNumberOfGuesses);
        public bool DidWin() => (CurrentWord == CodewordWithSpaces);
    }

    internal class UFO
    {
        private readonly string s0 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          !       !                |
   |          |       |                |
   |          !       !                |
   |          |  \0/  | __,---------.  |
   |          !   ]   !  (" + " \u00A1ay\u00FAdame! ) |\n" +
    @"   |          |   A   |   `---------'  |
   \\         !  / \  !               //";

        private readonly string s1 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          !       !                |
   |          |       |                |
   |          !   0   ! __,---------.  |
   |          |  /[\  |  (" + " \u00A1cojones! ) |\n" +
@"   |          !   A   !   `---------'  |
   |          |  / \  |                |
   \\         !       !               //";

        private readonly string s2 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          |       |                |
   |          !   0/  ! __,---------.  |
   |          |  /[   |  (" + " \u00A1pendejo! ) |\n" +
@"   |          !   A   !   `---------'  |
   |          |  / \  |                |
   |          !       !                |
   \\                                 //";

        private readonly string s3 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          !  \0   !                |
   |          |   ]\  | ___,-------.   |
   |          !   A   !   /" + " \u00A1pinche \\  |\n" +
    @"   |          |  / \  |   \ incapaz!/  |
   |          !       !    `-------'   |
   |                                   |
   \\                                 //";

        private readonly string s4 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          |  /]\  |                |
   |          !   A   !\__,--------.   |
   |          |  / \  |  /" + "\u00A1pi\u00E9nsala \\  |\n" +
    "   |          !       !  \\ imb\u00E9cil! /  |\n" +
@"   |                      `--------'   |
   |                                   |
   \\                                 //";

        private readonly string s5 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          !   A   ! \__,-------.   |
   |          |  / \  |   /" + "\u00A1m\u00E9ndigo \\  |\n" +
    @"   |          !       !   \ tarado! /  |
   |                       `-------'   |
   |                                   |
   |                                   |
   \\                                 //";

        private readonly string s6 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          |  / \  |                |
   |          !       !  ,---------.   |
   |                 \__/" + "\u00A1gilipollas\\  |\n" +
    @"   |                    \ de mierda!/  |
   |                     `---------'   |
   |                                   |
   \\                                 //";

        private readonly string s7 =
    @"   //          .-'""'-.                \\
   |          /._\0/_.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |          !       !                |
   |                \_,----------.     |
   |                 /" + "\u00A1la  concha \\    |\n" +
@"   |                 \ de la lora!/    |
   |                  `----------'     |
   |                                   |
   \\                                 //";

        private readonly string s8 =
    @"   //          .-'""'-.                \\
   |          /._._._.\                |
   |     .--""-'-""-'-""-'-""--.           |
   |  <( ooOo6ooOo6ooOo6ooOo )>        |
   |     `'-.,_,_,_,_,_,.-'`           |
   |                                   |
   |                                   |
   |        ,-------.                  |
   |       /" + "\u00A1de puta \\___    _ 0       |\n" +
@"   |       \  madre! /      / ]\_      |
   |        `-------'         A        |
   \\                        / \      //";

        private readonly string[] ufos;
        private int stage;

        public UFO() => ufos = [s0, s1, s2, s3, s4, s5, s6, s7, s8];

        public void AddPart()
        {
            stage++;

            if (stage >= ufos.Length)
            {
                Console.WriteLine("staging error");
                stage = ufos.Length - 1;
                Console.ReadKey();
            }
        }

        public void SetFinal() => stage = ufos.Length - 1;
        public string Showcase() => ufos[stage];
        public override string ToString() => "I'm a UFO object...";
    }
}        // end of game namespace

namespace words
{
    public class Program
    {
        private static bool lengthError = false;

        public static List<string> wordsArray = [
            "apocalypse", "scream", "tiger", "millennial", "espionage", "extravagant",
            "apathy", "generous", "rose", "fascination", "revolution", "artificial",
            "strife", "discrepancy", "ordinary", "rage", "devotion", "imagination",
            "delirious", "excruciating", "grateful", "capacity", "marathon", "volcano",
            "trend", "domination", "dramatic", "peaceful", "organic", "page", "hope",
            "duck", "pope", "rope", "graceful", "divine", "elementary", "basic", "genius",
            "iconic", "reservation", "planetary", "momentary", "sacrament", "viral",
            "lame", "game", "digress", "duress", "instinct", "devolution", "panic", "pain",
            "gain", "strain", "domain", "remain", "cage", "rock", "luck", "train", "drain",
            "trash", "stash", "smash", "dose", "pose", "lose", "hose", "most", "dock",
            "amphibian", "stream", "resilient", "egregious", "fragrance", "romance", "face",
            "observe", "deserve", "obscure", "tape", "extreme", "rain", "grape", "cape",
            "dart", "cart", "fair", "rude", "nice", "cruel", "love", "fade", "fast", "cast",
            "past", "last", "pass", "mass", "fake", "rake", "lake", "rows", "toes", "foes",
            "fog", "dog", "log", "hog", "lazy", "hazy", "ironic", "moronic", "magic", "sky",
            "logic", "logos", "foggy", "fun", "pun", "fur", "gun", "run", "rug", "tug",
            "life", "stress", "mess", "mind", "find", "freak", "leak", "race", "lace", "fax",
            "fact", "pact", "gaze", "maze", "lean", "cream", "dream", "steam", "information",
            "cry", "shy", "try", "why", "toys", "boy", "goat", "way", "mean", "fry", "tale",
            "spider", "emotion", "intense", "danger", "defense", "offense", "stranger",
            "ranger", "static", "stain", "stormy", "polite", "police", "glare", "delegation",
            "stage", "stereophonic", "crate", "cake", "case", "pace", "late", "fate", "mash",
            "date", "night", "gate", "mate", "cat", "rat", "fat", "mat", "map", "rest",
            "pest", "nest", "best", "test", "pear", "tear", "gear", "fear", "near", "rear",
            "dear", "beer", "bean", "bash", "star", "stay", "day", "ray", "cringe", "fringe",
            "prince", "quince", "robot", "cobra", "snake", "stake", "stand", "grand",
            "brand", "land", "band", "bear", "rare", "dare", "war", "wild", "waste", "taste",
            "paste", "grave", "wax", "boat", "water", "rated", "year", "cash", "crash",
            "fox", "box", "mop", "man", "tan", "fan", "gin", "girl", "live", "dive", "sting",
            "smear", "ride", "trade", "maid", "hide", "fellow", "glide", "fountain", "steer",
            "mountain", "money", "honey", "mad", "fad", "glad", "sad", "bad", "lad", "bid",
            "bin", "bat", "hat", "win", "wit", "bit", "sit", "lit", "hit", "chance", "legit",
            "glance", "lord", "word", "loan", "wand", "sand", "wait", "slope", "hate", "fly",
            "roar", "beat", "feet",  "heat", "neat", "seat", "follow", "need", "seed",
            "mellow", "beet", "deer", "bent", "rent", "tent", "went", "instigate", "lizard",
            "feed", "weep", "when", "treat", "seal", "hollow", "cheat", "device", "stance",
            "mice", "mug", "house", "ram", "jam", "rice", "dice", "regress", "impression",
            "repress", "brain", "grain", "splash", "main", "rap", "gut", "hut", "nap", "hug",
            "bug", "sage", "mage", "wizard", "mouse", "say", "may", "hay", "fantastic",
            "bombastic", "drape", "propagation", "desecration", "superstition", "fungus",
            "emancipation", "obfuscate", "denigrate", "fame", "dame", "lam", "ham", "join",
            "coin", "meme", "vain", "vase", "baby", "wary", "many", "dent", "send", "farm",
            "harm", "warm", "cold", "fold", "bold", "sold", "mold", "moan", "foam", "drone",
            "long", "gong", "bong", "bone", "lone", "wall", "pearl", "base", "leap", "leaf",
            "blow", "flow", "glow", "slow", "slot", "hot", "bot", "got", "not", "lot", "mic",
            "prank", "frank", "crank", "lice", "pic", "trick", "lungs", "humus", "rip",
            "nip", "sip", "lip", "reformation", "restitution", "infiltrate", "oration",
            "illustration", "erosion", "negation", "nut", "rot", "sun", "sin", "son", "bow",
            "moon", "soon", "loon", "foot", "loop", "goon", "swirl", "swift", "stiff", "dip",
            "lock", "mock", "suck", "buck", "funk", "sunk", "rank", "bank", "trans", "sniff",
            "phone", "stone", "tall", "hall", "gall", "ball", "balm", "palm", "fixation",
            "rotation", "clash", "brash", "sports", "shorts", "monument", "document", "line",
            "passion", "regiment", "sentient", "cow", "west", "loin", "steep", "deep", "now",
            "reef", "beef", "low", "sow", "how", "tow", "row", "elation", "mansion", "lay",
            "pension", "meek", "spank", "thick", "tone", "zone", "lime", "like", "sight",
            "light", "fight", "might", "right", "strength", "hold", "gold", "flat", "pale",
            "seek", "reek", "seen", "weed", "geek", "leek", "work", "warp", "wane", "mile",
            "tension", "nose", "vow", "fit", "slang", "rust", "dust", "must", "lust", "bust",
            "trunk", "troll", "trust", "truck", "leverage", "beverage", "book", "hook",
            "rook", "took", "look", "nook", "gook", "cook", "lend", "bend", "tend", "mend",
            "lens", "zen", "hen", "den", "dot", "yen", "cost", "lost", "host", "deal",
            "feel", "reel", "stuck", "steel", "will", "wheel", "whiff", "flash", "formal",
            "knife", "white", "black", "crack", "stack", "smack", "shack", "slack", "sale",
            "harsh", "marsh", "first", "burst", "nurse", "curse", "purse", "feral", "hail",
            "normal", "porous", "agnostic", "crane", "radiation", "humiliation", "slave",
            "trace", "blaze", "flame", "crave", "grade", "grass", "brass", "brave", "fuck",
            "mine", "mike", "mist", "gist", "list", "food", "mood", "hood", "good", "doom",
            "root", "loot", "loom", "zoom", "hoot", "boot", "boom", "bowl", "howl", "soul",
            "amazon", "just", "stupid", "rate", "bait", "reed", "space", "teen", "kill",
            "fill", "bill", "mill", "hill", "nail", "rail", "wail", "sail", "bail", "dean",
            "deaf", "quit", "spit", "slit", "slip", "slap", "keen", "peek", "peak", "sick",
            "wick", "lick", "beak", "queen", "queer", "seem", "seam", "zest", "part", "mart",
            "hard", "pot", "cot", "spear", "shear", "swear", "surge", "purge", "merge",
            "forge", "heal", "smart", "spring", "farce", "large", "lair", "liar", "tail",
            "gait", "fire", "wire", "pyre", "tire", "tier", "least", "beast", "lease",
            "feast", "east", "next", "peel", "real", "pill", "verge", "blame", "blast",
            "bloom", "groom", "shirt", "flirt", "sneer", "clear", "sheer", "swim", "sweep",
            "suit", "pool", "tool", "gust", "swing", "blare", "blade", "clean", "slide",
            "pride", "tribe", "pipe", "pine", "pint", "wine", "dine", "fine", "hint", "hind",
            "side", "wind", "dire", "high", "nigh", "sigh", "tint", "drop", "crop", "stop",
            "flop", "crow", "grow", "spot", "shot", "prop", "coat", "goal", "grip", "flip",
            "ship", "ruin", "skip", "drip", "neon", "lion", "crime", "spine", "shine", "pry",
            "swine", "slime", "poor", "gone", "done", "lane", "bane", "cane", "mane", "pane",
            "door", "sane", "pick", "sock", "hock", "node", "code", "load", "road", "toad",
            "soap", "soar", "soak", "soil", "boil", "coil", "jail", "pail", "toil", "foil",
            "toll", "mall", "fall", "call", "doll", "told", "wood", "wool", "fool", "cool",
            "sway", "pray", "flay", "shape", "vape", "gape", "slay", "tray", "play", "clay",
            "fest", "jest", "less", "crest", "press", "dress", "quest", "guest", "chess",
            "seer", "week", "force", "peal", "weal", "monk", "dunk", "junk", "fist", "milk",
            "silk", "folk", "talk", "walk", "speed", "green", "fleet", "greet", "dry",
            "tank", "mink", "link", "wink", "pike", "hike", "bike", "wide", "pile", "more",
            "manipulation", "resuscitation", "proclamation", "decapitation", "excavation",
            "define", "design", "resign", "malign", "benign", "demonstration", "smirk",
            "strangulation", "electrocution", "dismembering", "distinguished", "bore", "go",
            "lore", "gore", "fork", "dork", "task", "pork", "sort", "store", "shore", "for",
            "whore", "score", "scope", "clone", "cone", "distorted", "hole", "pole", "sole",
            "fowl", "to", "at", "is", "haste", "by", "in", "flack", "whack", "flank", "fare",
            "care", "pair", "hair", "sheet", "sweet", "sheen", "breed", "flask", "torque",
            "dye", "bye", "be", "sheep", "creep", "street", "respiration", "destination",
            "lie", "die", "leave", "leash", "learn", "steal", "mire", "spin", "do", "doe",
            "kite", "bite", "rite", "mite", "site", "file", "guile", "illumination",
            "degradation", "suffocation", "repetition", "apparition", "sensational", "malt",
            "frustration", "proficient", "sufficient", "cult", "bolt", "salt", "relation",
            "halt", "pub", "bud", "mud", "put", "but", "cut", "same", "tame", "give", "jolt",
            "shock", "stock", "block", "reveal", "appeal", "surreal", "unreal", "dwell",
            "swell", "smell", "shell", "quell", "spell", "build", "quilt", "guilt", "stilt",
            "type", "no", "yes", "pay", "oh", "hey", "stall", "stamp", "cramp", "tramp",
            "raid", "made", "stale", "floor", "spark", "spunk", "cope", "chunk", "brink",
            "solid", "fluid", "flush", "crush", "rash", "hash", "lash", "dash", "phase",
            "phrase", "please", "ease", "disease", "obese", "cease", "peace", "crease",
            "grease", "tease", "wheeze", "sneeze", "serpent", "service", "certain", "cheese",
            "curtain", "fleece", "freeze", "so", "get", "it", "on", "up", "and", "my", "the",
            "beach", "peach", "bleach", "reach", "teach", "each", "leech", "energy", "sly",
            "entity", "destiny", "enemy", "elevate", "generate", "void", "envy", "return",
            "envoy", "employ", "enjoy", "deploy", "decoy", "devoid", "stride", "strive",
            "string", "bring", "drink", "destroy", "shrink", "stripe", "strip", "snack",
            "snow", "snap", "snare", "stare", "stairs", "chair", "determination", "bionic",
            "toxic", "tonic", "sonic", "androgynous", "homogeneous", "superfluous", "animal",
            "contamination", "strenuous", "vigorous", "enormous", "efficacious", "practical",
            "radical", "rational", "logical", "dogmatic", "fanatical", "superficial", "rid",
            "extraordinary", "repetitious", "sacrilegious", "superstitious", "indignation",
            "concoction", "altercation", "computation", "substitution", "subterranean", "id",
            "desperation", "obliterate", "recuperate", "enumerate", "revelation", "nation",
            "potion", "lotion", "motion", "bet", "knit", "red", "led", "wet", "set", "form",
            "norm", "alteration", "divination", "confirmation", "suspicion", "elimination",
            "division", "resistance", "existence", "restoration", "audacity", "lucidity",
            "extremity", "motivation", "inspiration", "oblivious", "contagious", "graphic",
            "tremendous", "horrendous", "depravity", "dignity", "flare", "rapid", "vapid",
            "morbidity", "ostentatious", "advantageous", "terrible", "terrific", "specific",
            "horrific", "stupendous", "obligatory", "respiratory", "exposition", "sporadic",
            "documentary", "special", "spatial", "racial", "permission", "submission", "big",
            "dominance", "exuberance", "glacier", "hallucination", "promotion", "cognition",
            "resignation", "designation", "vision", "volition", "restricted", "addicted",
            "revision", "intoxicated", "medicated", "drugs", "prescription", "subscription",
            "level", "label", "repel", "lapel", "rebel", "hotel", "motel", "camel", "cartel",
            "caramel", "cater", "later", "crater", "traitor", "trader", "pander", "wonder",
            "wander", "launder", "Lawn", "lunch", "crunch", "munch", "ranch", "range", "end",
            "hunch", "bunch", "punch", "staunch", "park", "stork", "cork", "make", "bake",
            "take", "sake", "wake", "dark", "bark", "mark", "save", "safe", "wage", "weight",
            "wave", "rave", "cave", "shave", "into", "gastronomy", "supersonic",  "official",
            "hydroponics", "astronomical", "physiological", "biological", "environmental",
            "regulatory", "suicidal", "maniacal", "belt", "melt", "gel", "sell", "hell",
            "bell", "well", "nerve", "serve", "verb", "heart", "charm", "orb", "starve",
            "carve", "reservoir", "deceptive", "receptive", "inception", "digestion", "off",
            "request", "respect", "dissect", "expectation", "restaurant", "gymnasium", "of",
            "magnesium", "obsession", "aggression", "ignorance", "recession", "delusion",
            "obvious", "devious", "heinous", "jealous", "glorious", "away", "delay", "relay",
            "convey", "survey", "rule", "rune", "conversation", "registration", "suggestion",
            "innovation", "cultivation", "chasm", "spasm", "erase", "ligation", "gradation",
            "investment", "testament", "entail", "detail", "retail", "break", "steak", "nun",
            "brake", "stile", "style", "glean", "legion", "region", "fresh", "frisk", "risk",
            "section", "faction", "fiction", "diction", "frame", "friend", "disk", "shaft",
            "mask", "art", "aim", "ail", "fail", "shade", "shame", "shark", "sharp", "shard",
            "heap", "reap", "sleep", "stark", "shank", "plank", "dead", "drunk", "skunk",
            "aggravate", "aggrandize", "criticize", "scrutinize", "pulverize", "astonish",
            "demolish", "relish", "punish", "furnish", "sandwich", "garnish", "relinquish",
            "creature", "feature", "future", "caress", "clueless", "guess", "jet", "let",
            "frost", "ghost", "toast", "boast", "roast", "ban", "dam", "cam", "tap", "lap",
            "gap", "cap", "lamb", "stunned", "bewildered", "amazed", "transmutation", "lo",
            "dazed", "confused", "misused", "refuse", "use", "muse", "abused", "unused",
            "repercussion", "intuition", "emergency", "outstanding", "astounding", "meal",
            "advocacy", "reputation", "substance", "entrance", "embrace", "distance", "mow",
            "reuse", "salutation", "deliverance", "grievance", "occupancy", "adventurous",
            "brainless", "truce", "advertise", "glamorize", "devise", "revise", "refutation",
            "suffice", "reprise", "demonize", "moralize", "monetize", "summarize", "throw",
            "sacrifice", "mesmerize", "entice", "edifice", "memorize", "televise", "show",
            "bemused", "new", "sew", "dew", "rue", "glitch", "bitch", "stitch", "snitch",
            "ditch", "pitch", "witch", "rich", "niche", "feeling", "ceiling", "dressing",
            "blessing", "opportunity", "calamity", "gravity", "terrain", "bedazzled",
            "fixed", "stretch", "wretch", "suppose", "solemn", "emblem", "diagram", "arm",
            "alarm", "disarm", "disaster", "narrow", "sorrow", "borrow", "burrow", "dorm",
            "porn", "horn", "born", "torn", "corn", "creed", "riff", "cliff", "grief",
            "strategy", "adversity", "velocity", "compliment", "complement", "condiment",
            "complicity", "tranquility", "terminate", "eliminate", "illuminate", "amalgam",
            "resuscitate", "elucidate", "divulge", "distribute", "contribute", "retribute",
            "anagram", "animate", "agitate", "ensign", "visage", "mirage", "align", "sign",
            "compartment", "symphony", "melody", "harmony", "distortion", "commotion", "eye",
            "premonition", "recognize", "decorate", "stimulate", "organize", "create", "rye",
            "confiscate", "decimate", "turbulence", "disturbance", "mutation", "conviction",
            "prediction", "reaction", "attraction", "upheaval", "adrenaline", "gasoline",
            "momentous", "exquisite", "attribute", "requisite", "alienation", "transcend",
            "ascend", "descend", "rescind", "restraint", "trending", "remember", "motor",
            "odor", "rotor", "lotus", "honesty", "modesty", "botox", "borax", "color", "ion",
            "limb", "time", "rhyme", "chyme", "grime", "spike", "shallow", "yellow", "ice",
            "brief", "belief", "relief", "spicy", "sour", "hour", "flour", "prowl", "growl",
            "drool", "stool", "response", "ambulance", "emerge", "lurk", "rely", "angst",
            "spy", "commiserate", "berate", "oblate", "negate", "emphasize", "legalize",
            "ostracize", "demoralize", "rationalize", "capitalize", "obstruction", "ingrate",
            "capitulate", "necessitate", "reimburse", "obtain", "insane", "retain", "great",
            "grate", "spherical", "numerical", "subliminal", "empirical", "investigation",
            "reduction", "small", "smidge", "ridge", "bridge", "fridge", "quiet", "quite",
            "utilize", "ability", "agility", "commence", "commerce", "conquest", "navigate",
            "override", "position", "possible", "ponder", "superior", "inferior", "exterior",
            "interior", "realize", "remorse", "express", "ingress", "juvenile", "infantile",
            "record", "adore", "abhor", "detest", "contest", "concert", "expert", "center",
            "enter", "render", "lender", "window", "sender", "candor", "camp", "ramp", "amp",
            "lamp", "damp", "hinder", "hamper", "thunder", "asunder", "tangled", "senile",
            "singer", "linger", "winter", "listen", "single", "mingle", "minute", "menace",
            "virtue", "virtual", "total", "mental", "menial", "genial", "gender", "gesture",
            "posture", "institute", "prostitute", "destitute", "inspection", "elaborate",
            "lemon", "demon", "reason", "season", "liaison", "lesson", "lesion", "treason",
            "structure", "construct", "report", "import", "deport", "export", "dim", "deem",
            "esteem", "beam", "sea", "pea", "bee", "salad", "ballad", "concession", "oil",
            "connection", "turmoil", "detour", "revolt", "conceal", "ratio", "radio",
            "station", "status", "state", "relate", "extinguish", "spade", "toke", "poke",
            "joke", "smoke", "cloak", "stoke", "mute", "flute", "lure", "flag", "rag", "tag",
            "lump", "hump", "bump", "stomp", "stump", "trench", "drench", "stench", "fluke",
            "possibility", "motorcycle", "trample", "flume", "fume", "sum", "bum", "clap",
            "clamp", "gamble", "handle", "mangle", "jungle", "preamble", "jumble", "stumble",
            "humble", "able", "fable", "cable", "stable", "table", "angle", "tangle", "hang",
            "dangle", "gang", "fang", "song", "ample", "apple", "grapple", "staple", "maple",
            "bind", "bird", "ramble", "rumble", "straight", "conjure", "confess", "age",
            "aid", "ask", "bleak", "weak", "stick", "slick", "dowse", "browse", "arouse",
            "carouse", "blouse", "devout", "tout", "mouth", "south", "north", "tirade",
            "parade", "parachute", "helicopter", "impossible", "responsible", "deposit",
            "impose", "expose", "repose", "depose", "post", "paradise","confide", "reside",
            "revive", "connive", "contrive", "comply", "imply", "reply", "supply", "miss",
            "diss", "piss", "fish", "quiche", "wish", "fulminate", "laminate", "denounce",
            "pounce", "ounce", "fumigate", "scold", "scale", "trail", "mail", "prime",
            "inclined", "refine", "opine", "irresponsible", "reciprocity", "doubt", "clout",
            "recollect", "disrespectful", "airplane", "refrain", "inflict", "average",
            "conflict", "deflect", "reflect", "select", "elect", "release", "compromise",
            "retract", "extract", "distract", "contract", "demise", "surmise", "desire",
            "retire", "expire", "conspiracy", "divisive", "draw", "crawl", "maul", "haul",
            "point", "clue", "glue", "plan", "bomb", "tomb", "dome", "some", "come", "home",
            "concentration", "effort", "malpractice", "strict", "souvenir", "language",
            "action", "guild", "spill", "chill", "dill", "ill", "fruit", "truth", "group",
            "nitroglycerin", "atmospheric", "catapult", "residue", "product", "model",
            "interrogate", "alternative", "conditional", "excommunicate",  "role", "roll",
            "conglomerate", "extrapolate", "interpolate", "specificity", "automatic", "mole",
            "repetitive","exterminator", "incapacitate", "reproduction", "celebration", 
            "experimental", "detrimental", "rudimentary", "exponentially", "argumentative",
            "resurgent", "insurgent", "invitational", "intertwined", "recreational", "old",
            "ale", "collaborate", "regurgitate", "facilitate", "few", "stew", "odd", "grill",
            "drill", "still", "shill", "quench", "bench", "regard", "disseminate", "monster",
            "ant", "ax", "ace", "add", "bask", "ape", "port", "fort", "core", "sore", "pore", 
            "synchronize", "frequency", "admonishment", "fermentation", "encapsulation",
            "polymorphism", "cybersecurity", "inheritance", "stability", "proficiency",
            "merchandise", "ornamental", "fundamental", "relevance", "avalanche", "tribute",
            "territorial", "memorial", "membership", "measure", "treasure", "pleasure",
            "expedition", "simulation", "physicality", "represent", "repent", "formalize",
            "redistribute", "reconstitute", "instability", "convergence", "technology",
            "commercialize", "stipulation", "reformulate", "technocracy", "idiocracy", "vet",
            "intermittent", "international", "conduct", "restructure", "web", "incredible",
            "remarkable", "incredulous", "concentrated", "track", "wreck", "check", "flick",
            "federation", "compatible", "correspond", "congratulate", "temporary", "remote",
            "sanctuary", "monastery", "instantaneous", "instance", "instruct", "surprise",
            "psychopath", "introvert", "comprehend", "understand", "decode", "demote", "ode",
            "revoke", "result", "devote", "annotated", "repulsiveness", "conformity", "ad",
            "longevity", "grudge", "smudge", "judge", "nudge", "budge", "fudge", "rivaled",
            "reviled", "beguile", "exile", "textile", "text", "sex", "diagonal", "repugnant",
            "retardant", "vengeance", "sentence", "indignant", "considerable", "emergence",
            "potentiality", "admirable", "efficient", "resourceful", "commendable", "urge",
            "reprehensible", "indefensible", "dereliction", "television", "supervision",
            "restroom", "installation", "upgrade", "uptake", "uphill", "uptown", "downtown",
            "downturn", "showtime", "shrapnel", "industrialize", "recycle", "bicycle",
            "cycle", "baguette", "rosette", "tenet", "container", "divider", "trap", "crap",
            "foul", "contemptible", "depart", "department", "enact", "enable", "disable",
            "scapegoat", "steamboat", "skyscraper", "undertaker", "restorative", "decor",
            "decorative", "restore", "ignore", "galore", "connote", "support", "comfort",
            "software", "discipline", "trespass", "coalesce", "definite", "infinity", "relegate", 
        ];

        private static int _2sCount, _3sCount, _4sCount, _5sCount,
                    _6sCount, _7sCount, _8sCount, _9sCount,
                    _10sCount, _11sCount, _12sCount, _13sCount;
        public static int TwosCount { get { return _2sCount; } }
        public static int ThreesCount { get { return _3sCount; } }
        public static int FoursCount {  get { return _4sCount; } }
        public static int FivesCount { get { return _5sCount; } }
        public static int SixesCount { get { return _6sCount; } }
        public static int SevensCount { get { return _7sCount; } }
        public static int EightsCount { get { return _8sCount; } }
        public static int NinesCount { get { return _9sCount; } }
        public static int TensCount { get { return _10sCount; } }
        public static int ElevensCount { get { return _11sCount; } }
        public static int TwelvesCount { get { return _12sCount; } }
        public static int ThirteensCount { get { return _13sCount; } }

        public static void GetLengthCounts()
        {
            _13sCount = 0; 
            _12sCount = 0;
            _11sCount = 0;
            _10sCount = 0;
            _9sCount = 0;
            _8sCount = 0;
            _7sCount = 0;
            _6sCount = 0;
            _5sCount = 0;
            _4sCount = 0;
            _3sCount = 0;
            _2sCount = 0;

            foreach (string x in wordsArray)
            {
                if (x.Length > 13)
                {
                    lengthError = true;
                    Console.Write("a newly entered word was over 13 chars...exiting.");
                    return;
                }

                if (x.Length == 13) _13sCount++;
                else if (x.Length == 12) _12sCount++;
                else if (x.Length == 11) _11sCount++;
                else if (x.Length == 10) _10sCount++;
                else if (x.Length == 9) _9sCount++;
                else if (x.Length == 8) _8sCount++;
                else if (x.Length == 7) _7sCount++;
                else if (x.Length == 6) _6sCount++;
                else if (x.Length == 5) _5sCount++;
                else if (x.Length == 4) _4sCount++;
                else if (x.Length == 3) _3sCount++;
                else _2sCount++;
            }
        }

    //     private static void Main(string[] args)
    //     {
    //         GetLengthCounts();

    //         if (lengthError)
    //         {
    //             return;
    //         }

    //         Console.Write(" size tots: ");
    //         Console.WriteLine($"2 = {_2sCount} | 3 = {_3sCount} | 4 = {_4sCount} | 5 = {_5sCount} ({_2sCount + _3sCount + _4sCount + _5sCount})");
    //         Console.WriteLine($"           6 = {_6sCount} | 7 = {_7sCount} | 8 = {_8sCount} | 9 = {_9sCount} ({_6sCount + _7sCount + _8sCount + _9sCount})");
    //         Console.WriteLine($"           10 = {_10sCount} | 11 = {_11sCount} | 12 = {_12sCount} | 13 = {_13sCount} ({_10sCount + _11sCount + _12sCount + _13sCount})");

    //         Console.Write("       count: " + wordsArray.Count + " | ");

    //         if (wordsArray.Count != wordsArray.Distinct().Count())
    //         {
    //             Console.WriteLine("duplicates:");
    //         }
    //         else
    //         {
    //             Console.WriteLine("no duplicates");
    //         }

    // //   var longest = wordsArray.OrderByDescending(s => s.Length).First();
    // //   string superLongWord = longest.ToString();
    //         Console.Write($"    max length: 13 | ");

    //         var duplicates = wordsArray
    //                         .GroupBy(a => a)
    //                         .Where(b => b.Count() > 1)
    //                         .Select(c => c.Key)
    //                         .ToList();

    //         if (duplicates.Count != 0)
    //         {
    //             foreach (string s in duplicates)
    //             {
    //                 Console.Write(s + " ");
    //             }
    //         }
    //         else
    //         {
    //             Console.Write("list was empty...");
    //         }
    //     }
    }
}        // end of words namespace
