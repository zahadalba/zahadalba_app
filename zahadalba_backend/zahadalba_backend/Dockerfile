# --- Этап 1: Сборка (Build) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["zahadalba_backend.csproj", "./"]
RUN dotnet restore "zahadalba_backend.csproj"

COPY . .
RUN dotnet publish "zahadalba_backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Этап 2: Финальный образ (Runtime) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "zahadalba_backend.dll"]