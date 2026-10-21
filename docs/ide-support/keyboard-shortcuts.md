# Keyboard Shortcuts

Every Reqnroll command's keyboard shortcut or menu location, with one table per IDE.

Reqnroll adds no menu of its own to the IDE's menu bar. Commands that have no default shortcut
are in the editor's right-click menu, where they appear as a single Reqnroll group, or in the
IDE's command search (the Command Palette in VS Code, Find Action in Rider). Each command links
to its feature page for details.

:::{tab-set}

````{tab-item} Visual Studio
:sync: vs

| Command | Shortcut | Menu |
|---|---|---|
| [Go to Step Definition](navigation-features/go-to-definition.md) | F12, or Ctrl+Click | — |
| [Rename Step](editing-features/rename-step.md) | F2 (unambiguous binding) | Right-click → Rename Step... (ambiguous binding, opens picker dialog) |
| [Comment / Uncomment](editing-features/comment-uncomment.md) | Ctrl+/ (toggle), Ctrl+K, Ctrl+C (comment), Ctrl+K, Ctrl+U (uncomment) *(native VS defaults)* | Edit → Advanced → Toggle Line Comment / Comment Selection / Uncomment Selection |
| [Format Document](editing-features/formatting.md) | Ctrl+K, Ctrl+D *(native VS default)* | — |
| [Find Step Definition Usages](navigation-features/find-usages.md) | — | Assign a shortcut to Find Step Usages (Tools → Options → Keyboard) |
| [Find Unused Step Definitions](navigation-features/find-unused.md) | — | Right-click in a C# file → Find Unused Step Definitions in Projects |
| [Hook Navigation](navigation-features/hook-navigation.md) | — | Right-click → Find Hooks |
| [Define Missing Steps](editing-features/defining-steps.md) (quick-fix) | Ctrl+. (lightbulb) *(native VS default)* | — |
````

````{tab-item} VS Code
:sync: vscode

| Command | Shortcut | Menu |
|---|---|---|
| [Go to Step Definition](navigation-features/go-to-definition.md) | F12, or Ctrl+Click / Cmd+Click | — |
| [Rename Step](editing-features/rename-step.md) | F2 (in a `.feature` file or a C# step binding) | Right-click → Reqnroll: Rename Step..., or Command Palette |
| [Comment / Uncomment](editing-features/comment-uncomment.md) | Ctrl+/ (Cmd+/ on macOS) | Command Palette → Reqnroll: Comment/Uncomment |
| [Format Document](editing-features/formatting.md) | Shift+Alt+F (⇧⌥F on macOS) *(native VS Code default)* | — |
| [Find Step Definition Usages](navigation-features/find-usages.md) | — | Command Palette → Reqnroll: Find Step Usages, or the CodeLens |
| [Find Unused Step Definitions](navigation-features/find-unused.md) | — | Right-click in a C# file → Reqnroll: Find Unused Step Definitions in Projects, or Command Palette |
| [Hook Navigation](navigation-features/hook-navigation.md) | — | Right-click → Reqnroll: Find Hooks, or Command Palette |
| [Define Missing Steps](editing-features/defining-steps.md) (quick-fix) | Ctrl+. (Cmd+. on macOS) *(native VS Code default)* | Command Palette → Reqnroll: Define Steps |
| [Toggle inlay hints](editing-features/inlay-hints.md) | Hold Ctrl-Alt (⌥ on macOS) *(only in an `...UnlessPressed` mode)* | — |
| [Show the Reqnroll log](troubleshooting.md#where-are-the-logs-and-how-do-i-change-the-log-level) | — | Command Palette → Reqnroll: Show Output Channel |
````

````{tab-item} Rider
:sync: rider

| Command | Shortcut | Menu |
|---|---|---|
| [Go to Step Definition](navigation-features/go-to-definition.md) | Ctrl+B / Cmd+B, or Ctrl+Click (F12 / Ctrl+Click in the Visual Studio keymaps) *(native Rider "Go to Declaration" default)* | — |
| [Rename Step](editing-features/rename-step.md) — `.feature` side | Shift+F6 | Right-click → Reqnroll: Rename Step... |
| [Rename Step](editing-features/rename-step.md) — C# side | — *(native Rider Shift+F6 stays bound to ordinary C# rename)* | Right-click → Reqnroll: Rename Step... |
| [Comment / Uncomment](editing-features/comment-uncomment.md) | Whatever your keymap binds to *Comment with Line Comment* (Ctrl+/ in the default keymap; Ctrl+K, Ctrl+C / Ctrl+K, Ctrl+U in the Visual Studio keymaps) *(native)* | — |
| [Format Document](editing-features/formatting.md) | Ctrl+Alt+L (⌘⌥L on macOS) *(native Rider "Reformat Code" default)* | — |
| [Find Step Definition Usages](navigation-features/find-usages.md) | — | Find Action → Reqnroll: Find Step Usages, or the CodeLens |
| [Find Unused Step Definitions](navigation-features/find-unused.md) | — | Right-click in a C# file → Reqnroll: Find Unused Step Definitions in Projects, or Find Action |
| [Hook Navigation](navigation-features/hook-navigation.md) | — | Right-click → Reqnroll: Find Hooks |
| [Toggle inlay hints](editing-features/inlay-hints.md) | — | Find Action → Reqnroll: Show Feature Inlay Hints |
````

:::

```{admonition} Native IDE shortcuts vs. Reqnroll-specific commands
:class: note

Rows marked *(native ... default)* are your IDE's own keybinding for a
generic editor action (rename, format, go-to-declaration, quick-fix) that
Reqnroll participates in — they're not something this extension defines,
and they'll differ if you've customized your keymap. Rows with no such
note are commands this extension adds.
```
