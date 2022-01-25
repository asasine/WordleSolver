namespace WordleSolver
{
    public class Words
    {
        private List<Word> words;

        public Words(string wordsPath, string? wordsToRemovePath = null)
        {
            HashSet<Word> wordsToRemove;
            if (File.Exists(wordsToRemovePath))
            {
                wordsToRemove = File.ReadAllLines(wordsToRemovePath)
                    .Select(line => line.Trim())
                    .Where(line => line.Length >= 0)
                    .Select(line => new Word(line))
                    .ToHashSet();
            }
            else
            {
                wordsToRemove = new HashSet<Word>();
            }

            var lines = File.ReadAllLines(wordsPath)
                .Select(line => line.Trim())
                .Where(line => line.Length >= 0)
                .Select(line => new Word(line))
                .Except(wordsToRemove)
                .ToList();

            this.words = new List<Word>(lines);
            SortWords();
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
            var distinctGreyLetters = greyLetters.Aggregate(new Dictionary<char, int>(5), (accumulated, tuple) =>
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

        private void SortWords()
        {
            this.words.Sort(new Statistics(this.words));
        }

        private static string Pluralize(string singularWord, string pluralWord, int count) => count == 1 ? singularWord : pluralWord;

        private class Statistics : Comparer<Word>
        {
            private readonly IDictionary<char, double> relativeFrequencies;

            public Statistics(IReadOnlyCollection<Word> words)
            {
                var counts = new Dictionary<char, int>(26);
                for (char letter = 'a'; letter <= 'z'; letter++)
                {
                    counts[letter] = 0;
                }

                foreach (var word in words)
                {
                    foreach (var letter in word.word)
                    {
                        counts[letter]++;
                    }
                }

                double total = counts.Sum(pair => pair.Value);
                this.relativeFrequencies = new Dictionary<char, double>(26);
                foreach (var (letter, count) in counts)
                {
                    this.relativeFrequencies[letter] = count / total;
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

                // same letter uniqueness and frequencies, choose arbitrarily
                return 1;
            }
        }
    }
}
