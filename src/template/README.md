<!-- PROJECT SHIELDS -->
[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
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

[license-shield]: https://img.shields.io/github/license/Genocs/genocs-library?color=2da44e&style=flat-square
[license-url]: https://github.com/Genocs/genocs-library/blob/main/LICENSE
[build-shield]: https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg?branch=main
[build-url]: https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml
[downloads-shield]: https://img.shields.io/nuget/dt/Genocs.Microservice.Template.svg?color=2da44e&label=downloads&logo=nuget
[downloads-url]: https://www.nuget.org/packages/Genocs.Microservice.Template
[contributors-shield]: https://img.shields.io/github/contributors/Genocs/genocs-library.svg?style=flat-square
[contributors-url]: https://github.com/Genocs/genocs-library/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Genocs/genocs-library?style=flat-square
[forks-url]: https://github.com/Genocs/genocs-library/network/members
[stars-shield]: https://img.shields.io/github/stars/Genocs/genocs-library.svg?style=flat-square
[stars-url]: https://img.shields.io/github/stars/Genocs/genocs-library?style=flat-square
[issues-shield]: https://img.shields.io/github/issues/Genocs/genocs-library?style=flat-square
[issues-url]: https://github.com/Genocs/genocs-library/issues
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


<p align="center">
    <img src="./assets/genocs-library-logo.png" alt="icon">
</p>


# Genocs .NET library

This repo contains a set of libraries designed by Genocs. The library is built on top of .NET8. 

Packages are available on [NuGet Genocs](https://www.nuget.org/profiles/gioema_nocco).

## The idea

***Build a software library to be cloud agnostic***

Building a software library to be cloud agnostic has several advantages. First, it allows developers to create applications that can be deployed on any cloud platform without having to rewrite code or make major changes. This makes it easier for developers to quickly deploy their applications across multiple cloud providers. Additionally, it reduces the cost of development and maintenance since developers don’t have to write separate code for each cloud provider. Finally, it increases the scalability of applications since they can be easily deployed on different cloud platforms with minimal effort. 

***Cloud agnostic by use of Containers***

The advantages of using containers are numerous. Containers provide a lightweight, portable, and isolated environment for applications to run in, allowing them to be easily moved between different systems. This makes it easier to deploy applications quickly and reliably across different environments. Additionally, containers can help reduce resource consumption by running multiple applications on the same host, as each container is isolated from the others. This helps to improve efficiency and scalability. Finally, containers provide an additional layer of security, as they are isolated from the underlying operating system and other applications.

## Infrastructure

In this section you can find the infrastructure components to setup the environment.
You will use ***Docker compose*** to setup the infrastructure components.


``` bash
cd ./containers
# Setup the infrastructure
docker compose -f ./infrastructure-bare.yml --env-file ./.env --project-name genocs up -d
docker compose -f ./infrastructure-monitoring.yml --env-file ./.env --project-name genocs up -d
docker compose -f ./infrastructure-scaling.yml --env-file ./.env --project-name genocs up -d
docker compose -f ./infrastructure-security.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup sqlserver database (no need if you use postgres)
docker compose -f ./infrastructure-sqlserver.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup elk stack
docker compose -f ./infrastructure-elk.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup AI ML components prepared by Genocs
docker compose -f ./infrastructure-ml.yml --env-file ./.env --project-name genocs up -d
```

`infrastructure-bare.yml` allows to install the basic infrastructure components. Basic components are the [RabbitMQ](https://rabbitmq.com), [Redis](https://redis.io), [Mongo](https://mongodb.com), [Postgres](https://www.postgresql.org/).


- [rabbitmq](http://localhost:15672/)
- Redis
- MongoDb
- PostgreSQL


`infrastructure-monitoring.yml` allows to install the monitoring infrastructure components.

Inside the file you can find:

- Prometheus
- Grafana
- influxdb
- jaeger
- seq

`infrastructure-scaling.yml` allows to install the scaling infrastructure components.

Inside the file you can find:

- Fabio
- consul

`infrastructure-security.yml` allows to install the security infrastructure components.

Inside the file you can find:

- vault (Hashicorp)

The script below allows to setup the infrastructure components. This means that you can find all the containers inside the same network.

The network is called `genocs`.

``` yml 
networks:
  genocs:
    name: genocs-network
    driver: bridge
```

Remember to add the network configuration inside your docker compose file to setup the network, before running the containers.


## ***Kubernetes cluster***

You can setup the application inside a Kubernetes cluster.

Check the repo [enterprise-containers](https://github.com/Genocs/enterprise-containers) to setup a Kubernetes cluster. 
There you can find scripts, configuration files and documentation to setup a cluster from scratch.

## **Libraries**
You can find a full documentation on:
[**Documentation**](https://genocs-blog.netlify.app/library/)



## Support



Use [**api-workbench**](./api-workbench.rest) inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin 

## Configuration

``` json

```
---

## Build Solution

Following are the step to build the solution.
``` bash
# Build the solution 
dotnet build

# Run project with console
dotnet run --project ./src/WebApi
```

### How to build Docker image

``` bash
# Build webapi
docker build -t genocs/genocs.microservice.template-webapi:2.0.0 -t genocs/genocs.microservice.template-webapi:latest -f ./dockerfile .

# Push on Dockerhub
docker push genocs/genocs.microservice.template-webapi:2.0.0
docker push genocs/genocs.microservice.template-webapi:latest
```
---

### How to BUILD & RUN the application

The build and run process can be done by using docker-compose

``` bash

# Build with docker compose
docker compose -f ./docker-compose.yml -f ./docker-compose.override.yml --env-file ./local.env --project-name genocs-app build

# *** Before running the solution remember to check ***
# *** if the infrastructure services were setup     ***

# Run with docker compose
docker compose -f ./docker-compose.yml --env-file ./local.env --project-name genocs-app up -d

# Clean Docker cache
docker builder prune
```


### Deploy in a cloud instance

You can deploy Demo Application with one click in Heroku, Microsoft Azure, or Google Cloud Platform: 

[<img src="https://www.herokucdn.com/deploy/button.svg" height="30px">](https://heroku.com/deploy?template=https://github.com/heartexlabs/label-studio/tree/heroku-persistent-pg)
[<img src="https://aka.ms/deploytoazurebutton" height="30px">](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fheartexlabs%2Flabel-studio%2Fmaster%2Fazuredeploy.json)
[<img src="https://deploy.cloud.run/button.svg" height="30px">](https://deploy.cloud.run)

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
  

[![buy-me-a-coffee](https://raw.githubusercontent.com/Genocs/blazor-template/main/assets/buy-me-a-coffee.png "buy-me-a-coffee")](https://www.buymeacoffee.com/genocs)

## Code Contributors

This project exists thanks to all the people who contribute. [Submit your PR and join the team!](CONTRIBUTING.md)

[![genocs contributors](https://contrib.rocks/image?repo=Genocs/blazor-template "genocs contributors")](https://github.com/genocs/blazor-template/graphs/contributors)

## Financial Contributors

Become a financial contributor and help me sustain the project. [Support the Project!](https://opencollective.com/genocs/contribute)

<a href="https://opencollective.com/genocs"><img src="https://opencollective.com/genocs/individuals.svg?width=890"></a>


## Acknowledgements
- [devmentors](https://github.com/devmentors)
- [abp](https://github.com/abpframework)


- simple changes