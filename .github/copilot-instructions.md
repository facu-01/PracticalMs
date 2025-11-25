# GitHub Copilot Instructions

## Project Overview
This is a .NET 9 Web API project using Wolverine for HTTP endpoints and message handling.

## Architecture & Patterns
- **Framework**: ASP.NET Core 9.0 with Wolverine.Http
- **Endpoints**: Wolverine HTTP endpoints with conventional routing


## Code Style & Conventions
- Use wolverine endpoints
- Prefer static methods for HTTP endpoints
- Keep Program.cs clean and concise
- Use conventional commits for commit messages and in Spanish

## Important Notes
- Do NOT use `opts.Policies.AddMiddleware<T>()` for HTTP endpoints (that's for message handlers only)

## Debugging
- Use VS Code with C# extension
- Debug configuration targets `Api/bin/Debug/net9.0/Api.dll`


## WOLVERINE DOC
(Files content cropped to 300k characters, download full ingest to see more)
================================================
FILE: README.md
================================================
Wolverine
======

[![Discord](https://img.shields.io/discord/1074998995086225460?color=blue&label=Chat%20on%20Discord)](https://discord.gg/WMxrvegf8H)

Wolverine is a *Next Generation .NET Mediator and Message Bus*. Check out
the [documentation website at https://wolverinefx.net/](https://wolverinefx.net/).

## Support Plans

<div align="center">
    <img src="https://www.jasperfx.net/logo.png" alt="JasperFx logo" width="70%">
</div>

While Wolverine is open source, [JasperFx Software offers paid support and consulting contracts](https://jasperfx.net)
for Wolverine.

## Help us keep working on this project 💚

[Become a Sponsor on GitHub](https://github.com/sponsors/JasperFX)

## Working with the Code

To work with the code, just open the `wolverine.sln` file in the root of the repository and go. If you want to run
integration tests though, you'll want Docker installed locally
and to start the matching testing services with:

```bash
docker compose up -d
```

There's a separate README in the Azure Service Bus tests as those require an actual cloud set up (sorry, but blame
Microsoft for not having a local Docker based emulator ala Localstack).

## Contributor's Guide

For contributors, there's a light naming style Jeremy refuses to let go of that he's used for *gulp* 20+ years:

1. All public or internal members should be Pascal cased
2. All private or protected members should be Camel cased
3. Use `_` as a prefix for private fields

The build is scripted out with [Bullseye](https://github.com/adamralph/bullseye) in the `/build` folder. To run the
build file locally, use `build` with Windows or `./build.sh` on OSX or Linux.

## Documentation

All the documentation content is in the `/docs` folder. The documentation is built and published
with [Vitepress](https://vitepress.vuejs.org/) and
uses [Markdown Snippets](https://github.com/SimonCropp/MarkdownSnippets) for code samples. To run the documentation
locally, you'll need a recent version of Node.js installed. To start the documentation website, first run:

```bash
npm install
```

Then start the actual website with:

```bash
npm run docs
```

To update the code sample snippets, use:

```bash
npm run mdsnippets
```

## History

This is a little sad, but Wolverine started as a project named "[Jasper](https://github.com/jasperfx/jasper)" way, way
back in 2015 as an intended reboot of an even older project
named [FubuMVC / FubuTransportation](https://fubumvc.github.io) that
was a combination web api framework and asynchronous message bus. What is now Wolverine was meant to build upon what we
thought was the positive aspects of fubu's programming model but do so with a
much more efficient runtime. Wolverine was largely rebooted, revamped, and renamed in 2022 with the intention of being
combined with [Marten](https://martendb.io) into the "critter stack" for highly productive
and highly performant server side development in .NET.





================================================
FILE: Analysis.Build.props
================================================
<?xml version="1.0" encoding="utf-8"?>
<Project>
    <PropertyGroup>
        <AnalysisModeReliability>true</AnalysisModeReliability>
        <EnableNETAnalyzers>true</EnableNETAnalyzers>
        <AnalysisLevel>latest</AnalysisLevel>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Microsoft.VisualStudio.Threading.Analyzers" Version="17.0.64" PrivateAssets="All"/>
        <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="5.0.3" PrivateAssets="All" Condition=" '$(TargetFrawework)' == 'netstandard2.0' "/>
    </ItemGroup>
</Project>



================================================
FILE: build.cmd
================================================
:; set -eo pipefail
:; SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)
:; ${SCRIPT_DIR}/build.sh "$@"
:; exit $?

@ECHO OFF
powershell -ExecutionPolicy ByPass -NoProfile -File "%~dp0build.ps1" %*



================================================
FILE: build.ps1
================================================
[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$BuildArguments
)

Write-Output "PowerShell $($PSVersionTable.PSEdition) version $($PSVersionTable.PSVersion)"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# CONFIGURATION
###########################################################################

$BuildProjectFile = "$PSScriptRoot\build\build.csproj"
$TempDirectory = "$PSScriptRoot\\.nuke\temp"

$DotNetGlobalFile = "$PSScriptRoot\\global.json"
$DotNetInstallUrl = "https://dot.net/v1/dotnet-install.ps1"
$DotNetChannel = "STS"

$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_NOLOGO = 1

###########################################################################
# EXECUTION
###########################################################################

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

# If dotnet CLI is installed globally and it matches requested version, use for execution
if ($null -ne (Get-Command "dotnet" -ErrorAction SilentlyContinue) -and `
     $(dotnet --version) -and $LASTEXITCODE -eq 0) {
    $env:DOTNET_EXE = (Get-Command "dotnet").Path
}
else {
    # Download install script
    $DotNetInstallFile = "$TempDirectory\dotnet-install.ps1"
    New-Item -ItemType Directory -Path $TempDirectory -Force | Out-Null
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    (New-Object System.Net.WebClient).DownloadFile($DotNetInstallUrl, $DotNetInstallFile)

    # If global.json exists, load expected version
    if (Test-Path $DotNetGlobalFile) {
        $DotNetGlobal = $(Get-Content $DotNetGlobalFile | Out-String | ConvertFrom-Json)
        if ($DotNetGlobal.PSObject.Properties["sdk"] -and $DotNetGlobal.sdk.PSObject.Properties["version"]) {
            $DotNetVersion = $DotNetGlobal.sdk.version
        }
    }

    # Install by channel or version
    $DotNetDirectory = "$TempDirectory\dotnet-win"
    if (!(Test-Path variable:DotNetVersion)) {
        ExecSafe { & powershell $DotNetInstallFile -InstallDir $DotNetDirectory -Channel $DotNetChannel -NoPath }
    } else {
        ExecSafe { & powershell $DotNetInstallFile -InstallDir $DotNetDirectory -Version $DotNetVersion -NoPath }
    }
    $env:DOTNET_EXE = "$DotNetDirectory\dotnet.exe"
    $env:PATH = "$DotNetDirectory;$env:PATH"
}

Write-Output "Microsoft (R) .NET SDK version $(& $env:DOTNET_EXE --version)"

if (Test-Path env:NUKE_ENTERPRISE_TOKEN) {
    & $env:DOTNET_EXE nuget remove source "nuke-enterprise" > $null
    & $env:DOTNET_EXE nuget add source "https://f.feedz.io/nuke/enterprise/nuget" --name "nuke-enterprise" --username "PAT" --password $env:NUKE_ENTERPRISE_TOKEN > $null
}

ExecSafe { & $env:DOTNET_EXE build $BuildProjectFile /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet }
ExecSafe { & $env:DOTNET_EXE run --project $BuildProjectFile --no-build -- $BuildArguments }



================================================
FILE: build.sh
================================================
#!/usr/bin/env bash

bash --version 2>&1 | head -n 1

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_PROJECT_FILE="$SCRIPT_DIR/build/build.csproj"
TEMP_DIRECTORY="$SCRIPT_DIR//.nuke/temp"

DOTNET_GLOBAL_FILE="$SCRIPT_DIR//global.json"
DOTNET_INSTALL_URL="https://dot.net/v1/dotnet-install.sh"
DOTNET_CHANNEL="STS"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

###########################################################################
# EXECUTION
###########################################################################

function FirstJsonValue {
    perl -nle 'print $1 if m{"'"$1"'": "([^"]+)",?}' <<< "${@:2}"
}

# If dotnet CLI is installed globally and it matches requested version, use for execution
if [ -x "$(command -v dotnet)" ] && dotnet --version &>/dev/null; then
    export DOTNET_EXE="$(command -v dotnet)"
else
    # Download install script
    DOTNET_INSTALL_FILE="$TEMP_DIRECTORY/dotnet-install.sh"
    mkdir -p "$TEMP_DIRECTORY"
    curl -Lsfo "$DOTNET_INSTALL_FILE" "$DOTNET_INSTALL_URL"
    chmod +x "$DOTNET_INSTALL_FILE"

    # If global.json exists, load expected version
    if [[ -f "$DOTNET_GLOBAL_FILE" ]]; then
        DOTNET_VERSION=$(FirstJsonValue "version" "$(cat "$DOTNET_GLOBAL_FILE")")
        if [[ "$DOTNET_VERSION" == ""  ]]; then
            unset DOTNET_VERSION
        fi
    fi

    # Install by channel or version
    DOTNET_DIRECTORY="$TEMP_DIRECTORY/dotnet-unix"
    if [[ -z ${DOTNET_VERSION+x} ]]; then
        "$DOTNET_INSTALL_FILE" --install-dir "$DOTNET_DIRECTORY" --channel "$DOTNET_CHANNEL" --no-path
    else
        "$DOTNET_INSTALL_FILE" --install-dir "$DOTNET_DIRECTORY" --version "$DOTNET_VERSION" --no-path
    fi
    export DOTNET_EXE="$DOTNET_DIRECTORY/dotnet"
    export PATH="$DOTNET_DIRECTORY:$PATH"
fi

echo "Microsoft (R) .NET SDK version $("$DOTNET_EXE" --version)"

if [[ ! -z ${NUKE_ENTERPRISE_TOKEN+x} && "$NUKE_ENTERPRISE_TOKEN" != "" ]]; then
    "$DOTNET_EXE" nuget remove source "nuke-enterprise" &>/dev/null || true
    "$DOTNET_EXE" nuget add source "https://f.feedz.io/nuke/enterprise/nuget" --name "nuke-enterprise" --username "PAT" --password "$NUKE_ENTERPRISE_TOKEN" --store-password-in-clear-text &>/dev/null || true
fi

"$DOTNET_EXE" build "$BUILD_PROJECT_FILE" /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
"$DOTNET_EXE" run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"



================================================
FILE: Directory.Build.props
================================================
<?xml version="1.0" encoding="utf-8"?>
<Project>
    <PropertyGroup>
        <LangVersion>12</LangVersion>
        <NoWarn>1570;1571;1572;1573;1574;1587;1591;1701;1702;1711;1735;0618</NoWarn>
        <Authors>Jeremy D. Miller;Babu Annamalai;Jaedyn Tonee;</Authors>
        <PackageIconUrl>https://github.com/JasperFx/wolverine/blob/main/docs/public/logo.png?raw=true</PackageIconUrl>
        <PackageProjectUrl>http://github.com/jasperfx/wolverine</PackageProjectUrl>
        <PackageLicenseExpression>MIT</PackageLicenseExpression>
        <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
        <NoWarn>1570;1571;1572;1573;1574;1587;1591;1701;1702;1711;1735;0618</NoWarn>
        <ImplicitUsings>true</ImplicitUsings>
        <Nullable>enable</Nullable>
        <Version>5.4.0</Version>
        <RepositoryUrl>$(PackageProjectUrl)</RepositoryUrl>
        <PublishRepositoryUrl>true</PublishRepositoryUrl>
        <EmbedUntrackedSources>true</EmbedUntrackedSources>
        <DebugType>embedded</DebugType>
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
        <AddSyntheticProjectReferencesForSolutionDependencies>false</AddSyntheticProjectReferencesForSolutionDependencies>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.SourceLink.GitHub" Version="1.1.1" PrivateAssets="All"/>
    </ItemGroup>

</Project>



================================================
FILE: docker-compose.yml
================================================
version: '3'

networks:
  app-tier:
    driver: bridge

services:
  postgresql:
    image: "postgres:latest"
    ports:
      - "5433:5432"
    environment:
      - POSTGRES_DATABASE=postgres
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres

  gcp-pubsub:
    image: gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators
    ports:
      - "8085:8085"
    command:
      [
        "gcloud",
        "--quiet",
        "beta",
        "emulators",
        "pubsub",
        "start",
        "--host-port",
        "0.0.0.0:8085",
        "--project",
        "wolverine",
        "--verbosity",
        "debug",
        "--log-http",
        "--user-output-enabled",
      ]
  
  rabbitmq:
    image: "rabbitmq:management"
    ports:
      - "5672:5672"
      - "15672:15672"

  sqlserver:
    image: "mcr.microsoft.com/azure-sql-edge"
    ports:
      - "1434:1433"
    environment:
      - "ACCEPT_EULA=Y"
      - "SA_PASSWORD=P@55w0rd"
      - "MSSQL_PID=Developer"

  pulsar:
    image: "apachepulsar/pulsar:latest"
    ports:
      - "6650:6650"
      - "8080:8080"
    command: bin/pulsar standalone

  localstack:
    image: localstack/localstack:stable
    ports:
      - "127.0.0.1:4566:4566"            # LocalStack Gateway
      - "127.0.0.1:4510-4559:4510-4559"  # external services port range
    environment:
      - DEBUG=${DEBUG-}
      - PERSISTENCE=${PERSISTENCE-}
      - LAMBDA_EXECUTOR=${LAMBDA_EXECUTOR-}
      - LOCALSTACK_API_KEY=${LOCALSTACK_API_KEY-}  # only required for Pro
      - DOCKER_HOST=unix:///var/run/docker.sock
    volumes:
      - "${LOCALSTACK_VOLUME_DIR:-./volume}:/var/lib/localstack"
      - "/var/run/docker.sock:/var/run/docker.sock"

  zookeeper:
    image: confluentinc/cp-zookeeper:latest
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
    ports:
      - 22181:2181
  
  kafka:
    image: confluentinc/confluent-local:latest
    ports:
      - "8082:8082"
      - "9092:9092"
      - "9101:9101"
        
  redis-server:
    image: "redis:alpine"
    command: redis-server
    ports:
      - "6379:6379"


================================================
FILE: global.json
================================================
{
  "sdk": {
    "version": "7.0.101",
    "rollForward": "latestMajor",
    "allowPrerelease": false
  }
}


================================================
FILE: kill-dotnet.bat
================================================
taskkill /F /IM dotnet.exe /T


================================================
FILE: LICENSE
================================================
The MIT License (MIT)

Copyright (c) Jeremy D. Miller and Contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.



================================================
FILE: mdsnippets.json
================================================
{
  "Convention": "InPlaceOverwrite",
  "LinkFormat": "GitHub",
  "UrlPrefix": "https://github.com/JasperFx/wolverine/blob/main",
  "ExcludeMarkdownDirectories": [
    "documentation"
  ],
  "ExcludeSnippetDirectories": [],
  "TreatMissingAsWarning": false
}



================================================
FILE: netlify.toml
================================================
[build]
    publish = "docs/.vitepress/dist/"

[[redirects]]
  from = "/discord"
  to = "https://discord.gg/WMxrvegf8H"
  status = 301
  force = false


================================================
FILE: package.json
================================================
{
  "private": true,
  "scripts": {
    "vitepress-dev": "vitepress dev docs --port 5050 --open",
    "vitepress-build": "vitepress build docs",
    "mdsnippets": "mdsnippets",
    "docs": "concurrently --group mdsnippets \"vitepress dev docs --port 5050 --open\"",
    "docs:build": "concurrently --group mdsnippets \"vitepress build docs\"",
    "docs:publish": "netlify deploy --prod"
  },
  "devDependencies": {
    "concurrently": "^9.1.2",
    "markdown-it-block-embed": "^0.0.3",
    "netlify-cli": "^21.5.0",
    "vitepress": "1.6.3",
    "vitepress-plugin-llms": "^1.5.1",
    "vitepress-plugin-mermaid": "^2.0.17"
  }
}



================================================
FILE: slim.sln
================================================
﻿
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine", "src\Wolverine\Wolverine.csproj", "{9ACD7ED7-9531-4873-AF52-73A853680AF0}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CoreTests", "src\Testing\CoreTests\CoreTests.csproj", "{D4C60B74-28A1-440F-B322-257820ACE224}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Module1", "src\Testing\Module1\Module1.csproj", "{930E278C-9A71-4CF4-9FF3-9409EAE2B69D}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Module2", "src\Testing\Module2\Module2.csproj", "{DBCD9866-B1D7-4496-A9F8-063215800BA2}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "OrderExtension", "src\Testing\OrderExtension\OrderExtension.csproj", "{EEC11B85-5CB3-4AD2-B383-175931423C1F}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine.ComplianceTests", "src\Testing\Wolverine.ComplianceTests\Wolverine.ComplianceTests.csproj", "{48D2E939-53C9-405D-BF56-E0470D469816}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "JasperFx", "..\jasperfx\src\JasperFx\JasperFx.csproj", "{7D6E36DD-13A7-4D72-94F9-3B63D254BB10}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "JasperFx.RuntimeCompiler", "..\jasperfx\src\JasperFx.RuntimeCompiler\JasperFx.RuntimeCompiler.csproj", "{C0CE5A87-484B-4805-80F5-965507D43BB8}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Lamar", "..\lamar\src\Lamar\Lamar.csproj", "{D4CB97BD-C308-42C9-870A-AD8ED63D39C7}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Lamar.Microsoft.DependencyInjection", "..\lamar\src\Lamar.Microsoft.DependencyInjection\Lamar.Microsoft.DependencyInjection.csproj", "{7FBD0930-6DBD-496B-9A38-B16842BFD500}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{9ACD7ED7-9531-4873-AF52-73A853680AF0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{9ACD7ED7-9531-4873-AF52-73A853680AF0}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{9ACD7ED7-9531-4873-AF52-73A853680AF0}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{9ACD7ED7-9531-4873-AF52-73A853680AF0}.Release|Any CPU.Build.0 = Release|Any CPU
		{D4C60B74-28A1-440F-B322-257820ACE224}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{D4C60B74-28A1-440F-B322-257820ACE224}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{D4C60B74-28A1-440F-B322-257820ACE224}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{D4C60B74-28A1-440F-B322-257820ACE224}.Release|Any CPU.Build.0 = Release|Any CPU
		{930E278C-9A71-4CF4-9FF3-9409EAE2B69D}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{930E278C-9A71-4CF4-9FF3-9409EAE2B69D}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{930E278C-9A71-4CF4-9FF3-9409EAE2B69D}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{930E278C-9A71-4CF4-9FF3-9409EAE2B69D}.Release|Any CPU.Build.0 = Release|Any CPU
		{DBCD9866-B1D7-4496-A9F8-063215800BA2}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{DBCD9866-B1D7-4496-A9F8-063215800BA2}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{DBCD9866-B1D7-4496-A9F8-063215800BA2}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{DBCD9866-B1D7-4496-A9F8-063215800BA2}.Release|Any CPU.Build.0 = Release|Any CPU
		{EEC11B85-5CB3-4AD2-B383-175931423C1F}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{EEC11B85-5CB3-4AD2-B383-175931423C1F}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{EEC11B85-5CB3-4AD2-B383-175931423C1F}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{EEC11B85-5CB3-4AD2-B383-175931423C1F}.Release|Any CPU.Build.0 = Release|Any CPU
		{48D2E939-53C9-405D-BF56-E0470D469816}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{48D2E939-53C9-405D-BF56-E0470D469816}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{48D2E939-53C9-405D-BF56-E0470D469816}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{48D2E939-53C9-405D-BF56-E0470D469816}.Release|Any CPU.Build.0 = Release|Any CPU
		{7D6E36DD-13A7-4D72-94F9-3B63D254BB10}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{7D6E36DD-13A7-4D72-94F9-3B63D254BB10}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{7D6E36DD-13A7-4D72-94F9-3B63D254BB10}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{7D6E36DD-13A7-4D72-94F9-3B63D254BB10}.Release|Any CPU.Build.0 = Release|Any CPU
		{C0CE5A87-484B-4805-80F5-965507D43BB8}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C0CE5A87-484B-4805-80F5-965507D43BB8}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C0CE5A87-484B-4805-80F5-965507D43BB8}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C0CE5A87-484B-4805-80F5-965507D43BB8}.Release|Any CPU.Build.0 = Release|Any CPU
		{D4CB97BD-C308-42C9-870A-AD8ED63D39C7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{D4CB97BD-C308-42C9-870A-AD8ED63D39C7}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{D4CB97BD-C308-42C9-870A-AD8ED63D39C7}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{D4CB97BD-C308-42C9-870A-AD8ED63D39C7}.Release|Any CPU.Build.0 = Release|Any CPU
		{7FBD0930-6DBD-496B-9A38-B16842BFD500}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{7FBD0930-6DBD-496B-9A38-B16842BFD500}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{7FBD0930-6DBD-496B-9A38-B16842BFD500}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{7FBD0930-6DBD-496B-9A38-B16842BFD500}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal



================================================
FILE: wolverine_slim.sln
================================================
﻿
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "JasperFx", "..\jasperfx\src\JasperFx\JasperFx.csproj", "{6F81D306-DEAF-489A-B711-A5547B914DA9}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "JasperFx.Events", "..\jasperfx\src\JasperFx.Events\JasperFx.Events.csproj", "{77AE3D7C-8B85-4771-9A30-E1EE1385FA28}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Weasel.Core", "..\weasel\src\Weasel.Core\Weasel.Core.csproj", "{AC0D087E-ACA4-486E-BB43-796727973362}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Weasel.Postgresql", "..\weasel\src\Weasel.Postgresql\Weasel.Postgresql.csproj", "{281A16DC-92DA-4781-A576-FE35E284C495}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Marten", "..\marten\src\Marten\Marten.csproj", "{66DB00EA-6D6B-4DB6-92E3-51E845786246}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "JasperFx.RuntimeCompiler", "..\jasperfx\src\JasperFx.RuntimeCompiler\JasperFx.RuntimeCompiler.csproj", "{C4BCE2FD-C646-4FF9-971C-152537B7F4EA}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine", "src\Wolverine\Wolverine.csproj", "{99518293-38F2-4277-8353-CC27921B20B3}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine.RDBMS", "src\Persistence\Wolverine.RDBMS\Wolverine.RDBMS.csproj", "{0408A2A5-4F0C-40AB-83D4-749D508AE550}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine.Postgresql", "src\Persistence\Wolverine.Postgresql\Wolverine.Postgresql.csproj", "{8DA34E4A-E4A6-43EE-9F2A-DCD582D3877F}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine.Marten", "src\Persistence\Wolverine.Marten\Wolverine.Marten.csproj", "{13B06F95-C6A7-4BA1-B55A-EB75A83772E9}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MultiTenantedTodoWebService", "src\Samples\MultiTenantedTodoService\MultiTenantedTodoService\MultiTenantedTodoWebService.csproj", "{FEFBAE14-DB90-499B-B209-30914F87FDCD}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MultiTenantedTodoWebService.Tests", "src\Samples\MultiTenantedTodoService\MultiTenantedTodoWebService.Tests\MultiTenantedTodoWebService.Tests.csproj", "{C7CB23C7-95B5-448E-9CAD-673082AD0908}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Alba", "..\alba\src\Alba\Alba.csproj", "{58F98CBF-B5F3-47F3-B05D-47D4D957D159}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine.Http", "src\Http\Wolverine.Http\Wolverine.Http.csproj", "{67942C15-FADA-4B38-A2D0-D85D8A1F90A5}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Wolverine.SqlServer", "src\Persistence\Wolverine.SqlServer\Wolverine.SqlServer.csproj", "{798A97D3-45BE-41A3-89B3-F45140278659}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{6F81D306-DEAF-489A-B711-A5547B914DA9}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{6F81D306-DEAF-489A-B711-A5547B914DA9}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{6F81D306-DEAF-489A-B711-A5547B914DA9}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{6F81D306-DEAF-489A-B711-A5547B914DA9}.Release|Any CPU.Build.0 = Release|Any CPU
		{77AE3D7C-8B85-4771-9A30-E1EE1385FA28}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{77AE3D7C-8B85-4771-9A30-E1EE1385FA28}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{77AE3D7C-8B85-4771-9A30-E1EE1385FA28}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{77AE3D7C-8B85-4771-9A30-E1EE1385FA28}.Release|Any CPU.Build.0 = Release|Any CPU
		{AC0D087E-ACA4-486E-BB43-796727973362}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{AC0D087E-ACA4-486E-BB43-796727973362}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{AC0D087E-ACA4-486E-BB43-796727973362}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{AC0D087E-ACA4-486E-BB43-796727973362}.Release|Any CPU.Build.0 = Release|Any CPU
		{281A16DC-92DA-4781-A576-FE35E284C495}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{281A16DC-92DA-4781-A576-FE35E284C495}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{281A16DC-92DA-4781-A576-FE35E284C495}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{281A16DC-92DA-4781-A576-FE35E284C495}.Release|Any CPU.Build.0 = Release|Any CPU
		{66DB00EA-6D6B-4DB6-92E3-51E845786246}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{66DB00EA-6D6B-4DB6-92E3-51E845786246}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{66DB00EA-6D6B-4DB6-92E3-51E845786246}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{66DB00EA-6D6B-4DB6-92E3-51E845786246}.Release|Any CPU.Build.0 = Release|Any CPU
		{C4BCE2FD-C646-4FF9-971C-152537B7F4EA}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C4BCE2FD-C646-4FF9-971C-152537B7F4EA}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C4BCE2FD-C646-4FF9-971C-152537B7F4EA}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C4BCE2FD-C646-4FF9-971C-152537B7F4EA}.Release|Any CPU.Build.0 = Release|Any CPU
		{99518293-38F2-4277-8353-CC27921B20B3}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{99518293-38F2-4277-8353-CC27921B20B3}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{99518293-38F2-4277-8353-CC27921B20B3}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{99518293-38F2-4277-8353-CC27921B20B3}.Release|Any CPU.Build.0 = Release|Any CPU
		{0408A2A5-4F0C-40AB-83D4-749D508AE550}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{0408A2A5-4F0C-40AB-83D4-749D508AE550}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{0408A2A5-4F0C-40AB-83D4-749D508AE550}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{0408A2A5-4F0C-40AB-83D4-749D508AE550}.Release|Any CPU.Build.0 = Release|Any CPU
		{8DA34E4A-E4A6-43EE-9F2A-DCD582D3877F}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{8DA34E4A-E4A6-43EE-9F2A-DCD582D3877F}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{8DA34E4A-E4A6-43EE-9F2A-DCD582D3877F}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{8DA34E4A-E4A6-43EE-9F2A-DCD582D3877F}.Release|Any CPU.Build.0 = Release|Any CPU
		{13B06F95-C6A7-4BA1-B55A-EB75A83772E9}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{13B06F95-C6A7-4BA1-B55A-EB75A83772E9}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{13B06F95-C6A7-4BA1-B55A-EB75A83772E9}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{13B06F95-C6A7-4BA1-B55A-EB75A83772E9}.Release|Any CPU.Build.0 = Release|Any CPU
		{FEFBAE14-DB90-499B-B209-30914F87FDCD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{FEFBAE14-DB90-499B-B209-30914F87FDCD}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{FEFBAE14-DB90-499B-B209-30914F87FDCD}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{FEFBAE14-DB90-499B-B209-30914F87FDCD}.Release|Any CPU.Build.0 = Release|Any CPU
		{C7CB23C7-95B5-448E-9CAD-673082AD0908}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C7CB23C7-95B5-448E-9CAD-673082AD0908}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C7CB23C7-95B5-448E-9CAD-673082AD0908}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C7CB23C7-95B5-448E-9CAD-673082AD0908}.Release|Any CPU.Build.0 = Release|Any CPU
		{58F98CBF-B5F3-47F3-B05D-47D4D957D159}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{58F98CBF-B5F3-47F3-B05D-47D4D957D159}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{58F98CBF-B5F3-47F3-B05D-47D4D957D159}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{58F98CBF-B5F3-47F3-B05D-47D4D957D159}.Release|Any CPU.Build.0 = Release|Any CPU
		{67942C15-FADA-4B38-A2D0-D85D8A1F90A5}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{67942C15-FADA-4B38-A2D0-D85D8A1F90A5}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{67942C15-FADA-4B38-A2D0-D85D8A1F90A5}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{67942C15-FADA-4B38-A2D0-D85D8A1F90A5}.Release|Any CPU.Build.0 = Release|Any CPU
		{798A97D3-45BE-41A3-89B3-F45140278659}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{798A97D3-45BE-41A3-89B3-F45140278659}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{798A97D3-45BE-41A3-89B3-F45140278659}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{798A97D3-45BE-41A3-89B3-F45140278659}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal



================================================
FILE: xunit.runner.json
================================================
﻿{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "diagnosticMessages": true,
  "longRunningTestSeconds": 60
}


================================================
FILE: docs/index.md
================================================
---
layout: home
sidebar: false

title: Wolverine
titleTemplate: Robust Event Driven Architectures with Simpler Code

hero:
  name: Wolverine
  text: Build Robust Event Driven Architectures with Simpler Code
  tagline: The messaging and web development framework that gets out of your way
  image:
    src: /logo.png
    alt: Wolverine Logo image
  actions:
    - theme: brand
      text: Get Started
      link: /introduction/what-is-wolverine

features:
- title: 💪 Write Less Code
  details: A unique approach to writing server side code that delivers fast performance & provides an effective middleware strategy whilst keeping out of the way of your application code
- title: ⚡️ Messaging
  details: Everything you need for a robust messaging solution between services including support for many popular transports, message failure policies, and persistent inbox/outbox messaging
- title: 📚 Asynchronous Processing
  details: Use Wolverine as an in memory command bus to easily leverage asynchronous and parallel processing within a single or multiple processes

footer: MIT Licensed | Copyright © Jeremy D. Miller and contributors.
---



================================================
FILE: docs/vite.config.js
================================================
export default {
  server: {
    fsServe: {
      root: '../'
    }
  },
  build: {
    chunkSizeWarningLimit: 3000
  }
}



================================================
FILE: docs/guide/basics.md
================================================
# Basics

![Wolverine Messaging Architecture](/messages.jpeg)

One way or another, Wolverine is all about messages within your system or between systems (Wolverine considers HTTP to just be a different flavor of message 😃). 
Staying inside a single Wolverine system, a message is typically just a .NET class or struct or C#/F# record. A message generally
represents either a "command" that should trigger an operation or an "event" that just lets another part of your system know that 
something happened. Just know that as far as Wolverine is concerned, those are roles and unlike some other messaging frameworks, will have no impact whatsoever on Wolverine's handling or implementation.

Here's a couple simple samples:

<!-- snippet: sample_DebutAccount_command -->
<a id='snippet-sample_debutaccount_command'></a>
```cs
// A "command" message
public record DebitAccount(long AccountId, decimal Amount);

// An "event" message
public record AccountOverdrawn(long AccountId);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageBusBasics.cs#L69-L77' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_debutaccount_command' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The next concept in Wolverine is a message handler, which is just a method that "knows" how to process an incoming message. Here's an extremely
simple example:

<!-- snippet: sample_DebitAccountHandler -->
<a id='snippet-sample_debitaccounthandler'></a>
```cs
public static class DebitAccountHandler
{
    public static void Handle(DebitAccount account)
    {
        Console.WriteLine($"I'm supposed to debit {account.Amount} from account {account.AccountId}");
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageBusBasics.cs#L57-L67' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_debitaccounthandler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Wolverine can act as a completely local mediator tool that allows your code to invoke the handler for a message at any time without having
to know anything about exactly how that message is processed with this usage:

<!-- snippet: sample_invoke_debit_account -->
<a id='snippet-sample_invoke_debit_account'></a>
```cs
public async Task invoke_debit_account(IMessageBus bus)
{
    // Debit $250 from the account #2222
    await bus.InvokeAsync(new DebitAccount(2222, 250));
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageBusBasics.cs#L37-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_invoke_debit_account' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There's certainly some value in Wolverine just being a command bus running inside of a single process, Wolverine also allows you to both publish and process messages received through external infrastructure like [Rabbit MQ](https://www.rabbitmq.com/)
or [Pulsar](https://pulsar.apache.org/).

To put this into perspective, here's how a Wolverine application could be connected to the outside world:

![Wolverine Messaging Architecture](/WolverineMessaging.png)

:::tip
The diagram above should just say "Message Handler" as Wolverine makes no structural differentiation between commands or events, but Jeremy is being too lazy to fix the diagram.
:::


## Terminology

* *Message* -- Typically just a .NET class or C# record that can be easily serialized. See [messages and serialization](/guide/messages) for more information
* *Envelope* -- Wolverine's [Envelope Wrapper](https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html) model that wraps the raw messages with metadata 
* *Message Handler* -- A method or function that "knows" how to process an incoming message. See [Message Handlers](/guide/handlers/) for more information
* *Transport* -- This refers to the support within Wolverine for external messaging infrastructure tools like [Rabbit MQ](/guide/messaging/transports/rabbitmq/), [Amazon SQS](/guide/messaging/transports/sqs/), [Azure Service Bus](/guide/messaging/transports/azure-service-bus/), or Wolverine's built in [TCP transport](/guide/messaging/transports/tcp)
* *Endpoint* -- The configuration for a Wolverine connection to some sort of external resource like a Rabbit MQ exchange or an Amazon SQS queue. The [Async API](https://www.asyncapi.com/) specification refers to this as a *channel*, and Wolverine may very well change its nomenclature in the future to be consistent with Async API. 
* *Sending Agent* -- You won't use this directly in your own code, but Wolverine's internal adapters to publish outgoing messages to transport endpoints
* *Listening Agent* -- Again, an internal detail of Wolverine that receives messages from external transport endpoints, and mediates between the transports and executing the message handlers
* *Node* -- Not to be confused with Node.js or Kubernetes "Node", in this documentation, "node" is just meant to be a running instance of your Wolverine application within an application cluster of any sort
* *Agent* -- Wolverine has a concept of stateful software "agents" that run on a single node, with Wolverine controlling the distribution of the agents. This is mostly used behind the scenes, but just know that it exists
* *Message Store* -- Database storage for Wolverine's [inbox/outbox persistent messaging](/guide/durability/). A durable message store is necessary for Wolverine to support leader election, node/agent assignments, durable scheduled messaging in most cases, and its [transactional inbox/outbox](/guide/durability/) support
* *Durability Agent* -- An internal subsystem in Wolverine that runs in a background service to interact with the message store for Wolverine's [transactional inbox/outbox](https://microservices.io/patterns/data/transactional-outbox.html) functionality




================================================
FILE: docs/guide/codegen.md
================================================
# Working with Code Generation

::: warning
If you are experiencing noticeable startup lags or seeing spikes in memory utilization with an application using
Wolverine, you will want to pursue using either the `Auto` or `Static` modes for code generation as explained in this guide.
:::

Wolverine uses runtime code generation to create the "adaptor" code that Wolverine uses to call into 
your message handlers. Wolverine's [middleware strategy](/guide/handlers/middleware) also uses this strategy to "weave" calls to 
middleware directly into the runtime pipeline without requiring the copious usage of adapter interfaces
that is prevalent in most other .NET frameworks.

That's great when everything is working as it should, but there's a couple issues:

1. The usage of the Roslyn compiler at runtime *can sometimes be slow* on its first usage. This can lead to sluggish *cold start*
   times in your application that might be problematic in serverless scenarios for examples.
2. There's a little bit of conventional magic in how Wolverine finds and applies middleware or passed arguments
   to your message handlers or HTTP endpoint handlers.

Not to worry though, Wolverine has several facilities to either preview the generated code for diagnostic purposes to 
really understand how Wolverine is interacting with your code and to optimize the "cold start" by generating the dynamic
code ahead of time so that it can be embedded directly into your application's main assembly and discovered from there.

By default, Wolverine runs with "dynamic" code generation where all the necessary generated types are built on demand
the first time they are needed. This is perfect for a quick start to Wolverine, and might be fine in smaller projects even
at production time.

::: warning
Note that you may need to delete the existing source code when you change
handler signatures or add or remove middleware. Nothing in Wolverine is able
to detect that the generated source code needs to be rewritten
:::

Lastly, you have a couple options about how Wolverine handles the dynamic code generation as shown below:

<!-- snippet: sample_codegen_type_load_mode -->
<a id='snippet-sample_codegen_type_load_mode'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // The default behavior. Dynamically generate the
        // types on the first usage
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;

        // Never generate types at runtime, but instead try to locate
        // the generated types from the main application assembly
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;

        // Hybrid approach that first tries to locate the types
        // from the application assembly, but falls back to
        // generating the code and dynamic type. Also writes the
        // generated source code file to disk
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/CodegenUsage.cs#L13-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_codegen_type_load_mode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

At development time, use the `Dynamic` mode if you are actively changing handler
signatures or the application of middleware that might be changing the generated code. 

Even at development time, if the handler signatures are relatively stable, you can use
the `Auto` mode to use pre-generated types locally. This may help you have a quicker
development cycle -- especially if you like to lean heavily on integration testing where
you're quickly starting and stopping your application. The `Auto` mode will write the generated
source code for missing types to the `Internal/Generated` folder under your main application 
project.

At production time, if there is any issue whatsoever with resource utilization, the Wolverine team
recommends using the `Static` mode where all types are assumed to be pre-generated into what Wolverine
thinks is the application assembly (more on this in the troubleshooting guide below).

::: tip
Most of the facilities shown here will require the [Oakton command line integration](./command-line).
:::

## Embedding Codegen in Docker

This blog post from Oskar Dudycz will apply to Wolverine as well: [How to create a Docker image for the Marten application](https://event-driven.io/en/marten_and_docker/)

At this point, the most successful mechanism and sweet spot is to run the codegen as `Dynamic` at development time, but generating
the code artifacts just in time for production deployments. From Wolverine's sibling project Marten, see this section on [Application project setup](https://martendb.io/devops/devops.html#application-project-set-up)
for embedding the code generation directly into your Docker images for deployment.

## Troubleshooting Code Generation Issues

::: warning
There's nothing magic about the `Auto` mode, and Wolverine isn't (yet) doing any file comparisons against the generated code and
the current version of the application. At this point, the Wolverine community recommends against using the `Auto` mode
for code generation as it has not added much value and can cause some confusion.
:::

In all cases, don't hesitate to reach out to the Wolverine team in the Discord link at the top right of this page to 
ask for help with any codegen related issues.

If Wolverine is throwing exceptions in `Static` mode saying that it cannot find the expected pre-generated types, here's 
your checklist of things to check:

Are the expected generated types written to files in the main application project before that project is compiled? The pre-generation
works by having the source code written into the assembly in the first place.

Is Wolverine really using the correct application assembly when it looks for pre-built handlers or HTTP endpoints? Wolverine will log
what *it* thinks is the application assembly upfront, but it can be fooled in certain project structures. To override the application
assembly choice to help Wolverine out, use this syntax:

<!-- snippet: sample_overriding_application_assembly -->
<a id='snippet-sample_overriding_application_assembly'></a>
```cs
using var host = Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Override the application assembly to help
        // Wolverine find its handlers
        // Should not be necessary in most cases
        opts.ApplicationAssembly = typeof(Program).Assembly;
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/BootstrappingSamples.cs#L10-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_overriding_application_assembly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If the assembly choice is correct, and the expected code files are really in `Internal/Generated` exactly as you'd expect, make
sure there's no accidental `<Exclude />` nodes in your project file. *Don't laugh, that's actually happened to Wolverine users*

::: warning
Actually, while the Wolverine team mostly uses JetBrains Rider that doesn't exhibit this behavior, we found out the hard way interacting with other folks that
Visual Studio.Net will add the `<Exclude />` into your `csproj` file when you manually delete the generated code files
sometimes.
:::

If you see issues with *Marten* document providers, make sure that you have registered that document with Marten itself. At this point,
Wolverine does not automatically register `Saga` types with Marten. See [Marten's own documentation](https://martendb.io) about document type discovery.

## Wolverine Code Generation and IoC <Badge type="tip" text="5.0" />

::: info
Why, you ask, does Wolverine do any of this? Wolverine was originally conceived of as the successor to the 
[FubuMVC & FubuTransportation](https://fubumvc.github.io) projects from the early 2010's. A major lesson learned
from FubuMVC was that we needed to reduce object allocations, layering, runaway `Exception` stack traces, and allow
for more flexible and streamlined handler or endpoint method signatures. To that end we fully embraced using runtime code
generation -- and this was built well before source generators were available. 

As for the IoC part of this strategy, we ask you, what's the very fastest IoC tool in .NET? The answer of course, is 
"no IoC container."
:::

Wolverine's code generation uses the configuration of your IoC tool to create the generated code wrappers 
around your raw message handlers, HTTP endpoints, and middleware methods. Whenever possible, Wolverine is trying to
completely eliminate your application's IoC tool from the runtime code by generating the necessary constructor function
invocations to exactly mimic your application's IoC configuration. 

::: info
Because you should care about this, Wolverine is absolutely generating `using` or `await using` for any objects it
creates through constructor calls that implements `IDisposable` or `IAsyncDisposable`.
:::

When generating the adapter classes, Wolverine can infer which method arguments or type dependencies can be sourced
from your application's IoC container configuration. If Wolverine can determine a way to generate all the necessary
constructor calls to create any necessary services registered with a `Scoped` or `Transient` lifetime, Wolverine will generate
code with the constructors. In this case, any IoC services that are registered with a `Singleton` lifetime
will be "inlined" as constructor arguments into the generated adapter class itself for a little better efficiency.

::: warning
The usage of a service locator within the generated code will naturally be a little less efficient just because there
is more runtime overhead. More dangerously, the service locator usage can sometimes foul up the scoping of services
like Wolverine's `IMessageBus` or Marten's `IDocumentSession` that are normally built outside of the IoC container
:::

If Wolverine cannot determine a path to generate
code for raw constructor construction of any registered services for a message handler or HTTP endpoint, Wolverine 
will fall back to generating code with the [service locator pattern](https://en.wikipedia.org/wiki/Service_locator_pattern) 
using a scoped container (think [IServiceScopeFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescopefactory?view=net-9.0-pp)).

Here's some facts you do need to know about this whole process:

* The adapter classes generated by Wolverine for both message handlers and HTTP endpoints are effectively singleton
  scoped and only ever built once
* Wolverine will try to bring `Singleton` scoped services through the generated adapter type's constructor function *one time*
* Wolverine will have to fall back to the service locator usage if any service dependency that has a `Scoped` or `Transient`
  lifetime is either an `internal` type or uses an "opaque" Lambda registration (think `IServiceCollection.AddScoped(s => {})`)

::: tip
The code generation using IoC configuration is tested with both the built in .NET `ServiceProvider` and [Lamar](https://jasperfx.github.io/lamar). It 
is theoretically possible to use other IoC tools with Wolverine, but only if you are *only* using `IServiceCollection`
for your IoC configuration.
:::

As of Wolverine 5.0, you now have the ability to better control the usage of the service locator in Wolverine's
code generation to potentially avoid unwanted usage:

<!-- snippet: sample_configuring_ServiceLocationPolicy -->
<a id='snippet-sample_configuring_servicelocationpolicy'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    // This is the default behavior. Wolverine will allow you to utilize
    // service location in the codegen, but will warn you through log messages
    // when this happens
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;

    // Tell Wolverine to just be quiet about service location and let it
    // all go. For any of you with small children, I defy you to get the 
    // Frozen song out of your head now...
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;

    // Wolverine will throw exceptions at runtime if it encounters
    // a message handler or HTTP endpoint that would require service
    // location in the code generation
    // Use this option to disallow any undesirably service location
    opts.ServiceLocationPolicy = ServiceLocationPolicy.NotAllowed;
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/ServiceLocationUsage.cs#L11-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_servicelocationpolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: note
[Wolverine.HTTP has some additional control over the service locator](/guide/http/#using-the-httpcontext-requestservices) to utilize the shared scoped container
with the rest of the AspNetCore pipeline. 
:::

## Allow List for Service Location <Badge type="tip" text="5.0" />

Wolverine always reverts to using a service locator when it encounters an "opaque" Lambda registration that has either
a `Scoped` or `Transient` service lifetime. You can explicitly create an "allow" list of service types that can use
a service locator pattern while allowing the rest of the code generation for the message handler or HTTP endpoint to use
the more predictable and efficient generated constructor functions with this syntax:

<!-- snippet: sample_always_use_service_location -->
<a id='snippet-sample_always_use_service_location'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    // other configuration

    // Use a service locator for this service w/o forcing the entire
    // message handler adapter to use a service locator for everything
    opts.CodeGeneration.AlwaysUseServiceLocationFor<IServiceGatewayUsingRefit>();
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Http/Wolverine.Http.Tests/CodeGeneration/service_location_assertions.cs#L45-L57' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_always_use_service_location' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For example, this functionality might be helpful for:

* [Refit proxies](https://github.com/reactiveui/refit) that are registered in IoC with a Lambda registration, but might not use any other services
* EF Core `DbContext` types that might require some runtime configuration to construct themselves, but don't use other services (a [JasperFx Software](https://jasperfx.net) client
  ran into this needing to conditionally opt into read replica usage, so hence, this feature made it into Wolverine 5.0)

## Environment Check for Expected Types

As a new option in Wolverine 1.7.0, you can also add an environment check for the existence of the expected pre-built types
to [fail fast](https://en.wikipedia.org/wiki/Fail-fast) on application startup like this:

<!-- snippet: sample_asserting_all_pre_built_types_exist_upfront -->
<a id='snippet-sample_asserting_all_pre_built_types_exist_upfront'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
    {
        if (builder.Environment.IsProduction())
        {
            opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;

            opts.Services.CritterStackDefaults(cr =>
            {
                // I'm only going to care about this in production
                cr.Production.AssertAllPreGeneratedTypesExist = true;
            });
        }
    });

using var host = builder.Build();
await host.StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/CodegenUsage.cs#L38-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_asserting_all_pre_built_types_exist_upfront' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that you would have to opt into using the environment checks on application startup, and maybe even force .NET
to make hosted service failures stop the application. 

See [Oakton's Environment Check functionality](https://jasperfx.github.io/oakton/guide/host/environment.html) for more information (the old Oakton documentation is still relevant for
JasperFx). 

## Previewing the Generated Code

::: tip
All of these commands are from the JasperFx.CodeGeneration.Commands library that Wolverine adds as 
a dependency. This is shared with [Marten](https://martendb.io) as well.
:::

To preview the generated source code, use this command line usage from the root directory of your .NET project:

```bash
dotnet run -- codegen preview
```

## Generating Code Ahead of Time

To write the source code ahead of time into your project, use:

```bash
dotnet run -- codegen write
```

This command **should** write all the source code files for each message handler and/or HTTP endpoint handler to `/Internal/Generated/WolverineHandlers`
directly under the root of your project folder.

## Handling Code Generation with Wolverine when using Aspire or Microsoft.Extensions.ApiDescription.Server

When integrating **Wolverine** with **Aspire**, or using `Microsoft.Extensions.ApiDescription.Server` to generate OpenAPI files at build time, you may encounter issues with code generation because connection strings are only provided by Aspire when the application is run.
This limitation affects both Wolverine codegen and OpenAPI schema generation, because these processes require connection strings during their execution.

To work around this, add a helper class that detects if we are just generating code (either by the Wolverine codegen command or during OpenAPI generation).
You can then conditionally disable external Wolverine transports and message persistence to avoid configuration errors.

```csharp
public static class CodeGeneration
{
    public static bool IsRunningGeneration()
    {
        return Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider" || Environment.GetCommandLineArgs().Contains("codegen");
    }
}
```

Example use
```csharp
if (CodeGeneration.IsRunningGeneration())
{
    builder.Services.DisableAllExternalWolverineTransports();
    builder.Services.DisableAllWolverineMessagePersistence();
}

builder.Services.AddWolverine(options =>
{
   var connectionString = builder.Configuration.GetConnectionString("postgres");
   if (CodeGeneration.IsRunningGeneration() == false)
   {
       var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
       options.PersistMessagesWithPostgresql(dataSource, "wolverine");
   }
}
```

## Optimized Workflow

Wolverine and [Marten](https://martendb.io) both use the shared JasperFx library for their code generation, 
and you can configure different behavior for production versus development time for both tools (and any future
"CritterStack" tools) with this usage:

<!-- snippet: sample_use_optimized_workflow -->
<a id='snippet-sample_use_optimized_workflow'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Use "Auto" type load mode at development time, but
        // "Static" any other time
        opts.Services.CritterStackDefaults(x =>
        {
            x.Production.GeneratedCodeMode = TypeLoadMode.Static;
            x.Production.ResourceAutoCreate = AutoCreate.None;

            // Little draconian, but this might be helpful
            x.Production.AssertAllPreGeneratedTypesExist = true;

            // These are defaults, but showing for completeness
            x.Development.GeneratedCodeMode = TypeLoadMode.Dynamic;
            x.Development.ResourceAutoCreate = AutoCreate.CreateOrUpdate;
        });
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/CodegenUsage.cs#L63-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_use_optimized_workflow' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which will use:

1. `TypeLoadMode.Auto` when the .NET environment is "Development" and try to write new source code to file
2. `TypeLoadMode.Static` for other .NET environments for optimized cold start times



================================================
FILE: docs/guide/command-line.md
================================================
# Command Line Integration

@[youtube](3C5bacH0akU)

With help from its [JasperFx](https://github.com/JasperFx) team mate [Oakton](https://jasperfx.github.io/oakton), Wolverine supports quite a few command line diagnostic and resource management
tools. To get started, apply Oakton as the command line parser in your applications as shown in the last line of code in this
sample application bootstrapping from Wolverine's [Getting Started](/tutorials/getting-started):

<!-- snippet: sample_Quickstart_Program -->
<a id='snippet-sample_quickstart_program'></a>
```cs
using JasperFx;
using Quickstart;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// The almost inevitable inclusion of Swashbuckle:)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For now, this is enough to integrate Wolverine into
// your application, but there'll be *many* more
// options later of course :-)
builder.Host.UseWolverine();

// Some in memory services for our application, the
// only thing that matters for now is that these are
// systems built by the application's IoC container
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<IssueRepository>();

var app = builder.Build();

// An endpoint to create a new issue that delegates to Wolverine as a mediator
app.MapPost("/issues/create", (CreateIssue body, IMessageBus bus) => bus.InvokeAsync(body));

// An endpoint to assign an issue to an existing user that delegates to Wolverine as a mediator
app.MapPost("/issues/assign", (AssignIssue body, IMessageBus bus) => bus.InvokeAsync(body));

// Swashbuckle inclusion
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// Opt into using JasperFx for command line parsing
// to unlock built in diagnostics and utility tools within
// your Wolverine application
return await app.RunJasperFxCommands(args);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Quickstart/Program.cs#L1-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart_program' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

From this project's root in the command line terminal tool of your choice, type:

```bash
dotnet run -- help
```

and you *should* get this hopefully helpful rundown of available command options:

```bash
The available commands are:
                                                                                                    
  Alias       Description                                                                           
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  check-env   Execute all environment checks against the application                                
  codegen     Utilities for working with JasperFx.CodeGeneration and JasperFx.RuntimeCompiler       
  describe    Writes out a description of your running application to either the console or a file  
  help        List all the available commands                                                       
  resources   Check, setup, or teardown stateful resources of this system                           
  run         Start and run this .Net application                                                   
  storage     Administer the Wolverine message storage                                                       
                                                                                                    

Use dotnet run -- ? [command name] or dotnet run -- help [command name] to see usage help about a specific command

```

## Describe a Wolverine Application

::: tip
While Wolverine certainly knows upfront what message types it handles, you may need to help Wolverine "know" what types
will be outgoing messages later with the [message discovery](/guide/messages.html#message-discovery) support.
:::

Wolverine is admittedly a configuration-heavy framework, and some combinations of conventions, policies, and explicit configuration
could easily lead to confusion about how the system is going to behave. To help ameliorate that possible situation -- but also to help the 
Wolverine team be able to remotely support folks using Wolverine -- you have this command line tool:

```bash
dotnet run -- describe
```

At this time, a Wolverine application will spit out command line reports about its configuration that
will describe:

* "Wolverine Options" - the basics properties as configured, including what Wolverine thinks is the application assembly and any registered extensions
* "Wolverine Listeners" - a tabular list of all the configured listening endpoints, including local queues, within the system and information about how they are configured
* "Wolverine Message Routing" - a tabular list of all the message routing for *known* messages published within the system
* "Wolverine Sending Endpoints" - a tabular list of all *known*, configured endpoints that send messages externally
* "Wolverine Error Handling" - a preview of the active message failure policies active within the system
* "Wolverine Http Endpoints" - shows all Wolverine HTTP endpoints. This is only active if WolverineFx.HTTP is used within the system

## Other Highlights

* See the [code generation support](./codegen)
* The `storage` command helps manage the [durable messaging support](./durability/)
* Wolverine has direct support for [Oakton](https://jasperfx.github.io/oakton) environment checks and resource management that
  can be very helpful for Wolverine integrations with message brokers or database servers








================================================
FILE: docs/guide/configuration.md
================================================
# Configuration

::: info
As of 3.0,  Wolverine **does not require the usage of the [Lamar](https://jasperfx.github.io/lamar) IoC container**, and will no longer replace the built in .NET container with Lamar.

Wolverine 3.0 *is* tested with both the built in `ServiceProvider` and Lamar. It's theoretically possible to use other
IoC containers now as long as they conform to the .NET conforming container, but this isn't tested by the Wolverine team.
:::

Wolverine is configured with the `IHostBuilder.UseWolverine()` or `HostApplicationBuilder` extension methods, with the actual configuration
living on a single `WolverineOptions` object. The `WolverineOptions` is the configuration model for your Wolverine application,
and as such it can be used to configure directives about:

* Basic elements of your Wolverine system like the system name itself
* Connections to [external messaging infrastructure](/guide/messaging/introduction) through Wolverine's *transport* model
* Messaging endpoints for either listening for incoming messages or subscribing endpoints
* [Subscription rules](/guide/messaging/subscriptions) for outgoing messages
* How [message handlers](/guide/messages) are discovered within your application and from what assemblies
* Policies to control how message handlers function, or endpoints are configured, or error handling policies

![Wolverine Configuration Model](/configuration-model.png)

## With ASP.NET Core

::: info
Do note that there's some [additional configuration to use WolverineFx.HTTP](/guide/http/integration) as well.
:::

Below is a sample of adding Wolverine to an ASP.NET Core application that is bootstrapped with
`WebApplicationBuilder`:

<!-- snippet: sample_Quickstart_Program -->
<a id='snippet-sample_quickstart_program'></a>
```cs
using JasperFx;
using Quickstart;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// The almost inevitable inclusion of Swashbuckle:)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For now, this is enough to integrate Wolverine into
// your application, but there'll be *many* more
// options later of course :-)
builder.Host.UseWolverine();

// Some in memory services for our application, the
// only thing that matters for now is that these are
// systems built by the application's IoC container
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<IssueRepository>();

var app = builder.Build();

// An endpoint to create a new issue that delegates to Wolverine as a mediator
app.MapPost("/issues/create", (CreateIssue body, IMessageBus bus) => bus.InvokeAsync(body));

// An endpoint to assign an issue to an existing user that delegates to Wolverine as a mediator
app.MapPost("/issues/assign", (AssignIssue body, IMessageBus bus) => bus.InvokeAsync(body));

// Swashbuckle inclusion
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// Opt into using JasperFx for command line parsing
// to unlock built in diagnostics and utility tools within
// your Wolverine application
return await app.RunJasperFxCommands(args);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Quickstart/Program.cs#L1-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart_program' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## "Headless" Applications

:::tip
The `WolverineOptions.Services` property can be used to add additional IoC service registrations with
either the standard .NET `IServiceCollection` model syntax.
:::

For "headless" console applications with no user interface or HTTP service endpoints, the bootstrapping
can be done with just the `HostBuilder` mechanism as shown below:

<!-- snippet: sample_bootstrapping_headless_service -->
<a id='snippet-sample_bootstrapping_headless_service'></a>
```cs
return await Host.CreateDefaultBuilder(args)
    .UseWolverine(opts =>
    {
        opts.ServiceName = "Subscriber1";

        opts.Discovery.DisableConventionalDiscovery().IncludeType<Subscriber1Handlers>();

        opts.ListenAtPort(MessagingConstants.Subscriber1Port);

        opts.UseRabbitMq().AutoProvision();

        opts.ListenToRabbitQueue(MessagingConstants.Subscriber1Queue);

        // Publish to the other subscriber
        opts.PublishMessage<RabbitMessage2>().ToRabbitQueue(MessagingConstants.Subscriber2Queue);

        // Add Open Telemetry tracing
        opts.Services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService("Subscriber1"))
                .AddJaegerExporter()

                // Add Wolverine as a source
                .AddSource("Wolverine");
        });
    })

    // Executing with Oakton as the command line parser to unlock
    // quite a few utilities and diagnostics in our Wolverine application
    .RunOaktonCommands(args);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/OpenTelemetry/Subscriber1/Program.cs#L10-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_headless_service' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As of Wolverine 3.0, you can also use the `HostApplicationBuilder` mechanism as well:

<!-- snippet: sample_bootstrapping_with_auto_apply_transactions_for_sql_server -->
<a id='snippet-sample_bootstrapping_with_auto_apply_transactions_for_sql_server'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("database");

    opts.Services.AddDbContextWithWolverineIntegration<SampleDbContext>(x =>
    {
        x.UseSqlServer(connectionString);
    });

    // Add the auto transaction middleware attachment policy
    opts.Policies.AutoApplyTransactions();
});

using var host = builder.Build();
await host.StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/EfCoreTests/SampleUsageWithAutoApplyTransactions.cs#L16-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_auto_apply_transactions_for_sql_server' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And lastly, you can just use `IServiceCollection.AddWolverine()` by itself.

## Replacing ServiceProvider with Lamar

If you run into any trouble whatsoever with code generation after upgrading to Wolverine 3.0, please:

1. Please [raise a GitHub issue in Wolverine](https://github.com/JasperFx/wolverine/issues/new/choose) with some description of the offending message handler or http endpoint
2. Fall back to Lamar for your IoC tool

To use Lamar, add this Nuget to your main project:

```bash
dotnet add package Lamar.Microsoft.DependencyInjection
```

If you're using `IHostBuilder` like you might for a simple console app, it's:

<!-- snippet: sample_use_lamar_with_host_builder -->
<a id='snippet-sample_use_lamar_with_host_builder'></a>
```cs
// With IHostBuilder
var builder = Host.CreateDefaultBuilder();
builder.UseLamar();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/DocumentationSamples.cs#L14-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_use_lamar_with_host_builder' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In a web application, it's:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();
```

and with `HostApplicationBuilder`, try:

```csharp
var builder = Host.CreateApplicationBuilder();

// Little ugly, and Lamar *should* have a helper for this...
builder.ConfigureContainer<ServiceRegistry>(new LamarServiceProviderFactory());
```

## Splitting Configuration Across Modules <Badge type="tip" text="5.0" />

To keep your `UseWolverine()` configuration from becoming too huge or to keep specific configuration maybe
within different modules within your system, you can use [Wolverine extensions](/guide/extensions).

You can also use the `IServiceCollection.ConfigureWolverine()` method to add configuration to your
Wolverine application from outside the main `UseWolverine()` code as shown below:

<!-- snippet: sample_using_configure_wolverine -->
<a id='snippet-sample_using_configure_wolverine'></a>
```cs
var builder = Host.CreateApplicationBuilder();

// Baseline Wolverine configuration
builder.Services.AddWolverine(opts =>
{
    
});

// This would be applied as an extension
builder.Services.ConfigureWolverine(w =>
{
    // There is a specific helper for this, but just go for it
    // as an easy example
    w.Durability.Mode = DurabilityMode.Solo;
});

using var host = builder.Build();

host.Services.GetRequiredService<IWolverineRuntime>()
    .Options
    .Durability
    .Mode
    .ShouldBe(DurabilityMode.Solo);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/using_configure_wolverine.cs#L14-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_configure_wolverine' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/diagnostics.md
================================================
# Diagnostics

Wolverine can be configuration intensive, allows for quite a bit of customization if you want to go down that road, and involves
quite a bit of external infrastructure. All of those things can be problematic, so Wolverine tries to provide diagnostic tools
to unwind what's going on inside the application and the application's configuration. 

Many of the diagnostics explained in this page are part of the [JasperFx command line integration](https://jasperfx.github.io/oakton) <== NOT SURE OF THE RIGHT URL. As a reminder,
to utilize this command line integration, you need to apply JasperFx as your command line parser as shown in the last line of the quickstart
sample `Program.cs` file:

<!-- snippet: sample_Quickstart_Program -->
<a id='snippet-sample_quickstart_program'></a>
```cs
using JasperFx;
using Quickstart;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// The almost inevitable inclusion of Swashbuckle:)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For now, this is enough to integrate Wolverine into
// your application, but there'll be *many* more
// options later of course :-)
builder.Host.UseWolverine();

// Some in memory services for our application, the
// only thing that matters for now is that these are
// systems built by the application's IoC container
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<IssueRepository>();

var app = builder.Build();

// An endpoint to create a new issue that delegates to Wolverine as a mediator
app.MapPost("/issues/create", (CreateIssue body, IMessageBus bus) => bus.InvokeAsync(body));

// An endpoint to assign an issue to an existing user that delegates to Wolverine as a mediator
app.MapPost("/issues/assign", (AssignIssue body, IMessageBus bus) => bus.InvokeAsync(body));

// Swashbuckle inclusion
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// Opt into using JasperFx for command line parsing
// to unlock built in diagnostics and utility tools within
// your Wolverine application
return await app.RunJasperFxCommands(args);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Quickstart/Program.cs#L1-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart_program' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Command Line Description

From the command line at the root of your project, you can get a textual report about your Wolverine application
including discovered handlers, messaging endpoints, and error handling through this command:

```bash
dotnet run -- describe
```

## Previewing Generated Code

If you ever have any question about the applicability of Wolverine (or custom) conventions or the middleware that
is configured for your application, you can see the exact code that Wolverine generates around your messaging handlers
or HTTP endpoint methods from the command line.

To write out all the generated source code to the `/Internal/Generated/WolverineHandlers` folder of your application (or designated application assembly),
use this command:

```bash
dotnet run -- codegen write
```

The naming convention for the files is `[Message Type Name]Handler#######` where the numbers are just a hashed suffix to disambiguate
message types with the same name, but in different namespaces.

Or if you just want to preview the code into your terminal window, you can also say:

```bash
dotnet run -- codegen preview
```

## Environment Checks

::: info
Wolverine 4.0 will embrace the new [IHealthCheck](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.ihealthcheck?view=net-8.0) model in .NET as a replacement for the older, JasperFx-centric
environment check model in this section. 
:::

Wolverine's external messaging transports and the durable inbox/outbox support expose [Oakton's environment checks](https://jasperfx.github.io/oakton/guide/host/environment.html)
facility to help make your Wolverine applications be self diagnosing on configuration or connectivity issues like:

* Can the application connect to its configured database?
* Can the application connect to its configured Rabbit MQ / Amazon SQS / Azure Service Bus message brokers?
* Is the underlying IoC container registrations valid?

To exercise this functionality, try:

```bash
dotnet run -- check-env
```

Or even at startup, you can use:

```bash
dotnet run -- check-env
```

to have the environment checks executed at application startup, but just realize that the application will shutdown if any
checks fail.

## Troubleshooting Handler Discovery

Wolverine has admittedly been a little challenging for some new users to get used to its handler discovery. If you are not seeing
Wolverine discover and use a message handler type and method, try this mechanism temporarily so that Wolverine can
try to explain why it's not picking that type and method up as a message handler:

<!-- snippet: sample_describe_handler_match -->
<a id='snippet-sample_describe_handler_match'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Surely plenty of other configuration for Wolverine...

        // This *temporary* line of code will write out a full report about why or
        // why not Wolverine is finding this handler and its candidate handler messages
        Console.WriteLine(opts.DescribeHandlerMatch(typeof(MyMissingMessageHandler)));
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/HandlerDiscoverySamples.cs#L148-L160' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_describe_handler_match' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Troubleshooting Message Routing

Among other information, you can find a preview of how Wolverine will route known message types through the command line
with:

```bash
dotnet run -- describe
```

Part of this output is a table of the known message types and the routed destination of any subscriptions. You can enhance
this diagnostic by helping Wolverine to [discover message types](/guide/messages#message-discovery) in your system. 

And lastly, there's a programmatic way to "preview" the Wolverine message routing at runtime that might 
be helpful:

<!-- snippet: sample_using_preview_subscriptions -->
<a id='snippet-sample_using_preview_subscriptions'></a>
```cs
public static void using_preview_subscriptions(IMessageBus bus)
{
    // Preview where Wolverine is wanting to send a message
    var outgoing = bus.PreviewSubscriptions(new BlueMessage());
    foreach (var envelope in outgoing)
    {
        // The URI value here will identify the endpoint where the message is
        // going to be sent (Rabbit MQ exchange, Azure Service Bus topic, Kafka topic, local queue, etc.)
        Debug.WriteLine(envelope.Destination);
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Runtime/Routing/routing_rules.cs#L90-L104' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_preview_subscriptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->




================================================
FILE: docs/guide/extensions.md
================================================
# Configuration Extensions

::: warning
As of Wolverine 3.0 and our move to directly support non-Lamar IoC containers, it is no longer
possible to alter service registrations through Wolverine extensions that are themselves registered
in the IoC container at bootstrapping time.
:::

Wolverine supports the concept of extensions for modularizing Wolverine configuration with implementations of the `IWolverineExtension` interface:

<!-- snippet: sample_IWolverineExtension -->
<a id='snippet-sample_iwolverineextension'></a>
```cs
/// <summary>
///     Use to create loadable extensions to Wolverine applications
/// </summary>
public interface IWolverineExtension
{
    /// <summary>
    ///     Make any alterations to the WolverineOptions for the application
    /// </summary>
    /// <param name="options"></param>
    void Configure(WolverineOptions options);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/IWolverineExtension.cs#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iwolverineextension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Here's a sample:

<!-- snippet: sample_SampleExtension -->
<a id='snippet-sample_sampleextension'></a>
```cs
public class SampleExtension : IWolverineExtension
{
    public void Configure(WolverineOptions options)
    {
        // Add service registrations
        options.Services.AddTransient<IFoo, Foo>();

        // Alter settings within the application
        options
            .UseNewtonsoftForSerialization(settings => settings.TypeNameHandling = TypeNameHandling.None);
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/ExtensionSamples.cs#L9-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sampleextension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Extensions can be applied programmatically against the `WolverineOptions` like this:

<!-- snippet: sample_including_extension -->
<a id='snippet-sample_including_extension'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Including a single extension
        opts.Include<SampleExtension>();

        // Or add a Wolverine extension that needs
        // to use IoC services
        opts.Services.AddWolverineExtension<ConfigurationUsingExtension>();

    })

    .ConfigureServices(services =>
    {
        // This is the same logical usage, just showing that it
        // can be done directly against IServiceCollection
        services.AddWolverineExtension<ConfigurationUsingExtension>();
    })

    .StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/ExtensionSamples.cs#L52-L75' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_including_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Lastly, you can also add `IWolverineExtension` types to your IoC container registration that will be applied to `WolverineOptions` just
before bootstrapping Wolverine at runtime. This was originally added to allow for test automation scenarios where you might want
to override part of the Wolverine setup during tests. As an example, consider this common usage for disabling external transports
during testing:

<!-- snippet: sample_disabling_the_transports_from_web_application_factory -->
<a id='snippet-sample_disabling_the_transports_from_web_application_factory'></a>
```cs
// This is using Alba to bootstrap a Wolverine application
// for integration tests, but it's using WebApplicationFactory
// to do the actual bootstrapping
await using var host = await AlbaHost.For<Program>(x =>
{
    // I'm overriding
    x.ConfigureServices(services => services.DisableAllExternalWolverineTransports());
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Middleware/AppWithMiddleware.Tests/try_out_the_middleware.cs#L29-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disabling_the_transports_from_web_application_factory' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Behind the scenes, Wolverine has a small extension like this:

<!-- snippet: sample_DisableExternalTransports -->
<a id='snippet-sample_disableexternaltransports'></a>
```cs
internal class DisableExternalTransports : IWolverineExtension
{
    public void Configure(WolverineOptions options)
    {
        options.ExternalTransportsAreStubbed = true;
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/HostBuilderExtensions.cs#L384-L394' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disableexternaltransports' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And that extension is just added to the application's IoC container at test bootstrapping time like this:

<!-- snippet: sample_extension_method_to_disable_external_transports -->
<a id='snippet-sample_extension_method_to_disable_external_transports'></a>
```cs
public static IServiceCollection DisableAllExternalWolverineTransports(this IServiceCollection services)
{
    services.AddSingleton<IWolverineExtension, DisableExternalTransports>();
    return services;
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/HostBuilderExtensions.cs#L361-L369' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_extension_method_to_disable_external_transports' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In usage, the `IWolverineExtension` objects added to the IoC container are applied *after* the inner configuration
inside your application's `UseWolverine()` set up.

As another example, `IWolverineExtension` objects added to the IoC container can also use services injected into the 
extension object from the IoC container as shown in this example that uses the .NET `IConfiguration` service:

<!-- snippet: sample_configuration_using_extension -->
<a id='snippet-sample_configuration_using_extension'></a>
```cs
public class ConfigurationUsingExtension : IWolverineExtension
{
    private readonly IConfiguration _configuration;

    // Use constructor injection from your DI container at runtime
    public ConfigurationUsingExtension(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(WolverineOptions options)
    {
        // Configure the wolverine application using
        // the information from IConfiguration
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/ExtensionSamples.cs#L26-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuration_using_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There's also a small helper method to register Wolverine extensions like so:

## Modifying Transport Configuration

If your Wolverine extension needs to apply some kind of extra configuration to the transport integration, most of the 
transport packages support a `WolverineOptions.ConfigureTransportName()` extension method that will let you make
additive configuration changes to the transport integration for items like declaring extra queues, topics, exchanges, subscriptions or overriding
dead letter queue behavior. For example:

1. `ConfigureRabbitMq()`
2. `ConfigureKafka()`
3. `ConfigureAzureServiceBus()`
4. `ConfigureAmazonSqs()`

## Asynchronous Extensions

::: tip
This was added to Wolverine 2.3, specifically for a user needing to use the [Feature Flag library](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core) from Microsoft. 
:::

There is also any option for creating Wolverine extensions that need to use asynchronous methods to configure
the `WolverineOptions` using the `IAsyncWolverineExtension` library. A sample is shown below:

<!-- snippet: sample_async_Wolverine_extension -->
<a id='snippet-sample_async_wolverine_extension'></a>
```cs
public class SampleAsyncExtension : IAsyncWolverineExtension
{
    private readonly IFeatureManager _features;

    public SampleAsyncExtension(IFeatureManager features)
    {
        _features = features;
    }

    public async ValueTask Configure(WolverineOptions options)
    {
        if (await _features.IsEnabledAsync("Module1"))
        {
            // Make any kind of Wolverine configuration
            options
                .PublishMessage<Module1Message>()
                .ToLocalQueue("module1-high-priority")
                .Sequential();
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Acceptance/using_async_extensions.cs#L64-L88' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_async_wolverine_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which can be added to your application with this extension method on `IServiceCollection`:

<!-- snippet: sample_registering_async_extension -->
<a id='snippet-sample_registering_async_extension'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.Services.AddFeatureManagement();
        opts.Services.AddSingleton(featureManager);

        // Adding the async extension to the underlying IoC container
        opts.Services.AddAsyncWolverineExtension<SampleAsyncExtension>();

    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Acceptance/using_async_extensions.cs#L42-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_registering_async_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Asynchronous Extensions and Wolverine.HTTP

Just a heads up, there's a timing issue between the application of asynchronous Wolverine extensions
and the usage of the Wolverine.HTTP `MapWolverineEndpoints()` method. If you need the asynchronous 
extensions to apply to the HTTP configuration, you need to help Wolverine out by explicitly calling
this method in your `Program` file *after* building the `WebApplication`, but before calling 
`MapWolverineEndpoints()` like so:

<!-- snippet: sample_calling_ApplyAsyncWolverineExtensions -->
<a id='snippet-sample_calling_applyasyncwolverineextensions'></a>
```cs
var app = builder.Build();

// In order for async Wolverine extensions to apply to Wolverine.HTTP configuration,
// you will need to explicitly call this *before* MapWolverineEndpoints()
await app.Services.ApplyAsyncWolverineExtensions();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Http/WolverineWebApi/Program.cs#L166-L174' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_calling_applyasyncwolverineextensions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Wolverine Plugin Modules

::: warning
This functionality will likely be eliminated in Wolverine 3.0. 
:::

::: tip
Use this sparingly, but it might be advantageous for adding extra instrumentation or extra middleware
:::

If you want to create a Wolverine extension assembly that automatically loads itself into an application just
by being referenced by the project, you can use a combination of `IWolverineExtension` and the `[WolverineModule]`
assembly attribute.

Assuming that you have an implementation of `IWolverineExtension` named `Module1Extension`, you can mark your module library
with this attribute to automatically add that extension to Wolverine:

<!-- snippet: sample_using_wolverine_module_to_load_extension -->
<a id='snippet-sample_using_wolverine_module_to_load_extension'></a>
```cs
[assembly: WolverineModule<Module1Extension>]
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/Module1/Properties/AssemblyInfo.cs#L29-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_wolverine_module_to_load_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Disabling Assembly Scanning

Some Wolverine users have seen rare issues with the assembly scanning cratering an application with out of memory
exceptions in the case of an application directory being the same as the root of a Docker container. *If* you experience
that issue, or just want a faster start up time, you can disable the automatic extension discovery using this syntax:

<!-- snippet: sample_disabling_assembly_scanning -->
<a id='snippet-sample_disabling_assembly_scanning'></a>
```cs
using var host = await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.DisableConventionalDiscovery();
    }, ExtensionDiscovery.ManualOnly)
    
    .StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/bootstrapping_specs.cs#L67-L77' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disabling_assembly_scanning' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/index.md
================================================
# Wolverine Guides

Welcome to the Wolverine documentation website! See the content in the left hand pane.



================================================
FILE: docs/guide/logging.md
================================================
# Instrumentation and Metrics

Wolverine logs through the standard .NET `ILogger` abstraction, and there's nothing special you need to do
to enable that logging other than using one of the standard approaches for bootstrapping a .NET application
using `IHostBuilder`. Wolverine is logging all messages sent, received, and executed inline.

::: info
Inside of message handling, Wolverine is using `ILogger<T>` where `T` is the **message type**. So if you want
to selectively filter logging levels in your application, rely on the message type rather than the handler type.
:::

## Configuring Message Logging Levels

::: tip
This functionality was added in Wolverine 1.7.
:::

Wolverine automatically logs the execution start and stop of all message handling with `LogLevel.Debug`. Likewise, Wolverine
logs the successful completion of all messages (including the capture of cascading messages and all middleware) with `LogLevel.Information`.
However, many folks have found this logging to be too intrusive. Not to worry, you can quickly override the log levels
within Wolverine for your system like so:

<!-- snippet: sample_turning_down_message_logging -->
<a id='snippet-sample_turning_down_message_logging'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Turn off all logging of the message execution starting and finishing
        // The default is Debug
        opts.Policies.MessageExecutionLogLevel(LogLevel.None);

        // Turn down Wolverine's built in logging of all successful
        // message processing
        opts.Policies.MessageSuccessLogLevel(LogLevel.Debug);
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/LoggingUsage.cs#L26-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_turning_down_message_logging' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The sample up above turns down the logging on a global, application level. If you have some kind of command message where
you don't want logging for that particular message type, but do for all other message types, you can override the log
level for only that specific message type like so:

<!-- snippet: sample_customized_handler_using_Configure -->
<a id='snippet-sample_customized_handler_using_configure'></a>
```cs
public class CustomizedHandler
{
    public void Handle(SpecialMessage message)
    {
        // actually handle the SpecialMessage
    }

    public static void Configure(HandlerChain chain)
    {
        chain.Middleware.Add(new CustomFrame());

        // Turning off all execution tracking logging
        // from Wolverine for just this message type
        // Error logging will still be enabled on failures
        chain.SuccessLogLevel = LogLevel.None;
        chain.ProcessingLogLevel = LogLevel.None;
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/can_customize_handler_chain_through_Configure_call_on_HandlerType.cs#L25-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_customized_handler_using_configure' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Methods on message handler types with the signature:

```csharp
public static void Configure(HandlerChain chain)
```

will be called by Wolverine to apply message type specific overrides to Wolverine's message handling.

## Controlling Message Specific Logging and Tracing

While Open Telemetry tracing can be disabled on an endpoint by endpoint basis, you may want to disable Open Telemetry
tracing for specific message types. You may also want to modify the log levels for message success and message execution
on a message type by message type basis. While you *can* also do that with custom handler chain policies, the easiest
way to do that is to use the `[WolverineLogging]` attribute on either the handler type or the handler method as shown 
below:

<!-- snippet: sample_using_Wolverine_Logging_attribute -->
<a id='snippet-sample_using_wolverine_logging_attribute'></a>
```cs
public class QuietMessage;

public class QuietMessageHandler
{
    [WolverineLogging(
        telemetryEnabled:false,
        successLogLevel: LogLevel.None,
        executionLogLevel:LogLevel.Trace)]
    public void Handle(QuietMessage message)
    {
        Console.WriteLine("Hush!");
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Acceptance/logging_configuration.cs#L27-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_wolverine_logging_attribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Log Message Execution Start

Wolverine is absolutely meant for "grown up development," so there's a few options for logging and instrumentation. While Open Telemetry logging 
is built in and will always give you the activity span for message execution start and finish, you may want the start of each
message execution to be logged as well. Rather than force your development teams to write repetitive logging statements for every single
message handler method, you can ask Wolverine to do that for you:

<!-- snippet: sample_log_message_starting -->
<a id='snippet-sample_log_message_starting'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Opt into having Wolverine add a log message at the beginning
        // of the message execution
        opts.Policies.LogMessageStarting(LogLevel.Information);
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/LoggingUsage.cs#L11-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_log_message_starting' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This will append log entries looking like this:

```text
[09:41:00 INF] Starting to process <MessageType> (<MessageId>)
```

With only the defaults, Wolverine is logging the type of message and the message id. As shown in the next section, you can also add
additional context to these log messages.

In conjunction with the "audited members" that are added to these logging statements, all the logging in Wolverine is using structural logging
for better searching within your logs. 

## Contextual Logging with Audited Members

::: warning
Be cognizant of the information you're writing to log files or Open Telemetry data and whether or not that data
is some kind of protected data like personal data identifiers.
:::

Wolverine gives you the ability to mark public fields or properties on message types as "audited members" that will be
part of the logging messages at the beginning of message execution described in the preview section, and also in the Open Telemetry support described in the
next section.

To explicitly mark members as "audited", you *can* use attributes within your message types (and these are inherited) like so:

<!-- snippet: sample_using_audit_attribute -->
<a id='snippet-sample_using_audit_attribute'></a>
```cs
public class AuditedMessage
{
    [Audit]
    public string Name { get; set; }

    [Audit("AccountIdentifier")] public int AccountId;
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/auditing_determination.cs#L86-L96' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_audit_attribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or if you are okay using a common message interface for common identification like "this message targets an account/organization/tenant/client"
like the `IAccountCommand` shown below:

<!-- snippet: sample_account_message_for_auditing -->
<a id='snippet-sample_account_message_for_auditing'></a>
```cs
// Marker interface
public interface IAccountMessage
{
    public int AccountId { get; }
}

// A possible command that uses our marker interface above
public record DebitAccount(int AccountId, decimal Amount) : IAccountMessage;
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/auditing_determination.cs#L111-L122' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_account_message_for_auditing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can specify audited members through this syntax:

<!-- snippet: sample_explicit_registration_of_audit_properties -->
<a id='snippet-sample_explicit_registration_of_audit_properties'></a>
```cs
// opts is WolverineOptions inside of a UseWolverine() call
opts.Policies.ForMessagesOfType<IAccountMessage>().Audit(x => x.AccountId);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/auditing_determination.cs#L73-L78' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_explicit_registration_of_audit_properties' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This will extend your log entries to like this:

```text
[09:41:00 INFO] Starting to process IAccountMessage ("018761ad-8ed2-4bc9-bde5-c3cbb643f9f3") with AccountId: "c446fa0b-7496-42a5-b6c8-dd53c65c96c8"
```

## Open Telemetry

Wolverine also supports the [Open Telemetry](https://opentelemetry.io/docs/instrumentation/net/) standard for distributed tracing. To enable
the collection of Open Telemetry data, you need to add Wolverine as a data source as shown in this
code sample:

<!-- snippet: sample_enabling_open_telemetry -->
<a id='snippet-sample_enabling_open_telemetry'></a>
```cs
// builder.Services is an IServiceCollection object
builder.Services.AddOpenTelemetryTracing(x =>
{
    x.SetResourceBuilder(ResourceBuilder
            .CreateDefault()
            .AddService("OtelWebApi")) // <-- sets service name
        .AddJaegerExporter()
        .AddAspNetCoreInstrumentation()

        // This is absolutely necessary to collect the Wolverine
        // open telemetry tracing information in your application
        .AddSource("Wolverine");
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/OpenTelemetry/OtelWebApi/Program.cs#L36-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enabling_open_telemetry' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip
Wolverine 1.7 added the ability to disable Open Telemetry tracing on an endpoint by endpoint basis, and **finally** turned
off Otel tracing of internal Wolverine messages
:::

Open Telemetry tracing can be selectively disabled on an endpoint by endpoint basis with this API:

<!-- snippet: sample_disabling_open_telemetry_by_endpoint -->
<a id='snippet-sample_disabling_open_telemetry_by_endpoint'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts
            .PublishAllMessages()
            .ToPort(2222)

            // Disable Open Telemetry data collection on
            // all messages sent, received, or executed
            // from this endpoint
            .TelemetryEnabled(false);
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/DisablingOpenTelemetry.cs#L11-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disabling_open_telemetry_by_endpoint' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Note that this `TelemetryEnabled()` method is available on all possible subscriber and listener types within Wolverine.
This flag applies to all messages sent, received, or executed at a particular endpoint.

Wolverine endeavors to publish OpenTelemetry spans or activities for meaningful actions within a Wolverine application. Here
are the specific span names, activity names, and tag names emitted by Wolverine:

<!-- snippet: sample_wolverine_open_telemetry_tracing_spans_and_activities -->
<a id='snippet-sample_wolverine_open_telemetry_tracing_spans_and_activities'></a>
```cs
/// <summary>
/// ActivityEvent marking when an incoming envelope is discarded
/// </summary>
public const string EnvelopeDiscarded = "wolverine.envelope.discarded";

/// <summary>
/// ActivityEvent marking when an incoming envelope is being moved to the error queue
/// </summary>
public const string MovedToErrorQueue = "wolverine.error.queued";

/// <summary>
/// ActivityEvent marking when an incoming envelope does not have a known message
/// handler and is being shunted to registered "NoHandler" actions
/// </summary>
public const string NoHandler = "wolverine.no.handler";

/// <summary>
/// ActivityEvent marking when a message failure is configured to pause the message listener
/// where the message was handled. This is tied to error handling policies
/// </summary>
public const string PausedListener = "wolverine.paused.listener";

/// <summary>
/// Span that is emitted when a listener circuit breaker determines that there are too many
/// failures and listening should be paused
/// </summary>
public const string CircuitBreakerTripped = "wolverine.circuit.breaker.triggered";

/// <summary>
/// Span emitted when a listening agent is started or restarted
/// </summary>
public const string StartingListener = "wolverine.starting.listener";

/// <summary>
/// Span emitted when a listening agent is stopping
/// </summary>
public const string StoppingListener = "wolverine.stopping.listener";

/// <summary>
/// Span emitted when a listening agent is being paused
/// </summary>
public const string PausingListener = "wolverine.pausing.listener";

/// <summary>
/// ActivityEvent marking that an incoming envelope is being requeued after a message
/// processing failure
/// </summary>
public const string EnvelopeRequeued = "wolverine.envelope.requeued";

/// <summary>
/// ActivityEvent marking that an incoming envelope is being retried after a message
/// processing failure
/// </summary>
public const string EnvelopeRetry = "wolverine.envelope.retried";

/// <summary>
/// ActivityEvent marking than an incoming envelope has been rescheduled for later
/// execution after a failure
/// </summary>
public const string ScheduledRetry = "wolverine.envelope.rescheduled";

/// <summary>
/// Tag name trying to explain why a sender or listener was stopped or paused
/// </summary>
public const string StopReason = "wolverine.stop.reason";

/// <summary>
/// The Wolverine Uri that identifies what sending or listening endpoint the activity
/// refers to
/// </summary>
public const string EndpointAddress = "wolverine.endpoint.address";

/// <summary>
/// A stop reason when back pressure policies call for a pause in processing in a single endpoint
/// </summary>
public const string TooBusy = "TooBusy";

/// <summary>
/// A span emitted when a sending agent for a specific endpoint is paused
/// </summary>
public const string SendingPaused = "wolverine.sending.pausing";

/// <summary>
/// A span emitted when a sending agent is resuming after having been paused
/// </summary>
public const string SendingResumed = "wolverine.sending.resumed";

/// <summary>
/// A stop reason when sending agents are paused after too many sender failures
/// </summary>
public const string TooManySenderFailures = "TooManySenderFailures";
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Runtime/WolverineTracing.cs#L27-L121' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_wolverine_open_telemetry_tracing_spans_and_activities' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Message Correlation

::: tip
Each individual message transport technology like Rabbit MQ, Azure Service Bus, or Amazon SQS has its own flavor of *Envelope Wrapper*, but Wolverine
uses its own `Envelope` structure internally and maps between its canonical representation and the transport specific envelope wrappers at runtime.
:::

As part of Wolverine's instrumentation, it tracks the causality between messages received and published by Wolverine. It also enables you to correlate Wolverine
activity back to inputs from outside of Wolverine like ASP.Net Core request ids. The key item here is Wolverine's `Envelope` class (see the [Envelope Wrapper](https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html) pattern discussed in the venerable Enterprise Integration Patterns) that holds messages
the message and all the metadata for the message within Wolverine handling. 

| Property       | Type                | Source                                                           | Description                                                                              |
|----------------|---------------------|------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| Id             | `Guid` (Sequential) | Assigned by Wolverine                                            | Identifies a specific Wolverine message                                                  |
| CorrelationId  | `string`            | See the following discussion                                     | Correlating identifier for the logical workflow or system action across multiple actions |
| ConversationId | `Guid`              | Assigned by Wolverine                                            | Id of the immediate message or workflow that caused this envelope to be sent             |
| SagaId         | `string`            | Assigned by Wolverine                                            | Identifies the current stateful saga that this message refers to, if part of a stateful saga |
| TenantId       | `string`            | Assigned by user on IMessageBus, but transmitted across messages | User defined tenant identifier for multi-tenancy strategies |

Correlation is a little bit complicated. The correlation id is originally owned at the `IMessageBus` or `IMessageContext` level. By default,
the `IMessageBus.CorrelationId` is set to be the [root id of the current System.Diagnostics.Activity](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.activity.rootid?view=net-7.0#system-diagnostics-activity-rootid).
That's convenient, because it would hopefully, automatically tie your Wolverine behavior to outside activity like ASP.Net Core HTTP requests. 

If you are publishing messages within the context of a Wolverine handler -- either with `IMessageBus` / `IMessageContext` or through cascading messages -- the correlation id of any outgoing
messages will be the correlation id of the original message that is being currently handled. 

If there is no existing correlation id from either a current activity or a previous message, Wolverine will assign a new correlation id
as a `Guid` value converted to a string.


## Metrics

Wolverine is automatically tracking several performance related metrics through the [System.Diagnostics.Metrics](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics?view=net-8.0) types, 
which sets Wolverine users up for being able to export their system’s performance metrics to third party observability tools like Honeycomb or Datadog that support Open Telemetry metrics. The current set of metrics in Wolverine are shown below:

::: warning
The metrics for the inbox, outbox, and scheduled message counts were unfortunately lost when Wolverine introduced multi-tenancy. They
will be added back to Wolverine in 4.0.
:::

| Metric Name                  | Metric Type                                                                                               | Description                                                                                                                                                                                                                                                                            |
|------------------------------|-----------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| wolverine-messages-sent      | [Counter](https://opentelemetry.io/docs/reference/specification/metrics/api/#counter)                     | Number of messages sent                                                                                                                                                                                                                                                                |
| wolverine-execution-time     | [Histogram](https://opentelemetry.io/docs/reference/specification/metrics/api/#histogram)                 | Execution time in milliseconds                                                                                                                                                                                                                                                         |
| wolverine-messages-succeeded | Counter                                                                                                   | Number of messages successfully processed                                                                                                                                                                                                                                              |
| wolverine-dead-letter-queue  | Counter                                                                                                   | Number of messages moved to dead letter queues                                                                                                                                                                                                                                         |
| wolverine-effective-time     | Histogram                                                                                                 | Effective time between a message being sent and being completely handled in milliseconds. Right now this works between Wolverine to Wolverine application sending and from NServiceBus applications sending to Wolverine applications through Wolverine’s NServiceBus interoperability. |
| wolverine-execution-failure  | Counter                                                                                                   | Number of message execution failures. Tagged by exception type                                                                                                                                                                                                                         |

As a sample set up for publishing metrics, here's a proof of concept built with Honeycomb as the metrics collector:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .UseWolverine((context, opts) =>
    {
        opts.ServiceName = "Metrics";
 
        // Open Telemetry *should* cover this anyway, but
        // if you want Wolverine to log a message for *beginning*
        // to execute a message, try this
        opts.Policies.LogMessageStarting(LogLevel.Debug);
         
        // For both Open Telemetry span tracing and the "log message starting..."
        // option above, add the AccountId as a tag for any command that implements
        // the IAccountCommand interface
        opts.Policies.ForMessagesOfType<IAccountCommand>().Audit(x => x.AccountId);
         
        // Setting up metrics and Open Telemetry activity tracing
        // to Honeycomb
        var honeycombOptions = context.Configuration.GetHoneycombOptions();
        honeycombOptions.MetricsDataset = "Wolverine:Metrics";
         
        opts.Services.AddOpenTelemetry()
            // enable metrics
            .WithMetrics(x =>
            {
                // Export metrics to Honeycomb
                x.AddHoneycomb(honeycombOptions);
            })
             
            // enable Otel span tracing
            .WithTracing(x =>
            {
                x.AddHoneycomb(honeycombOptions);
                x.AddSource("Wolverine");
            });
 
    })
    .UseResourceSetupOnStartup()
    .Build();
 
await host.RunAsync();
```

### Additional Metrics Tags

You can add additional tags to the performance metrics per message type for system specific correlation in tooling like Datadog, Grafana, or Honeycomb. From
an example use case that I personally work with, let's say that our system handles multiple message types that all refer to a specific client entity we're going
to call "Organization Code." For the sake of performance correlation and troubleshooting later, we would like to have an idea about how the system performance
varies between organizations. To do that, we will be adding the "Organization Code" as a tag to the performance metrics.

First, let's start by using a common interface called `IOrganizationRelated` interface that just provides a common way
of exposing the `OrganizationCode` for these message types handled by Wolverine. Next, the mechanism to adding the "Organization Code" to the metrics is to use the `Envelope.SetMetricsTag()` method
to tag the current message being processed. Going back to the `IOrganizationRelated` marker interface, we can add some middleware that acts on
`IOrganizationRelated` messages to add the metrics tag as shown below:

<!-- snippet: sample_organization_tagging_middleware -->
<a id='snippet-sample_organization_tagging_middleware'></a>
```cs
// Common interface on message types within our system
public interface IOrganizationRelated
{
    string OrganizationCode { get; }
}

// Middleware just to add a metrics tag for the organization code
public static class OrganizationTaggingMiddleware
{
    public static void Before(IOrganizationRelated command, Envelope envelope)
    {
        envelope.SetMetricsTag("org.code", command.OrganizationCode);
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MetricsSamples.cs#L43-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_organization_tagging_middleware' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Finally, we'll add the new middleware to all message handlers where the message implements the `IOrganizationRelated` interface like so:

<!-- snippet: sample_using_organization_tagging_middleware -->
<a id='snippet-sample_using_organization_tagging_middleware'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Add this middleware to all handlers where the message can be cast to
        // IOrganizationRelated
        opts.Policies.ForMessagesOfType<IOrganizationRelated>().AddMiddleware(typeof(OrganizationTaggingMiddleware));
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MetricsSamples.cs#L10-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_organization_tagging_middleware' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Tenant Id Tagging

<!-- snippet: sample_tenant_id_tagging -->
<a id='snippet-sample_tenant_id_tagging'></a>
```cs
public static async Task publish_operation(IMessageBus bus, string tenantId, string name)
{
    // All outgoing messages or executed messages from this
    // IMessageBus object will be tagged with the tenant id
    bus.TenantId = tenantId;
    await bus.PublishAsync(new SomeMessage(name));
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MetricsSamples.cs#L30-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_tenant_id_tagging' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/messages.md
================================================
# Messages and Serialization

The ultimate goal of Wolverine is to allow developers to route messages representing some work to do within the system
to the proper handler that can handle that message. Here's some facts about messages in Wolverine:

* By role, you can think of messages as either a command you want to execute or as an event raised somewhere in your system
  that you want to be handled by separate code or in a separate thread
* Messages in Wolverine **must be public types**
* Unlike other .NET messaging or command handling frameworks, there's no requirement for Wolverine messages to be an interface or require any mandatory interface or framework base classes
* Have a string identity for the message type that Wolverine will use as an identification when storing messages
  in either durable message storage or within external transports

The default serialization option is [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-8.0), as this is now mature, seems to work with just about anything now, and sets you up
for relatively easy integration with a range of external non-Wolverine applications. You also have the option to fall back
to Newtonsoft.JSON or to use higher performance [MemoryPack](/guide/messages.html#memorypack-serialization) or [MessagePack](/guide/messages.html#messagepack-serialization) or [Protobuf](/guide/messages.html#protobuf-serialization) integrations with Wolverine.

## Message Type Name or Alias

Let's say that you have a basic message structure like this:

<!-- snippet: sample_PersonBorn1 -->
<a id='snippet-sample_personborn1'></a>
```cs
public class PersonBorn
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // This is obviously a contrived example
    // so just let this go for now;)
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L13-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_personborn1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

By default, Wolverine will identify this type by just using the .NET full name like so:

<!-- snippet: sample_ootb_message_alias -->
<a id='snippet-sample_ootb_message_alias'></a>
```cs
[Fact]
public void message_alias_is_fullname_by_default()
{
    new Envelope(new PersonBorn())
        .MessageType.ShouldBe(typeof(PersonBorn).FullName);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L32-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ootb_message_alias' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

However, if you want to explicitly control the message type because you aren't sharing the DTO types or for some
other reason (readability? diagnostics?), you can override the message type alias with an attribute:

<!-- snippet: sample_override_message_alias -->
<a id='snippet-sample_override_message_alias'></a>
```cs
[MessageIdentity("person-born")]
public class PersonBorn
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L47-L59' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_override_message_alias' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which now gives you different behavior:

<!-- snippet: sample_explicit_message_alias -->
<a id='snippet-sample_explicit_message_alias'></a>
```cs
[Fact]
public void message_alias_is_fullname_by_default()
{
    new Envelope(new PersonBorn())
        .MessageType.ShouldBe("person-born");
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L63-L72' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_explicit_message_alias' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Message Discovery

::: tip
Wolverine does not yet support the Async API standard, but the message discovery described in this section
is also partially meant to enable that support later.
:::

Strictly for diagnostic purposes in Wolverine (like the message routing preview report in `dotnet run -- describe`), you can mark your message types to help Wolverine "discover" outgoing message 
types that will be published by the application by either implementing one of these marker interfaces (all in the main `Wolverine` namespace):

<!-- snippet: sample_message_type_discovery -->
<a id='snippet-sample_message_type_discovery'></a>
```cs
public record CreateIssue(string Name) : IMessage;

public record DeleteIssue(Guid Id) : IMessage;

public record IssueCreated(Guid Id, string Name) : IMessage;
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageDiscovery.cs#L6-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_message_type_discovery' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip
The marker types shown above may be helpful in transitioning an existing codebase from NServiceBus to Wolverine.
:::

You can optionally use an attribute to mark a type as a message:

<!-- snippet: sample_using_WolverineMessage_attribute -->
<a id='snippet-sample_using_wolverinemessage_attribute'></a>
```cs
[WolverineMessage]
public record CloseIssue(Guid Id);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageDiscovery.cs#L16-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_wolverinemessage_attribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or lastly, make up your own criteria to find and mark message types within your system as shown below:

<!-- snippet: sample_use_your_own_marker_type -->
<a id='snippet-sample_use_your_own_marker_type'></a>
```cs
opts.Discovery.CustomizeHandlerDiscovery(types => types.Includes.Implements<IDiagnosticsMessageHandler>());
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Diagnostics/DiagnosticsApp/Program.cs#L39-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_use_your_own_marker_type' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Note that only types that are in assemblies either marked with `[assembly: WolverineModule]` or the main application assembly
or an explicitly registered assembly will be discovered. See [Handler Discovery](/guide/handlers/discovery) for more information about the assembly scanning.


## Versioning

By default, Wolverine will just assume that any message is "V1" unless marked otherwise.
Going back to the original `PersonBorn` message class in previous sections, let's say that you
create a new version of that message that is no longer structurally equivalent to the original message:

<!-- snippet: sample_PersonBorn_V2 -->
<a id='snippet-sample_personborn_v2'></a>
```cs
[MessageIdentity("person-born", Version = 2)]
public class PersonBornV2
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthday { get; set; }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L78-L88' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_personborn_v2' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `[MessageIdentity("person-born", Version = 2)]` attribute usage tells Wolverine that this class is "Version 2" for the `message-type` = "person-born."

Wolverine will now accept or publish this message using the built in Json serialization with the content type of `application/vnd.person-born.v2+json`.
Any custom serializers should follow some kind of naming convention for content types that identify versioned representations.


## Serialization

::: warning
Just in time for 1.0, Wolverine switched to using System.Text.Json as the default serializer instead of Newtonsoft.Json. Fingers crossed!
:::

Wolverine needs to be able to serialize and deserialize your message objects when sending messages with external transports like Rabbit MQ or when using the inbox/outbox message storage.
To that end, the default serialization is performed with [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/api/system.text.json?view=net-6.0) but
you may also opt into using old, battle tested Newtonsoft.Json.

And to instead opt into using System.Text.Json with different defaults -- which can give you better performance but with
increased risk of serialization failures -- use this syntax where `opts` is a `WolverineOptions` object:

<!-- snippet: sample_opting_into_STJ -->
<a id='snippet-sample_opting_into_stj'></a>
```cs
opts.UseSystemTextJsonForSerialization(stj =>
{
    stj.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Transports/Local/local_integration_specs.cs#L26-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_opting_into_stj' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

When using Newtonsoft.Json, the default configuration is:

<!-- snippet: sample_default_newtonsoft_settings -->
<a id='snippet-sample_default_newtonsoft_settings'></a>
```cs
return new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto,
    PreserveReferencesHandling = PreserveReferencesHandling.Objects
};
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Runtime/Serialization/NewtonsoftSerializer.cs#L146-L154' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_default_newtonsoft_settings' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To customize the Newtonsoft.Json serialization, use this option:

<!-- snippet: sample_CustomizingJsonSerialization -->
<a id='snippet-sample_customizingjsonserialization'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.UseNewtonsoftForSerialization(settings =>
        {
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        });
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L161-L172' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_customizingjsonserialization' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### MessagePack Serialization

Wolverine supports the [MessagePack](https://github.com/neuecc/MessagePack-CSharp) serializer for message serialization through the `WolverineFx.MessagePack` Nuget package.
To enable MessagePack serialization through the entire application, use:

<!-- snippet: sample_using_messagepack_for_the_default_for_the_app -->
<a id='snippet-sample_using_messagepack_for_the_default_for_the_app'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Make MessagePack the default serializer throughout this application
        opts.UseMessagePackSerialization();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Extensions/Wolverine.MessagePack.Tests/Samples.cs#L10-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_messagepack_for_the_default_for_the_app' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Likewise, you can use MessagePack on selected endpoints like this:

<!-- snippet: sample_using_messagepack_on_selected_endpoints -->
<a id='snippet-sample_using_messagepack_on_selected_endpoints'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Use MessagePack on a local queue
        opts.LocalQueue("one").UseMessagePackSerialization();

        // Use MessagePack on a listening endpoint
        opts.ListenAtPort(2223).UseMessagePackSerialization();

        // Use MessagePack on one subscriber
        opts.PublishAllMessages().ToPort(2222).UseMessagePackSerialization();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Extensions/Wolverine.MessagePack.Tests/Samples.cs#L24-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_messagepack_on_selected_endpoints' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### MemoryPack Serialization

Wolverine supports the high performance [MemoryPack](https://github.com/Cysharp/MemoryPack) serializer through the `WolverineFx.MemoryPack` Nuget package.
To enable MemoryPack serialization through the entire application, use:

<!-- snippet: sample_using_memorypack_for_the_default_for_the_app -->
<a id='snippet-sample_using_memorypack_for_the_default_for_the_app'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Make MemoryPack the default serializer throughout this application
        opts.UseMemoryPackSerialization();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Extensions/Wolverine.MemoryPack.Tests/Samples.cs#L10-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_memorypack_for_the_default_for_the_app' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Likewise, you can use MemoryPack on selected endpoints like this:

<!-- snippet: sample_using_memorypack_on_selected_endpoints -->
<a id='snippet-sample_using_memorypack_on_selected_endpoints'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Use MemoryPack on a local queue
        opts.LocalQueue("one").UseMemoryPackSerialization();

        // Use MemoryPack on a listening endpoint
        opts.ListenAtPort(2223).UseMemoryPackSerialization();

        // Use MemoryPack on one subscriber
        opts.PublishAllMessages().ToPort(2222).UseMemoryPackSerialization();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Extensions/Wolverine.MemoryPack.Tests/Samples.cs#L24-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_memorypack_on_selected_endpoints' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Protobuf Serialization

Wolverine supports Google's data interchange format [Protobuf](https://github.com/protocolbuffers/protobuf) through the `WolverineFx.Protobuf` Nuget package.
To enable Protobuf serialization through the entire application, use:

<!-- snippet: sample_using_protobuf_for_the_default_for_the_app -->
<a id='snippet-sample_using_protobuf_for_the_default_for_the_app'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Make Protobuf the default serializer throughout this application
        opts.UseProtobufSerialization();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Extensions/Wolverine.Protobuf.Tests/Samples.cs#L10-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_protobuf_for_the_default_for_the_app' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Likewise, you can use Protobuf on selected endpoints like this:

<!-- snippet: sample_using_protobuf_on_selected_endpoints -->
<a id='snippet-sample_using_protobuf_on_selected_endpoints'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Use Protobuf on a local queue
        opts.LocalQueue("one").UseProtobufSerialization();

        // Use Protobuf on a listening endpoint
        opts.ListenAtPort(2223).UseProtobufSerialization();

        // Use Protobuf on one subscriber
        opts.PublishAllMessages().ToPort(2222).UseProtobufSerialization();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Extensions/Wolverine.Protobuf.Tests/Samples.cs#L24-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_protobuf_on_selected_endpoints' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->




## Versioned Message Forwarding

If you make breaking changes to an incoming message in a later version, you can simply handle both versions of that message separately:

<!-- snippet: sample_PersonCreatedHandler -->
<a id='snippet-sample_personcreatedhandler'></a>
```cs
public class PersonCreatedHandler
{
    public static void Handle(PersonBorn person)
    {
        // do something w/ the message
    }

    public static void Handle(PersonBornV2 person)
    {
        // do something w/ the message
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L113-L128' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_personcreatedhandler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or you could use a custom `IMessageDeserializer` to read incoming messages from V1 into the new V2 message type, or you can take advantage of message forwarding
so you only need to handle one message type using the `IForwardsTo<T>` interface as shown below:

<!-- snippet: sample_IForwardsTo_PersonBornV2 -->
<a id='snippet-sample_iforwardsto_personbornv2'></a>
```cs
public class PersonBorn : IForwardsTo<PersonBornV2>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public PersonBornV2 Transform()
    {
        return new PersonBornV2
        {
            FirstName = FirstName,
            LastName = LastName,
            Birthday = new DateTime(Year, Month, Day)
        };
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L90-L111' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iforwardsto_personbornv2' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which forwards to the current message type:

<!-- snippet: sample_PersonBorn_V2 -->
<a id='snippet-sample_personborn_v2'></a>
```cs
[MessageIdentity("person-born", Version = 2)]
public class PersonBornV2
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthday { get; set; }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageVersioning.cs#L78-L88' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_personborn_v2' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Using this strategy, other systems could still send your system the original `application/vnd.person-born.v1+json` formatted
message, and on the receiving end, Wolverine would know to deserialize the Json data into the `PersonBorn` object, then call its
`Transform()` method to build out the `PersonBornV2` type that matches up with your message handler.

## "Self Serializing" Messages

::: info
This was originally built for an unusual MQTT requirement, but is going to be used extensively by Wolverine
internals as a tiny optimization
:::

This is admittedly an oddball use case for micro-optimization, but you may embed the serialization logic for a message type right into the 
message type itself through Wolverine's `ISerializable` interface as shown below:

<!-- snippet: sample_intrinsic_serialization -->
<a id='snippet-sample_intrinsic_serialization'></a>
```cs
public class SerializedMessage : ISerializable
{
    public string Name { get; set; } = "Bob Schneider";

    public byte[] Write()
    {
        return Encoding.Default.GetBytes(Name);
    }

    // You'll need at least C# 11 for static methods
    // on interfaces!
    public static object Read(byte[] bytes)
    {
        var name = Encoding.Default.GetString(bytes);
        return new SerializedMessage { Name = name };
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Serialization/intrinsic_serialization.cs#L21-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_intrinsic_serialization' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Wolverine will see the interface implementation of the message type, and automatically opt into using this "intrinsic" 
serialization. 




================================================
FILE: docs/guide/migration.md
================================================
# Migration Guide

## Key Changes in 5.0

5.0 had very few breaking changes in the public API, but some in "publinternals" types most users would never touch. The
biggest change in the internals is the replacement of the venerable [TPL DataFlow library](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) 
with the [System.Threading.Channels library](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
in every place that Wolverine uses in memory queueing. The only change this caused to the public API was the removal of
the option for direct configuration of the TPL DataFlow `ExecutionOptions`. Endpoint ordering and parallelization options
are unchanged otherwise in the public fluent interface for configuration. 

The `IntegrateWithWolverine()` syntax for ["ancillary stores"](/guide/durability/marten/ancillary-stores) changed to a [nested closure](https://martinfowler.com/dslCatalog/nestedClosure.html) syntax to be more consistent
with the syntax for the main [Marten](https://martendb.io) store. The [Wolverine managed distribution of Marten projections and subscriptions](/guide/durability/marten/distribution)
now applies to the ancillary stores as well. 

The new [Partitioned Sequential Messaging](/guide/messaging/partitioning) feature is a potentially huge step forward for
building a Wolverine system that can efficiently and resiliently handle concurrent access to sensitive resources.

The [Aggregate Handler Workflow](/guide/durability/marten/event-sourcing) feature with Marten now supports strong typed identifiers.

The declarative data access features with Marten (`[Aggregate]`, `[ReadAggregate]`, `[Entity]` or `[Document]`) can utilize
Marten batch querying for better efficiency when a handler or HTTP endpoint uses more than one declaration for data loading.

Better control over how [Wolverine generates code with respect to IoC container usage](/guide/codegen.html#wolverine-code-generation-and-ioc).

`IServiceContainer` moved to the `JasperFx` namespace.

By and large, we've *tried* to replace any API nomenclature using "master" with "main."

## Key Changes in 4.0

* Wolverine dropped all support for .NET 6/7
* The previous dependencies on Oakton, JasperFx.Core, and JasperFx.CodeGeneration were all combined into a single [JasperFx](https://github.com/jasperfx/jasperfx) library. There are shims for any method with "Oakton" in its name, but these are marked as `[Obsolete]`. You can pretty well do a find and replace for "Oakton" to "JasperFx". If your Oakton command classes live in a different project than the runnable application, add this to that project's `Properties/AssemblyInfo.cs` file:
  ```cs
  using JasperFx;

  [assembly: JasperFxAssembly]
  ```
  This attribute replaces the older Oakton assembly attribute:
  ```cs
  using Oakton;

  [assembly: OaktonCommandAssembly]
  ```
* Internally, the full "Critter Stack" is trying to use `Uri` values to identify databases when targeting multiple databases in either a modular monolith approach or with multi-tenancy
* Many of the internal dependencies like Marten or AWS SQS SDK Nugets were updated
* The signature of the Kafka `IKafkaEnvelopeMapper` changed somewhat to be more efficient in message serialization
* Wolverine now supports [multi-tenancy through separate databases for EF Core](/guide/durability/efcore/multi-tenancy)
* The Open Telemetry span names for executing a message are now the [Wolverine message type name](/guide/messages.html#message-type-name-or-alias)

## Key Changes in 3.0

The 3.0 release did not have any breaking changes to the public API, but does come with some significant internal
changes.

### Lamar Removal

::: tip
Lamar is more "forgiving" than the built in `ServiceProvider`. If after converting to Wolverine 3.0, you receive
messages from `ServiceProvider` about not being able to resolve this, that, or the other, just go back to Lamar with
the steps in this guide.
:::

The biggest change is that Wolverine is no longer directly coupled to the [Lamar IoC library](https://jasperfx.github.io/lamar) and
Wolverine will no longer automatically replace the built in `ServiceProvider` with Lamar. At this point it is theoretically
possible to use Wolverine with any IoC library that fully supports the ASP.Net Core DI conformance behavior, but Wolverine
has only been tested against the default `ServiceProvider` and Lamar IoC containers. 

Do be aware if moving to Wolverine 3.0 that Lamar is more forgiving than `ServiceProvider`, so there might be some hiccups
if you choose to forgo Lamar. See the [Configuration Guide](/guide/configuration) for more information. Lamar does still have a little more
robust support for the code generation abilities in Wolverine (Wolverine uses the IoC configuration to generate code to inline
dependency creation in a way that's more efficient than an IoC tool at runtime -- when it can).

::: tip
If you have any issues with Wolverine's code generation about your message handlers or HTTP endpoints after upgrading to Wolverine 3.0,
please open a GitHub issue with Wolverine, but just know that you can probably fall back to using Lamar as the IoC tool
to "fix" those issues with code generation planning.
:::

Wolverine 3.0 can now be bootstrapped with the `HostApplicationBuilder` or any standard .NET bootstrapping mechanism through
`IServiceCollection.AddWolverine()`. The limitation of having to use `IHostBuilder` is gone.

### Marten Integration

The Marten/Wolverine `IntegrateWithWolverine()` integration syntax changed from a *lot* of optional arguments to a single
call with a nested lambda registration like this:

<!-- snippet: sample_using_integrate_with_wolverine_with_multiple_options -->
<a id='snippet-sample_using_integrate_with_wolverine_with_multiple_options'></a>
```cs
services.AddMarten(opts =>
    {
        opts.Connection(Servers.PostgresConnectionString);
        opts.DisableNpgsqlLogging = true;
    })
    .IntegrateWithWolverine(w =>
    {
        w.MessageStorageSchemaName = "public";
        w.TransportSchemaName = "public";
    })
    .ApplyAllDatabaseChangesOnStartup();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/DuplicateMessageSending/Program.cs#L50-L64' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_integrate_with_wolverine_with_multiple_options' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

All Marten/Wolverine integration options are available by this one syntax call now, with the exception of event subscriptions.

### Wolverine.RabbitMq

The RabbitMq transport recieved a significant overhaul for 3.0.

#### RabbitMq Client v7

The RabbitMq .NET client has been updated to v7, bringing with it an internal rewrite to support async I/O and vastly improved memory usage & throughput. This version also supports OTEL out of the box.

::: warning
RabbitMq v7 is newly released. If you use another RabbitMQ wrapper/bus in your application, hold off on upgrading until it also supports v7.
:::

#### Conventional Routing Improvements
- Queue bindings can now be manually overridden on a per-message basis via `BindToExchange`, this is useful for scenarios where you wish to use conventional naming between different applications but need other exchange types apart from `FanOut`. This should make conventional routing the default usage in the majority of situations. See [Conventional Routing](/guide/messaging/transports/rabbitmq/conventional-routing) for more information.
- Conventional routing entity creation has been split between the sender and receive side. Previously the sender would generate all exchange and queue bindings, but now if the sender has no handlers for a specific message, the queues will not be created.

#### General RabbitMQ Improvements
- Added support for Headers exchange
- Queues now apply bindings instead of exchanges. This is an internal change and shouldn't result in any obvious differences for users.
- The configuration model has expanded flexibility with Queues now bindable to Exchanges, alongside the existing model of Exchanges binding to Queues.
- The previous `BindExchange()` syntax was renamed to `DeclareExchange()` to better reflect Rabbit MQ operations

### Sagas

Wolverine 3.0 added optimistic concurrency support to the stateful `Saga` support. This will potentially cause database
migrations for any Marten-backed `Saga` types as it will now require the numeric version storage.

### Leader Election

The leader election functionality in Wolverine has been largely rewritten and *should* eliminate the issues with poor 
behavior in clusters or local debugging time usage where nodes do not gracefully shut down. Internal testing has shown
a significant improvement in Wolverine's ability to detect node changes and rollover the leadership election.

### Wolverine.PostgresSql

The PostgreSQL transport option requires you to explicitly set the `transportSchema`, or Wolverine will fall through to
using `wolverine_queues` as the schema for the database backed queues. Wolverine will no longer use the envelope storage
schema for the queues.

### Wolverine.Http

For [Wolverine.Http usage](/guide/http/), the Wolverine 3.0 usage of the less capable `ServiceProvider` instead of the previously
mandated [Lamar](https://jasperfx.github.io/lamar) library necessitates the usage of this API to register necessary
services for Wolverine.HTTP in addition to adding the Wolverine endpoints:

<!-- snippet: sample_adding_http_services -->
<a id='snippet-sample_adding_http_services'></a>
```cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Necessary services for Wolverine HTTP
// And don't worry, if you forget this, Wolverine
// will assert this is missing on startup:(
builder.Services.AddWolverineHttp();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Http/WolverineWebApi/Program.cs#L35-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_adding_http_services' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Also for Wolverine.Http users, the `[Document]` attribute behavior in the Marten integration is now "required by default."

### Azure Service Bus

The Azure Service Bus will now "sanitize" any queue/subscription names to be all lower case. This may impact your usage of
conventional routing. Please report any problems with this to GitHub.

### Messaging

The behavior of `IMessageBus.InvokeAsync<T>(message)` changed in 3.0 such that the `T` response **is not also published as a 
message** at the same time when the initial message is sent with request/response semantics. Wolverine has gone back and forth
in this behavior in its life, but at this point, the Wolverine thinks that this is the least confusing behavioral rule. 

You can selectively override this behavior and tell Wolverine to publish the response as a message no matter what
by using the new 3.0 `[AlwaysPublishResponse]` attribute like this:

<!-- snippet: sample_using_AlwaysPublishResponse -->
<a id='snippet-sample_using_alwayspublishresponse'></a>
```cs
public class CreateItemCommandHandler
{
    // Using this attribute will force Wolverine to also publish the ItemCreated event even if
    // this is called by IMessageBus.InvokeAsync<ItemCreated>()
    [AlwaysPublishResponse]
    public async Task<(ItemCreated, SecondItemCreated)> Handle(CreateItemCommand command, IDocumentSession session)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = command.Name
        };

        session.Store(item);

        return (new ItemCreated(item.Id, item.Name), new SecondItemCreated(item.Id, item.Name));
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/MartenTests/Bugs/Bug_305_invoke_async_with_return_not_publishing_with_tuple_return_value.cs#L65-L86' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_alwayspublishresponse' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/runtime.md
================================================
# Runtime Architecture

::: info
Wolverine makes absolutely no differentiation between logical [events and commands](https://codeopinion.com/commands-events-whats-the-difference) within your system. To Wolverine,
everything is just a message.
:::

The two key parts of a Wolverine application are messages:

<!-- snippet: sample_DebutAccount_command -->
<a id='snippet-sample_debutaccount_command'></a>
```cs
// A "command" message
public record DebitAccount(long AccountId, decimal Amount);

// An "event" message
public record AccountOverdrawn(long AccountId);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageBusBasics.cs#L69-L77' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_debutaccount_command' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And the message handling code for the messages, which in Wolverine's case just means a function or method that accepts the message type as its first argument like so:

<!-- snippet: sample_DebitAccountHandler -->
<a id='snippet-sample_debitaccounthandler'></a>
```cs
public static class DebitAccountHandler
{
    public static void Handle(DebitAccount account)
    {
        Console.WriteLine($"I'm supposed to debit {account.Amount} from account {account.AccountId}");
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/MessageBusBasics.cs#L57-L67' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_debitaccounthandler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Invoking a Message Inline

At runtime, you can use Wolverine to invoke the message handling for a message *inline* in the current executing thread with Wolverine effectively acting as a mediator:

![Invoke Wolverine Handler](/invoke-handler.png)

It's a bit more complicated than that though, as the inline invocation looks like this simplified sequence diagram:

![Invoke a Message Inline](/invoke-message-sequence-diagram.png)

As you can hopefully see, even the inline invocation is adding some value beyond merely "mediating" between the caller
and the actual message handler by:

1. Wrapping Open Telemetry tracing and execution metrics around the execution
2. Correlating the execution in logs to the original calling activity
3. Providing some inline retry [error handling policies](/guide/handlers/error-handling) for transient errors
4. Publishing [cascading messages](/guide/handlers/cascading) from the message execution only *after* the execution succeeds as an in memory outbox


## Asynchronous Messaging

::: info
You can, of course, happily publish messages to an external queue and consume those very same messages later in the
same process.
:::

Wolverine supports asynchronous messaging through both its [local, in-process queueing](/guide/messaging/transports/local) mechanism and through external
messaging brokers like Kafka, Rabbit MQ, Azure Service Bus, or Amazon SQS. The local queueing is a valuable way to add
background processing to a system, and can even be durably backed by a database with full-blown transactional inbox/outbox
support to retain in process work across unexpected system shutdowns or restarts. What the local queue cannot do is share
work across a cluster of running nodes. In other words, you will have to use external messaging brokers to achieve any
kind of [competing consumer](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html) work sharing for better scalability. 

::: info
Wolverine listening agents all support competing consumers out of the box for work distribution across a node cluster -- unless you are purposely opting into [strictly ordered listeners](/guide/messaging/listeners.html#strictly-ordered-listeners) where only one
node is allowed to handle messages from a given queue or subscription.
:::

The other main usage of Wolverine is to send messages from your current process to another process through some kind of external transport like a Rabbit MQ/Azure Service Bus/Amazon SQS queue and
have Wolverine execute that message in another process (or back to the original process):

![Send a Message](/sending-message.png)

The internals of publishing a message are shown in this simplified sequence diagram:

![Publish a Message](/publish-message-sequence-diagram.png)

Along the way, Wolverine has to:

1. Serialize the message body
2. Route the outgoing message to the proper subscriber(s)
3. Utilize any publishing rules like "this message should be discarded after 10 seconds"
4. Map the outgoing Wolverine `Envelope` representation of the message into whatever the underlying transport (Azure Service Bus et al.) uses
5. Actually invoke the actual messaging infrastructure to send out the message

On the flip side, listening for a message follows this sequence shown for the "happy path" of receiving a message through Rabbit MQ:

![Listen for a Message](/listen-for-message-sequence-diagram.png)

During the listening process, Wolverine has to:

1. Map the incoming Rabbit MQ message to Wolverine's own `Envelope` structure
2. Determine what the actual message type is based on the `Envelope` data
3. Find the correct executor strategy for the message type
4. Deserialize the raw message data to the actual message body
5. Call the inner message executor for that message type
6. Carry out quite a bit of Open Telemetry activity tracing, report metrics, and just plain logging
7. Evaluate any errors against the error handling policies of the application or the specific message type

## Endpoint Types

::: info
Not all transports support all three types of endpoint modes, and will helpfully assert when you try to choose
an invalid option.
:::

### Inline Endpoints

Wolverine endpoints come in three basic flavors, with the first being **Inline** endpoints:

<!-- snippet: sample_using_process_inline -->
<a id='snippet-sample_using_process_inline'></a>
```cs
// Configuring a Wolverine application to listen to
// an Azure Service Bus queue with the "Inline" mode
opts.ListenToAzureServiceBusQueue(queueName, q => q.Options.AutoDeleteOnIdle = 5.Minutes()).ProcessInline();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Transports/Azure/Wolverine.AzureServiceBus.Tests/InlineSendingAndReceivingCompliance.cs#L29-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_process_inline' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

With inline endpoints, as the name implies, calling `IMessageBus.SendAsync()` immediately sends the message to the external
message broker. Likewise, messages received from an external message queue are processed inline before Wolverine acknowledges
to the message broker that the message is received.

![Inline Endpoints](/inline-endpoint.png)

In the absence of a durable inbox/outbox, using inline endpoints is "safer" in terms of guaranteed delivery. As you might 
think, using inline agents can bottle neck the message processing, but that can be alleviated by opting into parallel listeners.

### Buffered Endpoints

In the second **Buffered** option, messages are queued locally between the actual external broker and the Wolverine handlers or senders.

To opt into buffering, you use this syntax:

<!-- snippet: sample_buffered_in_memory -->
<a id='snippet-sample_buffered_in_memory'></a>
```cs
// I overrode the buffering limits just to show
// that they exist for "back pressure"
opts.ListenToAzureServiceBusQueue("incoming")
    .BufferedInMemory(new BufferingLimits(1000, 200));
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Transports/Azure/Wolverine.AzureServiceBus.Tests/DocumentationSamples.cs#L139-L146' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_buffered_in_memory' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

At runtime, you have a local [TPL Dataflow queue](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) between the Wolverine callers and the broker:

![Buffered Endpoints](/buffered-endpoint.png)

On the listening side, buffered endpoints do support [back pressure](https://www.educative.io/answers/techniques-to-exert-back-pressure-in-distributed-systems) (of sorts) where Wolverine will stop the actual message 
listener if too many messages are queued in memory to avoid chewing up your application memory. In transports like Amazon SQS that only support batched
message sending or receiving, `Buffered` is the default mode as that facilitates message batching.

`Buffered` message sending and receiving can lead to higher throughput, and should be considered for cases where messages
are ephemeral or expire and throughput is more important than delivery guarantees. The downside is that messages in the 
in memory queues can be lost in the case of the application shutting down unexpectedly -- but Wolverine tries to "drain"
the in memory queues on normal application shutdown.

### Durable Endpoints

**Durable** endpoints behave like **buffered** endpoints, but also use the [durable inbox/outbox message storage](/guide/durability/) to create much
stronger guarantees about message delivery and processing. You will need to use `Durable` endpoints in order to truly
take advantage of the persistent outbox mechanism in Wolverine. To opt into making an endpoint durable, use this syntax:

<!-- snippet: sample_durable_endpoint -->
<a id='snippet-sample_durable_endpoint'></a>
```cs
// I overrode the buffering limits just to show
// that they exist for "back pressure"
opts.ListenToAzureServiceBusQueue("incoming")
.UseDurableInbox(new BufferingLimits(1000, 200));

opts.PublishAllMessages().ToAzureServiceBusQueue("outgoing")
    .UseDurableOutbox();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Transports/Azure/Wolverine.AzureServiceBus.Tests/DocumentationSamples.cs#L236-L246' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_durable_endpoint' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or use policies to do this in one fell swoop (which may not be what you actually want, but you could do this!):

<!-- snippet: sample_all_outgoing_are_durable -->
<a id='snippet-sample_all_outgoing_are_durable'></a>
```cs
opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Transports/Azure/Wolverine.AzureServiceBus.Tests/DocumentationSamples.cs#L149-L153' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_all_outgoing_are_durable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As shown below, the `Durable` endpoint option adds an extra step to the `Buffered` behavior to add database storage of the 
incoming and outgoing messages:

![Durable Endpoints](/durable-endpoints.png)

Outgoing messages are deleted in the durable outbox upon successful sending acknowledgements from the external broker. Likewise,
incoming messages are also deleted from the durable inbox upon successful message execution.

The `Durable` endpoint option makes Wolverine's [local queueing](/guide/messaging/transports/local) robust enough to use for cases where you need 
guaranteed processing of messages, but don't want to use an external broker.

## How Wolverine Calls Your Message Handlers

![A real wolverine](/real_wolverine.jpeg)

Wolverine is a little different animal from the tools with similar features in the .NET ecosystem (pun intended:). Instead of the typical strategy of
requiring you to implement an adapter interface of some sort in *your* code, Wolverine uses [dynamically generated code](./codegen) to "weave" its internal adapter code and 
even middleware around your message handler code. 

In ideal circumstances, Wolverine is able to completely remove the runtime usage of an IoC container for even better performance. The
end result is a runtime pipeline that is able to accomplish its tasks with potentially much less performance overhead than comparable .NET frameworks 
that depend on adapter interfaces and copious runtime usage of IoC containers.

See [Code Generation in Wolverine](/guide/codegen) for much more information about this model and how it relates to the execution pipeline.

## Nodes and Agents

![Nodes and Agents](/nodes-and-agents.png)

Wolverine has some ability to distribute "sticky" or stateful work across running nodes in your application. To do so, 
Wolverine tracks the running "nodes" (just means an executing instance of your Wolverine application) and elects a 
single leader to distribute and assign "agents" to the running "nodes". Wolverine has built in health monitoring that
can detect when any node is offline to redistribute working agents to other nodes. Wolverine is also able to "fail over" the
leader assignment to a different node if the original leader is determined to be down. Likewise, Wolverine will redistribute
running agent assignments when new nodes are brought online.

::: info
You will have to have some kind of durable message storage configured for your application for the leader election
and agent assignments to function.
:::

The stateful, running "agents" are exposed through an `IAgent`
interface like so:

<!-- snippet: sample_IAgent -->
<a id='snippet-sample_iagent'></a>
```cs
/// <summary>
///     Models a constantly running background process within a Wolverine
///     node cluster
/// </summary>
public interface IAgent : IHostedService // Standard .NET interface for background services
{
    /// <summary>
    ///     Unique identification for this agent within the Wolverine system
    /// </summary>
    Uri Uri { get; }
    
    // Not really used for anything real *yet*, but 
    // hopefully becomes something useful for CritterWatch
    // health monitoring
    AgentStatus Status { get; }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Runtime/Agents/IAgent.cs#L9-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iagent' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_iagent-1'></a>
```cs
/// <summary>
///     Models a constantly running background process within a Wolverine
///     node cluster
/// </summary>
public interface IAgent : IHostedService // Standard .NET interface for background services
{
    /// <summary>
    ///     Unique identification for this agent within the Wolverine system
    /// </summary>
    Uri Uri { get; }
    
    // Not really used for anything real *yet*, but 
    // hopefully becomes something useful for CritterWatch
    // health monitoring
    AgentStatus Status { get; }
}

public class CompositeAgent : IAgent
{
    private readonly List<IAgent> _agents;
    public Uri Uri { get; }

    public CompositeAgent(Uri uri, IEnumerable<IAgent> agents)
    {
        Uri = uri;
        _agents = agents.ToList();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var agent in _agents)
        {
            await agent.StartAsync(cancellationToken);
        }

        Status = AgentStatus.Running;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var agent in _agents)
        {
            await agent.StopAsync(cancellationToken);
        }

        Status = AgentStatus.Running ;
    }

    public AgentStatus Status { get; private set; } = AgentStatus.Stopped;
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Runtime/Agents/IAgent.cs#L7-L64' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iagent-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

With related groups of agents built and assigned by IoC-registered implementations of this interface:

<!-- snippet: sample_IAgentFamily -->
<a id='snippet-sample_iagentfamily'></a>
```cs
/// <summary>
///     Pluggable model for managing the assignment and execution of stateful, "sticky"
///     background agents on the various nodes of a running Wolverine cluster
/// </summary>
public interface IAgentFamily
{
    /// <summary>
    ///     Uri scheme for this family of agents
    /// </summary>
    string Scheme { get; }

    /// <summary>
    ///     List of all the possible agents by their identity for this family of agents
    /// </summary>
    /// <returns></returns>
    ValueTask<IReadOnlyList<Uri>> AllKnownAgentsAsync();

    /// <summary>
    ///     Create or resolve the agent for this family
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="wolverineRuntime"></param>
    /// <returns></returns>
    ValueTask<IAgent> BuildAgentAsync(Uri uri, IWolverineRuntime wolverineRuntime);

    /// <summary>
    ///     All supported agent uris by this node instance
    /// </summary>
    /// <returns></returns>
    ValueTask<IReadOnlyList<Uri>> SupportedAgentsAsync();

    /// <summary>
    ///     Assign agents to the currently running nodes when new nodes are detected or existing
    ///     nodes are deactivated
    /// </summary>
    /// <param name="assignments"></param>
    /// <returns></returns>
    ValueTask EvaluateAssignmentsAsync(AssignmentGrid assignments);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Runtime/Agents/IAgentFamily.cs#L16-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iagentfamily' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Built in examples of the agent and agent family are:

* Wolverine's built-in durability agent to recover orphaned messages from nodes that are detected to be offline, with one agent per tenant database
* Wolverine uses the agent assignment for "exclusive" message listeners like the strictly ordered listener option
* The integrated Marten projection and subscription load distribution

## IoC Container Integration

::: info
Wolverine has been tested with both the built in `ServiceProvider` and [Lamar](https://jasperfx.github.io/lamar), which was originally built
specifically to support what ended up becoming Wolverine. The previous limitation to only supporting Lamar was lifted in Wolverine 3.0.
:::

Wolverine is a significantly different animal than other .NET frameworks, and uses the IoC container quite differently than most
.NET application frameworks. For the most part, Wolverine is looking at the IoC container registrations and trying to generate code
to mimic the IoC behavior in the message handler and HTTP endpoint adapters that Wolverine generates internally. The benefits of this model are:

* The pre-generated code can tell you a lot about how Wolverine is handling your code, including any registered middleware
* The fastest IoC container is no IoC container
* Less conditional logic at runtime 
* Much slimmer exception stack traces when things inevitably go wrong. Wolverine's predecessor tool ([FubuMVC](https://fubumvc.github.io)) use nested objects created on every request or message for its middleware strategy, and the exception messages coming out of handler code could be *epic* with a lot of middleware active.

The downside is that Wolverine does not play well with the kind of runtime IoC tricks
other frameworks rely on for passing state. For example, because Wolverine.HTTP does not use the ASP.Net Core request services
to build endpoint types and its dependencies at runtime, it's a little clumsier to pass state from ASP.Net Core middleware
written into scoped IoC services, with custom multi-tenancy approaches being the usual cause of this. Wolverine certainly has its
own multi-tenancy support, and we don't think this is really a serious problem for most usages, but it has caused friction for
some Wolverine users converting from other frameworks.



================================================
FILE: docs/guide/samples.md
================================================
# Sample Projects

There are several sample projects in the Wolverine codebase showing off bits and pieces of Wolverine functionality:

| Project                                                                                                                                      | Description                                                                                              |
|----------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------|
| [Quickstart](https://github.com/JasperFx/wolverine/tree/main/src/Samples/Quickstart)                                                         | The sample application in the quick start tutorial                                                       |
| [CQRSWithMarten](https://github.com/JasperFx/wolverine/tree/main/src/Samples/CQRSWithMarten)                                                 | Shows off the event sourcing integration between [Marten](https://martendb.io) and Wolverine             |
| [CommandBus](https://github.com/JasperFx/wolverine/tree/main/src/Samples/CommandBus)                                                         | Wolverine as an in memory "command bus" for asynchronous processing                                      |
| [InMemoryMediator](https://github.com/JasperFx/wolverine/tree/main/src/Samples/InMemoryMediator)                                             | Wolverine with EF Core and Sql Server as a mediator inside an ASP.Net Core service                       |
| [OptimizedArtifactWorkflowSample](https://github.com/JasperFx/wolverine/tree/main/src/Samples/OptimizedArtifactWorkflowSample)               | Using Wolverine's optimized workflow for pre-generating handler types                                    |
| [OrderSagaSample](https://github.com/JasperFx/wolverine/tree/main/src/Samples/OrderSagaSample)                                               | Stateful sagas with Marten                                                                               |
| [WebApiWithMarten](https://github.com/JasperFx/wolverine/tree/main/src/Samples/WebApiWithMarten)                                             | Using Marten with Wolverine for ASP.Net Core web services                                                |
| [ItemService](https://github.com/JasperFx/wolverine/tree/main/src/Samples/EFCoreSample/ItemService)                                          | EF Core, Sql Server, and Wolverine.Http to integrate the Wolverine inbox/outbox                          |
| [AppWithMiddleware](https://github.com/JasperFx/wolverine/tree/main/src/Samples/Middleware/AppWithMiddleware)                                | Building middleware for Wolverine handlers                                                               |
| [PingPong](https://github.com/JasperFx/wolverine/tree/main/src/Samples/PingPong)                                                             | A classic "ping/pong" sample of sending messages between two Wolverine processes using the TCP transport |
| [PingPongWithRabbitMq](https://github.com/JasperFx/wolverine/tree/main/src/Samples/PingPongWithRabbitMq)                                     | Another "ping/pong" sample, but this time using Rabbit MQ                                                |
| [TodoWebService](https://github.com/JasperFx/wolverine/tree/main/src/Samples/TodoWebService/TodoWebService)                                  | Using Marten, Wolverine, and Wolverine.Http to build a simple ASP.Net Core service                       | 
| [MultiTenantedTodoWebService](https://github.com/JasperFx/wolverine/tree/main/src/Samples/MultiTenantedTodoService/MultiTenantedTodoService) | Same as above, but this time with separate databases for each tenant |
| [IncidentService](https://github.com/jasperfx/wolverine/tree/main/src/Samples/IncidentService)                                               | Use the full "Critter Stack" to build a CQRS architcture with event sourcing |



================================================
FILE: docs/guide/serverless.md
================================================
# Wolverine and Serverless

::: tip
No telling when this would happen, but there is an "ultra efficient" serverless model planned for Wolverine
that will lean even heavier into code generation as a way to optimize its usage within serverless functions. Track that [forthcoming
work on GitHub](https://github.com/JasperFx/wolverine/issues/34).
:::

Wolverine was very much originally envisioned for usage in long running processes, and as such, wasn't initially well suited to
serverless technologies like [Azure Functions](https://azure.microsoft.com/en-us/products/functions) or [AWS Lambda functions](https://aws.amazon.com/pm/lambda).

If you're choosing to use Wolverine HTTP endpoints or message handling as part of a serverless function, we have three
main suggestions about making Wolverine be more successful:

1. Make any outgoing [message endpoints](/guide/runtime.html#endpoint-types) be *Inline* so that messages are sent immediately
2. Utilize the new *Serverless* optimized mode
3. Absolutely take advantage of [pre-generated types]() to cut down the all important cold start problem with serverless functions

## Serverless Mode

::: tip
Wolverine's [Transactional Inbox/Outbox](/guide/durability/) is very unsuitable for usage within serverless functions, so you'll definitely
want to disable it through the mode shown below
:::

First off, let's say that you want to use the transactional
middleware for either Marten or EF Core within your serverless functions. That's all good, but you will want to turn off
all of Wolverine's transactional inbox/outbox functionality with this setting that was added in 1.10.0:

<!-- snippet: sample_configuring_the_serverless_mode -->
<a id='snippet-sample_configuring_the_serverless_mode'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.Services.AddMarten("some connection string")

            // This adds quite a bit of middleware for
            // Marten
            .IntegrateWithWolverine();

        // You want this maybe!
        opts.Policies.AutoApplyTransactions();

        // But wait! Optimize Wolverine for usage within Serverless
        // and turn off the heavy duty, background processes
        // for the transactional inbox/outbox
        opts.Durability.Mode = DurabilityMode.Serverless;
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/DurabilityModes.cs#L12-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_the_serverless_mode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Pre-Generate All Types

The runtime code generation that Wolverine does comes with a potentially non-trivial "cold start" problem with its first
usage. In serverless architectures, that's probably intolerable. With Wolverine, you can bypass that cold start problem
by opting into [pre-generated types](/guide/codegen.html#generating-code-ahead-of-time).

## Use Inline Endpoints

If you are using Wolverine to send cascading messages from handlers in serverless functions, you will want to use
*Inline* endpoints where the messages are sent immediately without any background processing as would be normal with *Buffered* or *Durable*
endpoints:

<!-- snippet: sample_usage_of_send_inline -->
<a id='snippet-sample_usage_of_send_inline'></a>
```cs
.UseWolverine(opts =>
{
    opts.UseRabbitMq().AutoProvision().AutoPurgeOnStartup();
    opts
        .PublishAllMessages()
        .ToRabbitQueue(queueName)

        // This option is important inside of Serverless functions
        .SendInline();
})
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Transports/RabbitMQ/Wolverine.RabbitMQ.Tests/Bugs/Bug_189_fails_if_there_are_many_messages_in_queue_on_startup.cs#L20-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_usage_of_send_inline' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/testing.md
================================================
# Test Automation Support

The Wolverine team absolutely believes in Test Driven Development and the importance of strong test automation strategies as a key part of sustainable development. To that end,
Wolverine's conceptual design from the very beginning (Wolverine started as "Jasper" in 2015!) has been to maximize testability by trying
to decouple application code from framework or other infrastructure concerns.

See Jeremy's blog post [How Wolverine allows for easier testing](https://jeremydmiller.com/2022/12/13/how-wolverine-allows-for-easier-testing/) for an introduction to unit testing Wolverine message handlers.

Also see [Wolverine Best Practices](/introduction/best-practices) for other helpful tips.

And this:

@[youtube](ODSAGAllsxw)

## Integration Testing with Tracked Sessions

::: tip
This is the recommended approach for integration testing against Wolverine message handlers
if there are any outgoing messages or asynchronous behavior as a result of the messages being
handled in your test scenario.
:::

::: info
As of Wolverine 3.13, the same extension methods shown here are available off of `IServiceProvider`
in addition to the original support off of `IHost` if you happen to be writing integration tests
by spinning up just an IoC container and not the full `IHost` in your test harnesses.
:::

So far we've been mostly focused on unit testing Wolverine handler methods individually with
unit tests without any direct coupling to infrastructure. Great, that's a great start,
but you're eventually going to also need some integration tests, and invoking or publishing messages
is a very logical entry point for integration testing.

First, why integration testing with Wolverine?

1. Wolverine is probably most effective when you're heavily leveraging middleware or Wolverine conventions, and only an integration test is really going to get through the entire "stack"
2. You may frequently want to test the interaction between your application code and infrastructure concerns like databases
3. Handling messages will frequently spawn other messages that will be executed in other threads or other processes, and you'll frequently want to write bigger tests that span across messages

::: tip
I'm not getting into it here, but remember that `IHost` is relatively
expensive to build, so you'll probably want it cached between
tests. Or at least be aware that it's expensive.
:::

This sample was taken from [an introductory blog post](https://jeremydmiller.com/2022/12/12/introducing-wolverine-for-effective-server-side-net-development/) that may give you some additional context for what's happening here.

Going back to our sample message handler for the `DebitAccount` in the previous sections,
let's say that we want an integration test that spans the middleware that looks up the `Account` data,
the Fluent Validation middleware, [Marten](https://martendb.io) usage, and even across to any cascading
messages that are also handled in process as a result of the original message. One of the big challenges
with automated testing against asynchronous processing is *knowing* when the "action" part of the "arrange/act/assert"
phase of the test is complete and it's safe to start making assertions. Anyone who has had the misfortune
to work with complicated Selenium test suites is very aware of this challenge.

Not to fear though, Wolverine comes out of the box with the concept of "tracked sessions" that you can use
to write predictable and reliable integration tests.

::: warning
I'm omitting the code necessary to set up system state first just to concentrate on
the Wolverine mechanics here.
:::

To start with tracked sessions, let's assume that you have an `IHost` for your Wolverine
application in your testing harness. Assuming you do, you can start a tracked session using
the `IHost.InvokeMessageAndWaitAsync()` extension method in Wolverine like this:

<!-- snippet: sample_using_tracked_session -->
<a id='snippet-sample_using_tracked_session'></a>
```cs
public async Task using_tracked_sessions()
{
    // The point here is just that you somehow have
    // an IHost for your application
    using var host = await Host.CreateDefaultBuilder()
        .UseWolverine().StartAsync();

    var debitAccount = new DebitAccount(111, 300);
    var session = await host.InvokeMessageAndWaitAsync(debitAccount);

    var overdrawn = session.Sent.SingleMessage<AccountOverdrawn>();
    overdrawn.AccountId.ShouldBe(debitAccount.AccountId);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/TestingSupportSamples.cs#L122-L138' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_tracked_session' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The tracked session mechanism utilizes Wolverine's internal instrumentation to "know" when all the outstanding
work in the system is complete. In this case, if the `AccountOverdrawn` message spawned from `DebitAccount`
is handled locally, the `InvokeMessageAndWaitAsync()` call will not return until the other messages
that are routed locally are finished processing or the test times out. The tracked session will also throw
an `AggregateException` with any exceptions encountered by any message being handled within the activity
that is tracked.

Note that you'll probably *mostly* *invoke* messages in these tests, but there are additional extension
methods on `IHost` for other `IMessageBus` operations.

:::info
The Tracked Session includes only messages sent, published, or scheduled during the tracked session.
Messages sent before the tracked session are not included in the tracked session.
:::

Finally, there are some more advanced options in tracked sessions you may find useful as
shown below:

<!-- snippet: sample_advanced_tracked_session_usage -->
<a id='snippet-sample_advanced_tracked_session_usage'></a>
```cs
public async Task using_tracked_sessions_advanced(IHost otherWolverineSystem)
{
    // The point here is just that you somehow have
    // an IHost for your application
    using var host = await Host.CreateDefaultBuilder()
        .UseWolverine().StartAsync();

    var debitAccount = new DebitAccount(111, 300);
    var session = await host

        // Start defining a tracked session
        .TrackActivity()

        // Override the timeout period for longer tests
        .Timeout(1.Minutes())

        // Be careful with this one! This makes Wolverine wait on some indication
        // that messages sent externally are completed
        .IncludeExternalTransports()

        // Make the tracked session span across an IHost for another process
        // May not be super useful to the average user, but it's been crucial
        // to test Wolverine itself
        .AlsoTrack(otherWolverineSystem)

        // This is actually helpful if you are testing for error handling
        // functionality in your system
        .DoNotAssertOnExceptionsDetected()
        
        // Hey, just in case failure acks are getting into your testing session
        // and you do not care for the tests, tell Wolverine to ignore them
        .IgnoreFailureAcks()

        // Again, this is testing against processes, with another IHost
        .WaitForMessageToBeReceivedAt<LowBalanceDetected>(otherWolverineSystem)
        
        // Wolverine does this automatically, but it's sometimes
        // helpful to tell Wolverine to not track certain message
        // types during testing. Especially messages originating from
        // some kind of polling operation
        .IgnoreMessageType<IAgentCommand>()
        
        // Another option
        .IgnoreMessagesMatchingType(type => type.CanBeCastTo<IAgentCommand>())

        // There are many other options as well
        .InvokeMessageAndWaitAsync(debitAccount);

    var overdrawn = session.Sent.SingleMessage<AccountOverdrawn>();
    overdrawn.AccountId.ShouldBe(debitAccount.AccountId);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/TestingSupportSamples.cs#L140-L194' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_advanced_tracked_session_usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The samples shown above inlcude `Sent` message records, but there are more properties available in the `TrackedSession` object.
In accordance with the `MessageEventType` enum, you can access these properties on the `TrackedSession` object:

<!-- snippet: sample_record_collections -->
<a id='snippet-sample_record_collections'></a>
```cs
public enum MessageEventType
{
    Received,
    Sent,
    ExecutionStarted,
    ExecutionFinished,
    MessageSucceeded,
    MessageFailed,
    NoHandlers,
    NoRoutes,
    MovedToErrorQueue,
    Requeued,
    Scheduled,
    Discarded
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Tracking/MessageEventType.cs#L3-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_record_collections' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Let's consider we're testing a Wolverine application which publishes a message, when a change to a watched folder is detected. The part we want to test is that a message is actually published when a file is added to the watched folder. We can use the `TrackActivity` method to start a tracked session and then use the `ExecuteAndWaitAsync` method to wait for the message to be published when the file change has happened.

<!-- snippet: sample_send_message_on_file_change -->
<a id='snippet-sample_send_message_on_file_change'></a>
```cs
public record FileAdded(string FileName);

public class FileAddedHandler
{
    public Task Handle(
        FileAdded message
    ) =>
        Task.CompletedTask;
}

public class RandomFileChange
{
    private readonly IMessageBus _messageBus;

    public RandomFileChange(
        IMessageBus messageBus
    ) => _messageBus = messageBus;

    public async Task SimulateRandomFileChange()
    {
        // Delay task with a random number of milliseconds
        // Here would be your FileSystemWatcher / IFileProvider
        await Task.Delay(
            TimeSpan.FromMilliseconds(
                new Random().Next(100, 1000)
            )
        );
        var randomFileName = Path.GetRandomFileName();
        await _messageBus.SendAsync(new FileAdded(randomFileName));
    }
}

public class When_message_is_sent : IAsyncLifetime
{
    private IHost _host;

    public async Task InitializeAsync()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureServices(
            services => { services.AddSingleton<RandomFileChange>(); }
        );
        hostBuilder.UseWolverine();

        _host = await hostBuilder.StartAsync();
    }
    
    [Fact]
    public async Task should_be_in_session_using_service_provider()
    {
        var randomFileChange = _host.Services.GetRequiredService<RandomFileChange>();

        var session = await _host.Services
            .TrackActivity()
            .Timeout(2.Seconds())
            .ExecuteAndWaitAsync(
                (Func<IMessageContext, Task>)(
                    async (
                        _
                    ) => await randomFileChange.SimulateRandomFileChange()
                )
            );

        session
            .Sent
            .AllMessages()
            .Count()
            .ShouldBe(1);
        
        session
            .Sent
            .AllMessages()
            .First()
            .ShouldBeOfType<FileAdded>();
    }

    [Fact]
    public async Task should_be_in_session()
    {
        var randomFileChange = _host.Services.GetRequiredService<RandomFileChange>();

        var session = await _host
            .TrackActivity()
            .Timeout(2.Seconds())
            .ExecuteAndWaitAsync(
                (Func<IMessageContext, Task>)(
                    async (
                        _
                    ) => await randomFileChange.SimulateRandomFileChange()
                )
            );

        session
            .Sent
            .AllMessages()
            .Count()
            .ShouldBe(1);
        
        session
            .Sent
            .AllMessages()
            .First()
            .ShouldBeOfType<FileAdded>();
    }

    public async Task DisposeAsync() => await _host.StopAsync();
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/TestingSupportSamples.cs#L218-L326' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_send_message_on_file_change' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As you can see, we just have to start our application, attach a tracked session to it, and then wait for the message to be published. This way, we can test the whole process of the application, from the file change to the message publication, in a single test.

## Dealing with Scheduled Messages  <Badge type="tip" text="4.12" />

As I'm sure you can imagine, [scheduled local execution](/guide/messaging/transports/local.html#scheduling-local-execution) and [scheduled message delivery](/guide/messaging/message-bus.html#scheduling-message-delivery-or-execution)
can easily be confusing for testing and have occasionally caused trouble for Wolverine users using the tracked session functionality. At this point,
Wolverine now tracks any scheduled messages a little separately under an `ITrackedSession.Scheduled` collection, and any message that is
scheduled for later execution or delivery is automatically interpreted as "complete" in the tracked session.

You can also force the "tracked session" to immediately "replay" any scheduled messages tracked in the original session by:

1. Invoking any messages that were scheduled for local execution
2. Sending any messages that were scheduled for delivery to the original destination

and returning a brand new tracked session for the "replay."

Here's an example from our test suite. First though, here's the message handlers in question (remember, this is rigged up 
for testing):

<!-- snippet: sample_handlers_for_trigger_scheduled_message -->
<a id='snippet-sample_handlers_for_trigger_scheduled_message'></a>
```cs
public static DeliveryMessage<ScheduledMessage> Handle(TriggerScheduledMessage message)
{
    // This causes a message to be scheduled for delivery in 5 minutes from now
    return new ScheduledMessage(message.Text).DelayedFor(5.Minutes());
}

public static void Handle(ScheduledMessage message) => Debug.WriteLine("Got scheduled message");
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/SlowTests/tracked_session_mechanics.cs#L153-L163' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_handlers_for_trigger_scheduled_message' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And the test that exercises this functionality:

<!-- snippet: sample_dealing_with_locally_scheduled_messages -->
<a id='snippet-sample_dealing_with_locally_scheduled_messages'></a>
```cs
// In this case we're just executing everything in memory
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.PersistMessagesWithPostgresql(Servers.PostgresConnectionString, "wolverine");
        opts.Policies.UseDurableInboxOnAllListeners();
    }).StartAsync();

// Should finish cleanly
var tracked = await host.SendMessageAndWaitAsync(new TriggerScheduledMessage("Chiefs"));

// Here's how you can query against the messages that were detected to be scheduled
tracked.Scheduled.SingleMessage<ScheduledMessage>()
    .Text.ShouldBe("Chiefs");

// This API will try to immediately play any scheduled messages immediately
var replayed = await tracked.PlayScheduledMessagesAsync(10.Seconds());
replayed.Executed.SingleMessage<ScheduledMessage>().Text.ShouldBe("Chiefs");
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/SlowTests/tracked_session_mechanics.cs#L71-L92' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_dealing_with_locally_scheduled_messages' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And now, a slightly more complicated test that tests the replay of a message scheduled
to go to a completely separate application:

<!-- snippet: sample_handling_scheduled_delivery_to_external_transport -->
<a id='snippet-sample_handling_scheduled_delivery_to_external_transport'></a>
```cs
var port1 = PortFinder.GetAvailablePort();
var port2 = PortFinder.GetAvailablePort();

using var sender = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.PublishMessage<ScheduledMessage>().ToPort(port2);
        opts.ListenAtPort(port1);
    }).StartAsync();

using var receiver = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.ListenAtPort(port2);
    }).StartAsync();

// Should finish cleanly
var tracked = await sender
    .TrackActivity()
    .IncludeExternalTransports()
    .AlsoTrack(receiver)
    .InvokeMessageAndWaitAsync(new TriggerScheduledMessage("Broncos"));

tracked.Scheduled.SingleMessage<ScheduledMessage>()
    .Text.ShouldBe("Broncos");

var replayed = await tracked.PlayScheduledMessagesAsync(10.Seconds());
replayed.Executed.SingleMessage<ScheduledMessage>().Text.ShouldBe("Broncos");
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/SlowTests/tracked_session_mechanics.cs#L98-L129' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_handling_scheduled_delivery_to_external_transport' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Extension Methods for Outgoing Messages

Your Wolverine message handlers will often have some need to publish, send, or schedule other messages as part of their work. At the unit 
test level you'll frequently want to validate the *decision* about whether or not to send a message. To aid
in those assertions, Wolverine out of the box includes some testing helper extension methods on `IEnumerable<object>`
inspired by the [Shouldly](https://github.com/shouldly/shouldly) project.

For an example, let's look at this message handler for applying a debit to a bank account that
will use [cascading messages](/guide/handlers/cascading) to raise a variable number of additional messages:

<!-- snippet: sample_AccountHandler_for_testing_examples -->
<a id='snippet-sample_accounthandler_for_testing_examples'></a>
```cs
[Transactional]
public static IEnumerable<object> Handle(
    DebitAccount command,
    Account account,
    IDocumentSession session)
{
    account.Balance -= command.Amount;

    // This just marks the account as changed, but
    // doesn't actually commit changes to the database
    // yet. That actually matters as I hopefully explain
    session.Store(account);

    // Conditionally trigger other, cascading messages
    if (account.Balance > 0 && account.Balance < account.MinimumThreshold)
    {
        yield return new LowBalanceDetected(account.Id)
            .WithDeliveryOptions(new DeliveryOptions { ScheduleDelay = 1.Hours() });
    }
    else if (account.Balance < 0)
    {
        yield return new AccountOverdrawn(account.Id);

        // Give the customer 10 days to deal with the overdrawn account
        yield return new EnforceAccountOverdrawnDeadline(account.Id);
    }

    yield return new AccountUpdated(account.Id, account.Balance);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/TestingSupportSamples.cs#L43-L75' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_accounthandler_for_testing_examples' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The testing extensions can be seen in action by the following test:

<!-- snippet: sample_handle_a_debit_that_makes_the_account_have_a_low_balance -->
<a id='snippet-sample_handle_a_debit_that_makes_the_account_have_a_low_balance'></a>
```cs
[Fact]
public void handle_a_debit_that_makes_the_account_have_a_low_balance()
{
    var account = new Account
    {
        Balance = 1000,
        MinimumThreshold = 200,
        Id = 1111
    };

    // Let's otherwise ignore this for now, but this is using NSubstitute
    var session = Substitute.For<IDocumentSession>();

    var message = new DebitAccount(account.Id, 801);
    var messages = AccountHandler.Handle(message, account, session).ToList();

    // Now, verify that the only the expected messages are published:

    // One message of type AccountUpdated
    messages
        .ShouldHaveMessageOfType<AccountUpdated>()
        .AccountId.ShouldBe(account.Id);

    // You can optionally assert against DeliveryOptions
    messages
        .ShouldHaveMessageOfType<LowBalanceDetected>(delivery =>
        {
            delivery.ScheduleDelay.Value.ShouldNotBe(TimeSpan.Zero);
        })
        .AccountId.ShouldBe(account.Id);

    // Assert that there are no messages of type AccountOverdrawn
    messages.ShouldHaveNoMessageOfType<AccountOverdrawn>();
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/TestingSupportSamples.cs#L80-L117' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_handle_a_debit_that_makes_the_account_have_a_low_balance' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The supported extension methods so far are in the [TestingExtensions](https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/TestingExtensions.cs) class.

As we'll see in the next section, you can also find a matching `Envelope` for a message type.

::: tip
I'd personally organize the testing against that handler with a context/specification pattern, but I just wanted to show the extension methods here.
:::


## TestMessageContext

::: tip
This testing mechanism is admittedly just a copy of the test support in older messaging frameworks in 
.NET. It's only useful as an argument passed into a handler method. We recommend using the "Tracked Session"
approach instead.
:::

In the section above we used cascading messages, but since there are some use cases -- or maybe even just
user preference -- that would lead you to directly use `IMessageContext` to send additional messages
from a message handler, Wolverine comes with the `TestMessageContext` class that can be used as a 
[test double spy](https://martinfowler.com/bliki/TestDouble.html) within unit tests.

Here's a different version of the message handler from the previous section, but this time using `IMessageContext`
directly:

<!-- snippet: sample_DebitAccountHandler_that_uses_IMessageContext -->
<a id='snippet-sample_debitaccounthandler_that_uses_imessagecontext'></a>
```cs
[Transactional]
public static async Task Handle(
    DebitAccount command,
    Account account,
    IDocumentSession session,
    IMessageContext messaging)
{
    account.Balance -= command.Amount;

    // This just marks the account as changed, but
    // doesn't actually commit changes to the database
    // yet. That actually matters as I hopefully explain
    session.Store(account);

    // Conditionally trigger other, cascading messages
    if (account.Balance > 0 && account.Balance < account.MinimumThreshold)
    {
        await messaging.SendAsync(new LowBalanceDetected(account.Id));
    }
    else if (account.Balance < 0)
    {
        await messaging.SendAsync(new AccountOverdrawn(account.Id), new DeliveryOptions{DeliverWithin = 1.Hours()});

        // Give the customer 10 days to deal with the overdrawn account
        await messaging.ScheduleAsync(new EnforceAccountOverdrawnDeadline(account.Id), 10.Days());
    }

    // "messaging" is a Wolverine IMessageContext or IMessageBus service
    // Do the deliver within rule on individual messages
    await messaging.SendAsync(new AccountUpdated(account.Id, account.Balance),
        new DeliveryOptions { DeliverWithin = 5.Seconds() });
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Middleware/AppWithMiddleware/Account.cs#L126-L161' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_debitaccounthandler_that_uses_imessagecontext' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To test this handler, we can use `TestMessageContext` as a stand in to just record
the outgoing messages and even let us do some assertions on exactly *how* the messages
were published. I'm using [xUnit.Net](https://xunit.net/) here, but this is certainly usable from other
test harness tools:

<!-- snippet: sample_when_the_account_is_overdrawn -->
<a id='snippet-sample_when_the_account_is_overdrawn'></a>
```cs
public class when_the_account_is_overdrawn : IAsyncLifetime
{
    private readonly Account theAccount = new Account
    {
        Balance = 1000,
        MinimumThreshold = 100,
        Id = Guid.NewGuid()
    };

    private readonly TestMessageContext theContext = new TestMessageContext();

    // I happen to like NSubstitute for mocking or dynamic stubs
    private readonly IDocumentSession theDocumentSession = Substitute.For<IDocumentSession>();

    public async Task InitializeAsync()
    {
        var command = new DebitAccount(theAccount.Id, 1200);
        await DebitAccountHandler.Handle(command, theAccount, theDocumentSession, theContext);
    }

    [Fact]
    public void the_account_balance_should_be_negative()
    {
        theAccount.Balance.ShouldBe(-200);
    }

    [Fact]
    public void raises_an_account_overdrawn_message()
    {
        // ShouldHaveMessageOfType() is an extension method in
        // Wolverine itself to facilitate unit testing assertions like this
        theContext.Sent.ShouldHaveMessageOfType<AccountOverdrawn>()
            .AccountId.ShouldBe(theAccount.Id);
    }

    [Fact]
    public void raises_an_overdrawn_deadline_message_in_10_days()
    {
        theContext.ScheduledMessages()
            // Find the wrapping envelope for this message type,
            // then we can chain assertions against the wrapping Envelope
            .ShouldHaveEnvelopeForMessageType<EnforceAccountOverdrawnDeadline>()
            .ScheduleDelay.ShouldBe(10.Days());
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Middleware/AppWithMiddleware.Tests/try_out_the_middleware.cs#L95-L148' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_when_the_account_is_overdrawn' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `TestMessageContext` mostly just collects an array of objects that are sent, published, or scheduled. The
same extension methods explained in the previous section can be used to verify the outgoing messages
and even *how* they were published.

As of Wolverine 1.8, `TestMessageContext` also supports limited expectations for request and reply using `IMessageBus.InvokeAsync<T>()`
as shown below:

<!-- snippet: sample_using_invoke_with_expected_response_with_test_message_context -->
<a id='snippet-sample_using_invoke_with_expected_response_with_test_message_context'></a>
```cs
var spy = new TestMessageContext();
var context = (IMessageContext)spy;

// Set up an expected response for a message
spy.WhenInvokedMessageOf<NumberRequest>()
    .RespondWith(new NumberResponse(12));

// Used for:
var response1 = await context.InvokeAsync<NumberResponse>(new NumberRequest(4, 5));

// Set up an expected response with a matching filter
spy.WhenInvokedMessageOf<NumberRequest>(x => x.X == 4)
    .RespondWith(new NumberResponse(12));

// Set up an expected response for a message to an explicit destination Uri
spy.WhenInvokedMessageOf<NumberRequest>(destination:new Uri("rabbitmq://queue/incoming"))
    .RespondWith(new NumberResponse(12));

// Used to set up:
var response2 = await context.EndpointFor(new Uri("rabbitmq://queue/incoming"))
    .InvokeAsync<NumberResponse>(new NumberRequest(5, 6));

// Set up an expected response for a message to a named endpoint
spy.WhenInvokedMessageOf<NumberRequest>(endpointName:"incoming")
    .RespondWith(new NumberResponse(12));

// Used to set up:
var response3 = await context.EndpointFor("incoming")
    .InvokeAsync<NumberResponse>(new NumberRequest(5, 6));
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/TestMessageContextTests.cs#L323-L355' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_invoke_with_expected_response_with_test_message_context' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Stubbing All External Transports

::: tip
In all cases here, Wolverine is disabling all external listeners, stubbing all outgoing
subscriber endpoints, and **not** making any connection to external brokers.
:::

Unlike some older .NET messaging tools, Wolverine comes out of the box
with its in-memory "mediator" functionality that allows you to directly
invoke any possible message handler in the system on demand without
any explicit configuration. Great, and that means that there's value
in just spinning up the application as is and executing locally -- but what
about any external transport dependencies that may be very inconvenient
to utilize in automated tests?

To that end, Wolverine allows you to completely disable all external
transports including the built in TCP transport. There's a couple different ways
to go about it. The simplest conceptual approach is to leverage the .NET environment
name like this:

<!-- snippet: sample_conditionally_disable_transports -->
<a id='snippet-sample_conditionally_disable_transports'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    // Other configuration...

    // IF the environment is "Testing", turn off all external transports
    if (builder.Environment.IsDevelopment())
    {
        opts.StubAllExternalTransports();
    }
});

using var host = builder.Build();
await host.StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/TestingSupportSamples.cs#L20-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_conditionally_disable_transports' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

I'm not necessarily comfortable with a lot of conditional hosting setup all the time,
so there's another option to use the `IServiceCollection.DisableAllExternalWolverineTransports()`
extension method as shown below:

<!-- snippet: sample_disabling_external_transports -->
<a id='snippet-sample_disabling_external_transports'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // do whatever you need to configure Wolverine
    })

    // Override the Wolverine configuration to disable all
    // external transports, broker connectivity, and incoming/outgoing
    // messages to run completely locally
    .ConfigureServices(services => services.DisableAllExternalWolverineTransports())

    .StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/Configuration/disabling_all_external_transports.cs#L12-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disabling_external_transports' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Finally, to put that in a little more context about how you might go about using it
in real life, let's say that we have out main application with a relatively clean
bootstrapping setup and a separate integration testing project. In this case we'd 
like to bootstrap the application from the integration testing project **as it is, except
for having all the external transports disabled**. In the code below, I'm using the [Alba](https://jasperfx.github.io/alba)
and [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests):

<!-- snippet: sample_disabling_the_transports_from_web_application_factory -->
<a id='snippet-sample_disabling_the_transports_from_web_application_factory'></a>
```cs
// This is using Alba to bootstrap a Wolverine application
// for integration tests, but it's using WebApplicationFactory
// to do the actual bootstrapping
await using var host = await AlbaHost.For<Program>(x =>
{
    // I'm overriding
    x.ConfigureServices(services => services.DisableAllExternalWolverineTransports());
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Middleware/AppWithMiddleware.Tests/try_out_the_middleware.cs#L29-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disabling_the_transports_from_web_application_factory' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In the sample above, I'm bootstrapping the `IHost` for my production application with 
all the external transports turned off in a way that's appropriate for integration testing
message handlers within the main application.

## Running Wolverine in "Solo" Mode <Badge type="tip" text="3.0" />

Wolverine's [leadership election](/guide/durability/leadership-and-troubleshooting.html#troubleshooting-and-leadership-election) process is necessary for distributing several background tasks in real life production,
but that subsystem can lead to some inconvenient sluggishness in [cold start times](https://dontpaniclabs.com/blog/post/2022/09/20/net-cold-starts/#:~:text=In%20software%20development%2C%20cold%20starts,have%20an%20increased%20start%20time.) in automation testing.

To sidestep that problem, you can direct Wolverine to run in "Solo" mode where the current process assumes that it's the 
only running node and automatically starts up all known background tasks immediately. 

To do so, you could do something like this in your main `Program` file:

<!-- snippet: sample_configuring_the_solo_mode -->
<a id='snippet-sample_configuring_the_solo_mode'></a>
```cs
var builder = Host.CreateApplicationBuilder();

builder.UseWolverine(opts =>
{
    opts.Services.AddMarten("some connection string")

        // This adds quite a bit of middleware for
        // Marten
        .IntegrateWithWolverine();

    // You want this maybe!
    opts.Policies.AutoApplyTransactions();

    if (builder.Environment.IsDevelopment())
    {
        // But wait! Optimize Wolverine for usage as
        // if there would never be more than one node running
        opts.Durability.Mode = DurabilityMode.Solo;
    }
});

using var host = builder.Build();
await host.StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/DurabilityModes.cs#L55-L82' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_the_solo_mode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or if you're using something like [WebHostFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) to 
bootstrap your Wolverine application in an integration testing harness, you can use this helper to override Wolverine into being
"Solo":

<!-- snippet: sample_using_run_wolverine_in_solo_mode_with_extension -->
<a id='snippet-sample_using_run_wolverine_in_solo_mode_with_extension'></a>
```cs
// This is bootstrapping the actual application using
// its implied Program.Main() set up
// For non-Alba users, this is using IWebHostBuilder 
Host = await AlbaHost.For<Program>(x =>
{
    x.ConfigureServices(services =>
    {
        // Override the Wolverine configuration in the application
        // to run the application in "solo" mode for faster
        // testing cold starts
        services.RunWolverineInSoloMode();

        // And just for completion, disable all Wolverine external 
        // messaging transports
        services.DisableAllExternalWolverineTransports();
    });
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Http/Wolverine.Http.Tests/IntegrationContext.cs#L28-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_run_wolverine_in_solo_mode_with_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Stubbing Message Handlers <Badge type="tip" text="5.1" />

To extend the test automation support even further, Wolverine now has a capability to "stub"
out message handlers in testing scenarios with pre-canned behavior for more reliable testing
in some situations. This feature was mostly conceived of for stubbing out calls to external
systems through `IMessageBus.InvokeAsync<T>()` where the request would normally be sent to 
an external system through a subscriber. 

Jumping into an example, let's say that your system interacts with another service that estimates
delivery costs for ordering items. At some point in the system you might reach out through
a request/reply call in Wolverine to estimate an item delivery before making a purchase
like this code:

<!-- snippet: sample_code_showing_remote_request_reply -->
<a id='snippet-sample_code_showing_remote_request_reply'></a>
```cs
// This query message is normally sent to an external system through Wolverine
// messaging
public record EstimateDelivery(int ItemId, DateOnly Date, string PostalCode);

// This message type is a response from an external system
public record DeliveryInformation(TimeOnly DeliveryTime, decimal Cost);

public record MaybePurchaseItem(int ItemId, Guid LocationId, DateOnly Date, string PostalCode, decimal BudgetedCost);
public record MakePurchase(int ItemId, Guid LocationId, DateOnly Date);
public record PurchaseRejected(int ItemId, Guid LocationId, DateOnly Date);

public static class MaybePurchaseHandler
{
    public static Task<DeliveryInformation> LoadAsync(
        MaybePurchaseItem command, 
        IMessageBus bus, 
        CancellationToken cancellation)
    {
        var (itemId, _, date, postalCode, budget) = command;
        var estimateDelivery = new EstimateDelivery(itemId, date, postalCode);
        
        // Let's say this is doing a remote request and reply to another system
        // through Wolverine messaging
        return bus.InvokeAsync<DeliveryInformation>(estimateDelivery, cancellation);
    }
    
    public static object Handle(
        MaybePurchaseItem command, 
        DeliveryInformation estimate)
    {

        if (estimate.Cost <= command.BudgetedCost)
        {
            return new MakePurchase(command.ItemId, command.LocationId, command.Date);
        }

        return new PurchaseRejected(command.ItemId, command.LocationId, command.Date);
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/StubbingHandlers.cs#L121-L163' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_code_showing_remote_request_reply' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And for a little more context, the `EstimateDelivery` message will always be sent to
an external system in this configuration:

<!-- snippet: sample_configuring_estimate_delivery -->
<a id='snippet-sample_configuring_estimate_delivery'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    opts
        .UseRabbitMq(builder.Configuration.GetConnectionString("rabbit"))
        .AutoProvision();

    // Just showing that EstimateDelivery is handled by
    // whatever system is on the other end of the "estimates" queue
    opts.PublishMessage<EstimateDelivery>()
        .ToRabbitQueue("estimates");
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/StubbingHandlers.cs#L14-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_estimate_delivery' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Using our 

In testing scenarios, maybe the external system isn't available at all, or it's just much more 
challenging to run tests that also include the external system, or maybe you'd just like to 
write more isolated tests against your service's behavior before even trying to integrate
with the other system (my personal preference anyway). To that end we can now stub
the remote handling like this:

<!-- snippet: sample_using_stub_handler_in_testing_code -->
<a id='snippet-sample_using_stub_handler_in_testing_code'></a>
```cs
public static async Task try_application(IHost host)
{
    host.StubWolverineMessageHandling<EstimateDelivery, DeliveryInformation>(
        query => new DeliveryInformation(new TimeOnly(17, 0), 1000));

    var locationId = Guid.NewGuid();
    var itemId = 111;
    var expectedDate = new DateOnly(2025, 12, 1);
    var postalCode = "78750";

    var maybePurchaseItem = new MaybePurchaseItem(itemId, locationId, expectedDate, postalCode,
        500);
    
    var tracked =
        await host.InvokeMessageAndWaitAsync(maybePurchaseItem);
    
    // The estimated cost from the stub was more than we budgeted
    // so this message should have been published
    
    // This line is an assertion too that there was a single message
    // of this type published as part of the message handling above
    var rejected = tracked.Sent.SingleMessage<PurchaseRejected>();
    rejected.ItemId.ShouldBe(itemId);
    rejected.LocationId.ShouldBe(locationId);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/StubbingHandlers.cs#L32-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_stub_handler_in_testing_code' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

After calling making this call:

```csharp
        host.StubWolverineMessageHandling<EstimateDelivery, DeliveryInformation>(
            query => new DeliveryInformation(new TimeOnly(17, 0), 1000));

```

Calling this from our Wolverine application:

```csharp
        // Let's say this is doing a remote request and reply to another system
        // through Wolverine messaging
        return bus.InvokeAsync<DeliveryInformation>(estimateDelivery, cancellation);
```

Will use the stubbed logic we registered. This is enabling you to use fake behavior for
difficult to use external services. 

For the next test, we can completely remove the stub behavior and revert back to the 
original configuration like this:

<!-- snippet: sample_clearing_out_stub_behavior -->
<a id='snippet-sample_clearing_out_stub_behavior'></a>
```cs
public static void revert_stub(IHost host)
{
    // Selectively clear out the stub behavior for only one message
    // type
    host.WolverineStubs(stubs =>
    {
        stubs.Clear<EstimateDelivery>();
    });
    
    // Or just clear out all active Wolverine message handler
    // stubs
    host.ClearAllWolverineStubs();
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/StubbingHandlers.cs#L63-L79' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_clearing_out_stub_behavior' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or instead, we can just completely replace the previously registered stub behavior
with completely new logic that will override our previous stub:

<!-- snippet: sample_override_previous_stub_behavior -->
<a id='snippet-sample_override_previous_stub_behavior'></a>
```cs
public static void override_stub(IHost host)
{
    host.StubWolverineMessageHandling<EstimateDelivery, DeliveryInformation>(
        query => new DeliveryInformation(new TimeOnly(17, 0), 250));

}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/StubbingHandlers.cs#L81-L90' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_override_previous_stub_behavior' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

So far, we've only looked at simple request/reply behavior, but what if a remote system 
receiving our message potentially makes multiple calls back to our system? Or really just
any kind of interaction more complicated than a single response for a request message?

We're still in business, we just have to use a little uglier signature for our stub:

<!-- snippet: sample_using_more_complex_stubs -->
<a id='snippet-sample_using_more_complex_stubs'></a>
```cs
public static void more_complex_stub(IHost host)
{
    host.WolverineStubs(stubs =>
    {
        stubs.Stub<EstimateDelivery>(async (
            EstimateDelivery message, 
            IMessageContext context, 
            IServiceProvider services,
            CancellationToken cancellation) =>
        {
            // do whatever you want, including publishing any number of messages
            // back through IMessageContext
            
            // And grab any other services you might need from the application 
            // through the IServiceProvider -- but note that you will have
            // to deal with scopes yourself here

            // This is an equivalent to get the response back to the 
            // original caller
            await context.PublishAsync(new DeliveryInformation(new TimeOnly(17, 0), 250));
        });
    });
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/StubbingHandlers.cs#L92-L118' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_more_complex_stubs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

A few notes about this capability:

* You can use any number of stubs for different message types at the same time
* Most of the testing samples use extension methods on `IHost`, but we know there are some users who bootstrap only an IoC container for integration tests, so all of the extension methods shown in this section are also available off of `IServiceProvider`
* The "stub" functions are effectively singletons. There's nothing fancier about argument matching or anything you might expect from a full fledged mock library like NSubstitute or FakeItEasy
* You can actually fake out the routing to message types that are normally handled by handlers within the application
* We don't believe this feature will be helpful for "sticky" message handlers where you may have multiple handlers for the same message type interally






================================================
FILE: docs/guide/durability/dead-letter-storage.md
================================================
# Dead Letter Storage

If [message storage](/guide/durability/) is configured for your application, and you're using either the local queues or messaging
transports where Wolverine doesn't (yet) support native [dead letter queueing](https://en.wikipedia.org/wiki/Dead_letter_queue), Wolverine is actually moving messages
to the `wolverine_dead_letters` table in your database in lieu of native dead letter queueing. 

You can browse the messages in this table and see some of the exception details that led them to being moved
to the dead letter queue. To recover messages from the dead letter queue after possibly fixing a production support
issue, you can update this table's `replayable` column for any messages you want to recover with some kind of
SQL command like:

```sql
update wolverine_dead_letters set replayable = true where exception_type = 'InvalidAccountException';
```

When you do this, Wolverine's durability agent that manages the inbox and outbox processing in the background
will move these messages back into active incoming message handling. Just note that this process happens
through some polling, so it won't be instantaneous.

To replay dead lettered messages back to the incoming table, you also have a command line option:

```bash
dotnet run -- storage replay
```

## Dead Letter Expiration <Badge type="tip" text="3.9" />

::: tip
You could see poor performance over time if the dead letter queue storage in the database gets excessively large,
so Wolverine does have an "opt in" feature to let old messages expire and be expunged from the storage.
:::

It's off by default (for backwards compatibility), but you can enable Wolverine to assign expiration times to dead letter
queue messages persisted to durable storage like this:

<!-- snippet: sample_enabling_dead_letter_queue_expiration -->
<a id='snippet-sample_enabling_dead_letter_queue_expiration'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {

        // This is required
        opts.Durability.DeadLetterQueueExpirationEnabled = true;

        // Default is 10 days. This is the retention period
        opts.Durability.DeadLetterQueueExpiration = 3.Days();

    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Testing/CoreTests/BootstrappingSamples.cs#L42-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enabling_dead_letter_queue_expiration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Note that Wolverine will use the message's `DeliverBy` value as the expiration if that exists, otherwise, Wolverine will
just add the `DeadLetterQueueExpiration` time to the current time. The actual stored messages are deleted by background
processes and won't be quite real time.

## Integrating Dead Letters REST API into Your Application

Integrating the Dead Letters REST API into your WolverineFX application provides an elegant and powerful way to manage dead letter messages directly through HTTP requests. This capability is crucial for applications that require a robust mechanism for dealing with message processing failures, enabling developers and administrators to query, replay, or delete dead letter messages as needed. Below, we detail how to add this functionality to your application and describe the usage of each endpoint.

To get started, install that Nuget reference:

```bash
dotnet add package WolverineFx.Http
```

### Adding Dead Letters REST API to Your Application

To integrate the Dead Letters REST API into your WolverineFX application, you simply need to register the endpoints in your application's startup process. This is done by calling `app.MapDeadLettersEndpoints();` within the `Configure` method of your `Startup` class or the application initialization block if using minimal API patterns. This method call adds the necessary routes and handlers for dead letter management to your application's routing table.

<!-- snippet: sample_register_dead_letter_endpoints -->
<a id='snippet-sample_register_dead_letter_endpoints'></a>
```cs
app.MapDeadLettersEndpoints()

    // It's a Minimal API endpoint group,
    // so you can add whatever authorization
    // or OpenAPI metadata configuration you need
    // for just these endpoints
    //.RequireAuthorization("Admin")

    ;
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Http/WolverineWebApi/Program.cs#L204-L214' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_register_dead_letter_endpoints' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Using the Dead Letters REST API

#### Query Dead Letters Endpoint

- **Path**: `/dead-letters/`
- **Method**: `POST`
- **Request Body**: `DeadLetterEnvelopeGetRequest`
  - `Limit` (uint): Number of records to return per page.
  - `StartId` (Guid?): Start fetching records after the specified ID.
  - `MessageType` (string?): Filter by message type.
  - `ExceptionType` (string?): Filter by exception type.
  - `ExceptionMessage` (string?): Filter by exception message.
  - `From` (DateTimeOffset?): Start date for fetching records.
  - `Until` (DateTimeOffset?): End date for fetching records.
  - `TenantId` (string?): Tenant identifier for multi-tenancy support.
- **Response**: `DeadLetterEnvelopesFoundResponse` containing a list of `DeadLetterEnvelopeResponse` objects and an optional `NextId` for pagination.

**Request Example**:

```json
{
  "Limit": 50,
  "MessageType": "OrderPlacedEvent",
  "ExceptionType": "InvalidOrderException"
}
```

**Reponse Example**:

```json
{
  "Messages": [
    {
      "Id": "4e3d5e88-e01f-4bcb-af25-6e4c14b0a867",
      "ExecutionTime": "2024-04-06T12:00:00Z",
      "Body": {
        "OrderId": 123456,
        "OrderStatus": "Failed",
        "Reason": "Invalid Payment Method"
      },
      "MessageType": "OrderFailedEvent",
      "ReceivedAt": "2024-04-06T12:05:00Z",
      "Source": "OrderService",
      "ExceptionType": "PaymentException",
      "ExceptionMessage": "The payment method provided is invalid.",
      "SentAt": "2024-04-06T12:00:00Z",
      "Replayable": true
    },
    {
      "Id": "5f2c3d1e-3f3d-46f9-ba29-dac8e0f9b078",
      "ExecutionTime": null,
      "Body": {
        "CustomerId": 78910,
        "AccountBalance": -150.75
      },
      "MessageType": "AccountOverdrawnEvent",
      "ReceivedAt": "2024-04-06T15:20:00Z",
      "Source": "AccountService",
      "ExceptionType": "OverdrawnException",
      "ExceptionMessage": "Account balance cannot be negative.",
      "SentAt": "2024-04-06T15:15:00Z",
      "Replayable": false
    }
  ],
  "NextId": "8a1d77f2-f91b-4edb-8b51-466b5a8a3a6f"
}
```

#### Replay Dead Letters Endpoint

- **Path**: `/dead-letters/replay`
- **Method**: `POST`
- **Description**: Marks specified dead letter messages as replayable. This operation signals the system to attempt reprocessing the messages, ideally after the cause of the initial failure has been resolved.

**Request Example**:

```json
{
  "Ids": ["d3b07384-d113-4ec8-98c4-b3bf34e2c572", "d3b07384-d113-4ec8-98c4-b3bf34e2c573"]
}
```

#### Delete Dead Letters Endpoint

- **Path**: `/dead-letters/`
- **Method**: `DELETE`
- **Description**: Permanently removes specified dead letter messages from the system. Use this operation to clear messages that are no longer needed or cannot be successfully reprocessed.

**Request Example**:

```json
{
  "Ids": ["d3b07384-d113-4ec8-98c4-b3bf34e2c574", "d3b07384-d113-4ec8-98c4-b3bf34e2c575"]
}
```

### Conclusion

By integrating the Dead Letters REST API into your WolverineFX application, you gain fine-grained control over the management of dead letter messages. This feature not only aids in debugging and resolving processing issues but also enhances the overall reliability of your message-driven applications.



================================================
FILE: docs/guide/durability/idempotency.md
================================================
# Idempotent Message Delivery

::: tip
There is nothing you need to do to opt into idempotent, no more than once message deduplication other than to be using the durable inbox
on any Wolverine listening endpoint where you want this behavior. 
:::

When applying the [durable inbox](/guide/durability/#using-the-inbox-for-incoming-messages) to [message listeners](/guide/messaging/listeners), you also get a no more than once, 
[idempotent](https://en.wikipedia.org/wiki/Idempotence) message delivery guarantee. This means that Wolverine will discard
any received message that it can detect has been previously handled. Wolverine does this with its durable inbox storage to check on receipt of a 
new message if that message is already known by its Wolverine identifier. 

Instead of immediately deleting message storage for a successfully completed message, Wolverine merely marks that the message is handled and keeps
that message in storage for a default of 5 minutes to protect against duplicate incoming messages. To override that setting, you have this option:

<!-- snippet: sample_configuring_KeepAfterMessageHandling -->
<a id='snippet-sample_configuring_keepaftermessagehandling'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // The default is 5 minutes, but if you want to keep
        // messages around longer (or shorter) in case of duplicates,
        // this is how you do it
        opts.Durability.KeepAfterMessageHandling = 10.Minutes();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L195-L206' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_keepaftermessagehandling' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/durability/index.md
================================================
# Durable Messaging

::: info
A major goal of Wolverine 4.0 is to bring the EF Core integration capabilities (including multi-tenancy support) up to match the current integration
with Marten, add event sourcing support for SQL Server, and at least envelope storage integration with CosmosDb.
:::

Wolverine can integrate with several database engines and persistence tools for:

* Durable messaging through the transactional inbox and outbox pattern
* Transactional middleware to simplify your application code
* Saga persistence
* Durable, scheduled message handling
* Durable & replayable dead letter queueing
* Node and agent assignment persistence that is necessary for Wolverine to do agent assignments (its virtual actor capability)

## Transactional Inbox/Outbox

See the blog post [Transactional Outbox/Inbox with Wolverine and why you care](https://jeremydmiller.com/2022/12/15/transactional-outbox-inbox-with-wolverine-and-why-you-care/) for more context.

One of Wolverine's most important features is durable message persistence using your application's database for reliable "[store and forward](https://en.wikipedia.org/wiki/Store_and_forward)" queueing with all possible Wolverine transport options, including the [lightweight TCP transport](/guide/messaging/transports/tcp) and external transports like the [Rabbit MQ transport](/guide/messaging/transports/rabbitmq).

It's a chaotic world out when high volume systems need to interact with other systems. Your system may fail, other systems may be down,
there's network hiccups, occasional failures -- and you still need your systems to get to a consistent state without messages just
getting lost en route.

Consider this sample message handler from Wolverine's [AppWithMiddleware sample project](https://github.com/JasperFx/wolverine/tree/main/src/Samples/Middleware):

<!-- snippet: sample_DebitAccountHandler_that_uses_IMessageContext -->
<a id='snippet-sample_debitaccounthandler_that_uses_imessagecontext'></a>
```cs
[Transactional]
public static async Task Handle(
    DebitAccount command,
    Account account,
    IDocumentSession session,
    IMessageContext messaging)
{
    account.Balance -= command.Amount;

    // This just marks the account as changed, but
    // doesn't actually commit changes to the database
    // yet. That actually matters as I hopefully explain
    session.Store(account);

    // Conditionally trigger other, cascading messages
    if (account.Balance > 0 && account.Balance < account.MinimumThreshold)
    {
        await messaging.SendAsync(new LowBalanceDetected(account.Id));
    }
    else if (account.Balance < 0)
    {
        await messaging.SendAsync(new AccountOverdrawn(account.Id), new DeliveryOptions{DeliverWithin = 1.Hours()});

        // Give the customer 10 days to deal with the overdrawn account
        await messaging.ScheduleAsync(new EnforceAccountOverdrawnDeadline(account.Id), 10.Days());
    }

    // "messaging" is a Wolverine IMessageContext or IMessageBus service
    // Do the deliver within rule on individual messages
    await messaging.SendAsync(new AccountUpdated(account.Id, account.Balance),
        new DeliveryOptions { DeliverWithin = 5.Seconds() });
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/Middleware/AppWithMiddleware/Account.cs#L126-L161' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_debitaccounthandler_that_uses_imessagecontext' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The handler code above is committing changes to an `Account` in the underlying database and potentially sending out additional messages based on the state of the `Account`. 
For folks who are experienced with asynchronous messaging systems who hear me say that Wolverine does not support any kind of 2 phase commits between the database and message brokers, 
you’re probably already concerned with some potential problems in that code above:

* Maybe the database changes fail, but there are “ghost” messages already queued that pertain to data changes that never actually happened
* Maybe the messages actually manage to get through to their downstream handlers and are applied erroneously because the related database changes have not yet been applied. That’s a race condition that absolutely happens if you’re not careful (ask me how I know 😦 )
* Maybe the database changes succeed, but the messages fail to be sent because of a network hiccup or who knows what problem happens with the message broker

What you need is to guarantee that both the outgoing messages and the database changes succeed or fail together, and that the new messages are not actually published until the database transaction succeeds. 
To that end, Wolverine relies on message persistence within your application database as its implementation of the [Transactional Outbox](https://microservices.io/patterns/data/transactional-outbox.html) pattern. Using the "outbox" pattern is a way to avoid the need for problematic
and slow [distributed transactions](https://en.wikipedia.org/wiki/Distributed_transaction) while still maintaining eventual consistency between database changes and the outgoing messages that are part of the logical transaction. Wolverine implementation of the outbox pattern
also includes a separate *message relay* process that will send the persisted outgoing messages in background processes (it's done by marshalling the outgoing message envelopes through [TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) queues if you're curious.)

If any node of a Wolverine system that uses durable messaging goes down before all the messages are processed, the persisted messages will be loaded from
storage and processed when the system is restarted. Wolverine does this through its [DurabilityAgent](https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Persistence/Durability/DurabilityAgent.cs) that will run within your application through Wolverine's
[IHostedService](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio) runtime that is automatically registered in your system through the `UseWolverine()` extension method.

::: tip
At the moment, Wolverine only supports Postgresql, Sql Server, and RavenDb as the underlying database and either [Marten](/guide/durability/marten) or
[Entity Framework Core](/guide/durability/efcore) as the application persistence framework.
:::

There are four things you need to enable for the transactional outbox (and inbox for incoming messages):

1. Set up message storage in your application, and manage the storage schema objects -- don't worry though, Wolverine comes with a lot of tooling to help you with that
2. Enroll outgoing subscriber or listener endpoints in the durable storage at configuration time
3. Enable Wolverine's transactional middleware or utilize one of Wolverine's outbox publishing services

The last bullet point varies a little bit between the [Marten integration](/guide/durability/marten) and the [EF Core integration](/guide/durability/efcore), so see the
the specific documentation on each for more details.


## Using the Outbox for Outgoing Messages

::: tip
It might be valuable to leave some endpoints as "buffered" or "inline" for message types that have limited lifetimes.
See the blog post [Ephemeral Messages with Wolverine](https://jeremydmiller.com/2022/12/20/ephemeral-messages-with-wolverine/) for an example of this.
:::

To make the Wolverine outbox feature persist messages in the durable message storage, you need to explicitly make the 
outgoing subscriber endpoints (Rabbit MQ queues or exchange/binding, Azure Service Bus queues, TCP port, etc.) be
configured to be durable.

That can be done either on specific endpoints like this sample:

<!-- snippet: sample_make_specific_subscribers_be_durable -->
<a id='snippet-sample_make_specific_subscribers_be_durable'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.PublishAllMessages().ToPort(5555)

            // This option makes just this one outgoing subscriber use
            // durable message storage
            .UseDurableOutbox();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L68-L80' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_make_specific_subscribers_be_durable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or globally through a built in policy:

<!-- snippet: sample_make_all_subscribers_be_durable -->
<a id='snippet-sample_make_all_subscribers_be_durable'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // This forces every outgoing subscriber to use durable
        // messaging
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L53-L63' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_make_all_subscribers_be_durable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Bumping out Stale Inbox/Outbox Messages <Badge type="tip" text="5.2" />

It should *not* be possible for there to be any path where a message gets "stuck" in the outbox tables without eventually
being sent by the originating node or recovered by a different node if the original node goes down first. However, it's 
an imperfect world. If you are using one of the relational backed message stores for Wolverine (SQL Server or PostgreSQL at this point),
you can "bump" a persisted record in the `wolverine_outgoing_envelopes` to be recovered and sent by the outbox by
setting the `owner_id` field to zero.

::: info
Just be aware that opting into the `OutboxStaleTime` or `InboxStaleTime` threshold will require database changes through Wolverine's database
migration subsystem
:::

You also have this setting to force Wolverine to automatically "bump" and older messages that seem to be stalled in
the outbox table or the inbox table:

<!-- snippet: sample_configuring_outbox_stale_timeout -->
<a id='snippet-sample_configuring_outbox_stale_timeout'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Bump any persisted message in the outbox tables
        // that is more than an hour old to be globally owned
        // so that the durability agent can recover it and force
        // it to be sent
        opts.Durability.OutboxStaleTime = 1.Hours();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L281-L293' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_outbox_stale_timeout' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Note that this will still respect the "deliver by" semantics. This is part of the polling that Wolverine normally does
against the inbox/outbox/node storage tables. Note that this will only happen if the setting above has a non-null
value.

## Using the Inbox for Incoming Messages

On the incoming side, external transport endpoint listeners can be enrolled into Wolverine's transactional inbox mechanics
where messages received will be immediately persisted to the durable message storage and tracked there until the message is
successfully processed, expires, discarded due to error conditions, or moved to dead letter storage.

To enroll individual listening endpoints or all listening endpoints in the Wolverine inbox mechanics, use
one of these options:

<!-- snippet: sample_configuring_durable_inbox -->
<a id='snippet-sample_configuring_durable_inbox'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.ListenAtPort(5555)

            // Make specific endpoints be enrolled
            // in the durable inbox
            .UseDurableInbox();

        // Make every single listener endpoint use
        // durable message storage
        opts.Policies.UseDurableInboxOnAllListeners();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L85-L101' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_durable_inbox' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Local Queues

When you mark a [local queue](/guide/messaging/transports/local) as durable, you're telling Wolverine to store every message published
to that queue be stored in the backing message database until it is successfully processed. Doing so makes even the local queues be able
to guarantee eventual delivery even if the current node where the message was published fails before the message is processed.

To configure individual or set durability on local queues by some kind of convention, consider these possible usages:

<!-- snippet: sample_durable_local_queues -->
<a id='snippet-sample_durable_local_queues'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.Policies.UseDurableLocalQueues();

        // or

        opts.LocalQueue("important").UseDurableInbox();

        // or conventionally, make the local queues for messages in a certain namespace
        // be durable
        opts.Policies.ConfigureConventionalLocalRouting().CustomizeQueues((type, queue) =>
        {
            if (type.IsInNamespace("MyApp.Commands.Durable"))
            {
                queue.UseDurableInbox();
            }
        });
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L106-L128' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_durable_local_queues' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Message Identity <Badge type="tip" text="3.7" />

Wolverine was originally conceived for a world in which micro-services were all the rage for software architectures. 
The world changed on us though, as folks are now interested in pursuing [Modular Monolith architectures](/tutorials/modular-monolith) where
you may be trying to effectively jam what used to be separate micro-services into a single process. 

In the "classic" Wolverine configuration, incoming messages to the Wolverine transactional inboxes use the message id
of the incoming `Envelope` objects as the primary key in message stores. Which breaks down if you have something like this:

![Receiving Same Message 2 or More Times](/receive-message-twice.png)

In the diagram above, I'm trying to show what might happen (and it has happened) when the same Wolverine message is sent
through an external broker and delivered more than once to the same downstream Wolverine application. In the "classic"
mode, Wolverine will treat all but the first message as duplicate messages and reject them -- even though you mean
these messages to be handled separately by different message handlers in your modular monolith.

Not to worry, you can now opt into this setting to identify an incoming message by the combination of message id *and*
destination:

<!-- snippet: sample_configuring_message_identity_to_use_id_and_destination -->
<a id='snippet-sample_configuring_message_identity_to_use_id_and_destination'></a>
```cs
var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver2");
        
        // This setting changes the internal message storage identity
        opts.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
    })
    .StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/SqlServerTests/Persistence/SqlServerMessageStore_with_IdAndDestination_Identity.cs#L28-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_message_identity_to_use_id_and_destination' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This might be an important setting for [modular monolith architectures](/tutorials/modular-monolith). 



================================================
FILE: docs/guide/durability/leadership-and-troubleshooting.md
================================================
# Troubleshooting and Leadership Election

::: info
The main reason to care about this topic is to be able to troubleshoot why messages left stranded by a failed node
are not being recovered in a timely manner
:::

For some technical background, the Wolverine transactional inbox today works through a process of [leadership election](https://en.wikipedia.org/wiki/Leader_election), where only one node 
at any one time is the leader. The recovery of messages from dormant nodes that shut down somehow before they could
finish sending their outgoing or processing all their incoming messages is done through a persistent background agent
assigned to one node by the leader node. 

Long story short, if the message recovery isn't happening very quickly, it's likely some kind of issue with the leadership
election failing to start or to fail over from the previous leader dropping off. 

::: tip
There is no harm in deleting rows from this table. It is strictly a log
:::

As of Wolverine 1.10, there is a table in the PostgreSQL or Sql Server backed message storage called `wolverine_node_records`
that just has a record of detected events relevant to the leader election. All of this information is also logged
through the standard .Net `ILogger`, but it might be easier to understand the data in this table. 

Next, check the `wolverine_nodes` and `wolverine_node_assignments` to see where Wolverine thinks all of the running
agents are across the active nodes. The actual leadership agent is `wolverine://leader`, and you can spot the current
leader by the matching row in the `wolverine_node_assignments` table that refers to the "leader" agent. 

If you are frequently stopping and starting a local process -- especially if you are doing that through a debugger -- you
may want to utilize the `Solo` durability mode explained below:


## Solo Mode

Let's say that you're working on an individual development machine and frequently stopping and starting the application.
You'd ideally like the transactional inbox and outbox processing to kick in fast, but that subsystem has some known hiccups
recovering from exactly the kind of ungraceful process shutdown that happens when developers suddenly kill off the application
running in a debugger. 

To alleviate the issues that developers have had in the past with this mode, Wolverine 1.10 introduced the "Solo" mode
where the system can be optimized to run as if there's never more than one running node:
[..](..%2F..)
<!-- snippet: sample_configuring_the_solo_mode -->
<a id='snippet-sample_configuring_the_solo_mode'></a>
```cs
var builder = Host.CreateApplicationBuilder();

builder.UseWolverine(opts =>
{
    opts.Services.AddMarten("some connection string")

        // This adds quite a bit of middleware for
        // Marten
        .IntegrateWithWolverine();

    // You want this maybe!
    opts.Policies.AutoApplyTransactions();

    if (builder.Environment.IsDevelopment())
    {
        // But wait! Optimize Wolverine for usage as
        // if there would never be more than one node running
        opts.Durability.Mode = DurabilityMode.Solo;
    }
});

using var host = builder.Build();
await host.StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/DurabilityModes.cs#L55-L82' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_the_solo_mode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Running your Wolverine application like this means that Wolverine is able to more quickly start the transactional inbox
and outbox at start up time, and also to immediately recover any persisted incoming or outgoing messages from the previous
execution of the service on your local development box.

## Metrics <Badge type="tip" text="3.6" />

::: tip
These metrics can be used to understand when a Wolverine system is distressed when these numbers grow larger
:::

Wolverine emits observable gauge metrics for the size of the persisted inbox, outbox, and scheduled message counts:

1. `wolverine-inbox-count` - number of persisted, `Incoming` envelopes in the durable inbox
2. `wolverine-outbox-count` - number of persisted, `Outgoing` envelopes in the durable outbox
3. `wolverine-scheduled-count` - number of persisted, `Scheduled` envelopes in the durable inbox

In all cases, if you are using some sort of multi-tenancy where envelopes are stored in separate databsases per tenant,
the metric names above will be suffixed with ".[database name]".

You can disable or modify the polling of these metrics by these settings:

<!-- snippet: sample_configuring_persistence_metrics -->
<a id='snippet-sample_configuring_persistence_metrics'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // This does assume that you have *some* kind of message
        // persistence set up
        
        // This is enabled by default, but just showing that
        // you *could* disable it
        opts.Durability.DurabilityMetricsEnabled = true;

        // The default is 5 seconds, but maybe you want it slower
        // because this does have to do a non-trivial query
        opts.Durability.UpdateMetricsPeriod = 10.Seconds();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L211-L228' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuring_persistence_metrics' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/durability/managing.md
================================================
# Managing Message Storage

::: info
Wolverine will automatically check for the existence of necessary database tables and functions to support the
configured message storage, and will also apply any necessary database changes to comply with the configuration automatically.
:::

Wolverine uses the [Oakton "Stateful Resource"](https://jasperfx.github.io/oakton/guide/host/resources.html) model for managing
infrastructure configuration at development or even deployment time for configured items like the database-backed message storage or
message broker queues.

## Disable Automatic Storage Migration

To disable the automatic storage migration, just flip this flag:

<!-- snippet: sample_disable_auto_build_envelope_storage -->
<a id='snippet-sample_disable_auto_build_envelope_storage'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        // Disable automatic database migrations for message
        // storage
        opts.AutoBuildMessageStorageOnStartup = AutoCreate.None;
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/DocumentationSamples/DisablingStorageConstruction.cs#L11-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_disable_auto_build_envelope_storage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Programmatic Management

Especially in automated tests, you may want to programmatically rebuild or clear out all persisted
messages. Here's a sample of the functionality in Wolverine to do just that:

<!-- snippet: sample_programmatic_management_of_message_storage -->
<a id='snippet-sample_programmatic_management_of_message_storage'></a>
```cs
// IHost would be your application in a testing harness
public static async Task testing_setup_or_teardown(IHost host)
{
    // Programmatically apply any outstanding message store
    // database changes
    await host.SetupResources();

    // Teardown the database message storage
    await host.TeardownResources();

    // Clear out any database message storage
    // also tries to clear out any messages held
    // by message brokers connected to your Wolverine app
    await host.ResetResourceState();

    var store = host.Services.GetRequiredService<IMessageStore>();

    // Rebuild the database schema objects
    // and delete existing message data
    // This is good for testing
    await store.Admin.RebuildAsync();

    // Remove all persisted messages
    await store.Admin.ClearAllAsync();
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L21-L49' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_programmatic_management_of_message_storage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Building Storage on Startup

To have any missing database schema objects built as needed on application startup, just add this option:

<!-- snippet: sample_resource_setup_on_startup -->
<a id='snippet-sample_resource_setup_on_startup'></a>
```cs
// This is rebuilding the persistent storage database schema on startup
builder.Host.UseResourceSetupOnStartup();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/EFCoreSample/ItemService/Program.cs#L55-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_resource_setup_on_startup' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Command Line Management

Assuming that you are using [Oakton](https://jasperfx.github.io/oakton) as your command line parser in your Wolverine application as
shown in this last line of a .NET 6/7 `Program` code file:

<!-- snippet: sample_using_jasperfx_for_command_line_parsing -->
<a id='snippet-sample_using_jasperfx_for_command_line_parsing'></a>
```cs
// Opt into using JasperFx for command parsing
await app.RunJasperFxCommands(args);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Samples/EFCoreSample/ItemService/Program.cs#L85-L90' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_jasperfx_for_command_line_parsing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And you're using the message persistence from either the `WolverineFx.SqlServer` or `WolverineFx.Postgresql`
or `WolverineFx.Marten` Nugets installed in your application, you will have some extended command line options
that you can discover from typing `dotnet run -- help` at the command line at the root of your project:

```bash
The available commands are:

  Alias       Description
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  check-env   Execute all environment checks against the application
  codegen     Utilities for working with JasperFx.CodeGeneration and JasperFx.RuntimeCompiler
  db-apply    Applies all outstanding changes to the database(s) based on the current configuration
  db-assert   Assert that the existing database(s) matches the current configuration
  db-dump     Dumps the entire DDL for the configured Marten database
  db-patch    Evaluates the current configuration against the database and writes a patch and drop file if there are
              any differences
  describe    Writes out a description of your running application to either the console or a file
  help        List all the available commands
  resources   Check, setup, or teardown stateful resources of this system
  run         Start and run this .Net application
  storage     Administer the envelope storage
```

There's admittedly some duplication here with different options coming from [Oakton](https://jasperfx.github.io/oakton) itself, the [Weasel.CommandLine](https://github.com/JasperFx/weasel) library,
and the `storage` command from Wolverine itself. To build out the schema objects for [message persistence](/guide/durability/), you
can use this command to apply any outstanding database changes necessary to bring the database schema to the Wolverine configuration:

```bash
dotnet run -- db-apply
```
> NOTE: See the [Exporting SQL Scripts](#exporting-sql-scripts) section down the page for details of applying migrations when integrating with Marten

or this option -- but just know that this will also clear out any existing message data:

```bash
dotnet run -- storage rebuild
```

or this option which will also attempt to create Marten database objects or any known Wolverine transport objects like
Rabbit MQ / Azure Service Bus / AWS SQS queues:

```bash
dotnet run -- resources setup
```

## Clearing Node Ownership

::: warning
Don't use this option in production if any nodes are currently running
:::

If you ever have a node crash and need to force any persisted, incoming or outgoing messages to be picked up 
by another node (this should be automatic anyway, but locks might persist and Wolverine might take a bit to recognize that a node has crashed),
you can release the ownership of messages of all persisted nodes by:

```bash
dotnet run -- storage release
```

## Deleting Message Data

At any time you can clear out any existing persisted message data with:

```bash
dotnet run -- storage clear
```

## Exporting SQL Scripts

If you just want to export the SQL to create the necessary database objects, you can use:

```bash
dotnet run -- db-dump export.sql
```
where `export.sql` should be a file name.

### Marten integration

When integrating with Marten, scripts must be generated seperately for both Marten and Wolverine resources.  
Resources are separated into databases and can be listed as below:

```bash
dotnet run -- db-list
# ┌────────────────────────────────────────┬───────────────────────────┐
# │ DatabaseUri                            │ SubjectUri                │
# ├────────────────────────────────────────┼───────────────────────────┤
# │ postgresql://localhost/postgres/orders │ marten://store/           │
# │ postgresql://localhost/postgres        │ wolverine://messages/main │
# └────────────────────────────────────────┴───────────────────────────┘
```

Once you've identified the database, pass the `-d` parameter with the `SubjectUri` from the output above to the `db-dump` command:

```bash
dotnet run -- db-dump -d marten://store/ export_marten.sql
dotnet run -- db-dump -d wolverine://messages/main export_wolverine.sql
```

## Disabling All Persistence <Badge type="tip" text="3.6" />

Let's say that you want to use the command line tooling to generate OpenAPI documentation, but do so
without Wolverine being able to connect to any external databases (or transports, and you'll have to disable both for this to work).
You can now do that with the option shown below as part of an [Alba](https://jasperfx.github.io/alba) test:

<!-- snippet: sample_bootstrap_with_no_persistence -->
<a id='snippet-sample_bootstrap_with_no_persistence'></a>
```cs
using var host = await AlbaHost.For<Program>(builder =>
{
    builder.ConfigureServices(services =>
    {
        // You probably have to do both
        services.DisableAllExternalWolverineTransports();
        services.DisableAllWolverineMessagePersistence();
    });
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Http/Wolverine.Http.Tests/bootstrap_with_no_persistence.cs#L14-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrap_with_no_persistence' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



================================================
FILE: docs/guide/durability/postgresql.md
================================================
# PostgreSQL Integration

::: info
Wolverine can happily use the PostgreSQL durability options with any mix of Entity Framework Core and/or
Marten as a higher level persistence framework
:::

Wolverine supports a PostgreSQL backed message persistence strategy and even a PostgreSQL backed messaging transport
option. To get started, add the `WolverineFx.Postgresql` dependency to your application:

```bash
dotnet add package WolverineFx.Postgresql
```

## Message Persistence

To enable PostgreSQL to serve as Wolverine's [transactional inbox and outbox](./), you just need to use the `WolverineOptions.PersistMessagesWithPostgresql()`
extension method as shown below in a sample:

<!-- snippet: sample_setup_postgresql_storage -->
<a id='snippet-sample_setup_postgresql_storage'></a>
```cs
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("postgres");

builder.Host.UseWolverine(opts =>
{
    // Setting up Postgresql-backed message storage
    // This requires a reference to Wolverine.Postgresql
    opts.PersistMessagesWithPostgresql(connectionString);

    // Other Wolverine configuration
});

// This is rebuilding the persistent storage database schema on startup
// and also clearing any persisted envelope state
builder.Host.UseResourceSetupOnStartup();

var app = builder.Build();

// Other ASP.Net Core configuration...

// Using JasperFx opens up command line utilities for managing
// the message storage
return await app.RunJasperFxCommands(args);
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PersistenceTests/Samples/DocumentationSamples.cs#L164-L190' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_setup_postgresql_storage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Optimizing the Message Store <Badge type="tip" text="5.3" />

For PostgreSQL, you can enable PostgreSQL backed partitioning for the inbox table
as an optimization. This is not enabled by default just to avoid causing database
migrations in a minor point release. Note that this will have some significant benefits
for inbox/outbox metrics gathering in the future:

<!-- snippet: sample_enabling_inbox_partitioning -->
<a id='snippet-sample_enabling_inbox_partitioning'></a>
```cs
var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.Durability.EnableInboxPartitioning = true;
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PostgresqlTests/compliance_using_table_partitioning.cs#L26-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enabling_inbox_partitioning' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## PostgreSQL Messaging Transport <Badge type="tip" text="2.5" />

::: info
All PostgreSQL queues are built into a *wolverine_queues* schema at this point. 
:::

The `WolverineFx.PostgreSQL` Nuget also contains a simple messaging transport that was mostly meant to be usable for teams
who want asynchronous queueing without introducing more specialized infrastructure. To enable this transport in your code,
use this option which *also* activates PostgreSQL backed message persistence:

<!-- snippet: sample_using_postgres_transport -->
<a id='snippet-sample_using_postgres_transport'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("postgres");
    opts.UsePostgresqlPersistenceAndTransport(
            connectionString, 
            
            // This argument is the database schema for the envelope storage
            // If separate logical services are targeting the same physical database,
            // you should use a separate schema name for each logical application
            // to make basically *everything* run smoother
            "myapp", 
            
            // This schema name is for the actual PostgreSQL queue tables. If using
            // the PostgreSQL transport between two logical applications, make sure
            // to use the same transportSchema!
            transportSchema:"queues")

        // Tell Wolverine to build out all necessary queue or scheduled message
        // tables on demand as needed
        .AutoProvision()

        // Optional that may be helpful in testing, but probably bad
        // in production!
        .AutoPurgeOnStartup();

    // Use this extension method to create subscriber rules
    opts.PublishAllMessages().ToPostgresqlQueue("outbound");

    // Use this to set up queue listeners
    opts.ListenToPostgresqlQueue("inbound")

        .CircuitBreaker(cb =>
        {
            // fine tune the circuit breaker
            // policies here
        })

        // Optionally specify how many messages to
        // fetch into the listener at any one time
        .MaximumMessagesToReceive(50);
});

using var host = builder.Build();
await host.StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PostgresqlTests/DocumentationSamples.cs#L12-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_postgres_transport' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The PostgreSQL transport is strictly queue-based at this point. The queues are configured as durable by default, meaning
that they are utilizing the transactional inbox and outbox. The PostgreSQL queues can also be buffered:

<!-- snippet: sample_setting_postgres_queue_to_buffered -->
<a id='snippet-sample_setting_postgres_queue_to_buffered'></a>
```cs
opts.ListenToPostgresqlQueue("sender").BufferedInMemory();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/PostgresqlTests/Transport/compliance_tests.cs#L64-L68' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_setting_postgres_queue_to_buffered' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Using this option just means that the PostgreSQL queues can be used for both sending or receiving with no integration
with the transactional inbox or outbox. This is a little more performant, but less safe as messages could be
lost if held in memory when the application shuts down unexpectedly. 

### Polling
Wolverine has a number of internal polling operations, and any PostgreSQL queues will be polled on a configured interval as Wolverine does not use the PostgreSQL `LISTEN/NOTIFY` feature at this time.   
The default polling interval is set in the `DurabilitySettings` class and can be configured at runtime as below:

```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    // Health check message queue/dequeue
    opts.Durability.HealthCheckPollingTime = TimeSpan.FromSeconds(10);
    
    // Node reassigment checks
    opts.Durability.NodeReassignmentPollingTime = TimeSpan.FromSeconds(5);
    
    // User queue poll frequency
    opts.Durability.ScheduledJobPollingTime = TimeSpan.FromSeconds(5);
}
```

::: info Control queue  
Wolverine has an internal control queue (`dbcontrol`) used for internal operations.  
This queue is hardcoded to poll every second and should not be changed to ensure the stability of the application.
:::


## Multi-Tenancy

As of Wolverine 4.0, you have two ways to use multi-tenancy through separate databases per tenant with PostgreSQL:

1. Using [Marten's multi-tenancy support](https://martendb.io/configuration/multitenancy.html) and the `IntegrateWithWolverine()` option
2. Directly configure PostgreSQL databases with Wolverine managed multi-tenancy <Badge type="tip" text="4.0" />

In both cases, if utilizing the PostgreSQL transport with multi-tenancy through separate databases per tenant, the PostgreSQL
queues will be built and monitored for each tenant database as well as any main, non-tenanted database. Also, Wolverine is able to utilize
completely different message storage for its transactional inbox and outbox for each unique database including any main database.
Wolverine is able to activate additional durability agents for itself for any tenant databases added at runtime for tenancy modes
that support dynamic discovery. 

To utilize Wolverine managed multi-tenancy, you have a couple main options. The simplest is just using a static configured
set of tenant id to database connections like so:

<!-- snippet: sample_static_tenant_registry_with_postgresql -->
<a id='snippet-sample_static_tenant_registry_with_postgresql'></a>
```cs
var builder = Host.CreateApplicationBuilder();

var configuration = builder.Configuration;

builder.UseWolverine(opts =>
{
    // First, you do have to have a "main" PostgreSQL database for messaging persistence
    // that will store information about running nodes, agents, and non-tenanted operations
    opts.PersistMessagesWithPostgresql(configuration.GetConnectionString("main"))

        // Add known tenants at bootstrapping time
        .RegisterStaticTenants(tenants =>
        {
            // Add connection strings for the expected tenant ids
            tenants.Register("tenant1", configuration.GetConnectionString("tenant1"));
            tenants.Register("tenant2", configuration.GetConnectionString("tenant2"));
            tenants.Register("tenant3", configuration.GetConnectionString("tenant3"));
        });
    
    opts.Services.AddDbContextWithWolverineManagedMultiTenancy<ItemsDbContext>((builder, connectionString, _) =>
    {
        builder.UseNpgsql(connectionString.Value, b => b.MigrationsAssembly("MultiTenantedEfCoreWithPostgreSQL"));
    }, AutoCreate.CreateOrUpdate);
});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/EfCoreTests/MultiTenancy/MultiTenancyDocumentationSamples.cs#L24-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_static_tenant_registry_with_postgresql' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Since the underlying [Npgsql library](https://www.npgsql.org/) supports the `DbDataSource` concept, and you might need to use this for a variety of reasons, you can also
directly configure `NpgsqlDataSource` objects for each tenant. This one might be a little more involved, but let's start
by saying that you might be using Aspire to configure PostgreSQL and both the main and tenant databases. In this usage,
Aspire will register `NpgsqlDataSource` services as `Singleton` scoped in your IoC container. We can build an `IWolverineExtension`
that utilizes the IoC container to register Wolverine like so:

<!-- snippet: sample_OurFancyPostgreSQLMultiTenancy -->
<a id='snippet-sample_ourfancypostgresqlmultitenancy'></a>
```cs
public class OurFancyPostgreSQLMultiTenancy : IWolverineExtension
{
    private readonly IServiceProvider _provider;

    public OurFancyPostgreSQLMultiTenancy(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Configure(WolverineOptions options)
    {
        options.PersistMessagesWithPostgresql(_provider.GetRequiredService<NpgsqlDataSource>())
            .RegisterStaticTenantsByDataSource(tenants =>
            {
                tenants.Register("tenant1", _provider.GetRequiredKeyedService<NpgsqlDataSource>("tenant1"));
                tenants.Register("tenant2", _provider.GetRequiredKeyedService<NpgsqlDataSource>("tenant2"));
                tenants.Register("tenant3", _provider.GetRequiredKeyedService<NpgsqlDataSource>("tenant3"));
            });
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/EfCoreTests/MultiTenancy/MultiTenancyDocumentationSamples.cs#L165-L188' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ourfancypostgresqlmultitenancy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And add that to the greater application like so:

<!-- snippet: sample_adding_our_fancy_postgresql_multi_tenancy -->
<a id='snippet-sample_adding_our_fancy_postgresql_multi_tenancy'></a>
```cs
var host = Host.CreateDefaultBuilder()
    .UseWolverine()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IWolverineExtension, OurFancyPostgreSQLMultiTenancy>();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/EfCoreTests/MultiTenancy/MultiTenancyDocumentationSamples.cs#L152-L161' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_adding_our_fancy_postgresql_multi_tenancy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: warning
Neither Marten nor Wolverine is able to dynamically tear down tenants yet. That's long planned, and honestly probably only happens
when an outside company sponsors that work.
:::

If you need to be able to add new tenants at runtime or just have more tenants than is comfortable living in static configuration
or plenty of other reasons I could think of, you can also use Wolverine's "master table tenancy" approach where tenant id
to database connection string information is kept in a separate database table. 

Here's a possible usage of that model:

<!-- snippet: sample_using_postgresql_backed_master_table_tenancy -->
<a id='snippet-sample_using_postgresql_backed_master_table_tenancy'></a>
```cs
var builder = Host.CreateApplicationBuilder();

var configuration = builder.Configuration;
builder.UseWolverine(opts =>
{
    // You need a main database no matter what that will hold information about the Wolverine system itself
    // and..
    opts.PersistMessagesWithPostgresql(configuration.GetConnectionString("wolverine"))

        // ...also a table holding the tenant id to connection string information
        .UseMasterTableTenancy(seed =>
        {
            // These registrations are 100% just to seed data for local development
            // Maybe you want to omit this during production?
            // Or do something programmatic by looping through data in the IConfiguration?
            seed.Register("tenant1", configuration.GetConnectionString("tenant1"));
            seed.Register("tenant2", configuration.GetConnectionString("tenant2"));
            seed.Register("tenant3", configuration.GetConnectionString("tenant3"));
        });

});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/EfCoreTests/MultiTenancy/MultiTenancyDocumentationSamples.cs#L95-L119' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_postgresql_backed_master_table_tenancy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: info
Wolverine's "master table tenancy" model was unsurprisingly based on Marten's [Master Table Tenancy](https://martendb.io/configuration/multitenancy.html#master-table-tenancy-model) feature
and even shares a little bit of supporting code now.
:::

Here's some more important background on the multi-tenancy support:

* Wolverine is spinning up a completely separate "durability agent" across the application to recover stranded messages in
  the transactional inbox and outbox, and that's done automatically for you
* The lightweight saga support for PostgreSQL absolutely works with this model of multi-tenancy
* Wolverine is able to manage all of its database tables including the tenant table itself (`wolverine_tenants`) across both the
  main database and all the tenant databases including schema migrations
* Wolverine's transactional middleware is aware of the multi-tenancy and can connect to the correct database based on the `IMesageContext.TenantId`
  or utilize the tenant id detection in Wolverine.HTTP as well
* You can "plug in" a custom implementation of `ITenantSource<string>` to manage tenant id to connection string assignments in whatever way works for your deployed system


## Lightweight Saga Usage <Badge type="tip" text="3.0" />

See the details on [Lightweight Saga Storage](/guide/durability/sagas.html#lightweight-saga-storage) for more information.

## Integration with Marten

The PostgreSQL message persistence and transport is automatically included with the `AddMarten().IntegrateWithWolverine()`
configuration syntax.





================================================
FILE: docs/guide/durability/ravendb.md
================================================
# RavenDb Integration <Badge type="tip" text="3.0" />

Wolverine supports a [RavenDb](https://ravendb.net/) backed message persistence strategy
option as well as RavenDb-backed transactional middleware and saga persistence. To get started, add the `WolverineFx.RavenDb` dependency to your application:

```bash
dotnet add package WolverineFx.RavenDb
```

and in your application, tell Wolverine to use RavenDb for message persistence:

<!-- snippet: sample_bootstrapping_with_ravendb -->
<a id='snippet-sample_bootstrapping_with_ravendb'></a>
```cs
var builder = Host.CreateApplicationBuilder();

// You'll need a reference to RavenDB.DependencyInjection
// for this one
builder.Services.AddRavenDbDocStore(raven =>
{
    // configure your RavenDb connection here
});

builder.UseWolverine(opts =>
{
    // That's it, nothing more to see here
    opts.UseRavenDbPersistence();
    
    // The RavenDb integration supports basic transactional
    // middleware just fine
    opts.Policies.AutoApplyTransactions();
});

// continue with your bootstrapping...
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/RavenDbTests/DocumentationSamples.cs#L14-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_ravendb' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Also see [RavenDb's own documentation](https://ravendb.net/docs/article-page/6.0/csharp/start/guides/aws-lambda/existing-project) for bootstrapping RavenDb inside of a .NET application. 

## Message Persistence

The [durable inbox and outbox](/guide/durability/) options in Wolverine are completely supported with 
RavenDb as the persistence mechanism. This includes scheduled execution (and retries), dead letter queue storage 
using the `DeadLetterMessage` collection, and the ability to replay designated messages in the dead letter queue
storage.

## Saga Persistence

The RavenDb integration can serve as a [Wolverine Saga persistence mechanism](/guide/durability/sagas). The only limitation is that
your `Saga` types can _only_ use strings as the identity for the `Saga`. 

<!-- snippet: sample_ravendb_saga -->
<a id='snippet-sample_ravendb_saga'></a>
```cs
public class Order : Saga
{
    // Just use this for the identity
    // of RavenDb backed sagas
    public string Id { get; set; }
    
    // Handle and Start methods...
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/RavenDbTests/DocumentationSamples.cs#L41-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ravendb_saga' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There's nothing else to do, if RavenDb integration is applied to your Wolverine, it's going to kick in
for saga persistence as long as your `Saga` type has a string identity property.

## Transactional Middleware

::: warning
The RavenDb transactional middleware **only** supports the RavenDb `IAsyncDocumentSession` service
:::

The normal configuration options for transactional middleware in Wolverine apply to the RavenDb backend, so either
mark handlers explicitly with `[Transactional]` like so:

<!-- snippet: sample_using_transactional_with_raven -->
<a id='snippet-sample_using_transactional_with_raven'></a>
```cs
public class CreateDocCommandHandler
{
    [Transactional]
    public async Task Handle(CreateDocCommand message, IAsyncDocumentSession session)
    {
        await session.StoreAsync(new FakeDoc { Id = message.Id });
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/RavenDbTests/DocumentationSamples.cs#L60-L71' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_transactional_with_raven' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or if you choose to do this more conventionally (which folks do tend to use quite often):

```csharp
        builder.UseWolverine(opts =>
        {
            // That's it, nothing more to see here
            opts.UseRavenDbPersistence();
            
            // The RavenDb integration supports basic transactional
            // middleware just fine
            opts.Policies.AutoApplyTransactions();
        });
```

and the transactional middleware will kick in on any message handler or HTTP endpoint that uses
the RavenDb `IAsyncDocumentSession` like this handler signature:

<!-- snippet: sample_raven_using_handler_for_auto_transactions -->
<a id='snippet-sample_raven_using_handler_for_auto_transactions'></a>
```cs
public class AlternativeCreateDocCommandHandler
{
    // Auto transactions would kick in just because of the dependency
    // on IAsyncDocumentSession
    public async Task Handle(CreateDocCommand message, IAsyncDocumentSession session)
    {
        await session.StoreAsync(new FakeDoc { Id = message.Id });
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/RavenDbTests/DocumentationSamples.cs#L73-L85' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_raven_using_handler_for_auto_transactions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The transactional middleware will also be applied for any usage of the `RavenOps` [side effects](/guide/handlers/side-effects) model
for Wolverine's RavenDb integration:

<!-- snippet: sample_using_ravendb_side_effects -->
<a id='snippet-sample_using_ravendb_side_effects'></a>
```cs
public record RecordTeam(string Team, int Year);

public static class RecordTeamHandler
{
    public static IRavenDbOp Handle(RecordTeam command)
    {
        return RavenOps.Store(new Team { Id = command.Team, YearFounded = command.Year });
    }
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/RavenDbTests/transactional_middleware.cs#L47-L59' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_ravendb_side_effects' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## System Control Queues

The RavenDb integration to Wolverine does not yet come with a built in database control queue
mechanism, so you will need to add that from external messaging brokers as in this example
using Azure Service Bus:

<!-- snippet: sample_enabling_azure_service_bus_control_queues -->
<a id='snippet-sample_enabling_azure_service_bus_control_queues'></a>
```cs
var builder = Host.CreateApplicationBuilder();
builder.UseWolverine(opts =>
{
    // One way or another, you're probably pulling the Azure Service Bus
    // connection string out of configuration
    var azureServiceBusConnectionString = builder
        .Configuration
        .GetConnectionString("azure-service-bus")!;

    // Connect to the broker in the simplest possible way
    opts.UseAzureServiceBus(azureServiceBusConnectionString)
        .AutoProvision()
        
        // This enables Wolverine to use temporary Azure Service Bus
        // queues created at runtime for communication between
        // Wolverine nodes
        .EnableWolverineControlQueues();

});
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Transports/Azure/Wolverine.AzureServiceBus.Tests/DocumentationSamples.cs#L193-L216' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enabling_azure_service_bus_control_queues' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For local development, there is also an option to let Wolverine just use its TCP transport
as a control endpoint with this configuration option:

```csharp
WolverineOptions.UseTcpForControlEndpoint();
```

In the option above, Wolverine is just looking for an unused port, and assigning that found port
as the listener for the node being bootstrapped. 

## RavenOps Side Effects

The `RavenOps` static class can be used as a convenience for RavenDb integration with Wolverine:

<!-- snippet: sample_ravenops -->
<a id='snippet-sample_ravenops'></a>
```cs
/// <summary>
/// Side effect helper class for Wolverine's integration with RavenDb
/// </summary>
public static class RavenOps
{
    /// <summary>
    /// Store a new RavenDb document
    /// </summary>
    /// <param name="document"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IRavenDbOp Store<T>(T document) => new StoreDoc<T>(document);

    /// <summary>
    /// Delete this document in RavenDb
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static IRavenDbOp DeleteDocument(object document) => new DeleteByDoc(document);

    /// <summary>
    /// Delete a RavenDb document by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static IRavenDbOp DeleteById(string id) => new DeleteById(id);
}
```
<sup><a href='https://github.com/JasperFx/wolverine/blob/main/src/Persistence/Wolverine.RavenDb/IRavenDbOp.cs#L36-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ravenops' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

See the Wolverine [side effects](/guide/handlers/side-effects) model for more information.

This integration also includes full support for the [storage action side effects](/guide/handlers/side-effects.html#storage-side-effects)
model when using RavenDb with Wolverine. 

## Entity Attribute Loading

The RavenDb integration is able to completely support the [Entity attribute usage](/guide/handlers/persistence.html#automatically-loading-entities-to-method-parameters).


=======================================


## RAZOR SLICES
# Razor Slices

[![CI (main)](https://github.com/DamianEdwards/RazorSlices/actions/workflows/ci.yml/badge.svg)](https://github.com/DamianEdwards/RazorSlices/actions/workflows/ci.yml)
[![Nuget](https://img.shields.io/nuget/v/RazorSlices)](https://www.nuget.org/packages/RazorSlices/)

*Lightweight* Razor-based templates for ASP.NET Core without MVC, Razor Pages, or Blazor, optimized for high-performance, unbuffered rendering with low allocations. Compatible with trimming and native AOT. Great for returning dynamically rendered HTML from Minimal APIs, middleware, etc. Supports .NET 8+

- [Getting Started](#getting-started)
- [Installation](#installation)
- [Features](#features)

## Getting Started

1. [Install the NuGet package](#installation) into your ASP.NET Core project (.NET 8+):

    ``` shell
    > dotnet add package RazorSlices
    ```

1. Create a directory in your project called *Slices* and add a *_ViewImports.cshtml* file to it with the following content:

    ``` cshtml
    @inherits RazorSliceHttpResult

    @using System.Globalization;
    @using Microsoft.AspNetCore.Razor;
    @using Microsoft.AspNetCore.Http.HttpResults;
    
    @tagHelperPrefix __disable_tagHelpers__:
    @removeTagHelper *, Microsoft.AspNetCore.Mvc.Razor
    ```

1. In the same directory, add a *Hello.cshtml* file with the following content:

    ``` cshtml
    @inherits RazorSliceHttpResult<DateTime>
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8">
        <title>Hello from Razor Slices!</title>
    </head>
    <body>
        <p>
            Hello from Razor Slices! The time is @Model
        </p>
    </body>
    </html>
    ```

    Each *.cshtml* file will have a proxy type generated for it by the Razor Slices source generator that you can use as the generic argument to the various APIs in Razor Slices for rendering slices.

1. Add a minimal API to return the slice in your *Program.cs*:

    ``` c#
    app.MapGet("/hello", () => Results.Extensions.RazorSlice<MyApp.Slices.Hello, DateTime>(DateTime.Now));
    ```

1. **Optional:** By default, all *.cshtml* files in your project are treated as Razor Slices (excluding *_ViewImports.cshtml* and *_ViewStart.cshtml_*). You can change this by setting the `EnableDefaultRazorSlices` property to `false` and the `GenerateRazorSlice` metadata property of the desired `RazorGenerate` items to `true` in your project file, e.g.:

    ``` xml
    <PropertyGroup>
      <EnableDefaultRazorSlices>false</EnableDefaultRazorSlices>
    </PropertyGroup>
    <ItemGroup>
      <!-- Only treat .cshtml files in Slices directory as Razor Slices -->
      <RazorSlice Include="Slices\**\*.cshtml" Exclude="Slices\**\_Layout.cshtml;Slices\**\_ViewImports.cshtml" />
    </ItemGroup>
    ```

    This will configure the Razor Slices source generator to only generate proxy types for *.cshtml* files in the *Slices* directory in your project.

## Installation

### NuGet Releases

[![Nuget](https://img.shields.io/nuget/v/RazorSlices)](https://www.nuget.org/packages/RazorSlices/)

This package is currently available from [nuget.org](https://www.nuget.org/packages/RazorSlices/):

``` console
> dotnet add package RazorSlices
```

### CI Builds

If you wish to use builds from this repo's `main` branch you can install them from [this repo's package feed](https://github.com/DamianEdwards/RazorSlices/pkgs/nuget/RazorSlices).

1. [Create a personal access token](https://github.com/settings/tokens/new) for your GitHub account with the `read:packages` scope with your desired expiration length:

    [<img width="583" alt="image" src="https://user-images.githubusercontent.com/249088/160220117-7e79822e-a18a-445c-89ff-b3d9ca84892f.png">](https://github.com/settings/tokens/new)

1. At the command line, navigate to your user profile directory and run the following command to add the package feed to your NuGet configuration, replacing the `<GITHUB_USER_NAME>` and `<PERSONAL_ACCESS_TOKEN>` placeholders with the relevant values:

    ``` shell
    ~> dotnet nuget add source -n GitHub -u <GITHUB_USER_NAME> -p <PERSONAL_ACCESS_TOKEN> https://nuget.pkg.github.com/DamianEdwards/index.json
    ```

1. You should now be able to add a reference to the package specifying a version from the [repository packages feed](https://github.com/DamianEdwards/RazorSlices/pkgs/nuget/RazorSlices)

1. See [these instructions](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry) for further details about working with GitHub package feeds.

## Features

The library is still new and features are being actively added.

### Currently supported

- ASP.NET Core 8.0 and above
- Strongly-typed models (via `@inherits RazorSlice<MyModel>` or `@inherits RazorSliceHttpResult<MyModel>`)
- Razor constructs:
  - [Implicit expressions](https://learn.microsoft.com/aspnet/core/mvc/views/razor#implicit-razor-expressions), e.g. `@someVariable`
  - [Explicit expressions](https://learn.microsoft.com/aspnet/core/mvc/views/razor#implicit-razor-expressions), e.g. `@(someBool ? thisThing : thatThing)`
  - [Control structures](https://learn.microsoft.com/aspnet/core/mvc/views/razor#control-structures), e.g. `@if()`, `@switch()`, etc.
  - [Looping](https://learn.microsoft.com/aspnet/core/mvc/views/razor#looping-for-foreach-while-and-do-while), e.g. `@for`, `@foreach`, `@while`, `@do`
  - [Code blocks](https://learn.microsoft.com/aspnet/core/mvc/views/razor#razor-code-blocks), e.g. `@{ var someThing = someOtherThing; }`
  - [Conditional attribute rendering](https://learn.microsoft.com/aspnet/core/mvc/views/razor#conditional-attribute-rendering)
  - [Functions](https://learn.microsoft.com/aspnet/core/mvc/views/razor#functions):

    ```cshtml
    @functions {
        private readonly string _someString = "A very important string";
        private int DoAThing() => 123;
    }
    ```

  - [Templated Razor methods](https://learn.microsoft.com/aspnet/core/mvc/views/razor#code-try-48), e.g.

    ```cshtml
    @inherits RazorSlice<Todo>

    <h1>@Title(Model)</h1>

    @functions {
        private IHtmlContent Title(Todo todo)
        {
            <text>Todo @todo.Id: @todo.Title</text>
            return HtmlString.Empty;
        }
    }
    ```

  - [Templated Razor delegates](https://learn.microsoft.com/aspnet/core/mvc/views/razor#templated-razor-delegates), e.g.

    ```cshtml
    @inherits RazorSlice<Todo>

    @{
        var tmpl = @<div>
            This is a templated Razor delegate. The following value was passed in: @item
        </div>;
    }

    @tmpl(DateTime.Now)
    ```

    **NOTE: Async templated Razor delegates are *NOT* supported and will throw an exception at runtime**

- DI-activated properties via `@inject`
- Rendering slices from slices (aka partials) via `@(await RenderPartialAsync<MyPartial>())`
- Using slices as layouts for other slices, including layouts with strongly-typed models:
  - For the layout slice, inherit from `RazorLayoutSlice` or `RazorLayoutSlice<TModel>` and use `@await RenderBodyAsync()` in the layout to render the body

      ```cshtml
      @inherits RazorLayoutSlice<LayoutModel>

      <!DOCTYPE html>
      <html lang="en">
      <head>
          <title>@Model.Title</title>
          @await RenderSectionAsync("head")
      </head>
      <body>
        @await RenderBodyAsync()

        <footer>
            @await RenderSectionAsync("footer")
        </footer>
      </body>
      </html>
      ```

  - For the slice using the layout, implement `IUsesLayout<TLayout>` or `IUsesLayout<TLayout, TModel>` to declare which layout to use. If using a layout with a model, ensure you implement the `LayoutModel` property in your `@functions` block, e.g

      ```cshtml
      @inherits RazorSlice<SomeModel>
      @implements IUsesLayout<LayoutSlice, LayoutModel>

      <div>
          @* Content here *@
      </div>

      @functions {
          public LayoutModel LayoutModel => new() { Title = "My Layout" };
      }
      ```

  - Layouts can render sections via `@await RenderSectionAsync("SectionName")` and slices can render content into sections by overriding `ExecuteSectionAsync`, e.g.:

      ```cshtml
      protected override Task ExecuteSectionAsync(string name)
      {
          if (name == "lorem-header")
          {
              <p class="text-info">This page renders a custom <code>IHtmlContent</code> type that contains lorem ipsum content.</p>
          }

          return Task.CompletedTask;
      }
      ```

    **NOTE: The `@section` directive is not supported as it's incompatible with the rendering approach of Razor Slices**

- Asynchronous rendering, i.e. the template can contain `await` statements, e.g. `@await WriteTheThing()`
- Writing UTF8 `byte[]` values directly to the output
- Rendering directly to `PipeWriter`, `Stream`, `TextWriter`, `StringBuilder`, and `string` outputs, including optimizations for not boxing struct values, zero-allocation rendering of primitives like numbers, etc. (rather than just calling `ToString()` on everything)
- Return a slice instance directly as an `IResult` in minimal APIs via `@inherits RazorSliceHttpResult` and `Results.Extensions.RazorSlice("/Slices/Hello.cshtml")`
- Full support for trimming and native AOT when used in conjunction with ASP.NET Core Minimal APIs
- Generated Razor Slice proxy types are `public sealed` by default. To unseal them and make them `public partial` for your own customization, set the `RazorSliceProxiesSealed` property to `false` in your project file, e.g.:
    ``` xml
    <PropertyGroup>
      <RazorSliceProxiesSealed>false</RazorSliceProxiesSealed>
    </PropertyGroup>
    ```

### Interested in supporting but not sure yet

- Extensions to help support using HTMX with Razor Slices 
- Getting small updates to the Razor compiler itself to get some usability and performance improvements, e.g.:
  - Don't mark the template's `ExecuteAsync` method as an `async` method unless the template contains `await` statements to save on the async state machine overhead
  - Support compiling static template elements to UTF8 string literals (`ReadOnlySpan<byte>`) instead of string literals to save on the UTF16 to UTF8 conversion during rendering
  - Support disabling the default registered `@addtaghelper` and `@model` directives which rely on MVC

### No intention to support

- Tag Helpers and View Components (they're tied to MVC and are intrinsically "heavy")
- `@model` directive (the Razor compiler does not support its use in conjunction with custom base-types via `@inherits`)
- `@attribute [Authorize]` (wrong layer of abstraction for minimal APIs, etc.)
- `@section` directive (the Razor compiler emits code that is incompatible with the rendering approach of Razor Slices)