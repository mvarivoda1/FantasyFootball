# Deploy — FantasyFootball

Aplikacija je kontejnerizirana (`Dockerfile`, multi-stage .NET 9). Migracije baze se
primjenjuju automatski pri startu (`ctx.Database.Migrate()` u `Program.cs`), pa nakon
deploya nije potreban ručni korak nad bazom.

## Konfiguracija (tajne / connection string)

Ništa osjetljivo nije u repozitoriju. U produkciji se postavlja preko env varijabli:

| Varijabla | Opis |
|---|---|
| `ConnectionStrings__FantasyFootballDbContext` | Connection string na produkcijsku SQL bazu |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Anthropic__ApiKey` | (opcionalno) API ključ za AI unos igrača |
| `Authentication__Google__ClientId` / `__ClientSecret` | (opcionalno) Google login |

## Lokalni smoke-test cijelog stacka (Docker)

```bash
docker compose up --build
# web → http://localhost:8080 , db (SQL Server) → localhost:1433
```

`web` servis čeka da `db` postane healthy (healthcheck) prije pokretanja.

---

## Opcija A — Azure App Service (+ Azure SQL)

```bash
# 1. Login i resource group
az login
az group create -n ff-rg -l westeurope

# 2. Azure SQL (server + baza)
az sql server create -g ff-rg -n ff-sql-<unique> -l westeurope \
  --admin-user ffadmin --admin-password "<JakaLozinka1!>"
az sql db create -g ff-rg -s ff-sql-<unique> -n FantasyFootball --service-objective Basic
az sql server firewall-rule create -g ff-rg -s ff-sql-<unique> \
  -n AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# 3a. Deploy iz koda (App Service izbuilda .NET)
az webapp up -g ff-rg -n ff-web-<unique> --runtime "DOTNET:9.0" --sku B1

# 3b. ILI deploy kontejnera (Azure Container Registry + Web App for Containers)
#   az acr create -g ff-rg -n ffacr<unique> --sku Basic --admin-enabled true
#   az acr build -r ffacr<unique> -t fantasyfootball:latest .
#   az webapp create -g ff-rg -p <plan> -n ff-web-<unique> \
#       --deployment-container-image-name ffacr<unique>.azurecr.io/fantasyfootball:latest

# 4. App settings (connection string + okruženje)
az webapp config appsettings set -g ff-rg -n ff-web-<unique> --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  ConnectionStrings__FantasyFootballDbContext="Server=tcp:ff-sql-<unique>.database.windows.net,1433;Initial Catalog=FantasyFootball;User ID=ffadmin;Password=<JakaLozinka1!>;Encrypt=True;TrustServerCertificate=False;"
```

App Service na portu sluša preko `$PORT`/8080 — `Dockerfile` već postavlja `ASPNETCORE_URLS=http://+:8080`.

---

## Opcija B — Google Cloud Run (+ Cloud SQL for SQL Server)

```bash
# 1. Projekt i build image-a
gcloud auth login
gcloud config set project <PROJECT_ID>
gcloud builds submit --tag gcr.io/<PROJECT_ID>/fantasyfootball

# 2. Cloud SQL (SQL Server) instanca + baza (ili koristi vanjsku SQL bazu)
gcloud sql instances create ff-sql --database-version=SQLSERVER_2022_STANDARD \
  --cpu=2 --memory=4GB --root-password="<JakaLozinka1!>" --region=europe-west1
gcloud sql databases create FantasyFootball --instance=ff-sql

# 3. Deploy na Cloud Run
gcloud run deploy fantasyfootball \
  --image gcr.io/<PROJECT_ID>/fantasyfootball \
  --region europe-west1 --allow-unauthenticated \
  --add-cloudsql-instances <PROJECT_ID>:europe-west1:ff-sql \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production \
  --set-env-vars 'ConnectionStrings__FantasyFootballDbContext=Server=<cloud-sql-ip>,1433;Initial Catalog=FantasyFootball;User ID=sqlserver;Password=<JakaLozinka1!>;Encrypt=True;TrustServerCertificate=True;'
```

Cloud Run prosljeđuje port preko `$PORT` (default 8080) — usklađeno s `Dockerfile`-om.

---

## Opcija C — Linux VM

```bash
# Na VM-u (Docker instaliran):
git clone <repo> && cd FantasyFootball
docker compose up -d --build         # web na :8080, db na :1433
# (produkcijski: Nginx reverse proxy + TLS ispred web kontejnera)
```
