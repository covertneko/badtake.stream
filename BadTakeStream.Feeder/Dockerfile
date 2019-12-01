FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["BadTakeStream.Feeder/BadTakeStream.Feeder.csproj", "BadTakeStream.Feeder/"]
COPY ["BadTakeStream.Shared/BadTakeStream.Shared.csproj", "BadTakeStream.Shared/"]
RUN dotnet restore "BadTakeStream.Feeder/BadTakeStream.Feeder.csproj"
COPY . .
WORKDIR "/src/BadTakeStream.Feeder"
RUN dotnet build "BadTakeStream.Feeder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BadTakeStream.Feeder.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BadTakeStream.Feeder.dll"]