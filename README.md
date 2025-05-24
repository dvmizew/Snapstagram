# Snapstagram

## Quick Start

Start SQL Server and run the application:

```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Snapstagram123!" -p 1433:1433 --name snapstagram-sql -d mcr.microsoft.com/mssql/server:2022-latest

# Setup and run application
dotnet restore
dotnet ef database update
dotnet run
```
## Useful Commands

```bash
# Stop/start SQL Server
docker stop snapstagram-sql
docker start snapstagram-sql

# Hot reload
dotnet watch run

# Clean restart
docker rm -f snapstagram-sql