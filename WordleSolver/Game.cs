namespace WordleSolver
{
    public class Game
    {
        private readonly Words words;
        private readonly IInput input;

        public Game(Words words, IInput input)
        {
            this.words = words;
            this.input = input;
        }

        public void Run()
        {
            Help();

            Guess? guess = null;
            for (int round = 0; round < 6; round++)
            {
                Console.WriteLine();
                Console.WriteLine($"Round {round + 1}");
                guess = null;
                while (guess == null)
                {
                    var bestWords = this.words.BestWords();
                    Console.WriteLine($"Best words (out of {this.words.Count()}):");
                    for (var wordIndex = 0; wordIndex < bestWords.Count; wordIndex++)
                    {
                        var word = bestWords.ElementAt(wordIndex);
                        Console.WriteLine($"{wordIndex}: {word}");
                    }

                    guess = GetGuess(bestWords);

                    if (guess == null)
                    {
                        var bestWord = bestWords.First();
                        Console.WriteLine($"Removing {bestWord}");
                        this.words.Remove(bestWord);
                    }
                }

                if (guess.IsWinning())
                {
                    break;
                }

                this.words.Guess(guess);
            }

            Console.WriteLine();
            if (guess == null || !guess.IsWinning())
            {
                Console.WriteLine("Better luck next time.");
            }
            else
            {
                Console.WriteLine("Nice!");
            }
        }

        private static void Help()
        {
            Console.WriteLine("Directions:");
            Console.WriteLine("<enter> to receive a new word.");
            Console.WriteLine("Enter a word from the suggestions.");
            Console.WriteLine("You can also enter a differnet 5-letter word.");
            Console.WriteLine($"Enter {GetColorsPhrase()} (space-separated) after selecting a word.");
            Console.WriteLine("The game ends after 6 guesses.");
        }

        private void Statistics()
        {
            Console.WriteLine(this.words.GetStatistics());
        }

        private Guess? GetGuess(IReadOnlyCollection<Word> words)
        {
            var word = GetWordInput(words);
            if (!word.HasValue)
            {
                return null;
            }

            var colors = GetColorInputs();

            return new Guess(word.Value, colors);
        }

        private Word? GetWordInput(IReadOnlyCollection<Word> words)
        {
            // input is either:
            // empty (asking for new word suggestions)
            // a number (selecting one of the suggested words)
            // a word (using an alternate word)
            var input = GetInput("Select a word with its index or enter a new one: ");
            if (input == null)
            {
                return null;
            }

            if (int.TryParse(input, out var wordIndex))
            {
                if (wordIndex < 0 || wordIndex > words.Count)
                {
                    Console.WriteLine($"The nunmber {wordIndex} is not between 0 and {words.Count}, try again");
                    return GetWordInput(words);
                }
                else
                {
                    var word = words.ElementAt(wordIndex);
                    Console.WriteLine($"Using word: {word}");
                    return word;
                }
            }

            if (input.Length == 5)
            {
                Console.WriteLine($"Using word: {input}");
                return new Word(input);
            }
            else
            {
                Console.WriteLine($"You entered a {input.Length}-letter word, please use 5 letters.");
                return GetWordInput(words);
            }
        }

        private IReadOnlyCollection<Color> GetColorInputs()
        {
            var input = GetInput($"Enter colors from {GetColorsPhrase("and")}: ");
            if (input == null)
            {
                return GetColorInputs();
            }

            var unparsedResults = input.Split(' ');
            if (unparsedResults.Length != 5)
            {
                Console.WriteLine($"Enter {GetColorsPhrase()} (space-separated) after guessing a word.");
                Console.WriteLine($"You provided {unparsedResults.Length} instead.");
                return GetColorInputs();
            }

            var results = unparsedResults
                .Select<string, (string original, Color? color)>(original =>
                {
                    if (Enum.TryParse(original, true, out Color color) && Enum.IsDefined(color))
                    {
                        return (original, color);
                    }
                    else
                    {
                        return (original, color: null);
                    }
                })
                .ToList()
                ;

            var badResults = results.Where(result => !result.color.HasValue).ToList();
            if (badResults.Any())
            {
                var phrase = string.Join("; ", badResults.Select(result => result.original));
                Console.WriteLine($"Unexpected input: {phrase}");
                return GetColorInputs();
            }

            var goodResults = results
                .Where(result => result.color.HasValue)
                .Select(result => result.color.GetValueOrDefault())
                .ToList();

            return goodResults;
        }

        private string? GetInput(string? prompt = null)
        {
            var line = this.input.ReadLine(prompt);
            if (line == "!help")
            {
                Console.WriteLine();
                Help();
                Console.WriteLine();
                return GetInput(prompt);
            }

            if (line == "!statistics" || line == "!stats")
            {
                Console.WriteLine();
                Statistics();
                Console.WriteLine();
                return GetInput(prompt);
            }

            return line;
        }

        private static string GetColorsPhrase(string conjunction = "or")
        {
            var colors = Enum.GetValues<Color>().Select(color => $"{color.ToString().ToLower()} ({(int)color})").ToArray();
            return $"{string.Join(", ", colors.Take(colors.Length - 1))}, {conjunction} {colors.Last()}";
        }
    }
}
