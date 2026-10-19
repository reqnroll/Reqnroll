---
hide-toc: true
---

# Feature Overview

What works, per IDE — and where something needs a second look, exactly
what to expect and what to do about it.

| Symbol | Meaning |
|---|---|
| ✅ | Works |
| ⚠️ | Works, with a caveat — see the note |
| ❌ | Not available in this IDE — see the note for the alternative |

## Editing Features

| Feature | VS Code | Visual Studio | Rider |
|---|:---:|:---:|:---:|
| [Syntax Highlighting](editing-features/syntax-highlighting.md) | ✅ | ✅ | ✅ |
| [Diagnostics — Errors & Warnings](editing-features/diagnostics.md) | ✅ | ✅ | ✅ |
| [Keyword & Step Completion](editing-features/completion.md) | ✅ | ✅ | ✅ |
| [Document & Table Formatting](editing-features/formatting.md) | ✅ | ✅ | ✅ |
| [Comment / Uncomment](editing-features/comment-uncomment.md) | ✅ | ✅ | ✅ |
| [Code Folding](editing-features/code-folding.md) | ✅ | ✅ | ✅ |
| [Document Outline](editing-features/document-outline.md) | ✅ | ❌¹ | ✅ |
| [Defining Steps](editing-features/defining-steps.md) | ✅ | ✅ | ✅ |
| [Rename Step](editing-features/rename-step.md) | ✅ | ✅ | ✅ |
| [Code Lens — Step Usage Counts](editing-features/code-lens.md) | ✅ | ✅ | ✅ |
| [Code Lens — Hook Matches](editing-features/code-lens.md) | ✅ | ✅ | ✅ |
| [Inlay Hints — Bound Step Info](editing-features/inlay-hints.md) | ✅ | ✅ | ✅ |
| [Clickable Tags](editing-features/clickable-tags.md) | ✅ | ✅ | ✅ |

## Navigation Features

```{admonition} Also surfaced via Code Lens
:class: note

Find Step Definition Usages and Hook Navigation both also show up
passively while you're reading code, via [Code Lens](editing-features/code-lens.md) —
a usage count above a step binding, or a hook-match count above a
`Feature:`/`Scenario:` line, that you can click straight through to the
same result these commands produce. Code Lens is listed under Editing
Features since it's something you see unprompted, not something you
invoke — but if you're looking for one of these two, Code Lens is often
the faster path since you don't have to invoke anything.
```

| Feature | VS Code | Visual Studio | Rider |
|---|:---:|:---:|:---:|
| [Go to Step Definition](navigation-features/go-to-definition.md) | ✅ | ✅ | ✅ |
| [Find Step Definition Usages](navigation-features/find-usages.md) | ✅ | ⚠️² | ✅ |
| [Find Unused Step Definitions](navigation-features/find-unused.md) | ✅ | ✅ | ✅ |
| [Hook Navigation ("Find Hooks")](navigation-features/hook-navigation.md) | ✅ | ✅ | ✅ |

## Running Scenarios

| Feature | VS Code | Visual Studio | Rider |
|---|:---:|:---:|:---:|
| [Running Scenarios](running-scenarios.md) | ❌⁴ | ✅ | ⚠️⁵ |

## Project Setup

| Feature | VS Code | Visual Studio | Rider |
|---|:---:|:---:|:---:|
| [New Project / Item Templates](new-project-templates.md) | ❌³ | ✅ | ❌³ |

---

¹ Visual Studio's native Document Outline window doesn't support
`.feature` files at all — a VS platform limitation, not something we can
configure around. See [Document Outline](editing-features/document-outline.md).

² Visual Studio's native **Find All References** (Shift+F12) does not
route to Reqnroll bindings — use the **N step usages** CodeLens above the
binding, or assign a keyboard shortcut to the **Find Step Usages** command (it
has no menu entry). See [Find Step Definition Usages](navigation-features/find-usages.md).

³ VS Code and Rider don't have an equivalent project wizard or file templates; create files with
the IDE's normal New File command and use the *Define missing step* quick fix to scaffold bindings
— see [New Project / Item Templates](new-project-templates.md).

⁴ VS Code has no Reqnroll-owned Run feature; install the **C# Dev Kit**
extension and use its own Test Explorer/CodeLens instead — see
[Running Scenarios](running-scenarios.md).

⁵ Rider's lens can only run a scenario, not debug it, and doesn't appear in
Rider's native Unit Tests tool window. See
[Running Scenarios](running-scenarios.md) for the details and the
workaround for debugging.

```{admonition} Preview status
:class: note

Some features above shipped after this page was last reviewed for accuracy
against the running extensions — if something here looks wrong or out of
date, please [file an issue](https://github.com/reqnroll/Reqnroll.IdeSupport/issues).
```
