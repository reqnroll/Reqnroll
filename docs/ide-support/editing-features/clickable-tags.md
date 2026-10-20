# Clickable Tags

A tag such as `@issue:1234` becomes a link to the matching item in your issue tracker. Tags are
underlined in the link color so you can see which ones are clickable. Hover a link to see its
target, and **Ctrl+Click** (**Cmd+Click** on macOS) to open it in your default handler.

Tags that match no configured pattern stay ordinary tags.

## Configuring links

Links come from the `ide.traceability.tagLinks` section of the project's `reqnroll.json`. Each entry
pairs a regular expression for the tag name with a URL template:

```json
"ide": {
  "traceability": {
    "tagLinks": [
      {
        "tagPattern": "issue:(?<id>\\d+)",
        "urlTemplate": "https://github.com/org/repo/issues/{id}"
      }
    ]
  }
}
```

* The pattern must match the **whole** tag name (without the `@`).
* Named capture groups fill the `{name}` placeholders in the template.
* The result must be an absolute URL, otherwise the tag is not linked.
* `javascript:`, `vbscript:`, `data:` and `command:` targets are never opened. Other schemes
  (`https:`, `file:`, `jira:`, and so on) are.
* Each project uses its own `reqnroll.json`, so a shared `.feature` file uses the patterns of its
  home project (see [Troubleshooting](../troubleshooting.md#a-shared-feature-file-shows-hooks-diagnostics-or-highlighting-from-the-wrong-project)).

See the [`traceability` section reference](#traceability-section) for the full schema.

### SpecSync projects

If the project has a `specsync.json`, tags of the form `<prefix>:<id>` link automatically to
`<projectUrl>/_workitems/edit/{id}`. No extra configuration is needed.

## Per IDE

:::{tab-set}

```{tab-item} Visual Studio
:sync: vs

Hovering a link shows its URL and "CTRL + click to follow link". Ctrl+Click opens it in your
system browser.
```

```{tab-item} VS Code
:sync: vscode

Ctrl+Click (Cmd+Click) the underlined tag. The URL is shown as the link tooltip.
```

```{tab-item} Rider
:sync: rider

Hovering a link for about half a second shows its target and how to follow it. Hold Ctrl (Cmd on
macOS) to get the hand cursor, and click to open it.
```

:::

```{note}
After you change `tagLinks` in `reqnroll.json`, rebuild the project so the change takes effect.
```
