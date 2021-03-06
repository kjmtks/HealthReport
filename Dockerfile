FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster
WORKDIR /app

EXPOSE 8080

RUN apt-get update -y && apt-get upgrade -yq && apt-get install -y curl git gnupg &&\
    curl -sL https://deb.nodesource.com/setup_10.x | bash - &&\
    apt-get install -y nodejs &&\
    apt-get install -yq binutils debootstrap &&\
    dotnet tool install --global dotnet-ef && export PATH="$PATH:/root/.dotnet/tools"

COPY *.sln .
COPY NCVC.App/*.csproj ./NCVC.App/
COPY NCVC.Parser/*.fsproj ./NCVC.Parser/
RUN mkdir -p /data && rm -rf ./NCVC.App/wwwroot/css/themes

COPY NCVC.App/. ./NCVC.App/
COPY NCVC.Parser/. ./NCVC.Parser/
WORKDIR /app/NCVC.App
RUN npm install
RUN dotnet restore
RUN dotnet publish -c Release -o out

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]
