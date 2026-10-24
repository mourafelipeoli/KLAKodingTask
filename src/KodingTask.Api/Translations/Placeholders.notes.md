# Placeholders

Prepares a text for DeepL's XML mode and back. In XML mode `&` and `<` are markup, so the
text is escaped first; then each `{{name}}` is wrapped in an ignored `<x>` tag. On the way
back the tags are removed (also when DeepL added spaces inside them) and the entities are
decoded. A text without placeholders survives the round trip unchanged.
