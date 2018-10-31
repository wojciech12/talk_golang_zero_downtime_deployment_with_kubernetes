FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["ZeroDowntimeDeployment/ZeroDowntimeDeployment.csproj", "ZeroDowntimeDeployment/"]
RUN dotnet restore "ZeroDowntimeDeployment/ZeroDowntimeDeployment.csproj"
COPY . .
WORKDIR "/src/ZeroDowntimeDeployment"
RUN dotnet build "ZeroDowntimeDeployment.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ZeroDowntimeDeployment.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ZeroDowntimeDeployment.dll"]