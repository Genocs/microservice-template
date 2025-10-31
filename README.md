<!-- PROJECT SHIELDS -->

[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
[![Packages][package-shield]][package-url]
[![Downloads][downloads-shield]][downloads-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Discord][discord-shield]][discord-url]
[![Gitter][gitter-shield]][gitter-url]
[![Twitter][twitter-shield]][twitter-url]
[![Twitterx][twitterx-shield]][twitterx-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

[license-shield]: https://img.shields.io/github/license/Genocs/microservice-template?color=2da44e&style=flat-square
[license-url]: https://github.com/Genocs/microservice-template/blob/main/LICENSE
[build-shield]: https://github.com/Genocs/microservice-template/actions/workflows/build_and_test.yml/badge.svg?branch=main
[build-url]: https://github.com/Genocs/microservice-template/actions/workflows/build_and_test.yml
[package-shield]: https://img.shields.io/badge/nuget-v.3.1.0-blue?&label=latests&logo=nuget
[package-url]: https://github.com/Genocs/microservice-template/actions/workflows/build_and_test.yml
[downloads-shield]: https://img.shields.io/nuget/dt/Genocs.Microservice.Template.svg?color=2da44e&label=downloads&logo=nuget
[downloads-url]: https://www.nuget.org/packages/Genocs.Microservice.Template
[contributors-shield]: https://img.shields.io/github/contributors/Genocs/microservice-template.svg?style=flat-square
[contributors-url]: https://github.com/Genocs/microservice-template/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Genocs/microservice-template?style=flat-square
[forks-url]: https://github.com/Genocs/microservice-template/network/members
[stars-shield]: https://img.shields.io/github/stars/Genocs/microservice-template.svg?style=flat-square
[stars-url]: https://img.shields.io/github/stars/Genocs/microservice-template?style=flat-square
[issues-shield]: https://img.shields.io/github/issues/Genocs/microservice-template?style=flat-square
[issues-url]: https://github.com/Genocs/microservice-template/issues
[discord-shield]: https://img.shields.io/discord/1106846706512953385?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square
[discord-url]: https://discord.com/invite/fWwArnkV
[gitter-shield]: https://img.shields.io/badge/chat-on%20gitter-blue.svg
[gitter-url]: https://gitter.im/genocs/
[twitter-shield]: https://img.shields.io/twitter/follow/genocs?color=1DA1F2&label=Twitter&logo=Twitter&style=flat-square
[twitter-url]: https://twitter.com/genocs
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=flat-square&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/
[twitterx-shield]: https://img.shields.io/twitter/url/https/twitter.com/genocs.svg?style=social
[twitterx-url]: https://twitter.com/genocs

<!-- PROJECT LOGO -->
<p align="center">
  <a href="https://github.com/Genocs/microservice-template">
    <img src="https://raw.githubusercontent.com/Genocs/microservice-template/main/assets/genocs-library-logo.png" alt=".NET Microservice Template">
  </a>
  <h3 align="center">.NET Microservice Template</h3>
  <p align="center">
    Open Source Solution Template For .NET9 Microservice
    <br />
    <a href="https://genocs-blog.netlify.app/microservice-template/general/getting-started/"><strong>Read the Documentation »</strong></a>
    <br />
    <br />
    <a href="https://github.com/Genocs/microservice-template/issues">Report Bug</a>
    ·
    <a href="https://github.com/Genocs/microservice-template/issues">Request Feature</a>
    .
    <a href="https://github.com/Genocs/microservice-template/issues">Request Documentation</a>
  </p>
</p>

# Genocs .NET Web API Microservice Template

Genocs .NET Web API Microservice Template is a starting point for your next `.NET9 Clean Architecture Project` that incorporates the most essential packages and features your projects will ever need including out of the box Multi-Tenancy support.

> As the name suggests, this is an API / Server Template. You can find other Client Template that consume this API under `@genocs` handle.
>
> - Find `Blazor Wasm Template` here - [Blazor Wasm Template](https://github.com/Genocs/blazor-wasm-template)

The template can be used with the `genocs cli`, `dotnet new` command or with the `Visual Studio 2022`, `Visual Studio Code` IDEs.

## 📋 Table of Contents

- [✨ Features](#✨features)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Template Options](#template-options)
- [Architecture Overview](#architecture-overview)
- [Development Workflow](#development-workflow)
- [Examples](#examples)
- [Troubleshooting](#troubleshooting)
- [Community & Support](#community--support)
- [Contributing](#contributing)
- [License](#license)

## ✨ Features

- Clean architecture template spanning Domain, Application, Infrastructure, and WebApi projects.
- Built-in multi-tenant foundation (Finbuckle) with per-tenant and shared database strategies.
- Production-ready infrastructure: background jobs, caching, localization, auditing, and real-time notifications.
- Developer tooling out of the box: Makefile automation, Terraform modules, Docker Compose, Postman and Thunder collections.

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (latest version)
- **IDE** (choose one):
  - [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (recommended)
  - [Visual Studio Code](https://code.visualstudio.com/) with C# extension
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
- **Optional for development**:
  - [Docker Desktop](https://www.docker.com/products/docker-desktop) for containerization
  - MongoDB, SQL Server
  - [NodeJS (16+)](https://nodejs.org/en/download)

## 🚀 Quick Start

### Install the Template

```bash
# Install the latest version
dotnet new install Genocs.Microservice.Template

# Or install a specific version
dotnet new install Genocs.Microservice.Template::3.1.0

# View all available options
dotnet new gnx-microservice --help

# Example with custom options
dotnet new gnx-microservice\
  --name "CompanyName.ServiceName" \
```

## 🏗️ Architecture Overview

The template generates a solution with the following structure:

```pl
src/
├── Application/             # CQRS commands, queries, validations, business workflows
├── Contracts/               # Shared DTOs, integration events, notifications, permissions
├── Domain/                  # Entities, aggregates, domain events, value objects
├── Infrastructure/          # EF Core persistence, caching, jobs, external integrations
├── Infrastructure.Tests/    # Automated tests for persistence, validation, caching
├── Migrators.MSSQL/         # SQL Server migrations and tooling
├── Migrators.MySQL/         # MySQL migrations and tooling
├── Migrators.Oracle/        # Oracle migrations and tooling
├── Migrators.PostgreSQL/    # PostgreSQL migrations and tooling
├── Migrators.SqLite/        # SQLite migrations and tooling
└── WebApi/                  # Host application, configuration, controllers, SignalR hub
```

## Features

- :white_check_mark: Built on [.NET 9](https://dotnet.microsoft.com/en-us/) with Clean Architecture layering (Domain, Application, Infrastructure, WebApi).
- :white_check_mark: Domain-driven design with CQRS (MediatR), Mapster mappings, FluentValidation, and specification support.
- :white_check_mark: Multi-tenancy powered by [Finbuckle](https://www.finbuckle.com/) with tenant management APIs, shared/per-tenant databases, and subscription upgrades.
- :white_check_mark: Database providers for PostgreSQL, SQL Server, MySQL, Oracle, and SQLite via Entity Framework Core 9.
- :white_check_mark: Optimized read path with integrated [Dapper](https://www.learndapper.com/) repository alongside EF Core.
- :white_check_mark: Background job processing through Hangfire with dashboard support and multi-provider storage.
- :white_check_mark: Observability with Serilog (console, file, Seq, Elasticsearch, AWS), structured logging, and auditing pipeline.
- :white_check_mark: Distributed caching abstractions with StackExchange Redis and per-tenant cache segregation.
- :white_check_mark: Authentication via JWT bearer tokens and Azure AD, plus fine-grained permission-based authorization.
- :white_check_mark: Localization using OrchardCore PO files, configurable cultures, and UI-ready resource catalogs.
- :white_check_mark: OpenAPI/NSwag integration for client generation, versioned APIs, and security schemas.
- :white_check_mark: Excel export (ClosedXML), file storage abstractions, Razor-based email templates, and MailKit delivery.
- :white_check_mark: Real-time notifications with SignalR and Redis backplane support.
- :white_check_mark: Tooling for real-world deployments: Makefile automation, Docker Compose samples, Terraform modules, Postman & Thunder collections.
- :white_check_mark: Code analysis & StyleCop configuration, plus Infrastructure-focused automated tests.
- :white_check_mark: Comprehensive documentation hosted on [genocs netlify](https://genocs-blog.netlify.app) and an active community.

## How to build and install the template locally

## How to build the template

Check nuget is installed on your machine. To download nuget, visit [nuget.org](https://www.nuget.org/downloads)

- Download the nuget latest version. At the time of writing this, the latest version is nuget.exe v6.7.0
- Add the nuget.exe to your PATH environment variable.
- run the following commands

To build the package run the following commands:

[custom-templates](https://docs.microsoft.com/en-us/dotnet/core/tools/custom-templates)

[dotnet-templating](https://github.com/dotnet/templating)

```bash
# To clone the repository
git clone https://github.com/Genocs/microservice-template
cd microservice-template

# To pack and build the template
dotnet pack ./src/Package.Template.csproj -p:PackageVersion=3.1.0 --configuration Release --output ./out

# To install the template
dotnet new install ./out/Genocs.Microservice.Template.3.1.0.nupkg

# To get the list of available templates
dotnet new gnx-microservice --help

# To uninstall the template
dotnet new uninstall Genocs.Microservice.Template

# Example of creating a new project with full functionality
dotnet new gnx-microservice --name {CompanyName.ServiceName} -gc full
```

### Miscellaneous

Useful commands:

```bash
# How to get the list of installed templates
dotnet new -u

# How to get the list of templates
dotnet new list
```

## Documentation

Read Documentation related to this template here - [Template Documentation](https://genocs-blog.netlify.app/microservice-template/)

> Feel free to contribute to the Documentation Repository - [Contribute Documentation](https://github.com/Genocs/genocs-library-docs)

## Getting Started

To get started with this Template, here are the available options:

- Install using the `genocs cli` tool. Use this for latest release versions of the Template only.
- Install using the `dotnet new install` tool. Use this for any versions of the Template.
- Fork the Repository. Use this if you want to always keep your version of the Template up-to date with the latest changes.

> Make sure that your DEV enviroment is setup, [Read the Development Environment Guide](https://genocs-blog.netlify.app/microservice-template/general/development-environment/)

### GENOCS CLI Tool

#### **Installation**

After you have installed .NET, you will need to install the `cli` console tool.

```bash
dotnet tool install -g genocs.cli
genocs install
```

This install the CLI tools and the associated Templates. You are now ready to create your first project!

[Documentation](https://genocs-blog.netlify.app/cli/)

#### Let get started

Here's how you would create a Solution using the Genocs .NET WebAPI Template.

Simply navigate to a new directory (wherever you want to place your new solution), and open up _bash prompt_ at the opened directory.

Run the following command. Note that, in this demonstration, I am naming my new solution as `CompanyName.ProjectName.ServiceName`.

```bash
genocs micro-webapi [n|new] CompanyName.ProjectName.ServiceName
```

This will create a new .NET9 Web API solution for you using the template.
For further steps and details, [Read the Getting Started Guide](https://genocs-blog.netlify.app/microservice-template/)

#### Update

To update the tool & templates, run the following commands

```bash
dotnet tool update genocs.cli --global
genocs update
```

### Forking the Repository

You would probably need to take this approach if you want to keep your source code up to date with the latest changes. To get started based on this repository, you need to get a copy locally. You have three options: `fork, clone, or download`.

- Make a fork of this repository in your GitHub account.
- Create your new `microservice-template` personal project by cloning the forked repository on your personal GitHub.
- Setup an upstream remote on your personal project pointing to your forked repository using command `git remote add upstream https://github.com/{githubuseraccount}/microservice-template` and `git remote set-url --push upstream DISABLE`

For step by step instructions, follow: [this](https://discord.com/channels/878181478972928011/892573122186838046/933513103688224838) and [this](https://gist.github.com/0xjac/85097472043b697ab57ba1b1c7530274).

### Run the template

## Makefile

So, for a better developer experience, I have added Makefile into the solution. Now that our solution is generated, let's navigate to the root folder of the solution and open up a command terminal.

To build the solution:

```bash
make build
```

By default, the solution is configured to work with postgresql database (mainly because of the OS licensing). So, you will have to make sure that postgresql database instance is up and running on your machine. You can modify the connection string to include your username and password. Connections strings can be found at `src/WebApi/Configurations/database.json` and `src/WebApi/Configurations/hangfire.json`. Once that's done, let's start up the API server.

```bash
make start
```

That's it, the application would connect to the defined postgresql database and start creating tables, and seed required data.

For testing this API, we have 3 options:

1. Swagger @ `localhost:5001/swagger`
2. Postman collections are available `./postman`
3. ThunderClient for VSCode. You will have to install the [Thunderclient](https://www.thunderclient.com/) extension for VSCode.

The default credentials to this API is:

```json
{
  "email": "admin@root.com",
  "password": "123Pa$$word!"
}
```

Open up Postman, Thunderclient or Swagger.

identity -> get-token

This is a **POST** Request. Here the body of the request will be the JSON (credentials) I specified earlier. And also, remember to pass the tenant id in the header of the request. The default tenant id is `root`.

Here is a sample CURL command for getting the tokens.

```curl
curl -X POST \
  'https://localhost:5001/api/tokens' \
  --header 'Accept: */*' \
  --header 'tenant: root' \
  --header 'Accept-Language: en-US' \
  --header 'Content-Type: application/json' \
  --data-raw '{
  "email": "admin@root.com",
  "password": "123Pa$$word!"
}'
```

And here is the response.

```json
{
  "token": "<your token>",
  "refreshToken": "<your refresh token>",
  "refreshTokenExpiryTime": "2025-01-01T00:00:00.0000000Z"
}
```

You will need to pass the `token` in the request headers to authenticate calls to the Genocs API!

For further steps and details, [Read the Getting Started Guide](https://genocs-blog.netlify.app/microservice-template)

## Containerization

The project, being .NET9, it is configured to have built-in support for containerization. That means, you really don't need a `dockerfile` to containerize the webapi.

To build a docker image, all you have to do is, ensure that docker-desktop or docker instance is running. And run the following command at the root of the solution.

```bash
make publish
```

You can also push the docker image directly to dockerhub or any supported registry by using the following command.

```bash
make publish-to-hub
```

You will have to update your docker registry/repo url in the Makefile though!

## Docker Compose

This project also comes with examples of docker compose files, where you can spin up the webapi and database instance in your local containers with the following commands.

```bash
#docker compose up - Boots up the webapi & postgresql container
make dcu
#docker compose down - Shuts down the webapi & postgresql containers
make dcd
```

There are also examples for MySQL & MSSQL variations. You can find the compose files under the `./infrastructure` folder. Read more about the compose scenarios in [infrastructure/docker/README.md](./infrastructure/docker/README.md).

## Cloud Deployment with Terraform + AWS ECS

We do support cloud deployment to AWS using terraform. The terraform files are available at the `./terraform` folder.

### Prerequisites

- Install Terraform
- Install & Configure AWS CLI profiles to allow terraform to provision resources for you. Please see this video about [AWS Credentials Management](https://www.youtube.com/watch?v=oY0-1mj4oCo&ab_channel=MukeshMurugan).

In brief, the terraform folder has 2 sub-folders:

- backend
- environments/staging

The Backend folder is internally used by Terraform for state management and locking. There is a one-time setup you have to do against this folder. Navigate to the backend folder and run the command.

```bash
terraform init
terraform apply -auto-approve
```

This would create the required S3 Buckets and DDB table for you.

Next is the `environments/staging` folder. Here too, run the following command.

```bash
terraform init
```

Once done, you can go the terraform.tfvars file to change the variables like:

- project tags
- docker image name
- ecs cluster name and so on.

After that, simply go back to the root of the solution and run the following command.

```bash
make ta
```

This will evaluate your terraform files and create a provision plan for you. Once you are ok, type in `yes` and the tool will start to deploy your .NET Microservice project as containers along with a RDS PostgreSQL instance. You will be receiving the hosted api url once the provisioning is completed!

To destroy the deployed resources, run the following

```bash
make td
```

## Links & Documentations

[Overview](https://genocs-blog.netlify.app/microservice-template/general/overview/)

[Getting Started](https://genocs-blog.netlify.app/microservice-template/general/getting-started/)

[Development Environment](https://genocs-blog.netlify.app/microservice-template/general/development-environment/)

[Participate in QNA & General Discussions](https://github.com/Genocs/microservice-template/discussions)

## Changelogs

View Complete [Changelogs](https://github.com/Genocs/microservice-template/blob/main/CHANGELOG.md).

## License

This project is licensed with the [MIT license](LICENSE).

## Community

- Discord [@genocs](https://discord.com/invite/fWwArnkV)
- Facebook Page [@genocs](https://facebook.com/Genocs)
- Youtube Channel [@genocs](https://youtube.com/c/genocs)

## Support

Has this Project helped you learn something New? or Helped you at work?

Here are a few ways by which you can support.

- ⭐ Leave a star!
- 🥇 Recommend this project to your colleagues.
- 🦸 Do consider endorsing me on LinkedIn for ASP.NET Core - [Connect via LinkedIn](https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/)
- ☕ If you want to support this project in the long run, consider [buying me a coffee](https://www.buymeacoffee.com/genocs)!

[![buy-me-a-coffee](https://raw.githubusercontent.com/Genocs/microservice-template/main/assets/buy-me-a-coffee.png "buy me a coffee")](https://www.buymeacoffee.com/genocs)

## Code Contributors

This project exists thanks to all the people who contribute. [Submit your PR and join the team!](CONTRIBUTING.md)

[![genocs contributors](https://contrib.rocks/image?repo=Genocs/microservice-template "genocs contributors")](https://github.com/Genocs/microservice-template/graphs/contributors)

## Financial Contributors

Become a financial contributor and help me sustain the project.

**Support the Project** on [Opencollective](https://opencollective.com/genocs)

## Acknowledgements

- [Mukesh Murugan](https://github.com/iammukeshm)
- [FullStackHero](https://fullstackhero.net)
