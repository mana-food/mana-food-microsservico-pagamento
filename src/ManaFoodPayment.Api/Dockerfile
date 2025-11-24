# Dockerfile para ManaFoodPayment.Api (.NET 9 WebAPI)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ManaFoodPayment.Api/ManaFoodPayment.Api.csproj ManaFoodPayment.Api/
COPY ManaFoodPayment.Application/ManaFoodPayment.Application.csproj ManaFoodPayment.Application/
COPY ManaFoodPayment.Infrastructure/ManaFoodPayment.Infrastructure.csproj ManaFoodPayment.Infrastructure/
COPY . .
RUN dotnet restore "ManaFoodPayment.Api/ManaFoodPayment.Api.csproj"
RUN dotnet build "ManaFoodPayment.Api/ManaFoodPayment.Api.csproj" -c Release -o /app/build
RUN dotnet publish "ManaFoodPayment.Api/ManaFoodPayment.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ManaFoodPayment.Api.dll"]
