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
            if (results.Count != Constants.WORD_LENGTH)
            {
                throw new ArgumentOutOfRangeException(nameof(results), $"The results must have {Constants.WORD_LENGTH} items.");
            }

            this.word = word;
            this.results = results;

            this.zipped = word.word.Zip(Enumerable.Range(0, Constants.WORD_LENGTH), results)
                .Select(tuple => new LetterGuess(tuple.First, tuple.Second, tuple.Third))
                .ToList();
        }

        public int Count => Constants.WORD_LENGTH;

        public IEnumerator<LetterGuess> GetEnumerator() => this.zipped.GetEnumerator();

        public bool IsWinning() => results.All(result => result == Color.Green);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly record struct LetterGuess(char letter, int index, Color color);
}
