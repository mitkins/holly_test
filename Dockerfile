# Latest .NET Core from https://hub.docker.com/_/microsoft-dotnet-core-sdk/ (not the nightly one)
FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview6-disco AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore "./HollyTest.Server/HollyTest.Server.csproj"
RUN dotnet publish "./HollyTest.Server/HollyTest.Server.csproj" -c Release -o out

# Latest ASP.NET Core from https://hub.docker.com/_/microsoft-dotnet-core-aspnet/ (not the nightly one)
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview6-disco
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
WORKDIR /app
COPY --from=build-env /app/out .
# From https://stackoverflow.com/a/56856274/23401
COPY --from=build-env /app/out/wwwroot/_content/hollytestclient ./wwwroot/
ENTRYPOINT ["dotnet", "HollyTest.Server.dll"]
