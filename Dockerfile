
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN PROJECT=$(find . -name "*.csproj" -not -path "*/obj/*" -not -path "*/bin/*" | head -n 1) && \
    dotnet publish "$PROJECT" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet $(find /app -maxdepth 1 -name '*.dll' | head -n 1 | xargs basename)"]

