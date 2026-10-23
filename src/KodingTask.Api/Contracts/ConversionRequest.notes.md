# ConversionRequest

Request bodies of the conversion endpoints.

`Amount` is a string, not a number: the task defines the input as text (`45 100`,
`25,1`), and JSON numbers cannot carry the comma, the spaces or a leading zero. Parsing
and validation are the server's job, which is the point of the exercise.

Both fields are nullable so that a missing value reaches the converter and comes back as
a precise error code (`Empty`) instead of a generic model-binding failure. A missing
`Language` means English, the default language of the task.
