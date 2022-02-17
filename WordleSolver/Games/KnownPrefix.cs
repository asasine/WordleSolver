namespace WordleSolver.Games
{
    /// <summary>
    /// Limits the set of words by a known prefix, then plays the game as normal.
    /// </summary>
    internal class KnownPrefix : IGame
    {
        private readonly Words words;
        private readonly IInput input;

        public KnownPrefix(Words words, IInput input)
        {
            this.words = words;
            this.input = input;
        }

        public void Run()
        {
            var prefix = GetPrefixInput();
            var wordsToRemove = this.words.Where(word => !word.word.StartsWith(prefix)).ToList();
            Console.WriteLine($"Removing {wordsToRemove.Count} words");
            foreach (var wordToRemove in wordsToRemove)
            {
                this.words.Remove(wordToRemove);
            }

            Console.WriteLine();

            var normalGame = new Game(this.words, input);
            normalGame.Run();
        }

        private string GetPrefixInput()
        {
            string? prefix = null;
            while (prefix == null)
            {
                prefix = input.ReadLine("Enter the known, green prefix: ");
                if (prefix != null)
                {
                    if (prefix.Length > Constants.WORD_LENGTH)
                    {
                        Console.WriteLine($"{prefix} is longer than {Constants.WORD_LENGTH} characters long.");
                        prefix = null;
                    }
                }
            }

            return prefix;
        }
    }
}
