# Local Setup

## Clone the code

Clone the repository with submodules

```
git clone --recurse-submodules https://github.com/reqnroll/Reqnroll.git
```

You need to clone the repository with submodules, because the code for the Reqnroll.TestProjectGenerator is located in [another repository](https://github.com/reqnroll/Reqnroll.TestProjectGenerator). This is due to the fact that this code is shared with other projects.

## Setting environment variables

### MSBUILDDISABLENODEREUSE

You have to set MSBUILDDISABLENODEREUSE to 1.
Reason for this is, that Reqnroll has an MSBuild Task that is used in the Reqnroll.Specs project. Because of the using of the task and MSBuild reuses processes, the file is loaded by MSBuild and will then lock the file and break the next build.

This environment variable controls the behaviour if MSBuild reuses processes. Setting to 1 disables this behaviour.

See [here](https://github.com/Microsoft/msbuild/wiki/MSBuild-Tips-&-Tricks) for more info.
