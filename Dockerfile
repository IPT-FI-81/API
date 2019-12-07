FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY /src .
RUN dotnet restore "Bijector.API.csproj"
RUN dotnet build "Bijector.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bijector.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bijector.API.dll"]
