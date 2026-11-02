FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/OrderAccumulator/OrderAccumulator.csproj src/OrderAccumulator/
RUN dotnet restore src/OrderAccumulator/OrderAccumulator.csproj
COPY src/OrderAccumulator/ src/OrderAccumulator/
RUN dotnet publish src/OrderAccumulator/OrderAccumulator.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir store logs && chown $APP_UID store logs
USER $APP_UID
ENV FIX_SOCKET_ACCEPT_HOST=0.0.0.0
EXPOSE 9876
ENTRYPOINT ["dotnet", "OrderAccumulator.dll"]
