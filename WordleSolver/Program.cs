namespace WordleSolver
{
    internal class Program
    {
        static void Main()
        {
            var words = new Words("../data/words_05_letters.txt", "../data/words_remove.txt");
            var consoleInput = new ConsoleInput();
            var game = new Game(words, consoleInput);
            game.Run();
        }
    }
}
