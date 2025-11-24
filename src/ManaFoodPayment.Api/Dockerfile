# Dockerfile para ManaFoodPayment.Api (.NET 9 WebAPI)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/ManaFoodPayment.Api/ManaFoodPayment.Api.csproj src/ManaFoodPayment.Api/
COPY src/ManaFoodPayment.Application/ManaFoodPayment.Application.csproj src/ManaFoodPayment.Application/
COPY src/ManaFoodPayment.Infrastructure/ManaFoodPayment.Infrastructure.csproj src/ManaFoodPayment.Infrastructure/
COPY . .
RUN dotnet restore "src/ManaFoodPayment.Api/ManaFoodPayment.Api.csproj"
RUN dotnet build "src/ManaFoodPayment.Api/ManaFoodPayment.Api.csproj" -c Release -o /app/build
RUN dotnet publish "src/ManaFoodPayment.Api/ManaFoodPayment.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ManaFoodPayment.Api.dll"]
