FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY Cryptocop.Software.sln ./
COPY Cryptocop.Software.API/Cryptocop.Software.API.csproj Cryptocop.Software.API/
COPY Cryptocop.Software.API.Models/Cryptocop.Software.API.Models.csproj Cryptocop.Software.API.Models/
COPY Cryptocop.Software.API.Repositories/Cryptocop.Software.API.Repositories.csproj Cryptocop.Software.API.Repositories/
COPY Cryptocop.Software.API.Services/Cryptocop.Software.API.Services.csproj Cryptocop.Software.API.Services/

RUN dotnet restore Cryptocop.Software.API/Cryptocop.Software.API.csproj

# Copy the rest of the source
COPY . .
RUN dotnet publish Cryptocop.Software.API/Cryptocop.Software.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Cryptocop.Software.API.dll"]
