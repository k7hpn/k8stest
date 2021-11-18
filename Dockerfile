#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["k8stest/k8stest.csproj", "k8stest/"]
RUN dotnet restore "k8stest/k8stest.csproj"
COPY . .
WORKDIR "/src/k8stest"
RUN dotnet build "k8stest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "k8stest.csproj" -c Release -o /app/publish

FROM base AS final

# Bring in metadata via --build-arg to publish
ARG BRANCH=unknown
ARG IMAGE_CREATED=unknown
ARG IMAGE_REVISION=unknown
ARG IMAGE_VERSION=unknown

# Configure image labels
LABEL branch=$branch \
    maintainer="Harald Nagel <dev@hpn.is>" \
    org.opencontainers.image.authors="Harald Nagel <dev@hpn.is>" \
    org.opencontainers.image.created=$IMAGE_CREATED \
    org.opencontainers.image.description="Basic test of Kubernetes with ASP.NET Core" \
    org.opencontainers.image.documentation="https://github.com/k7hpn/k8stest" \
    org.opencontainers.image.licenses="MIT" \
    org.opencontainers.image.revision=$IMAGE_REVISION \
    org.opencontainers.image.source="https://github.com/k7hpn/k8stest" \
    org.opencontainers.image.title="k8stest" \
    org.opencontainers.image.url="https://github.com/k7hpn/k8stest" \
    org.opencontainers.image.version=$IMAGE_VERSION

# Default image environment variable settings
ENV org.opencontainers.image.created=$IMAGE_CREATED \
    org.opencontainers.image.revision=$IMAGE_REVISION \
    org.opencontainers.image.version=$IMAGE_VERSION


WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "k8stest.dll"]
