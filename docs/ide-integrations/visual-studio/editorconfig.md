# Gherkin Formatting Settings for .editorconfig

You can fine-tune the formatting for Gherkin files in your project by tweaking settings in your `.editorconfig` file. These options control indentation, table styling, and more for `*.feature` files recognized by the Reqnroll Visual Studio Extension.

## Supported Settings

| Setting Name | Description | Value Type | Default |
| :-- | :-- | :-- | :-- |
| `gherkin_indent_feature_children` | Indent child elements of Feature (Background, Rule, Scenario, Scenario Outline) | boolean | `false` |
| `gherkin_indent_rule_children` | Indent child elements of Rule (Background, Scenario, Scenario Outline) | boolean | `false` |
| `gherkin_indent_steps` | Indent steps in scenarios | boolean | `true` |
| `gherkin_indent_and_steps` | Apply additional indentation to `And`/`But` steps in scenarios | boolean | `false` |
| `gherkin_indent_datatable` | Indent DataTable arguments within steps | boolean | `true` |
| `gherkin_indent_docstring` | Indent DocString arguments within steps | boolean | `true` |
| `gherkin_indent_examples` | Indent the Examples block in Scenario Outlines | boolean | `false` |
| `gherkin_indent_examples_table` | Indent the Examples table within Examples blocks | boolean | `true` |
| `gherkin_table_cell_padding_size` | Number of spaces to pad table cells on each side | integer | `1` |
| `gherkin_table_cell_right_align_numeric_content` | Right-align numeric values in table cells | boolean | `true` |

**Note:** Only options with explicit default values are shown here; others default to their unspecified .NET/C# behavior.

## Sample `.editorconfig` for Gherkin Files

Below is an example of an `.editorconfig` section for `*.feature` files, including a setting for UTF-8 charset:

```ini
[*.feature]
charset = utf-8
gherkin_indent_feature_children = true
gherkin_indent_steps = true
gherkin_indent_datatable = true
gherkin_indent_docstring = true
gherkin_indent_examples_table = true
gherkin_table_cell_padding_size = 2
gherkin_table_cell_right_align_numeric_content = true
```

Adjust the values above to match your project's formatting needs.



