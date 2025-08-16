# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY ["TaskManagerAPI.API/TaskManagerAPI.API.csproj", "TaskManagerAPI.API/"]
COPY ["TaskManagerAPI.Application/TaskManagerAPI.Application.csproj", "TaskManagerAPI.Application/"]
COPY ["TaskManagerAPI.Domain/TaskManagerAPI.Domain.csproj", "TaskManagerAPI.Domain/"]
COPY ["TaskManagerAPI.Infrastructure/TaskManagerAPI.Infrastructure.csproj", "TaskManagerAPI.Infrastructure/"]
COPY ["TaskManagerAPI.Tests/TaskManagerAPI.Tests.csproj", "TaskManagerAPI.Tests/"]

RUN dotnet restore "TaskManagerAPI.API/TaskManagerAPI.API.csproj"

# Копируем весь код и собираем
COPY . .
WORKDIR "/src/TaskManagerAPI.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskManagerAPI.API.dll"]
