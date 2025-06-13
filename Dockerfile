FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasýný kopyala ve restore et
COPY KitapProject.csproj .
RUN dotnet restore

# Tüm kaynak kodlarý kopyala
COPY . .

# Projeyi publish et
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KitapProject.dll"]