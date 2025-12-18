# Package Metadata Comparison Table - Reqnroll 3.3.1

This table shows the authors and other metadata fields for all Reqnroll packages in version 3.3.1.

## Complete Metadata Table

| Package | Authors | Copyright | Title/Description Notes |
|---------|---------|-----------|------------------------|
| Reqnroll | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.Assist.Dynamic | Oleg Koshmeliuk,Marcus Hammarberg, Reqnroll | Copyright © Oleg Koshmeliuk, Marcus Hammarberg, Reqnroll | - |
| Reqnroll.Autofac | Reqnroll | Copyright © Gaspar Nagy, Spec Solutions, Reqnroll | ⚠️ Authors field in .csproj has "Copyright ©" prefix |
| Reqnroll.CustomPlugin | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.ExternalData | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.MSTest | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.Microsoft.Extensions.DependencyInjection | Mark Hoek, Solid Token, Stef Heyenrath, Reqnroll | Copyright © Mark Hoek, Solid Token, Stef Heyenrath, Reqnroll | Has "Title" field set |
| Reqnroll.NUnit | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.SpecFlowCompatibility | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.TUnit | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.Templates.DotNet | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.Tools.MsBuild.Generation | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.Verify | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.Windsor | Reqnroll | Copyright © Gaspar Nagy, Spec Solutions, Reqnroll | Has "Title" field set |
| Reqnroll.xUnit | Reqnroll | Copyright © Reqnroll | - |
| Reqnroll.xunit.v3 | Reqnroll | Copyright © Reqnroll | - |

## Packages with Custom Attribution

Only 3 packages credit original or additional authors:

### 1. Reqnroll.Assist.Dynamic
- **Authors:** Oleg Koshmeliuk,Marcus Hammarberg, Reqnroll
- **Reason:** Credits original authors of this dynamic functionality
- **Source:** Set in project file

### 2. Reqnroll.Microsoft.Extensions.DependencyInjection
- **Authors:** Mark Hoek, Solid Token, Stef Heyenrath, Reqnroll
- **Reason:** Credits original authors of the DI integration
- **Source:** Set in project file: `<Authors>Mark Hoek, Solid Token, Stef Heyenrath, $(ReqnrollAuthors)</Authors>`

### 3. Reqnroll.Autofac
- **Authors:** Reqnroll (but Copyright field credits Gaspar Nagy)
- **Copyright:** Copyright © Gaspar Nagy, Spec Solutions, Reqnroll
- **Issue:** ⚠️ The .csproj file has `<Authors>Copyright © Gaspar Nagy, Spec Solutions, $(ReqnrollAuthors)</Authors>` which incorrectly puts "Copyright ©" in the Authors field
- **Note:** The nuspec shows just "Reqnroll" as authors, so the "Copyright ©" prefix may have been stripped during packaging

### 4. Reqnroll.Windsor
- **Authors:** Reqnroll
- **Copyright:** Copyright © Gaspar Nagy, Spec Solutions, Reqnroll
- **Reason:** Credits Gaspar Nagy and Spec Solutions in copyright

## Source of Metadata

All metadata comes from project files (`.csproj`):
- Most packages inherit from `Directory.Build.props` which sets `ReqnrollAuthors` to "Reqnroll"
- Packages can override with custom `<Authors>` tag in their project file
- The project-based packing (PR #914) uses these project file values directly

## Potential Issues Found

### Issue: Reqnroll.Autofac Authors Field
The Reqnroll.Autofac project file has:
```xml
<Authors>Copyright © Gaspar Nagy, Spec Solutions, $(ReqnrollAuthors)</Authors>
```

This should probably be:
```xml
<Authors>Gaspar Nagy, Spec Solutions, $(ReqnrollAuthors)</Authors>
<Copyright>Copyright © Gaspar Nagy, Spec Solutions, $(ReqnrollAuthors)</Copyright>
```

However, the resulting nuspec shows just "Reqnroll" as authors, so the packaging process may have handled this correctly.

## Dependencies with Extra Packages

### Reqnroll.xUnit - Xunit.SkippableFact

**Status:** ✅ This was already present before 3.3.0

The Reqnroll.xUnit package includes `Xunit.SkippableFact 1.4.13` as a dependency. This is:
- Used for conditional test execution in xUnit 2.x
- Already present in the codebase before PR #914
- Intentional and necessary for the xUnit integration
- Not present in other test framework adapters (they don't need it)

**Comparison:**
- ✅ **Reqnroll.xUnit (v2):** Includes Xunit.SkippableFact
- ❌ **Reqnroll.xunit.v3:** Does NOT include Xunit.SkippableFact (uses xUnit v3 APIs)
- ❌ **Reqnroll.NUnit:** No skippable fact equivalent
- ❌ **Reqnroll.MSTest:** No skippable fact equivalent  
- ❌ **Reqnroll.TUnit:** No skippable fact equivalent

## Summary for @304NotModified

### Question 1: "reqnroll.xunit has an extra dependency? Is that needed?"
**Answer:** Yes, `Xunit.SkippableFact` is needed and was already present before 3.3.0. It's specific to xUnit 2.x functionality.

### Question 2: "Is that also the case for other packages?"
**Answer:** No, only Reqnroll.xUnit (v2) has this dependency. Other test framework adapters don't need equivalent packages.

### Question 3: "Also some packages changed the metadata (authors etc), please list changes as a table."
**Answer:** See tables above. Key points:
- Most packages use "Reqnroll" as authors (inherited from `Directory.Build.props`)
- 3 packages credit original contributors (Assist.Dynamic, Microsoft.Extensions.DI, Windsor/Autofac in copyright)
- These attributions were in the project files before PR #914
- No evidence of unintended changes from project-based packing (without access to 3.2.1 packages for comparison)
- One potential issue: Autofac project file has "Copyright ©" in Authors field (may need cleanup)
