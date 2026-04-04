FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Groups/Groups.csproj", "src/Groups/"]
COPY ["shared/SharedLibraries/SharedLibraries.csproj", "shared/SharedLibraries/"]
RUN dotnet restore "src/Groups/Groups.csproj"
COPY . .
RUN dotnet publish "src/Groups/Groups.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Groups.dll"]