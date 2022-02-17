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

            if (args.Contains("--all-words"))
            {
                wordsPaths.Add("../data/valid_words.txt");
            }

            var words = new Words(wordsPaths.ToArray());
            var input = new ConsoleInput();

            IGame game;
            if (args.Contains("--mode=known-prefix"))
            {
                game = new KnownPrefix(words, input);
            }
            else
            {
                game = new Game(words, input);
            }

            game.Run();
        }
    }
}
