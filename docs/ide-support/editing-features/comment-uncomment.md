# Comment / Uncomment

The standard comment shortcuts toggle `#` comments on the selected line(s) in
a `.feature` file, the same as they would in any other file type (`Ctrl+/` on
Windows/Linux, `Cmd+/` on macOS). If nothing is selected, the line with the
cursor is used.

Visual Studio also supports its separate Comment Selection and Uncomment
Selection commands. VS Code also has a Command Palette command; Rider follows its keymap:

:::{tab-set}

```{tab-item} Visual Studio
:sync: vs

Reqnroll uses Visual Studio's standard comment commands, so they work with
their default shortcuts, from the **Edit → Advanced** menu, and with any
shortcut you've assigned to them yourself:

| Command | Default shortcut | What it does in a `.feature` file |
|---|---|---|
| **Edit.ToggleLineComment** | `Ctrl+/` | Uncomments the lines if they are all commented; otherwise comments them all |
| **Edit.CommentSelection** | `Ctrl+K, Ctrl+C` | Always adds a `#`, even to lines that are already commented |
| **Edit.UncommentSelection** | `Ctrl+K, Ctrl+U` | Removes one `#` from each commented line and leaves other lines unchanged |

There is no Reqnroll-specific Comment/Uncomment command in the context menu.
If you changed the shortcut, use **Tools → Options → Environment → Keyboard**
and set it on the `Edit.*` commands above.

```

```{tab-item} VS Code
:sync: vscode

`Ctrl+/` (`Cmd+/` on macOS) works directly, or use
**Reqnroll: Comment/Uncomment** from the Command Palette. There is no
context-menu entry for this command.


```

```{tab-item} Rider
:sync: rider

Reqnroll hooks into Rider's own **Comment with Line Comment** command, so
whatever your keymap binds to it works in a `.feature` file — `Ctrl+/` in the default keymap,
`Ctrl+K, Ctrl+C` / `Ctrl+K, Ctrl+U` in the Visual Studio keymaps — including keys you rebind.
There is no separate Reqnroll comment command and no context-menu entry.
In the Visual Studio keymaps `Ctrl+/` does not comment in a `.feature` file; it does
what that keymap says it does in C#.


```

:::
