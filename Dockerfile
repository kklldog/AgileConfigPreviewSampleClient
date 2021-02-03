FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AgileConfigPreviewSample.Client/AgileConfigPreviewSample.Client.csproj", "AgileConfigPreviewSample.Client/"]
RUN dotnet restore "AgileConfigPreviewSample.Client/AgileConfigPreviewSample.Client.csproj"
COPY . .
WORKDIR "/src/AgileConfigPreviewSample.Client"
RUN dotnet build "AgileConfigPreviewSample.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgileConfigPreviewSample.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgileConfigPreviewSample.Client.dll"]