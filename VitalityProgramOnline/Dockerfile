#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Установка времени
RUN rm -rf /etc/localtime \
    && ln -s /usr/share/zoneinfo/Europe/Moscow /etc/localtime \
    && echo "Europe/Moscow" > /etc/timezone

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["VitalityProgramOnline/VitalityProgramOnline.csproj", "VitalityProgramOnline/"]
RUN dotnet restore "VitalityProgramOnline/VitalityProgramOnline.csproj"
COPY . .
WORKDIR "/src/VitalityProgramOnline"
RUN dotnet build "VitalityProgramOnline.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VitalityProgramOnline.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VitalityProgramOnline.dll"]