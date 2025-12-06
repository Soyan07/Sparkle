# Use the official ASP.NET Core SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Sparkle.Api/Sparkle.Api.csproj", "Sparkle.Api/"]
COPY ["Sparkle.Infrastructure/Sparkle.Infrastructure.csproj", "Sparkle.Infrastructure/"]
COPY ["Sparkle.Domain/Sparkle.Domain.csproj", "Sparkle.Domain/"]
RUN dotnet restore "Sparkle.Api/Sparkle.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Sparkle.Api"
RUN dotnet build "Sparkle.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sparkle.Api.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sparkle.Api.dll"]
