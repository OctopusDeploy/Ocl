# Octopus Configuration Language (OCL)

The serialization library for the Octopus Configuration Language (OCL).

## Syntax

### EBNF

See [EBNF notation](https://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_form).

```ebnf
newline = "\n" | "\r\n"

name = { non_whitespace }

integer = { digit }
decimal = integer, ".", integer
string = double_quote, { not_a_double_quote }, double_quote

empty_array = "[", "]"
string_array = "[", { string }, "]"
decimal_array = "[", { decimal }, "]"
integer_array = "[", { integer }, "]"

literal = string | heredoc | decimal | integer | empty_array | string_array | decimal_array | integer_array

label = string

attribute = name, "=", literal 

block = name, { label }, "{", newline, [ body, newline ], "}", newline

body = { block | attribute }
document = body
```

### Names

Names identify a block or attribute. A Name can consist of any non-whitespace character. 

Names do not need to be unique. Attributes with the same name in the same block as another attribute or block
is generally *not* supported by the target schema. Blocks with the same name are common as that is the way to define 
lists of complex types.

### Numbers

`integers` and `decimals` are supported.

Exponential syntax (i.e. `1e6`) is not supported

e.g.
```hcl
int_attribute = 1
decimal_attribute = 1.3
```

### Quoted String

String can be declared by placing it between two `"` characters. 

Special character escaping is currently not supported. Therefore the string cannot contain a `"`.

### Heredoc

Strings can also be declared by using the Heredoc syntax. `<<` starts the heardoc block, followed by the "tag", which is one or more 
non-whitespace characters. This tag, when it appears on a line by itself (other than whitespace) denotes the end of the block. All lines
between the start and end lines are taken verbatim as the string value.

Escaping of characters is not support and is not required.

e.g.
```hcl
string_attribute = <<EOF
This
   is

  the "value"

EOF
```
represents the string
```
This
   is

  the "value"

```

### Indented Heredoc

The indentation if often important, and left justifying makes the file less readable. Therefore if the Heredoc block starts with `<<-` instead of `<<`,
the block can be indented. It works by finding the least indented non-whitespace-only line and unindenting by that number of characters. Tabs are treated
as a single character.

e.g.
```hcl
string_attribute = <<-EOF
                    This
                       is
                    
                      the "value"

                    EOF
```

represents the string
```
This
   is

  the "value"

```

 

### Attributes
Attributes are name value pairs. 

The name, `=`, and start of the value **must** be on the same line.

Hashes or dictionaries are not supported.

Valid:
```hcl
int_attribute = 1

heredoc_attribute = <<EOF
      Text
EOF

```

Invalid:
```hcl
int_attribute =
 1

int_attribute 
    = 1

heredoc_attribute = 
<<EOF
      Text
EOF

hash_attribute = {
    child = 1
}
```

### Blocks

Blocks represent a collection of blocks and attributes. Blocks start with a name and can have zero or more labels. 

The name, labels, and `{` **must** be on the same line. The closing brace **must** be on a line by itself. The exception is empty
blocks where the closing brace can be on the same line as the opening

e.g. Valid:
```hcl
inline_empty_block { }

empty_block {
}

block_with_children_and_labels "Label 1" "Label 2" {
    child_block {}
    child_attribute = 1
}
```

Invalid:
```hcl
my_block 
{
}

my block {
}
```