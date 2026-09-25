FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Integration.OutboxPublisher/Integration.OutboxPublisher.csproj"

RUN dotnet publish "Integration.OutboxPublisher/Integration.OutboxPublisher.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Integration.OutboxPublisher.dll"]