# AzureUserFlow Templates

<img src="icon.png" alt="icon" height="64" width="64">

Project templates for creating:
* *User Flow Extensions* for Entra External ID
* *API Connectors* for Azure AD B2C

Templates create Azure function apps (isolated runtime).

## Install

**Latest stable**

```bash
dotnet new install AxelKjonsberg.AzureUserFlow.Templates
```

**Specific version (incl. pre-release)**

```bash
dotnet new install AxelKjonsberg.AzureUserFlow.Templates::<version (e.g.1.0.0)>
```

## Create a project (and verify it builds)

```bash
dotnet new b2c-apiconnector -n MyApiConnector -o .
dotnet restore
dotnet build -c Release
```

## Parameters

| Name                         | Default                                                                 | Purpose                                  |
| ---------------------------- |-------------------------------------------------------------------------| ---------------------------------------- |
| `--UserFlowHelpersPackageId` | `AxelKjonsberg.AzureUserFlow.Helpers`                                   | NuGet package ID for the helpers library |
| `--UserFlowHelpersVersion`   | Pinned in template (normally the latest version of the Helpers package) | Helpers package version                  |

**Override example**

```bash
dotnet new b2c-apiconnector -n MyApiConnector \
  --UserFlowHelpersVersion 0.1.0-beta.2
```

## Update or Uninstall

```bash
dotnet new update
dotnet new uninstall AxelKjonsberg.AzureUserFlow.Templates
```

## License

MIT
