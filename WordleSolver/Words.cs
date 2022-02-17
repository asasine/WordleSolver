using System.Collections;
using System.Text;

namespace WordleSolver
{
    public class Words : IReadOnlyCollection<Word>
    {
        private List<Word> words;
        private readonly Statistics originalStatistics;

        int IReadOnlyCollection<Word>.Count => ((IReadOnlyCollection<Word>)words).Count;

        public Words(params string[] wordsPaths)
        {
            IEnumerable<string> lines = Enumerable.Empty<string>();
            foreach (var wordsPath in wordsPaths)
            {
                lines = lines.Concat(File.ReadAllLines(wordsPath));
            }

            lines = lines
                .Distinct()
                .Select(line => line.Trim())
                .Where(line => line.Length >= 0);

            this.words = lines.Select(line => new Word(line)).ToList();
            this.originalStatistics = new Statistics(this);
            SortWords(this.originalStatistics);
        }

        public void Guess(Guess guess)
        {
            var validWords = new List<Word>(this.words);

            validWords.Remove(guess.word);

            var greenLetters = guess.Where(g => g.color == Color.Green).ToList();
            var yellowLetters = guess.Where(g => g.color == Color.Yellow).ToList();
            var greyLetters = guess.Where(g => g.color == Color.Grey).ToList();

            var countRemovedGreen = RemoveByGreenConstraint(validWords, greenLetters, yellowLetters, greyLetters);
            Console.WriteLine($"Removed {countRemovedGreen} {Pluralize("word", "words", countRemovedGreen)} which {Pluralize("does", "do", countRemovedGreen)} not contain a {Color.Green} letter in the correct position.");

            var countRemovedGrey = RemoveByGreyConstraint(validWords, greenLetters, yellowLetters, greyLetters);
            Console.WriteLine($"Removed {countRemovedGrey} {Pluralize("word", "words", countRemovedGrey)} which {Pluralize("contains", "contain", countRemovedGrey)} too many a {Color.Grey} letters.");

            var (countRemovedContainsAllYellow, countRemovedContainsYellowInPosition) = RemoveByYellowConstraint(validWords, greenLetters, yellowLetters, greyLetters);
            Console.WriteLine($"Removed {countRemovedContainsAllYellow} {Pluralize("word", "words", countRemovedContainsAllYellow)} which {Pluralize("does", "do", countRemovedContainsAllYellow)} not contain all of the {Color.Yellow} letters.");
            Console.WriteLine($"Removed {countRemovedContainsYellowInPosition} {Pluralize("word", "words", countRemovedContainsYellowInPosition)} which {Pluralize("contains", "contain", countRemovedContainsYellowInPosition)} a {Color.Yellow} letter in an incorrect position.");

            int countRemaining = validWords.Count;
            var countRemovedTotal = this.words.Count - countRemaining;
            Console.WriteLine($"{countRemaining} valid {Pluralize("word", "words", countRemaining)} {Pluralize("remains", "remain", countRemaining)} from {words.Count} ({countRemovedTotal} eliminated)");

            this.words = validWords;
            SortWords();

        }

        private static int RemoveByGreenConstraint(List<Word> validWords, IReadOnlyCollection<LetterGuess> greenLetters, IReadOnlyCollection<LetterGuess> yellowLetters, IReadOnlyCollection<LetterGuess> greyLetters)
        {
            // remove all words which do not have any green letters in the correct position
            var countRemovedGreen = 0;
            foreach (var (letter, index, _) in greenLetters)
            {
                countRemovedGreen += validWords.RemoveAll(word => word.word[index] != letter);
            }

            return countRemovedGreen;
        }

        private static int RemoveByGreyConstraint(List<Word> validWords, IReadOnlyCollection<LetterGuess> greenLetters, IReadOnlyCollection<LetterGuess> yellowLetters, IReadOnlyCollection<LetterGuess> greyLetters)
        {
            // remove all words which contain a grey letter
            // edge case: if multiples of a letter are in the guess, and not all of them are in the final answer, the extras will be grey
            // thereore, only remove words where the count of the letter is less than or equal to the number of greens + yellows - greys of that letter
            var countRemovedGrey = 0;
            var distinctGreyLetters = greyLetters.Aggregate(new Dictionary<char, int>(Constants.WORD_LENGTH), (accumulated, tuple) =>
            {
                if (!accumulated.ContainsKey(tuple.letter))
                {
                    accumulated[tuple.letter] = 0;
                }

                accumulated[tuple.letter]++;

                return accumulated;
            });

            foreach (var letter in distinctGreyLetters.Keys)
            {
                var countLetterGreens = greenLetters.Count(tuple => tuple.letter == letter);
                var countLetterYellows = yellowLetters.Count(tuple => tuple.letter == letter);

                countRemovedGrey += validWords.RemoveAll(word =>
                {
                    var countLetter = word.word.Count(c => c == letter);
                    if (countLetter == 0)
                    {
                        return false;
                    }

                    var reservedLetters = countLetterGreens + countLetterYellows;
                    var unreservedLetters = countLetter - reservedLetters;
                    return unreservedLetters >= distinctGreyLetters[letter];
                });
            }

            return countRemovedGrey;
        }

        private static (int countRemovedContainsAllYellow, int countRemovedContainsYellowInPosition) RemoveByYellowConstraint(List<Word> validWords, IReadOnlyCollection<LetterGuess> greenLetters, IReadOnlyCollection<LetterGuess> yellowLetters, IReadOnlyCollection<LetterGuess> greyLetters)
        {
            // remove all words which do not have all of the yellow letters
            var countRemovedContainsAllYellow = validWords.RemoveAll(word => !yellowLetters.All(tuple => word.word.Contains(tuple.letter)));

            // remove all words which contain a yellow letter at the guessed position
            var countRemovedContainsYellowInPosition = 0;
            foreach (var (letter, index, _) in yellowLetters)
            {
                countRemovedContainsYellowInPosition += validWords.RemoveAll(word => word.word[index] == letter);
            }

            return (countRemovedContainsAllYellow, countRemovedContainsYellowInPosition);
        }

        public IReadOnlyCollection<Word> BestWords() => this.words.Take(5).ToArray();

        public int Count()
        {
            return words.Count;
        }

        public void Remove(Word word)
        {
            this.words.Remove(word);
        }

        public Statistics GetStatistics() => new(this);

        private void SortWords(Statistics? statistics = null)
        {
            this.words.Sort(statistics ?? new Statistics(this));
        }

        private static string Pluralize(string singularWord, string pluralWord, int count) => count == 1 ? singularWord : pluralWord;

        public IEnumerator<Word> GetEnumerator()
        {
            return ((IEnumerable<Word>)words).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)words).GetEnumerator();
        }

        public class Statistics : Comparer<Word>, IEquatable<Statistics>
        {
            private readonly Words words;
            private readonly IDictionary<char, double> relativeFrequencies;
            private readonly IReadOnlyCollection<IDictionary<char, double>> relativeFrequenciesPerPosition;

            public Statistics(Words words)
            {
                this.words = words;

                static IDictionary<char, double> createLetterDictionary()
                {
                    var dictionary = new Dictionary<char, double>();
                    for (char letter = 'a'; letter <= 'z'; letter++)
                    {
                        dictionary[letter] = 0;
                    }

                    return dictionary;
                }

                var countsPerPosition = Enumerable
                    .Repeat(0, Constants.WORD_LENGTH)
                    .Select(_ => createLetterDictionary())
                    .ToList();

                foreach (var word in this.words.words)
                {
                    for (int i = 0; i < Constants.WORD_LENGTH; i++)
                    {
                        var letter = word.word[i];
                        countsPerPosition.ElementAt(i)[letter]++;
                    }
                }

                var relativeFrequenciesPerPosition = Enumerable
                    .Repeat(0, Constants.WORD_LENGTH)
                    .Select(_ => createLetterDictionary())
                    .ToList();

                for (var i = 0; i < Constants.WORD_LENGTH; i++)
                {
                    var counts = countsPerPosition.ElementAt(i);
                    double total = counts.Sum(pair => pair.Value);
                    var relativeFrequencies = relativeFrequenciesPerPosition.ElementAt(i);
                    foreach (var (letter, count) in counts)
                    {
                        relativeFrequencies[letter] = count / total;
                    }
                }

                this.relativeFrequenciesPerPosition = relativeFrequenciesPerPosition;

                {
                    var counts = countsPerPosition.Aggregate(createLetterDictionary(), (counts, countsAtPosition) =>
                    {
                        foreach (var (letter, count) in countsAtPosition)
                        {
                            counts[letter] += count;
                        }

                        return counts;
                    });

                    double total = countsPerPosition.Sum(counts => counts.Sum(pair => pair.Value));
                    var relativeFrequencies = createLetterDictionary();
                    foreach (var (letter, count) in counts)
                    {
                        relativeFrequencies[letter] = count / total;
                    }

                    this.relativeFrequencies = relativeFrequencies;
                }
            }

            public override int Compare(Word x, Word y)
            {
                if (x.Equals(y))
                {
                    return 0;
                }

                // prefer words with unique letters
                var xDistinctLetterCount = x.word.Distinct().Count();
                var yDistinctLetterCount = y.word.Distinct().Count();
                if (xDistinctLetterCount != yDistinctLetterCount)
                {
                    return yDistinctLetterCount - xDistinctLetterCount;
                }

                // otherwise prefer words with more frequent letters
                var xRelativeFrequency = x.word.Sum(letter => this.relativeFrequencies[letter]);
                var yRelativeFrequency = y.word.Sum(letter => this.relativeFrequencies[letter]);
                if (xRelativeFrequency != yRelativeFrequency)
                {
                    if (xRelativeFrequency < yRelativeFrequency)
                    {
                        return 1;
                    }
                    else if (xRelativeFrequency > yRelativeFrequency)
                    {
                        return -1;
                    }
                }

                // same letter uniqueness and frequencies, choose based on the original statistics
                // unless this Statistics is equivalent to the original Statistics, in which case this is infinitely recursive
                if (!this.Equals(this.words.originalStatistics))
                {
                    var originalCompareTo = this.words.originalStatistics.Compare(x, y);
                    if (originalCompareTo != 0)
                    {
                        return originalCompareTo;
                    }
                }

                // original had same uniqueness and frequencies, choose arbitrarily
                return 1;
            }

            public bool Equals(Statistics? other)
            {
                if (other == null)
                {
                    return false;
                }

                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }

                return this.relativeFrequencies.Equals(other.relativeFrequencies);
            }

            public override string? ToString()
            {
                const int columnWidth = 8;
                var stringBuilder = new StringBuilder();
                var headers = new string[] { "letter", "total", "0", "1", "2", "3", "4" };
                foreach (var header in headers)
                {
                    stringBuilder.Append($"{header,columnWidth}");
                }

                stringBuilder.AppendLine();

                for (var letter = 'a'; letter <= 'z'; letter++)
                {
                    stringBuilder.Append($"{letter,columnWidth}");
                    stringBuilder.Append($"{this.relativeFrequencies[letter],columnWidth:P2}");
                    for (var i = 0; i < Constants.WORD_LENGTH; i++)
                    {
                        stringBuilder.Append($"{this.relativeFrequenciesPerPosition.ElementAt(i)[letter],columnWidth:P2}");
                    }

                    stringBuilder.AppendLine();
                }

                return stringBuilder.ToString();
            }

            public override bool Equals(object? obj) => obj != null && Equals(obj as Statistics);

            public override int GetHashCode() => this.words.GetHashCode();
        }
    }
}
