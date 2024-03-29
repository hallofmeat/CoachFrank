FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app 

# Restore
COPY *.sln .
COPY src/CoachFrank/*.csproj ./src/CoachFrank/
COPY src/CoachFrank.Commands/*.csproj ./src/CoachFrank.Commands/
COPY src/CoachFrank.Data/*.csproj ./src/CoachFrank.Data/
RUN dotnet restore 

# Build/Publish
COPY src/CoachFrank/. ./src/CoachFrank/
COPY src/CoachFrank.Commands/. ./src/CoachFrank.Commands/
COPY src/CoachFrank.Data/. ./src/CoachFrank.Data/
WORKDIR /app/src/CoachFrank
#TODO: --no-restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0-jammy AS runtime
WORKDIR /app 

COPY --from=build /app/publish ./

USER $APP_UID
ENTRYPOINT ["dotnet", "CoachFrank.dll"]