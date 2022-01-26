namespace WordleSolver
{
    internal class Program
    {
        static void Main()
        {
            var words = new Words("../data/words_of_the_day.txt");
            var consoleInput = new ConsoleInput();
            var game = new Game(words, consoleInput);
            game.Run();
        }
    }
}
