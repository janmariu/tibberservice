# Tibber service
A systemd service for receiving realtime power consumption data and 
inserting it to a influx database server.

# Building
buildpkg.sh creates tibber.deb in the debian folder. The debian package
contains a self contained dotnet binary and should run on most debian
based distributions.

# Installing
```
sudo dpkg -i tibber.deb
sudo nano /opt/tibber/appsettings.json
systemctl start tibber
```

