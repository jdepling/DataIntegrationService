FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Integration.Admin/Integration.Admin.csproj", "Integration.Admin/"]
COPY ["Integration.Data/Integration.Data.csproj", "Integration.Data/"]

RUN dotnet restore "Integration.Admin/Integration.Admin.csproj"

COPY . .
WORKDIR "/src/Integration.Admin"
RUN dotnet build "Integration.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Integration.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Integration.Admin.dll"]