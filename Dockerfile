FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app/src
COPY . .

RUN dotnet build --output /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app/build

COPY --from=build /app/build .

ENTRYPOINT [ "dotnet", "MoneyTracker.Api.dll" ]