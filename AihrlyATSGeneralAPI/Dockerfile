FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AihrlyATSGeneralAPI/AihrlyATSGeneralAPI.csproj", "AihrlyATSGeneralAPI/"]
RUN dotnet restore "AihrlyATSGeneralAPI/AihrlyATSGeneralAPI.csproj"
COPY . .
WORKDIR "/src/AihrlyATSGeneralAPI"
RUN dotnet build "AihrlyATSGeneralAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AihrlyATSGeneralAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AihrlyATSGeneralAPI.dll"]
