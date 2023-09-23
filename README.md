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
[package-shield]: https://img.shields.io/badge/nuget-v.0.7-blue?&label=latests&logo=nuget
[package-url]: https://github.com/Genocs/microservice-template/actions/workflows/build_and_test.yml
[downloads-shield]: https://img.shields.io/nuget/dt/Genocs.CLI.svg?color=2da44e&label=downloads&logo=nuget
[downloads-url]: https://www.nuget.org/packages/Genocs.CLI
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


![webapi banner](https://raw.githubusercontent.com/Genocs/microservice-template/main/assets/genocs-library-logo.png "genocs webapi")

## What's Genocs .NET Web API Microservice Template?

Genocs .NET Web API Microservice Template is a starting point for your next `.NET7 Clean Architecture Project` that incorporates the most essential packages and features your projects will ever need including out of the box Multi-Tenancy support.

> As the name suggests, this is an API / Server Template. You can find other Client Template that consume this API under `@genocs` handle.
> - Find `Blazor WebAssembly Template` here - https://github.com/Genocs/blazor-wasm-template


## YouTube Video - Genocs Library - Getting Started

`Watch the Getting started video here` : https://www.youtube.com/watch?v=TgxDvU---Ak


## Goals

The goal of this repository is to provide a complete and feature-rich starting point for any .NET Developer / Team to kick-start their next major project using .NET 7 Web API. This also serves the purpose of learning advanced concepts and implementations such as `Multitenancy, CQRS, Onion Architecture, Clean Coding standards, Docker Concepts, Cloud Deployments with Terraform to AWS, CI/CD Pipelines & Workflows` and so on.

## Features

- :white_check_mark: Built on .NET 7.0
- :white_check_mark: Follows Clean Architecture Principles
- :white_check_mark: Domain Driven Design
- :white_check_mark: Cloud Ready. Can be deployed to AWS Infrastructure as ECS Containers using Terraform!
- :white_check_mark: Docker-Compose File Examples
- :white_check_mark: Documented at [genocs netlify](https://genocs-blog.netlify.app)
- :white_check_mark: Multi Tenancy Support with Finbuckle
  - :white_check_mark: Create Tenants with Multi Database / Shared Database Support
  - :white_check_mark: Activate / Deactivate Tenants on Demand
  - :white_check_mark: Upgrade Subscription of Tenants - Add More Validity Months to each tenant!
- :white_check_mark: Supports MySQL, MSSQL, Oracle & PostgreSQL!

<details>
  <summary>Click to See More!</summary>

- :white_check_mark: Uses Entity Framework Core as DB Abstraction
- :white_check_mark: Flexible Repository Pattern
- :white_check_mark: Dapper Integration for Optimal Performance
- :white_check_mark: Serilog Integration with various Sinks - File, SEQ, Kibana
- :white_check_mark: OpenAPI - Supports Client Service Generation
- :white_check_mark: Mapster Integration for Quicker Mapping
- :white_check_mark: API Versioning
- :white_check_mark: Response Caching - Distributed Caching + REDIS
- :white_check_mark: Fluent Validations
- :white_check_mark: Audit Logging
- :white_check_mark: Advanced User & Role Based Permission Management
- :white_check_mark: Code Analysis & StyleCop Integration with Rulesets
- :white_check_mark: JSON Based Localization with Caching
- :white_check_mark: Hangfire Support - Secured Dashboard
- :white_check_mark: File Storage Service
- :white_check_mark: Test Projects
- :white_check_mark: JWT & Azure AD Authentication
- :white_check_mark: MediatR - CQRS
- :white_check_mark: SignalR Notifications
- :white_check_mark: & Much More
</details>

## Documentation

Read Documentation related to this Template here - https://genocs-blog.netlify.app/dotnet-templates/
> Feel free to contribute to the Documentation Repository - https://github.com/Genocs/genocs-library-docs

## Getting Started

To get started with this Template, here are the available options.

- Install using the `GENOCS CLI` tool. Use this for release versions of the Template only.
- Fork the Repository. Use this if you want to always keep your version of the Template up-to date with the latest changes.

> Make sure that your DEV enviroment is setup, [Read the Development Environment Guide](https://genocs-blog.netlify.app/dotnet-templates/general/development-environment/)

### GENOCS CLI Tool

#### Prerequisites

Before creating your first solution, you should ensure that your local machine has:

- **.NET 7** You can find the download [here](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).
- **NodeJS (16+)** You can find the download [here](https://nodejs.org/en/download).

#### Installation

After you have installed .NET, you will need to install the `CLI` console tool.

```bash
dotnet tool install -g genocs.cli
genocs install
```

This install the CLI tools and the associated Templates. You are now ready to create your first project!

[Documentation](https://genocs-blog.netlify.app/cli/)

#### Genocs .NET WebAPI Template
Here's how you would create a Solution using the Genocs .NET WebAPI Template.

Simply navigate to a new directory (wherever you want to place your new solution), and open up Command Prompt at the opened directory.

Run the following command. Note that, in this demonstration, I am naming my new solution as `MyCompany.MyProject.MyService`.

```bash
genocs api new MyCompany.MyProject.MyService
```

OR

```bash
genocs api n MyCompany.MyProject.MyService
```

This will create a new .NET 7 WebAPI solution for you using the Template.
For further steps and details, [Read the Getting Started Guide](https://genocs-blog.netlify.app/dotnet-templates/)

#### Update

To update the tool & templates, run the following commands

```bash
dotnet tool update genocs.cli --global
genocs update
```
### Forking the Repository

You would probably need to take this approach if you want to keep your source code up to date with the latest changes. To get started based on this repository, you need to get a copy locally. You have three options: fork, clone, or download.

- Make a fork of this repository in your GitHub account.
- Create your new `dotnet-webapi-template` personal project by cloning the forked repository on your personal GitHub.
- Setup an upstream remote on your personal project pointing to your forked repository using command `git remote add upstream https://github.com/{githubuseraccount}/dotnet-webapi-template` and `git remote set-url --push upstream DISABLE`

For step by step instructions, [follow this](https://discord.com/channels/878181478972928011/892573122186838046/933513103688224838) and [this](https://gist.github.com/0xjac/85097472043b697ab57ba1b1c7530274).


## Quick Start Guide

So, for a better developer experience, I have added Makefile into the solution. Now that our solution is generated, let's navigate to the root folder of the solution and open up a command terminal.

To build the solution,
```
make build
```

By default, the solution is configured to work with postgresql database (mainly because of the OS licensing). So, you will have to make sure that postgresql database instance is up and running on your machine. You can modify the connection string to include your username and password. Connections strings can be found at `src/Host/Configurations/database.json` and `src/Host/Configurations/hangfire.json`. Once that's done, let's start up the API server.

```
make start
```

That's it, the application would connect to the defined postgresql database and start creating tables, and seed required data.

For testing this API, we have 3 options.
1. Swagger @ `localhost:5001/swagger`
2. Postman collections are available `./postman`
3. ThunderClient for VSCode. You will have to install the Thunderclient extension for VSCode.

The default credentials to this API is:


```json
{
    "email":"admin@root.com",
    "password":"123Pa$$word!"
}
```

Open up Postman, Thunderclient or Swagger.

identity -> get-token

This is a POST Request. Here the body of the request will be the JSON (credentials) I specified earlier. And also, remember to pass the tenant id in the header of the request. The default tenant id is `root`.

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
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjM0YTY4ZjQyLWE0ZDgtNDNlMy1hNzE3LTI1OTczZjZmZTJjNyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQHJvb3QuY29tIiwiZnVsbE5hbWUiOiJyb290IEFkbWluIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InJvb3QiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zdXJuYW1lIjoiQWRtaW4iLCJpcEFkZHJlc3MiOiIxMjcuMC4wLjEiLCJ0ZW5hbnQiOiJyb290IiwiaW1hZ2VfdXJsIjoiIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbW9iaWxlcGhvbmUiOiIiLCJleHAiOjE2ODA5NDE3MzN9.VYNaNvk2T4YDvQ3wriXgk2W_Vy9zyEEhjveNauNAeJY",
  "refreshToken": "pyxO30zJK8KelpEXF0vPfbSbjntdlbbnxrZAlUFXfyE=",
  "refreshTokenExpiryTime": "2023-04-15T07:15:33.5187598Z"
}
```

You will need to pass the `token` in the request headers to authenticate calls to the Genocs API!

For further steps and details, [Read the Getting Started Guide](https://genocs-blog.netlify.app/dotnet-templates)

## Containerization

The API project, being .NET 7, it is configured to have built-in support for containerization. That means, you really don't need a Dockerfile to containerize the webapi.

To build a docker image, all you have to do is, ensure that docker-desktop or docker instance is running. And run the following command at the root of the solution.

```
make publish
```

You can also push the docker image directly to dockerhub or any supported registry by using the following command.

```
make publish-to-hub
```
You will have to update your docker registry / repo url in the Makefile though!

## Docker Compose

This project also comes with examples of docker compose files, where you can spin up the webapi and database instance in your local containers with the following commands.

``` powershell
#docker compose up - Boots up the webapi & postgresql container
make dcu 
#docker compose down - Shuts down the webapi & postgresql containers
make dcd 
```

There are also examples for mysql & mssql variations of the fsh webapi. You can find the other docker-compose files under the ./docker-compose folder. Read more about [docker-compose](./docker-compose/README.md) instructions & files here.

## Cloud Deployment with Terraform + AWS ECS

We do support cloud deployment to AWS using terraform. The terraform files are available at the `./terraform` folder.

### Prerequisites

- Install Terraform
- Install & Configure AWS CLI profiles to allow terraform to provision resources for you. Please see this video about [AWS Credentials Management](https://www.youtube.com/watch?v=oY0-1mj4oCo&ab_channel=MukeshMurugan).

In brief, the terraform folder has 2 sub-folders.
- backend
- environments/staging

The Backend folder is internally used by Terraform for state management and locking. There is a one-time setup you have to do against this folder. Navigate to the backend folder and run the command.

```
terraform init
terraform apply -auto-approve
```

This would create the required S3 Buckets and DDB table for you.

Next is the `environments/staging` folder. Here too, run the following command.

```
terraform init
```

Once done, you can go the terraform.tfvars file to change the variables like,
- project tags
- docker image name
- ecs cluster name and so on.

After that, simply go back to the root of the solution and run the following command.

```
make ta
```

This will evaluate your terraform files and create a provision plan for you. Once you are ok, type in `yes` and the tool will start to deploy your .NET WebAPI project as containers along with a RDS PostgreSQL instance. You will be receiving the hosted api url once the provisioning is completed!

To destroy the deployed resources, run the following
```
make td
```

## Links & Documentations

[Overview](https://https://genocs-blog.netlify.app/dotnet-templates/general/overview/)

[Getting Started](https://https://genocs-blog.netlify.app/dotnet-templates/general/getting-started/)

[Development Environment](https://https://genocs-blog.netlify.app/dotnet-templates/general/development-environment/)

[Participate in QNA & General Discussions](https://github.com/Genocs/microservice-template/discussions)

## License

This project is licensed with the [MIT license](LICENSE).

## Changelogs

View Complete [Changelogs](https://github.com/Genocs/microservice-template/blob/main/CHANGELOGS.md).

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
- ☕ If you want to support this project in the long run, [consider buying me a coffee](https://www.buymeacoffee.com/genocs)!
  

[![buy-me-a-coffee](https://raw.githubusercontent.com/Genocs/microservice-template/main/media/buy-me-a-coffee.png "buy-me-a-coffee")](https://www.buymeacoffee.com/genocs)

## Code Contributors

This project exists thanks to all the people who contribute. [Submit your PR and join the team!](CONTRIBUTING.md)

[![genocs contributors](https://contrib.rocks/image?repo=Genocs/microservice-template "genocs contributors")](https://github.com/genocs/microservice-template/graphs/contributors)

## Financial Contributors

Become a financial contributor and help me sustain the project. [Support the Project!](https://opencollective.com/genocs/contribute)

<a href="https://opencollective.com/genocs"><img src="https://opencollective.com/genocs/individuals.svg?width=890"></a>


## Acknowledgements

- [Mukesh Murugan](https://github.com/iammukeshm)
- [FullStackHero](https://fullstackhero.net)
