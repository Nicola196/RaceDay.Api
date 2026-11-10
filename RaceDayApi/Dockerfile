FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RaceDayApi/RaceDayApi.csproj", "RaceDayApi/"]
RUN dotnet restore "RaceDayApi/RaceDayApi.csproj"
COPY . .
WORKDIR "/src/RaceDayApi"
RUN dotnet publish "RaceDayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDayApi.dll"]
