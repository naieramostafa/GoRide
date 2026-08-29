FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RideSharing.sln ./
COPY src/RideSharing.Core/RideSharing.Core.csproj src/RideSharing.Core/
COPY src/RideSharing.Application/RideSharing.Application.csproj src/RideSharing.Application/
COPY src/RideSharing.Infrastructure/RideSharing.Infrastructure.csproj src/RideSharing.Infrastructure/
COPY src/RideSharing.Api/RideSharing.Api.csproj src/RideSharing.Api/
RUN dotnet restore

COPY . .
RUN dotnet publish src/RideSharing.Api/RideSharing.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RideSharing.Api.dll"]
