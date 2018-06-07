FROM microsoft/dotnet AS base
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE true
WORKDIR /app


# install libgdiplus for System.Drawing, wget for downloading font, unzip to unzip font, as well as, libunwind.
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libgdiplus libc6-dev wget unzip libunwind8

 # install x11 for System.Drawing
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libx11-dev

RUN wget https://github.com/RedHatBrand/Overpass/releases/download/3.0.2/overpass-desktop-fonts.zip
RUN unzip overpass-desktop-fonts.zip -d /usr/share/fonts/

  
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY lolresearchbot.csproj ./
COPY . .
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish lolresearchbot.csproj -c Release -r ubuntu-x64 -o /app


FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT /app/lolresearchbot
VOLUME [ "/data" ]
COPY _configuration_EXAMPLE.json /data/_configuration.json