FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# build application
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c release -o /app --no-self-contained --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:6.0

WORKDIR /app
COPY --from=build /app .

EXPOSE 6000
CMD ["dotnet", "./MasonBot.dll"]
