# Compatibility

## Supported operating systems

SpecSync is supported on all common operating systems that support .NET, including

* Windows
* Linux 
* MacOS

## .NET Versions

- .NET Framework 4.6.2
- .NET Framework 4.7.2
- .NET Framework 4.8.1
- .NET 8.0
- .NET 9.0

```{note}
Reqnroll can also be installed on any .NET frameworks that supports .NET Standard 2.0, including end-of-life (EOL) frameworks, but please note that Reqnroll does not support EOL frameworks and frameworks before .NET 8 are out of support by Microsoft.
```

## Visual Studio

- Visual Studio 2022 (Workloads: ASP.NET and web development **or** .NET desktop development)

## Test Execution Frameworks

- NUnit
- MsTest
- TUnit
- xUnit, xUnit v3

```{warning}
MsTest v4 preview is not yet supported.
```
  
## Versioning Policy

The core Reqnroll framework uses [semantic versioning](https://semver.org/), that means that only major version changes will introduce breaking changes. Minor version number increase represent new features or backwards compatible improvements and patch number increase represents bug fixes. This means that generally you should be able to upgrade to the latest package within the major number range without problems. 

The backwards compatibility applies for the specified behavior and the API used by the end-users, but also for interfaces commonly used by plugins. This means that if a plugin for example has been created for v2.0.0 generally suppose to work with v2.3.1 as well.

```{note}
The versioning policy is slightly different for the maintained Reqnroll integration plugins (e.g. `Reqnroll.Autofac`). See the detailed policy for these in the section below.
```

### Versioning policy of Reqnroll plugins

We maintain a couple of external integration plugins (e.g. `Reqnroll.Autofac`, or `Reqnroll.Verify`) that typically work as an adapter for some other tools or libraries. These plugins are generally versioned together with the Reqnroll core package, to make the versioning and the release process simpler. This means that we publish a new version of these plugins with each new Reqnroll version even if the plugin itself did not change.

The Reqnroll plugins also follow [semantic versioning](https://semver.org/), with the exception of the version changes of the integrated product. To understand the implication of this, please follow the description below.

In some cases there is a breaking change in the tool or library the plugin integrates with. E.g. `Reqnroll.Verify` v2.0.3 supports `Verify` v23, but there is a breaking change between v23 and v24 of the `Verify` package. In order to support v24, we update the plugin and include the change in a new *minor* release of Reqnroll (as now we *added* support for v24). So the new plugin will be included in Reqnroll v2.1.0.

If you upgrade to the latest minor release, you might observe a compatibility issue unless you also upgrade the integrated package (`Verify` in this case). You can keep using the older version of the package by using the latest Reqnroll version (v2.1.0), but use the previous version (v2.0.3) of the `Reqnroll.Verify` plugin. As plugins within the same main version are cross-compatible, the v2.0.3 version of the plugin will work with Reqnroll v2.1.0.
