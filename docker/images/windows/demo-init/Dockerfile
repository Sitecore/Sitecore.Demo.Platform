#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat
ARG BUILD_IMAGE
ARG BASE_IMAGE
ARG SOLUTION_IMAGE

FROM ${SOLUTION_IMAGE} as solution
FROM ${BASE_IMAGE} AS base

WORKDIR /app

FROM ${BUILD_IMAGE} AS build

WORKDIR /src

COPY ["Sitecore.Demo.Init.csproj", ""]

RUN dotnet restore "./Sitecore.Demo.Init.csproj"

COPY . .

WORKDIR "/src/."

RUN dotnet build "Sitecore.Demo.Init.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "Sitecore.Demo.Init.csproj" -c Release -o /app/publish

FROM base AS final

USER ContainerAdministrator

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV HOST_CD=placeholder
ENV HOST_CM=placeholder
ENV HOST_PS=placeholder
ENV HOST_ID=placeholder
ENV MARKETING_DEFINITIONS_APIKEY=placeholder
ENV ADMIN_USER_NAME=placeholder
ENV ADMIN_PASSWORD=placeholder

WORKDIR /app

COPY --from=solution /solution/src/ ./src

COPY ./sitecore.json .

RUN dotnet new tool-manifest
RUN dotnet nuget add source https://nuget.sitecore.com/resources/v3/index.json --name "Sitecore-Public-Nuget-Feed"
RUN dotnet tool install --add-source=https://nuget.sitecore.com/resources/v3/index.json sitecore.cli --version 5.2.113

COPY --from=publish /app/publish .
RUN mkdir status
ENTRYPOINT ["dotnet", "Sitecore.Demo.Init.dll"]