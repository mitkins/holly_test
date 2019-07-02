# FROM node:10.13.0-alpine as node
# WORKDIR /app
# COPY public ./public
# COPY src/index.js ./src/index.js
# COPY package*.json ./
# RUN npm install --progress=true --loglevel=silent
# COPY src/client ./src/client/
# RUN npm run build
#
# FROM microsoft/dotnet:2.2-sdk-alpine AS builder
# WORKDIR /source
# COPY . .
# RUN dotnet restore
# RUN dotnet publish -c Release -r linux-musl-x64 -o /app
#
# FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
# ENV ASPNETCORE_URLS=http://+:5000
# EXPOSE 5000
# WORKDIR /app
# COPY --from=builder /app .
# COPY --from=node /app/build ./wwwroot
# ENTRYPOINT ["./AspNetCoreDemoApp"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-buster AS build
WORKDIR /app
# Copy everything and build
COPY . ./
RUN dotnet restore "./HollyTest.Server/HolyTest.Server.csproj"
RUN dotnet publish "./HollyTest.Server/HolyTest.Server.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview5-buster-slim
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "HollyTest.Server.dll"]
