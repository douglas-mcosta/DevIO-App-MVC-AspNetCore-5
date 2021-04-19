#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/DevIO.App/DevIO.App.csproj", "src/DevIO.App/"]
COPY ["src/DevIO.Data/DevIO.Data.csproj", "src/DevIO.Data/"]
COPY ["src/DevIO.Business/DevIO.Business.csproj", "src/DevIO.Business/"]
RUN dotnet restore "src/DevIO.App/DevIO.App.csproj"
COPY . .
WORKDIR "/src/src/DevIO.App"
RUN dotnet build "DevIO.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DevIO.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevIO.App.dll"]