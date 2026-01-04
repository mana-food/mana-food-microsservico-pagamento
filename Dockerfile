# Multi-stage Dockerfile para ManaFood Payment API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/ManaFoodPayment.Api/ManaFoodPayment.Api.csproj", "ManaFoodPayment.Api/"]
COPY ["src/ManaFoodPayment.Application/ManaFoodPayment.Application.csproj", "ManaFoodPayment.Application/"]
COPY ["src/ManaFoodPayment.Domain/ManaFoodPayment.Domain.csproj", "ManaFoodPayment.Domain/"]
COPY ["src/ManaFoodPayment.Infrastructure/ManaFoodPayment.Infrastructure.csproj", "ManaFoodPayment.Infrastructure/"]

RUN dotnet restore "ManaFoodPayment.Api/ManaFoodPayment.Api.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/ManaFoodPayment.Api"
RUN dotnet build "ManaFoodPayment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ManaFoodPayment.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManaFoodPayment.Api.dll"]
