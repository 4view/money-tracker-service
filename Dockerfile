# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Копируем solution и все csproj для восстановления зависимостей (кэш-слой)
COPY money-tracker-service.sln ./
COPY src/MoneyTracker.Api/MoneyTracker.Api.csproj          src/MoneyTracker.Api/
COPY src/MoneyTracker.Application/MoneyTracker.Application.csproj  src/MoneyTracker.Application/
COPY src/MoneyTracker.Core/MoneyTracker.Core.csproj        src/MoneyTracker.Core/
COPY src/MoneyTracker.Data/MoneyTracker.Data.csproj        src/MoneyTracker.Data/

RUN dotnet restore

# Копируем весь исходный код
COPY src/ src/

# Публикуем в Release
RUN dotnet publish src/MoneyTracker.Api/MoneyTracker.Api.csproj \
    -c Release \
    -o /app/publish

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем только опубликованные файлы
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MoneyTracker.Api.dll"]