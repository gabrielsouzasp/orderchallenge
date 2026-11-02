FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/OrderGenerator.Web/OrderGenerator.csproj src/OrderGenerator.Web/
RUN dotnet restore src/OrderGenerator.Web/OrderGenerator.csproj
COPY src/OrderGenerator.Web/ src/OrderGenerator.Web/
RUN dotnet publish src/OrderGenerator.Web/OrderGenerator.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir store logs && chown $APP_UID store logs
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderGenerator.dll"]
