from os import PathLike
from pathlib import Path

def read_file(path: PathLike) -> set[str]:
    with open(path) as f:
        return set(filter(lambda word: len(word) > 0, map(lambda word: word.strip(), f.readlines())))

def read_directory(directory: Path) -> set[str]:
    if not (directory.exists() and directory.is_dir()):
        raise ValueError(f"Argument [directory] is not a valid path.")

    valid_words = read_file(directory / "valid_words.txt")
    words_of_the_day = read_file(directory / "words_of_the_day.txt")
    valid_words.update(words_of_the_day)
    return valid_words

if __name__ == "__main__":
    data_directory = Path(__file__).resolve().parents[0]

    updated_words = read_directory(data_directory)
    original_words = read_directory(data_directory / "original")

    words_removed = original_words - updated_words
    words_added = updated_words - original_words

    print(f"There have been {len(words_removed)} words removed.")
    print(f"There have been {len(words_added)} words added.")

    def print_words(words: set[str], word: str):
        if len(words) > 0:
            print()
            print(f"Words {word}:")
            for word in sorted(words):
                print(word)

    print_words(words_added, "added")
    print_words(words_removed, "removed")
