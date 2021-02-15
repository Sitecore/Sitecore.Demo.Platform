#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat
ARG BUILD_IMAGE
ARG BASE_IMAGE

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

ENV DOTNET_RUNNING_IN_CONTAINER=true

ENV HOST_CD=placeholder
ENV HOST_CM=placeholder
ENV HOST_PS=placeholder
ENV HOST_ID=placeholder
ENV MARKETING_DEFINITIONS_APIKEY=placeholder
ENV ADMIN_USER_NAME=placeholder
ENV ADMIN_PASSWORD=placeholder
ENV COVEO_ORGANIZATION_ID=
ENV COVEO_REGION=
ENV COVEO_API_KEY=
ENV COVEO_SEARCH_API_KEY=
ENV COVEO_FARM_NAME=
ENV COVEO_ADMIN_USER_NAME=

WORKDIR /app

COPY --from=publish /app/publish .
RUN mkdir status
ENTRYPOINT ["dotnet", "Sitecore.Demo.Init.dll"]