# Arguments

executable ATTR RULES [-v]

ATTR
  Path to text file defining valid attributes. Required.

RULES
  Path to text file defining substitutions. Required.

VERBOSE
  `-v` for extra output. Optional.

INPUT
  Type (or redirect) a comma-separated list of attributes to stdin.
  eg. `bone, meat`
  The program terminates after reading 1 line of text.



# Quirks

Attributes are case-insensitive.

Any non-whitespace character string is a valid attribute.

A comment begins with `#` as the first non-whitespace character.

Recipes may have duplicates.

The output of a recipe can be empty.

"Markdown" properly adds syntax highlighting.

Input files can be any text format, including `.md`.