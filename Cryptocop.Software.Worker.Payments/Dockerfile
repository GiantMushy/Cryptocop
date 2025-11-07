FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Cryptocop.Software.sln ./
COPY Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj Cryptocop.Software.Worker.Payments/
COPY Cryptocop.Software.API.Models/Cryptocop.Software.API.Models.csproj Cryptocop.Software.API.Models/
COPY Cryptocop.Software.API.Repositories/Cryptocop.Software.API.Repositories.csproj Cryptocop.Software.API.Repositories/
COPY Cryptocop.Software.API.Services/Cryptocop.Software.API.Services.csproj Cryptocop.Software.API.Services/

RUN dotnet restore Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj

COPY . .
RUN dotnet publish Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "Cryptocop.Software.Worker.Payments.dll"]
