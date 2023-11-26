#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FileService.WebAPI/FileService.WebAPI.csproj", "FileService.WebAPI/"]
COPY ["CommonInitializer/CommonInitializer.csproj", "CommonInitializer/"]
COPY ["Zack.ASPNETCore/Zack.ASPNETCore.csproj", "Zack.ASPNETCore/"]
COPY ["Zack.Commons/Zack.Commons.csproj", "Zack.Commons/"]
COPY ["Zack.EventBus/Zack.EventBus.csproj", "Zack.EventBus/"]
COPY ["Zack.Infrastructure/Zack.Infrastructure.csproj", "Zack.Infrastructure/"]
COPY ["Zack.DomainCommons/Zack.DomainCommons.csproj", "Zack.DomainCommons/"]
COPY ["Zack.JWT/Zack.JWT.csproj", "Zack.JWT/"]
COPY ["FileService.Infrastructure/FileService.Infrastructure.csproj", "FileService.Infrastructure/"]
COPY ["FileService.Domain/FileService.Domain.csproj", "FileService.Domain/"]
RUN dotnet restore "FileService.WebAPI/FileService.WebAPI.csproj"
COPY . .
WORKDIR "/src/FileService.WebAPI"
RUN dotnet build "FileService.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileService.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false -v n

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.WebAPI.dll"]