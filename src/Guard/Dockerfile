FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Guard/Guard.csproj", "src/Guard/"]
COPY ["shared/SharedLibraries/SharedLibraries.csproj", "shared/SharedLibraries/"]
RUN dotnet restore "src/Guard/Guard.csproj"
COPY . .
RUN dotnet publish "src/Guard/Guard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Guard.dll"]