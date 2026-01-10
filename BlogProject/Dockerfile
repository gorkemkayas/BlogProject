FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BlogProject/BlogProject.Web.csproj", "BlogProject/"]
COPY ["BlogProject.Application/BlogProject.Application.csproj", "BlogProject.Application/"]
COPY ["BlogProject.Domain/BlogProject.Domain.csproj", "BlogProject.Domain/"]
COPY ["BlogProject.Infrastructure/BlogProject.Infrastructure.csproj", "BlogProject.Infrastructure/"]
RUN dotnet restore "./BlogProject/BlogProject.Web.csproj"
COPY . .
WORKDIR "/src/BlogProject"
RUN dotnet build "./BlogProject.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlogProject.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogProject.Web.dll"]
