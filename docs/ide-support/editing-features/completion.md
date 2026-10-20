# Keyword & Step Completion

## Keyword completion

Typing at the start of a line in a scenario offers completions for the
keywords valid at that point (`Given`, `When`, `Then`, `And`, `But`,
`Scenario:`, `Feature:`, and so on). Completions are context-sensitive —
`Examples:` is only offered inside a Scenario Outline, `Background:` only
at feature level.

## Step completion

Typing a step line after a keyword offers completions for existing step
binding patterns, with parameter placeholders shown distinctly, and inserts
the full step text on selection.

:::{tab-set}

```{tab-item} Visual Studio
:sync: vs

![](completion/completion-vs.gif)
```

```{tab-item} VS Code
:sync: vscode

![](completion/completion-vscode.gif)
```

```{tab-item} Rider
:sync: rider

![](completion/completion-rider.gif)
```

:::

```{admonition} Rider completion — confirmed
:class: note

Completion works in Rider — it auto-negotiates via the IDE platform's
generic LSP completion support (the same mechanism diagnostics uses), no
Rider-specific plugin code needed. Confirmed live; see
[#414](https://github.com/reqnroll/Reqnroll.IdeSupport/issues/414).
```

## Tag completion

Typing `@` at a tag position offers the tags already used across the project, most-used first,
plus the built-in `@ignore` tag. Tags in other open `.feature` files are picked up as you type;
tags in closed files are read from disk. Completion is offered at every tag position, including a
second or third tag after a completed one, and a tag already typed on the line is not offered
again. A space also opens the list (after the space between two tags, or after a space typed on a
blank line before a tag). Accepting a suggestion replaces only the tag being typed.

## Non-English (dialect) keywords

Completions are sourced from the active Gherkin dialect in the project's
`reqnroll.json`. A project configured with `"language": "de"` offers
`Gegeben`, `Wenn`, `Dann` rather than `Given`, `When`, `Then`. See
[Feature Language](https://docs.reqnroll.net/latest/gherkin/feature-language.html)
in the main Reqnroll docs for the full list of supported languages.
