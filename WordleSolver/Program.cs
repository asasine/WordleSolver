namespace WordleSolver
{
    internal class Program
    {
        static void Main()
        {
            var words = new Words("../data/words_05_letters.txt", "../data/words_remove.txt");
            var consoleInput = new ConsoleInput();
            var input = new FixedInput(new string[]
            {
                "rager",
                "2 1 0 2 0",
            }, true, consoleInput);

            var game = new Game(words, input);
            game.Run();
        }
    }
}
