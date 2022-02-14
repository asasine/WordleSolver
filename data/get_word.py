import argparse
from datetime import date
from pathlib import Path

epoch = date(2021, 6, 19)

def get_word(number: int, directory: Path) -> str:
    with open(directory / "words_of_the_day.txt") as f:
        for line_number, line in enumerate(f):
            if line_number == number:
                return line.strip()

if __name__ == "__main__":
    day = (date.today() - epoch).days

    parser = argparse.ArgumentParser(description="Get the word of the day")
    parser.add_argument("-d", "--day", type=int, help=f"The day to get. Defaults to today ({day})", default=day)
    parser.add_argument("--original", action="store_true", help="Use the original list of words.")
    args = parser.parse_args()

    day = args.day

    data_directory = Path(__file__).resolve().parents[0]
    directory = data_directory / "original" if args.original else data_directory

    print(f"Wordle {day}: {get_word(day, directory)}")
