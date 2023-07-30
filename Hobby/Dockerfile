FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Hobby/Hobby.csproj", "Hobby/"]
RUN dotnet restore "Hobby/Hobby.csproj"
COPY . .
WORKDIR "/src/Hobby"
RUN dotnet build "Hobby.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hobby.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hobby.dll"]
