// this file will have the two console applications that are within the solution spaceMan3
// so 'words' and 'game' are the two apps... you will have to piece it together yourself
// but basically you make a root folder with mkdir, in this case i used SMS3,
// then you make the solution file with 'dotnet new sln -n spaceMan3',  for example
// and that makes the  ' spaceMan3.sln ' file that's the centerpiece of the project,
// then after you make dotnet new console -n game and dotnet new console -n words,
// you enter 'dotnet sln add words', and 'dotnet sln add game' and now you have the 
// two projects as part of the main solution... then in VS-Code, you open the folder up,
// you go to the game folder and you create files Game.cs, UFO.cs, the Program
// files you just edit for each app completely erase with the new code, then lastly
// right click on game.csproj and select 'add reference' and then choose 'words.csproj', 
// forgot exactly if it's a dialog or a command palette, but it's easy... then in
// the terminal first do a 'dotnet build' and then 'dotnet run --project game' or 'dotnet
// run --project words'... when you are at the top level, but if you navigate to each
// individual folder, then you can just do a 'dotnet run' without additional parameters.
// new features are it gives the instance id # in case there are two terminals running it,
// also gives better instructions when you first load it up, eg: to set the colors properly,
// etc. namely to make sure to have the text a certain way and then reverse it with the
// display settings to end up with a dark setting if you will...just looks better.
// but it can be ran either way, just saying... also features a colorized UFO.


// ...SMS3/game/Program.cs :

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

        private static string _instanceCode = string.Empty;
        public static string InstanceID {  get;  private set; } = string.Empty;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
            
            var instanceId = Guid.NewGuid();

            _instanceCode = Convert.ToString(instanceId) ?? "possible null reference";
            InstanceID = _instanceCode;

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
                Console.Write(" PLAY AGAIN?(y/n): ");

                Console.ForegroundColor = ConsoleColor.Black;
                string answer = Console.ReadLine() ?? "possible null assignment";

                if (!string.IsNullOrEmpty(answer))
                {
                    if (string.Equals(answer, "n", StringComparison.OrdinalIgnoreCase))
                    {
                        keepAlive = false;
                    }
                    else if (string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
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
}

//=====================EOF

// ...SMS3/game/Game.cs :

namespace game
{

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
            Console.Write("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"               instance id:\n   {game.Program.InstanceID}\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("             ~~~*~~*~~*~~*~~~");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("            CRACK the CODEWORD");
            Console.WriteLine("          and STOP the ABDUCTION!");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("             ~~~*~~*~~*~~*~~~\n");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("  1. set console to black text on white");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  2. set display to inverted colors");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("  you should now see white text on black");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  3. maximize panel to view images");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  4. press Ctrl+C to exit any time");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("  5. press <Enter> to start the game -> ");
            string unknown = Console.ReadLine() ?? "possible null assignment";
        }

        public void Inform()
        {
            if (Program.isExiting)
                return;

            Console.ForegroundColor = ConsoleColor.Green;

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
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            if (Program.isExiting)
                return;
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            if (Program.isExiting)
                return;
            Console.WriteLine("\n\n\n\n\n");
            if (Program.isExiting)
                return;


            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("    //===============================\\\\");

            PrintUFO(CurrentWrongGuesses);

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("    \\\\===============================//");

            if (ThereWasAnError)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(ErrorAsString);
                ThereWasAnError = false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   #{game.Program.InstanceID}");
            }

            Inform();

            Console.ForegroundColor = ConsoleColor.Black;
            if (!DidWin() && !DidLose())
            {
                int guessesRemaining = MaxNumberOfGuesses - CurrentWrongGuesses;
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

                int count = words.Program.wordsArray.Count;

                Console.ForegroundColor = ConsoleColor.Magenta;
                if (Codeword.Length > 9)
                {
                    Console.Write($"    length-{Codeword.Length}: {WordChoicesCount} of {count}");
                }
                else
                {
                    Console.Write($"     length-{Codeword.Length}: {WordChoicesCount} of {count}");
                }

                decimal perCent = (WordChoicesCount / (decimal)count) * (decimal)100;
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

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("     HIT");
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("miss");
                Console.Write(": ");

                var ordered = LttrsAlreadyUsed.OrderBy(l => l);

                foreach (char k in ordered)
                {
                    if (CurrentWord.Contains(k))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(Convert.ToString(k + " ").ToUpper());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(k + " ");
                    }
                }

                Console.WriteLine();
            }
            else if (!DidWin())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("\n\n   the codeword was: ");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(Codeword + "\n\n");
            }
            else
            {
                Console.WriteLine("\n\n");
            }
        }

        private void PrintUFO(int _currentWrongGuesses)
        {
            // have to determine the index of the showcase to be called 
            // each time and then each one will have a different print 
            // method for the face in a varying position each time or it 
            // won't show up at all so have ' if no face, then so and so, 
            // then if face, then nested within if face which index and 
            // then call the correct method.

            string specialInput = alienCraft.Showcase();
            int indexOfZero = GetIndexOfZero(specialInput);
            int indexOfSix = GetIndexOfSix(specialInput);
            int indexOfLetterO = GetIndexOfLetterO(specialInput);
            int indexOfLetterA = GetIndexOfLetterA(specialInput);

            char[] splitInput = specialInput.ToCharArray();
            for (int i = 0; i < splitInput.Length; i++)
            {
                if (i == indexOfLetterO || i == indexOfLetterO + 5 || i == indexOfLetterO + 10 || i == indexOfLetterO + 15)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(specialInput[i]);
                }
                else if (i == indexOfSix || i == indexOfSix + 5 || i == indexOfSix + 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(specialInput[i]);
                }
                else if (i == indexOfZero)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(specialInput[i]);
                }
                else if (i == indexOfLetterA)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(specialInput[i]);
                }
                else
                {
                    // anything else normal color
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(specialInput[i]);
                }
            }
            Console.WriteLine();
        }

        public static int GetIndexOfZero(string _specialInput)
        {
            bool containsZero = _specialInput.Contains('0');
            if (containsZero)
            {
                return _specialInput.IndexOf('0');
            }
            else
            {
                return -1;
            }
        }

        public static int GetIndexOfSix(string _specialInput)
        {
            bool containsSix = _specialInput.Contains('6');
            if (containsSix)
            {
                return _specialInput.IndexOf('6');
            }
            else
            {
                return -1;
            }
        }

        public static int GetIndexOfLetterO(string _specialInput)
        {
            bool containsLetterO = _specialInput.Contains('O');
            if (containsLetterO)
            {
                return _specialInput.IndexOf('O');
            }
            else
            {
                return -1;
            }
        }

        public static int GetIndexOfLetterA(string _specialInput)
        {
            bool containsLetterA = _specialInput.Contains('A');
            if (containsLetterA)
            {
                return _specialInput.IndexOf('A');
            }
            else
            {
                return -1;
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
}

//=====================EOF

// ...SMS3/game/UFO.cs :

namespace game
{
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
   |       /" + "\u00A1su puta \\___    _ 0       |\n" +
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
}

//=====================EOF

// ...SMS3/words/Program.cs :

namespace words
{
    public class Program
    {
        private static bool lengthError = false;

        public static readonly List<string> wordsArray = [
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
            "spider", "emotional", "intense", "danger", "defense", "offense", "stranger",
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
            "rook", "took", "look", "nook", "gook", "cook", "lend", "bend", "deal", "mend",
            "lens", "zen", "hen", "den", "dot", "yen", "cost", "lost", "host", "tender",
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
            "sway", "gay", "flay", "shape", "vape", "gape", "slay", "tray", "play", "clay",
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
            "kite", "bite", "rite", "mite", "site", "file", "guile", "illumination", "air",
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
            "envoy", "employ", "enjoy", "deployment", "decoy", "devoid", "stride", "strive",
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
            "wander", "launder", "lawn", "lunch", "crunch", "munch", "ranch", "range", "end",
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
            "enter", "bender", "lender", "window", "sender", "candor", "camp", "ramp", "amp",
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
            "impose", "expose", "repose", "depose", "post", "paradise","confide", "resident",
            "revive", "connive", "contrive", "comply", "imply", "reply", "supply", "miss",
            "diss", "piss", "fish", "quiche", "wish", "fulminate", "laminate", "denounce",
            "pounce", "ounce", "fumigate", "scold", "scale", "trail", "mail", "refinement",
            "inclined", "prime", "opine", "irresponsible", "reciprocity", "doubt", "clout",
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
            "downturn", "showtime", "shrapnel", "industrialize", "recycle", "bicycle", "ass",
            "cycle", "baguette", "rosette", "tenet", "container", "enact", "trap", "entrap",
            "foul", "contemptible", "depart", "department", "divider", "enable", "disable",
            "scapegoat", "steamboat", "skyscraper", "undertaker", "restorative", "decor",
            "decorative", "restore", "ignore", "galore", "connote", "support", "comfort",
            "software", "discipline", "trespass", "coalesce", "definite", "infinity", "cab",
            "relegate", "separate", "exculpate", "immigrant", "repatriate", "incinerate",
            "ingratiate", "infuriate", "repudiate", "exclamation", "congregate", "fixate",
            "asphyxiate", "contemporary", "commemorative", "elegance", "circumstance", "lab",
            "untangle", "reprimand", "continental", "stratosphere", "reborn", "deformity",
            "constellation", "comparison", "surface", "concerned", "sentimental", "appear",
            "coincidental","repudiation", "replacement", "devastated", "reinstated", "chord",
            "consternated", "consecrated", "constipated", "geopolitical", "tribulation",
            "ramification", "solicit", "implicit", "complicit", "revisit", "tuition", "raft",
            "fruition", "explicitly", "tenacious", "audacious", "duplicitous", "metallic",
            "draft", "craft", "party", "constant", "resonant", "portray", "consonant", "dab",
            "brainiac", "maniac", "intrusion", "seduction", "deduction", "compulsory", "raw",
            "differentiate", "cord", "sword", "pound", "mound", "round", "bound", "ground",
            "compressed", "entrenched", "expiration", "elevation", "consideration", "act",
            "reparation", "integration", "supplication", "unified", "gentrified", "concur", 
            "demarcation", "separation", "reconstruct", "refurbish", "conniption", "evil",
            "rendition", "trepidation", "jerk", "contortion", "sedition", "inculcate", "ash",
            "orgasm", "phantasm", "protoplasm", "solemnity", "sincerity", "contradiction",
            "sanctimonious", "purification", "application", "irreparable", "temptation",
            "irregularity", "impregnated", "reconcile", "occurrence", "recurrence", "schism",
            "prism", "price", "print", "mint", "cent", "lint", "flint", "gash", "kit", "lid",
            "sailboat", "raincoat", "turncoat", "immunity", "indemnify", "solidify", "lunge",
            "deterrence", "discernment", "condemnation", "remarks", "regards", "project",
            "ascension", "assumption", "presumption", "encampment", "enlightenment", "sound",
            "embankment", "accompany", "community", "assailant", "combatant", "predicament",
            "predicate", "prophecy", "predilection", "perversion", "prevention", "partake",
            "conceit", "defeat", "concede", "circumvent", "circumference", "conference",
            "bipolar", "disorder", "embodiment", "executioner", "synopsis", "holistic",
            "symmetry", "suite", "larva", "hearth", "earth", "dearth", "birth", "squirt",
            "dig", "gig", "fig", "rig", "wig", "jig", "league", "skin", "skate", "trip",
            "convulsion", "gyrate", "poison", "prison", "pardon", "proton", "pistol", "far",
            "petrol", "patrol", "portal", "atomic", "particle", "canticle", "patron", "car",
            "pertinent", "incident", "accidently", "postal", "coastal", "paint", "faint",
            "prize", "priceless", "amputated", "renovated", "seep", "brawl", "sprawl", "tar",
            "fraught", "rigorous", "stammer", "hammer", "obsolete", "controversial", "bar",
            "existential", "reckoning", "welcome", "succumb","become", "overcome", "kingdom",
            "ringtone", "symptomatic", "democratic", "nuanced", "volatile", "versatility",
            "pendant", "pennant", "ferment", "dormant", "remnant", "stagger", "wrestle",
            "fascist", "violent", "flower", "power", "twisted", "committed", "requite",
            "requiem", "meat", "wheat", "slate", "weird", "bizarre", "furnace", "harness",
            "renounce", "grimace", "curtail", "impale", "excel", "derail", "whale", "are",
            "guitar", "fuss", "justice", "juice", "bruise", "cruise", "moose", "loose",
            "ruse", "ritual", "habitual", "perpetual", "continuous", "monotonous", "stray",
            "ridiculous", "sink", "think", "blink", "stink", "collision", "remission", "inn",
            "pacification", "falsification", "junction", "function", "puncture", "rapture",
            "rupture", "capture", "scepter", "sever", "lever", "cabal", "devil", "revel",
            "friction", "fraction", "compact", "artifact", "rectification", "omission",
            "precision", "lounge", "source", "bounce", "bout", "binge", "young", "dignitary",
            "camper", "canopy", "cantor", "castor", "forecast", "card", "lard", "guard",
            "ward", "mast", "present", "resentment", "consensual", "carpet", "trumpet",
            "hostel", "hostile", "holiday", "shadow", "halo", "wing", "wrist", "stirred",
            "shaken", "mistaken", "taken", "noted", "novel", "shovel", "suffer", "buffer",
            "arrogance", "calumny", "catastrophe", "retard", "retarded", "satanic", "which",
            "switch", "sensor", "censure", "censored", "leisure", "seizure", "assure", "sag",
            "bass", "reassure", "temperament", "temperature", "literature", "dictatorial",
            "synthesizer", "tranquilizer", "computer", "laptop", "mobile", "mob", "mafia",
            "antagonistic", "idolatry", "ritualistic", "futuristic", "capitalist", "capital",
            "socialist", "communist", "eloquence", "sequence", "frequent", "certification",
            "recent", "decent", "indecent", "infatuation", "allegation", "promulgate",
            "propaganda", "organism", "activism", "theory", "relativity", "configuration",
            "capitulation", "surrender", "bandage", "rampage", "savage", "ravage", "cabbage",
            "vocabulary", "cemetery", "auxiliary", "redundancy", "extradition", "resort",
            "traditional", "reformed", "curiosity", "remedy", "cure", "sure", "pure", "puke",
            "slurp", "gulp", "pulp", "nuke", "duke", 

        ];

        private static int _2sCount, _3sCount, _4sCount, 
            _5sCount, _6sCount, _7sCount, _8sCount, _9sCount,
                _10sCount, _11sCount, _12sCount, _13sCount;
        private static int _allLettersCount;
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
        public static int AllLettersCount { get { return _allLettersCount; } }

        public static void GetLengthCounts()
        {
            _13sCount = 0; _12sCount = 0; _11sCount = 0; _10sCount = 0;
            _9sCount = 0; _8sCount = 0; _7sCount = 0; _6sCount = 0;
            _5sCount = 0; _4sCount = 0; _3sCount = 0; _2sCount = 0;

            foreach (string x in wordsArray)
            {
                if (x.Length > 13 || x.Length < 2)
                {
                    lengthError = true;
                    Console.WriteLine("\nERROR: \n  words must be between 2 and 13 chars inclusive--please correct...\n");
                    return;
                }

                if (x.Length == 13)
                    _13sCount++;
                else if (x.Length == 12)
                    _12sCount++;
                else if (x.Length == 11)
                    _11sCount++;
                else if (x.Length == 10)
                    _10sCount++;
                else if (x.Length == 9)
                    _9sCount++;
                else if (x.Length == 8)
                    _8sCount++;
                else if (x.Length == 7)
                    _7sCount++;
                else if (x.Length == 6)
                    _6sCount++;
                else if (x.Length == 5)
                    _5sCount++;
                else if (x.Length == 4)
                    _4sCount++;
                else if (x.Length == 3)
                    _3sCount++;
                else
                    _2sCount++;
            }
        }

        public static void GetLengthCountsPlusAverage()
        {
            _13sCount = 0; _12sCount = 0; _11sCount = 0; _10sCount = 0;
            _9sCount = 0; _8sCount = 0; _7sCount = 0; _6sCount = 0;
            _5sCount = 0;  _4sCount = 0; _3sCount = 0; _2sCount = 0;

            _allLettersCount = 0;

            foreach (string x in wordsArray)
            {
                if (x.Length > 13 || x.Length < 2)
                {
                    lengthError = true;
                    Console.WriteLine("\nERROR: \n  words must be between 2 and 13 chars inclusive--please correct...\n");
                    return;
                }

                if (x.Length == 13)
                    { _13sCount++; _allLettersCount += 13; }
                else if (x.Length == 12)
                    { _12sCount++; _allLettersCount += 12; }
                else if (x.Length == 11)
                    { _11sCount++; _allLettersCount += 11; }
                else if (x.Length == 10)
                    { _10sCount++; _allLettersCount += 10; }
                else if (x.Length == 9)
                    { _9sCount++; _allLettersCount += 9; }
                else if (x.Length == 8)
                    { _8sCount++; _allLettersCount += 8; }
                else if (x.Length == 7)
                    { _7sCount++; _allLettersCount += 7; }
                else if (x.Length == 6)
                    { _6sCount++; _allLettersCount += 6; }
                else if (x.Length == 5)
                    { _5sCount++; _allLettersCount += 5; }
                else if (x.Length == 4)
                    { _4sCount++; _allLettersCount += 4; }
                else if (x.Length == 3)
                    { _3sCount++; _allLettersCount += 3; }
                else
                    { _2sCount++; _allLettersCount += 2; }
            }
        }

        private static void Main(string[] args)
        {
            GetLengthCountsPlusAverage();

            if (lengthError)
            {
                return;
            }

            int shortTotal = _2sCount + _3sCount + _4sCount + _5sCount;
            int mediumTotal = _6sCount + _7sCount + _8sCount + _9sCount;
            int longTotal = _10sCount + _11sCount + _12sCount + _13sCount;

            decimal shortPercentage = (shortTotal / (decimal) wordsArray.Count) * (decimal) 100;
            decimal mediumPercentage = (mediumTotal / (decimal) wordsArray.Count) * (decimal) 100;
            decimal longPercentage = (longTotal / (decimal) wordsArray.Count) * (decimal) 100;

            Console.Write($"\n     short: {shortTotal} |");
            Console.WriteLine($"  2s:{_2sCount}  +  3s:{_3sCount} +  4s:{_4sCount} +  5s:{_5sCount} = {shortPercentage:F1}%");
            Console.Write($"    medium:  {mediumTotal} |");
            Console.WriteLine($"  6s:{_6sCount} +  7s:{_7sCount} +  8s:{_8sCount} +  9s:{_9sCount} = {mediumPercentage:F1}%");
            Console.Write($"      long:  {longTotal} |");
            Console.WriteLine($" 10s:{_10sCount} + 11s:{_11sCount} + 12s:{_12sCount}  + 13s:{_13sCount}  = {longPercentage:F1}%");
            Console.WriteLine("       ----------|-----------------------------------");
            Console.Write("     total: " + wordsArray.Count + " | ");

            if (wordsArray.Count != wordsArray.Distinct().Count())
            {
                Console.WriteLine("duplicates found:");
            }
            else
            {
                Console.WriteLine("no duplicates found");
            }

    //   var longest = wordsArray.OrderByDescending(s => s.Length).First();
    //   string superLongWord = longest.ToString();
    //        Console.Write($"longest:   13 | ");

            decimal averageLength = (decimal) _allLettersCount / (decimal) wordsArray.Count;
            Console.Write($"   average: {averageLength:F2} | ");

            var duplicates = wordsArray
                            .GroupBy(a => a)
                            .Where(b => b.Count() > 1)
                            .Select(c => c.Key)
                            .ToList();

            if (duplicates.Count != 0)
            {
                foreach (string s in duplicates)
                {
                    Console.Write(s + " ");
                }
            }
            else
            {
                Console.Write("the list is ready ...");
            }
        }
    }
}

//=====================EOF

