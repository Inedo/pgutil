# pgutil

We're still working on finishing up the documentation for ProGet 2024 and this article is on our TODO list. It's a pretty rough outline/draft, but we figured it'd be better than nothing. You can also visit our formal documentation at https://docs.inedo.com/docs/proget-pgutil

`pgutil` is an open-source, cross-platform command line tool that's easy to download and install.

## Commands & Usage

:::(Info) (TODO)
- easiest way to discover available commands are just to run `pgutil` to see a list of "command sets"
  -  typing in `pgutil packages` will show you all the commands under that set
  - to see all options available for a command, run `pgutil packages download --help`
- api documentation is organized around these command sets
- we anticipate adding commands and arguments
  - unlikely to remove them, unless it's in a major version with plenty of notice
  - this is why best to look in your version of pgutil for latest
:::

```
$> pgutil

    .--. --. ..- - .. .-..
        pgutil v1.0.0
    .--. --. ..- - .. .-..

Description:
  Perform operations against a ProGet server.

Usage:
  pgutil [command] [options]

Options:
  -?, --help  Show help and usage information

Commands:
  sources   Manages the list of sources located in the configuration file located at
            C:\Users\sdennis\AppData\Roaming\pgutil\pgutil.config
  packages  Work with packages on a ProGet server
  vulns     Audit packages and assess vulnerabilities
  builds    Manage SCA builds and SBOM documents
  licenses  Manage license definitions and audit package licenses
```
