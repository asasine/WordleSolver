using System.Collections;

namespace WordleSolver
{
    public class Guess : IReadOnlyCollection<LetterGuess>
    {
        public readonly Word word;
        public readonly IReadOnlyCollection<Color> results;

        private readonly IEnumerable<LetterGuess> zipped;

        public Guess(Word word, IReadOnlyCollection<Color> results)
        {
            if (results.Count != 5)
            {
                throw new ArgumentOutOfRangeException(nameof(results), "The results must have 5 items.");
            }

            this.word = word;
            this.results = results;

            this.zipped = word.word.Zip(Enumerable.Range(0, 5), results)
                .Select(tuple => new LetterGuess(tuple.First, tuple.Second, tuple.Third))
                .ToList();
        }

        public int Count => 5;

        public IEnumerator<LetterGuess> GetEnumerator() => this.zipped.GetEnumerator();

        public bool IsWinning() => results.All(result => result == Color.Green);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly record struct LetterGuess(char letter, int index, Color color);
}
