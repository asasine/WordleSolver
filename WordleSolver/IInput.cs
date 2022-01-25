namespace WordleSolver
{
    public interface IInput
    {
        /// <summary>
        /// Returns a line of input.
        /// If the input is blank, returns <see langword="null" />.
        /// If <paramref name="prompt"/> is not <see langword="null" />, prints it as an input prompt.
        /// </summary>
        /// <param name="prompt">An input prompt or <see langword="null" />.</param>
        /// <returns>The next line of input or <see langword="null" /> if it's empty.</returns>
        public string? ReadLine(string? prompt);
    }

    public class ConsoleInput : IInput
    {
        public string? ReadLine(string? prompt)
        {
            if (prompt != null)
            {
                Console.Write(prompt);
            }

            var input = Console.ReadLine()?.Trim().ToLower();
            if (input == null || input.Length == 0)
            {
                return null;
            }

            return input;
        }
    }

    public class FixedInput : IInput
    {
        private readonly IReadOnlyCollection<string> lines;
        private int index;
        private readonly bool writeInputToConsole;
        private readonly IInput? fallbackInput;

        public FixedInput(IReadOnlyCollection<string> lines, bool writeInputToConsole)
            : this(lines, writeInputToConsole, null)
        {
        }

        public FixedInput(IReadOnlyCollection<string> lines, bool writeInputToConsole, IInput? fallbackInput)
        {
            this.lines = lines;
            this.index = 0;
            this.writeInputToConsole = writeInputToConsole;
            this.fallbackInput = fallbackInput;
        }

        public string? ReadLine(string? prompt)
        {
            if (prompt != null)
            {
                Console.Write(prompt);
            }

            if (this.index < this.lines.Count)
            {
                var line = this.lines.ElementAt(this.index);
                this.index++;
                if (this.writeInputToConsole)
                {
                    Console.WriteLine(line);
                }

                return line;
            }
            else if (this.fallbackInput != null)
            {
                return fallbackInput.ReadLine(null);
            }
            else
            {
                return null;
            }
        }
    }
}
