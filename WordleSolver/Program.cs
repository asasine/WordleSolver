using WordleSolver.Games;

namespace WordleSolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var wordsPaths = new List<string>()
            {
                "../data/words_of_the_day.txt"
            };

            if (args.Length >= 1 && args[0].ToLower() == "--all-words")
            {
                wordsPaths.Add("../data/valid_words.txt");
            }

            var words = new Words(wordsPaths.ToArray());
            var consoleInput = new ConsoleInput();
            var game = GetGame(consoleInput, words);

            game.Run();
        }

        static IGame GetGame(IInput input, Words words)
        {
            return new Game(words, input);
        }
    }
}
