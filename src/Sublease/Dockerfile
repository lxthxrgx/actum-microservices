FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Sublease/Sublease.csproj", "src/Sublease/"]
COPY ["shared/SharedLibraries/SharedLibraries.csproj", "shared/SharedLibraries/"]
RUN dotnet restore "src/Sublease/Sublease.csproj"
COPY . .
RUN dotnet publish "src/Sublease/Sublease.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sublease.dll"]