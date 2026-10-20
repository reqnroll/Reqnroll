# Troubleshooting / FAQ

## How do I report a bug?

File an issue on the
[Reqnroll.IdeSupport repository](https://github.com/reqnroll/Reqnroll.IdeSupport/issues),
including your IDE and version, the extension version, and — if possible —
the relevant log file (see [Where are the logs](#where-are-the-logs-and-how-do-i-change-the-log-level)
below).

## Known per-IDE limitations

* **Visual Studio** — the native Document Outline window does not show
  `.feature` file structure. See [Document Outline](editing-features/document-outline.md).
* **Visual Studio** — native "Find All References" (Shift+F12) does not
  route to Reqnroll step bindings; use the dedicated entry point instead.
  See [Find Step Definition Usages](navigation-features/find-usages.md).
* **All IDEs** — a project that targets more than one framework
  (`<TargetFrameworks>`) is analysed for a single target framework only. See
  [Multi-targeted projects](#multi-targeted-projects-use-a-single-target-framework).

## A dialog explains why a rename, format or comment did nothing

When the language server rejects an edit for a reason you can fix, the IDE shows the reason in a
modal dialog. Typical cases are a Rename Step that changes a parameter count, a Format Document on
a file with no `Feature:` line yet, and a Comment/Uncomment on a document the server does not have
open. Fix the cause and run the command again. A rejection that happens automatically (format on
save, on paste, Code Cleanup on save) is only written to the log, never shown. An already-formatted
document stays silent. In Rider, an explicit **Reformat Code** shows the reason; the platform's own
formatter does not.

## Multi-targeted projects use a single target framework

If a test project declares more than one target framework (for example
`<TargetFrameworks>net8.0;net481</TargetFrameworks>`), Reqnroll IDE Support analyses it for
**one** of those frameworks. It reads step definitions and hooks from that framework's compiled
output assembly and does not combine or compare results across frameworks.

The IDE decides which framework that is, so it can differ between IDEs for the same project:

* **Visual Studio** — the target framework Visual Studio reports for the project.
* **VS Code** — the first framework listed in `TargetFrameworks`.
* **Rider** — the first build output Rider lists for the project.

There is no setting to choose the framework. In practice:

* **Bindings that exist for only one framework.** A step definition or hook compiled only for a
  framework other than the one in use is not discovered, so its steps can show as undefined and
  it is not counted by CodeLens or Find Usages. No warning is shown.
* **Bindings inside `#if` blocks.** Features that read `.cs` source while you edit (rather than
  the compiled assembly) do not evaluate conditions such as `#if NET481`. If the same binding is
  declared under more than one `#if` branch, it can be reported as a duplicate binding.
* **Different results in different IDEs.** If the IDEs pick different frameworks, the same
  project can show different binding results in each.

If your step definitions don't depend on the framework, none of this affects you. In VS Code,
listing the framework you want analysed first in `TargetFrameworks` selects it.

Full support for multi-targeted projects is not planned for the Beta; the reasoning is recorded
in [issue #1177](https://github.com/reqnroll/Reqnroll.IdeSupport/issues/1177).

## Can I have both extensions installed at once?

Both **can** be installed side by side — installing one doesn't remove the
other. But running both **enabled** at the same time is **not a supported
configuration**: with both active, you'll get duplicate/conflicting
behavior (e.g. two sets of diagnostics, two CodeLens annotations) for the
same `.feature` files.

If you have both installed, disable one: **Extensions → Manage
Extensions**, select the extension you're not using, and click
**Disable**. See [Installation](installation/index.md) (Visual Studio tab)
for how to tell the two listings apart in the Marketplace.

## A shared `.feature` file shows hooks, diagnostics, or highlighting from the "wrong" project

If the same `.feature` file is linked into more than one project — for example, a
`Calculator.feature` that physically lives in `ProjectA` and is also linked into `ProjectB` —
Code Lens hook-match counts, **Find Hooks**, diagnostics/squiggles, and syntax highlighting for
that file always reflect **the project that physically contains the file on disk**, regardless
of which project's node you used to open it (Solution Explorer, VS Code's Explorer, or Rider's
Project view).

This is deterministic, and by design rather than a bug: opening a file only identifies it by its
path, with no way to know which project's node you clicked through to get there, so a single
project has to be picked to drive what's shown. It's always the file's **home project** — the
one whose folder physically contains it — never whichever project you happened to navigate from.

If bindings differ between the two projects, expect Code Lens, [Hook Navigation](navigation-features/hook-navigation.md),
and diagnostics on the shared file to reflect the home project's bindings only, even when the
file is viewed "from" the other project.

## Visual Studio: a `ReqnrollActivation.feature` tab opens and closes at startup

Visual Studio starts the Reqnroll language server when a `.feature` file is *opened*. It does not
check files that are already open. On the first launch after the extension is installed or
updated, Visual Studio can restore your `.feature` tabs before it has registered the extension.
Those tabs would then get no Reqnroll features for the whole session.

To recover, the extension waits a few seconds after the solution loads. If a `.feature` file is
open and the language server has still not started, it briefly opens and closes a scratch file,
`%TEMP%\Reqnroll\ReqnrollActivation.feature`. That starts the language server, and your own
`.feature` tabs get their features. Your files are not closed or reloaded. On a normal start the
language server is already running, and nothing is opened.

To turn this off, set the environment variable `REQNROLL_IDE_DISABLE_ACTIVATION_TRIGGER` to any
value other than empty, `0` or `false` before starting Visual Studio. If a restored `.feature` tab
then has no Reqnroll features, close and reopen it.

## Visual Studio: GitHub Copilot suggestions compete with `.feature` file editing

Visual Studio does not offer a per-file-type or per-content-type way to turn off GitHub Copilot
(inline "ghost text" suggestions, or the lightbulb's Copilot-provided "Fix" action) for `.feature`
files specifically. Once Copilot is enabled in the
IDE at all, it applies uniformly to every open document — there's no content-type or file-extension
scoping to opt out of, and no supported extensibility point Reqnroll IDE Support could hook to
suppress it automatically for Gherkin. The empty "Fix" lightbulb that spins forever on a
`reqnroll.parser`/`reqnroll.binding` diagnostic is this same Copilot quick-fix provider finding
nothing to offer — not a Reqnroll IDE Support action.
IntelliCode's separate whole-line completions don't apply here — that feature is C#-only and never
activates on `.feature` files.

If Copilot's suggestions are getting in the way while editing Gherkin, the available controls are
all IDE-wide (Visual Studio has no per-language settings page for the `.feature` content type to
scope any of these narrower):

* **Turn off Copilot completions entirely** — click the **Copilot** badge (top-right of the editor)
  → uncheck **Completions**, or **Tools → Options → GitHub → Copilot → Completions**.
* **Make suggestions manual instead of automatic** (a lighter touch — keeps Copilot available on
  demand everywhere, including `.feature` files) — **Tools → Options → Text Editor → Inline
  Suggestions → General**, set **Inline Suggestions Invocation** to **Manual**. Trigger a suggestion
  only when wanted with **Alt+.** / **Alt+,**.
* **Org-managed exclusion by path** — if your organization has GitHub Copilot Business or
  Enterprise, an admin can configure
  [Content Exclusion](https://learn.microsoft.com/visualstudio/ide/visual-studio-github-copilot-admin#configure-content-exclusion)
  for a path pattern like `**/*.feature`, which blocks both completions and Chat context for
  matching files repo- or org-wide. This is configured server-side by an admin, not from within
  Visual Studio, and isn't available on individual/free Copilot plans.

## Where are the logs, and how do I change the log level?

:::{tab-set}

```{tab-item} Visual Studio
:sync: vs

Log files are written to `%LOCALAPPDATA%\Reqnroll\logs\`, one set per process,
named `reqnroll-vs-<role>-<yyyyMMdd>-<pid>.log`:

- `reqnroll-vs-server-*.log` — the LSP server's application log
- `reqnroll-vs-protocol-*.log` — protocol/wire-level internals
- `reqnroll-vs-ext-*.log` — the Visual Studio extension side
- `reqnroll-vs-codelens-sh-*.log` — the Run CodeLens components, which run in
  Visual Studio's separate CodeLens host process

The extension's own messages (Info and above) also appear in the **Reqnroll**
Output window pane (**View → Output**, then pick **Reqnroll** from the
dropdown), which comes to the front automatically on a warning or error. The
pane shows a one-line summary of each message; the full details, including
stack traces, are only in the log files above. The pane doesn't show the LSP
server's logs — check those files directly.

**Changing the log level:** there's no in-product setting. A normal
(released, VSIX-installed) build logs the LSP server at `Warning` level and the
extension side at `Info` by default. The
`REQNROLLVS_DEBUG` environment variable (set it to `1`, `true`, or a
[`TraceLevel`](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracelevel)
name, e.g. `Verbose`) raises the verbosity of **both sides**: the
**extension-side** loggers (`reqnroll-vs-ext-*.log`, `reqnroll-vs-codelens-sh-*.log`)
*and* the **LSP server's own** `reqnroll-vs-server-*.log`/`reqnroll-vs-protocol-*.log`
files. The server reads `REQNROLLVS_DEBUG` itself at startup and lets it override
whatever level Visual Studio requested; Visual Studio launches the server process
with its own environment inherited (no override), so anything set in the
environment `devenv.exe` runs in reaches the server too. This isn't
Visual-Studio-specific — the same environment variable, read the same way,
raises the server's logs no matter which of the three IDEs is hosting it (see
the note below and the Rider tab).

Set it before starting Visual Studio, since the variable is only read once, at
server/extension startup:

- Open a new **Command Prompt** and run `setx REQNROLLVS_DEBUG 1` (or a
  `TraceLevel` name), then close and reopen it so the change takes effect —
  `setx` writes the user environment but doesn't update the current session.
- Or **System Properties → Environment Variables** → add it under **User
  variables**.

Either way, restart Visual Studio afterwards (a new `devenv.exe` process picks
up the updated environment; an already-running one won't).
```

```{tab-item} VS Code
:sync: vscode

Two Output channels (**View → Output**, then pick from the dropdown):

- **Reqnroll** — a one-line summary of extension activation, LSP client
  start/connect/stop, and each command's outcome, auto-revealing on a warning
  or error. Also written to `reqnroll-vscode-app-<yyyyMMdd>-<pid>.log`.
- **Reqnroll LSP** — the language client's own connection-level diagnostics.
  Also written to `reqnroll-vscode-ext-<yyyyMMdd>-<pid>.log`.

The LSP wire trace has no Output channel; it goes only to the trace file
described below.

**Changing the log level:** set `"reqnroll.trace.server"` in
`settings.json` to `"off"`, `"messages"`, or `"verbose"`. This maps onto the
LSP server's own `--log-level` (`"off"`/`"messages"`/`"verbose"` →
`Warning`/`Info`/`Verbose`), so it raises the server's file-log verbosity.
Setting it to `"verbose"` also writes a timestamped trace file under
`%LOCALAPPDATA%\Reqnroll\logs\` (Windows), `~/Library/Logs/Reqnroll/logs/` (macOS),
or `~/.local/share/Reqnroll/logs/` (Linux):
`reqnroll-vscode-inspector-<timestamp>.log`. **Reload the window** after
changing this setting for it to take effect.

The LSP server's **protocol** log (`reqnroll-vscode-protocol-*.log`, OmniSharp's
own internal request-dispatch/DryIoc/JSON-RPC diagnostics) has its own dial:
set `"reqnroll.protocolLogLevel"` in `settings.json` to `"Off"`, `"Error"`,
`"Warning"`, `"Info"`, or `"Verbose"` (default `"Warning"`). It maps onto the
server's separate `--protocol-log-level` flag, independent of
`reqnroll.trace.server` (which drives `--log-level`). **Reload the window**
after changing it for it to take effect.

The `REQNROLLVS_DEBUG` environment variable (see the Visual Studio tab for
accepted values) also works here, and overrides `reqnroll.trace.server` for
the server's own logs specifically. VS Code's child process inherits the
environment the VS Code application itself was started with, so set the
variable there before launching VS Code:

- **Windows** — `setx REQNROLLVS_DEBUG 1` in a new Command Prompt, then
  restart VS Code.
- **macOS** — add `export REQNROLLVS_DEBUG=1` to your shell profile
  (`~/.zshrc`/`~/.bash_profile`) if you launch VS Code from a terminal (`code`);
  if you launch it from Spotlight/Finder/the Dock instead, a shell profile
  isn't read, so use `launchctl setenv REQNROLLVS_DEBUG 1` in Terminal instead
  (lasts for the current login session) and then restart VS Code.
- **Linux** — add `export REQNROLLVS_DEBUG=1` to your shell profile and
  restart VS Code.
```

```{tab-item} Rider
:sync: rider

Log files are written to a per-OS Reqnroll `logs` directory — Windows
`%LOCALAPPDATA%\Reqnroll\logs\`, macOS `~/Library/Logs/Reqnroll/logs/`, Linux
`~/.local/share/Reqnroll/logs/`:

- `reqnroll-rider-ext-<yyyyMMdd>-<pid>.log` — the plugin's own client-side
  glue log (lifecycle/diagnostics, not LSP wire traffic).
- `reqnroll-rider-server-<yyyyMMdd>-<pid>.log` /
  `reqnroll-rider-protocol-<yyyyMMdd>-<pid>.log` — the LSP server's application
  log and protocol/wire-level internals.
  The prefix is decided before the client connects, so a server started without
  an `--ide` value keeps the generic `lsp` prefix for that whole session even if
  the client then identifies itself in its `initialize` request.

These are not written to Rider's own `idea.log` or a dedicated tool window.

**Changing the log level:** the `REQNROLLVS_DEBUG` environment variable (see
the Visual Studio tab for accepted values) also controls the plugin's own `ext`
log. Unset, the `ext` log writes every level to the file (only the "Reqnroll"
console tool window is filtered). Set it to `Info`, `Warning` or `Error` to
raise the file's threshold, or `Off` to stop writing it; `1`, `true` or
`Verbose` keep every level. Unrecognized values are ignored. Restart Rider
after changing it.

The **LSP server's** own log level *can* be changed, though, the same way as
for the other two IDEs: the `REQNROLLVS_DEBUG` environment variable (see the
Visual Studio tab for accepted values) works here too. Rider starts the
server as a child process that inherits Rider's own environment, and the
server reads `REQNROLLVS_DEBUG` directly regardless of which IDE launched it
— so setting it in the environment Rider itself runs in raises
`reqnroll-lsp-server-*.log`/`reqnroll-lsp-protocol-*.log` to `Verbose`:

- **Windows** — `setx REQNROLLVS_DEBUG 1` in a new Command Prompt, then
  restart Rider.
- **macOS** — add `export REQNROLLVS_DEBUG=1` to your shell profile
  (`~/.zshrc`) if you launch Rider from a terminal; if you launch it from
  Spotlight/Finder/the Dock instead, use `launchctl setenv REQNROLLVS_DEBUG 1`
  in Terminal instead (lasts for the current login session) and then restart
  Rider.
- **Linux** — add `export REQNROLLVS_DEBUG=1` to your shell profile and
  restart Rider.
```

:::

```{admonition} No unified, user-facing log-level setting yet
:class: note

Log-level configuration is inconsistent across the three IDEs today — VS
Code has a real, in-product setting for it; Visual Studio and Rider only
have the `REQNROLLVS_DEBUG` environment-variable escape hatch, which raises
the LSP server's own logs under any of the three IDEs (plus the extension-side
logs, on Visual Studio) but isn't documented or discoverable anywhere in
either IDE's own UI. This gap is already tracked in
[issue #291](https://github.com/reqnroll/Reqnroll.IdeSupport/issues/291),
which covers giving all three IDEs a real, shared, in-product way to change
the LSP server's log level. If you need verbose logs for a bug report and
your IDE doesn't currently support raising the level from its own settings,
say so on that issue (or on your bug report) — it's useful signal for
prioritizing it.
```

## Where does telemetry data go?

The extension sends anonymous usage and diagnostic telemetry to help the
maintainers locate bugs and performance problems. It consists of event names,
counts, flags, durations and coarse duration buckets, plus the IDE kind and
extension/server versions. It never includes file paths, step text, test
names or source content. Exception messages are scrubbed of paths, URLs and quoted text, and
the stack trace sent with an error keeps only frames from Reqnroll IDE Support itself: any other
type, including your own, is replaced by `[external]`. The exception's own type name is sent as it is.
The telemetry includes a sampled `PerfSample` event (operation label, duration and duration
bucket) sent for about 5% of operations by default. The full list of events
and properties is in the
[telemetry events inventory](https://github.com/reqnroll/Reqnroll.IdeSupport/blob/main/docs/Telemetry-Events-Inventory.md).

To opt out, set the environment variable `REQNROLL_TELEMETRY_ENABLED=0` for
the IDE process; this works in Visual Studio, Rider and VS Code. VS Code also
honors its own `telemetry.telemetryLevel` setting. Visual Studio and Rider do
not currently consult their own IDE-level telemetry settings.

`REQNROLL_PERF_TELEMETRY_SAMPLE` overrides only the `PerfSample` rate (a
fraction from 0 to 1; `0` or `off` disables just that event). To see exactly
what would be sent, set `REQNROLL_TELEMETRY_DEBUG_LOG` to mirror every event
to a local file.
