FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["BadTakeStream.Api/BadTakeStream.Api.csproj", "BadTakeStream.Api/"]
COPY ["BadTakeStream.Shared/BadTakeStream.Shared.csproj", "BadTakeStream.Shared/"]
RUN dotnet restore "BadTakeStream.Api/BadTakeStream.Api.csproj"
COPY . .
WORKDIR "/src/BadTakeStream.Api"
RUN dotnet build "BadTakeStream.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BadTakeStream.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BadTakeStream.Api.dll"]