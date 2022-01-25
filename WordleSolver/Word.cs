namespace WordleSolver
{
    public readonly struct Word
    {
        public readonly string word;

        public Word(string word)
        {
            if (word == null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            if (word.Length != 5)
            {
                throw new ArgumentOutOfRangeException(nameof(word), "Word must have 5 letters.");
            }

            this.word = word;
        }

        public override string ToString() => this.word;
    }
}
