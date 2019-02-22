#installer env image
FROM mcr.microsoft.com/windows/nanoserver:1809 AS installer-env
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ADD https://nodejs.org/dist/v8.9.4/node-v8.9.4-win-x64.zip nodejs.zip
RUN Expand-Archive nodejs.zip -DestinationPath nodejs
RUN Remove-Item -Force nodejs.zip

#Build image
FROM microsoft/dotnet:2.2.103-sdk AS builder

COPY --from=installer-env ["nodejs/node-v8.9.4-win-x64", "C:/Program Files/nodejs"]

USER ContainerAdministrator 
RUN setx /M PATH "%PATH%;C:/Program Files/nodejs"
USER ContainerUser

COPY . .
RUN dotnet publish ./EmbyStat.Web/EmbyStat.Web.csproj --framework netcoreapp2.2 --configuration Release --runtime win7-x64 --output /app
RUN dotnet publish ./Updater/Updater.csproj --framework netcoreapp2.2 --configuration Release --runtime win7-x64 --output /app/updater

#Runtime image
FROM microsoft/dotnet:2.2.1-runtime AS base
LABEL author="UPing"

WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "EmbyStat.dll"]