FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["PatientsAPI/PatientsAPI.csproj", "PatientsAPI/"]
COPY ["PatientsAPI.Application/PatientsAPI.Application.csproj", "PatientsAPI.Application/"]
COPY ["PatientsAPI.Domain/PatientsAPI.Domain.csproj", "PatientsAPI.Domain/"]
COPY ["PatientsAPI.Infrastructure/PatientsAPI.Infrastructure.csproj", "PatientsAPI.Infrastructure/"]

RUN dotnet restore "PatientsAPI/PatientsAPI.csproj"

COPY . .
WORKDIR "/src/PatientsAPI"
RUN dotnet build "PatientsAPI.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "PatientsAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "PatientsAPI.dll"]