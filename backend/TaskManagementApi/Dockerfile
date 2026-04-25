FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
WORKDIR "/src/TaskManagementApi"
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskManagementApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet-ef migrations bundle -r linux-x64 -o /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
COPY --from=build /app/efbundle .

ENTRYPOINT ["dotnet", "TaskManagementApi.dll"]
