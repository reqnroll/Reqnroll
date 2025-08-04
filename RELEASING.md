# Releasing

This document describes how to make a release using GitHub Actions.  

There are two parts to making a release:

* [Prepare the release](#prepare-the-release)
* [Make the release](#make-the-release)

If you're making a major or minor release it is recommended to discuss it with the other contributors upfront (e.g. on Discord).

## Prepare the release

Anyone with permission to push to the `main` branch can prepare a release.

1. Add new information to `CHANGELOG.md`. Ideally the `CHANGELOG.md` should be up-to-date, but sometimes there will be accidental omissions when merging PRs.
    * Use `git log --format=format:"* %s (%an)" --reverse <last-version-tag>..HEAD` to list all commits since the last release.
    * Add changelog details under the `# [vNext]` heading; the release process will update this heading when it makes the release
1. Check & update contributors list (if applicable)
    * List recent contributors:
      ```
      git log --format=format:"%an <%ae>" --reverse <last-version-tag>..HEAD  | grep -vEi "(renovate|dependabot|Snyk)" | sort| uniq -i
      ```
    * Update contributors if necessary at the `Contributors of this release` part of the `# [vNext]` heading
1. The release process by default assumes the new releases to be patch releases, and whenever we merge a change that would require a minor or even major version number based on  [semver](https://semver.org/), we update the version at that time accordingly. When preparing the release you just need to double-check if the version is correct. You can find the current version number in the `Directory.Build.props` file.

## Make the release

Only people in group [release-managers](https://github.com/orgs/reqnroll/teams/release-managers) can make releases and only from the `main` branch.

### Making a preview release

Preview releases (aka pre-releases) are releases with a version number containing a version suffix, i.e. `1.2.3-pre1234`. 

Every CI build produces a set of pre-release packages with version numbers as `1.2.3-ciYYYYMMDD-BBB`, where `YYYYMMDD` refers to the date and `BBB` is the build number. We never publish these CI build packages but they can be used for exploratory testing.

We generally try to avoid publishing pre-releases, but if necessary, they shoud follow the version schema as `1.2.3-preYYYYMMDD-BBB`.

To release such a preview release, the following steps has to be done:

1. Open the CI workflow at GitHub: https://github.com/reqnroll/Reqnroll/actions/workflows/ci.yml
1. Choose the "Run workflow" button to trigger the release with the following settings:
   * `deploy_packages`: checked
   * `is_production_release`: not checked
   * `custom_version_suffix`: set it to `preYYYYMMDD`, where `YYYYMMDD` refers to the current date, e.g. `pre20240515`
   * `custom_configuration`: leave it on default
1. The CI workflow runs and ideally passes all core and testing jobs, but will stop for approval before running the `release` job.
1. Make sure everything is OK. You can even download the packages to be published for a smoke test if necessary. 
1. If everything is fine, approve the deployment job.
1. The job will publish the packages (but do not change the version or the changelog as this is only a preview release)

### Making a production release

Production releases (or just releases) are intended to use for any users. Their version number does not contain a version suffix.

To release such a preview release, the following steps has to be done:

1. Open the CI workflow at GitHub: https://github.com/reqnroll/Reqnroll/actions/workflows/ci.yml
1. Choose the "Run workflow" button to trigger the release with the following settings:
   * `deploy_packages`: checked
   * `is_production_release`: checked
   * `custom_version_suffix`: leave it empty
   * `custom_configuration`: leave it on default
1. The CI workflow runs and ideally passes all core and testing jobs, but will stop for approval before running the `release` job.
1. Make sure everything is OK. You can even download the packages to be published for a smoke test if necessary. 
1. If everything is fine, approve the deployment job.
1. The job will publish the packages, tag the current commit and create a new commit with the updated version number and changelog header.
