#!/bin/sh
dotnet publish tibberservice.csproj --self-contained --use-current-runtime -o debian/tibber/opt/tibber -a x64 --os linux -p:PublishSinglefile=true

find debian/ -name '.DS_Store' -exec rm {} \;
find debian/ -name 'appsettings.Development.json' -exec rm {} \;

dpkg --build debian/tibber

