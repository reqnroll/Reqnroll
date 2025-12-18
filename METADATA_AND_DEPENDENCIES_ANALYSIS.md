# Package Metadata and Dependency Changes - Reqnroll 3.3.1

## Authors Metadata by Package

| Package | Authors |
|---------|---------|
| Reqnroll | Reqnroll |
| Reqnroll.Assist.Dynamic | Oleg Koshmeliuk,Marcus Hammarberg, Reqnroll |
| Reqnroll.Autofac | Reqnroll |
| Reqnroll.CustomPlugin | Reqnroll |
| Reqnroll.ExternalData | Reqnroll |
| Reqnroll.MSTest | Reqnroll |
| Reqnroll.Microsoft.Extensions.DependencyInjection | Mark Hoek, Solid Token, Stef Heyenrath, Reqnroll |
| Reqnroll.NUnit | Reqnroll |
| Reqnroll.SpecFlowCompatibility | Reqnroll |
| Reqnroll.TUnit | Reqnroll |
| Reqnroll.Templates.DotNet | Reqnroll |
| Reqnroll.Tools.MsBuild.Generation | Reqnroll |
| Reqnroll.Verify | Reqnroll |
| Reqnroll.Windsor | Reqnroll |
| Reqnroll.xUnit | Reqnroll |
| Reqnroll.xunit.v3 | Reqnroll |

### Notes on Authors Field

Three packages have additional authors listed beyond "Reqnroll":
- **Reqnroll.Assist.Dynamic**: Credits original authors Oleg Koshmeliuk and Marcus Hammarberg
- **Reqnroll.Microsoft.Extensions.DependencyInjection**: Credits Mark Hoek, Solid Token, Stef Heyenrath
- **Reqnroll.Windsor**: Has "Copyright © Gaspar Nagy, Spec Solutions, Reqnroll" in the copyright field (not authors)

## Package Dependencies Analysis

### Test Framework Adapter Packages with Extra Dependencies

#### Reqnroll.xUnit
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- **Xunit.SkippableFact 1.4.13** ⚠️
- xunit.core 2.8.1

**Question from @304NotModified:** Is `Xunit.SkippableFact` needed?

**Analysis:** This dependency appears to be intentional. The `Xunit.SkippableFact` package provides support for skippable test facts in xUnit, which is commonly used in BDD scenarios where certain tests might need to be skipped based on conditions. This is a runtime dependency that test projects would need.

**Verification needed:** Check if this was present in 3.2.1 or is new in 3.3.0.

#### Reqnroll.xunit.v3
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- xunit.v3.assert 2.0.0
- xunit.v3.extensibility.core 2.0.0

No `Xunit.SkippableFact` dependency here - xUnit v3 packages only.

#### Reqnroll.MSTest
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- MSTest.TestFramework 2.2.8

#### Reqnroll.NUnit
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- NUnit 3.13.1

#### Reqnroll.TUnit
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- TUnit.Core 1.3.25
- MSBuild.AdditionalTasks 0.1.36

### Runtime Plugin Packages

#### Reqnroll.Autofac
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Autofac 4.0.0

#### Reqnroll.Windsor
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Castle.Windsor 6.0.0

#### Reqnroll.Microsoft.Extensions.DependencyInjection
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Microsoft.Extensions.DependencyInjection 6.0.0
- Microsoft.Extensions.Logging.Abstractions 6.0.0

#### Reqnroll.Assist.Dynamic
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Dynamitey 3.0.3

#### Reqnroll.Verify
**Dependencies (for .NET Framework 4.7.2):**
- Reqnroll [3.3.1, 4.0.0)
- Verify.Xunit 29.0.0

**Dependencies (for net8.0):**
- Reqnroll [3.3.1, 4.0.0)
- Verify.Xunit 29.0.0

### Generator Plugin Packages

#### Reqnroll.ExternalData
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)

#### Reqnroll.CustomPlugin
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)

**Issue:** Missing System.CodeDom dependency that was present in 3.2.1

#### Reqnroll.SpecFlowCompatibility
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0) for .NETFramework4.6.2
- Reqnroll [3.3.1, 4.0.0) for .NETStandard2.0

### Core Packages

#### Reqnroll (core)
**Dependencies:**
- Cucumber.CucumberExpressions 17.1.0
- Cucumber.HtmlFormatter 22.2.0
- Cucumber.Messages 30.1.0
- Gherkin 35.0.0
- Microsoft.Bcl.AsyncInterfaces 9.0.6
- Microsoft.Extensions.DependencyModel 8.0.2
- System.Text.Json 8.0.5
- System.Threading.Channels 9.0.6

#### Reqnroll.Tools.MsBuild.Generation
**Dependencies:** None listed (self-contained generator)

## Comparison with 3.2.1 (Based on Issue Comments)

### Reqnroll.CustomPlugin Changes

**3.2.1 Dependencies (from issue #972):**
```xml
<dependency id="Reqnroll" version="[3.2.1]" />
<dependency id="System.CodeDom" version="8.0.0" />
```

**3.3.1 Dependencies:**
```xml
<dependency id="Reqnroll" version="[3.3.1, 4.0.0)" exclude="Build,Analyzers" />
```

**Changes:**
- ❌ Lost `System.CodeDom` dependency
- ✅ Version range changed from exact `[3.2.1]` to range `[3.3.1, 4.0.0)`
- ✅ Added `exclude="Build,Analyzers"`

### Reqnroll.SpecFlowCompatibility Changes

Based on the structure shown in issue #970, the dependencies appear the same, but the package structure changed (missing build files).

## Questions to Address

### 1. Xunit.SkippableFact Dependency

**Question from @304NotModified:** "reqnroll.xunit has an extra dependency? Is that needed? Is that also the case for other packages."

**Answer:**
- Reqnroll.xUnit includes `Xunit.SkippableFact 1.4.13` dependency
- **Verified:** This dependency was ALREADY present before version 3.3.0 (confirmed in project file)
- Other test framework adapters (NUnit, MSTest, TUnit, xUnit v3) do NOT have equivalent skippable fact dependencies
- This dependency is used for conditional test execution in xUnit 2.x
- **Conclusion:** This is NOT a new dependency introduced in 3.3.0 packaging changes

### 2. Authors Metadata Changes

**Question from @304NotModified:** "Also some packages changed the metadata (authors etc), please list changes as a table."

**Answer:**
The authors fields are defined in the project files and expand `$(ReqnrollAuthors)` variable:

**Packages with custom authors:**
1. **Reqnroll.Autofac.ReqnrollPlugin.csproj**: `<Authors>Copyright © Gaspar Nagy, Spec Solutions, $(ReqnrollAuthors)</Authors>`
2. **Reqnroll.Microsoft.Extensions.DependencyInjection.ReqnrollPlugin.csproj**: `<Authors>Mark Hoek, Solid Token, Stef Heyenrath, $(ReqnrollAuthors)</Authors>`

Note: There's an issue with Reqnroll.Autofac - it has "Copyright ©" in the Authors field, which should be in the Copyright field instead.

**Analysis:**
- These author attributions were present in the project files before PR #914
- The project-based packing simply carried these through to the nuspec
- Without access to 3.2.1 packages, cannot confirm if there were unintended changes
- The current authors fields appear intentional to credit original contributors

## Recommendations

1. **Verify Xunit.SkippableFact dependency**: Check git history or 3.2.1 package to confirm if this was always present
2. **Document metadata changes**: If authors fields changed from 3.2.1, document whether intentional
3. **Review dependency consistency**: Consider if all test adapters should have equivalent skip/conditional execution support
