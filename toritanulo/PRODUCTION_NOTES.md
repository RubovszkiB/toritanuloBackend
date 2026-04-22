# Production beállítások

Éles környezetben a lokális értékeket környezeti változókból add meg. A backend a `TORITANULO_` prefixet is beolvassa.

## Kötelező backend értékek

- `TORITANULO_ConnectionStrings__DefaultConnection`
- `TORITANULO_Jwt__Key`
- `TORITANULO_Jwt__Issuer`
- `TORITANULO_Jwt__Audience`
- `TORITANULO_Cors__AllowedOrigins`

A `TORITANULO_Cors__AllowedOrigins` értéke pontos frontend origin legyen, például:

```text
https://toritanulo.pelda.hu
```

Több origin esetén pontosvesszővel válaszd el őket.

## Frontend

A frontend build előtt állítsd be:

- `VITE_API_BASE_URL`
- `VITE_BASE_PATH`

Ha a frontend domain gyökerén fut, a `VITE_BASE_PATH=/` maradhat.

## SPA routing

A frontend `public/_redirects` és `public/.htaccess` fájlokat is tartalmaz, hogy a mély route-ok frissítés után is az `index.html`-re essenek vissza Netlify-szerű vagy Apache környezetben.
## Docker + Aiven MySQL

Build:

```powershell
docker build -t toritanulo-backend .
```

Run Aiven adatbazissal:

```powershell
docker run --rm -p 8080:8080 `
  -e "TORITANULO_ConnectionStrings__DefaultConnection=server=mysql-38180182-toritanulo.e.aivencloud.com;port=11833;database=defaultdb;user=avnadmin;password=YOUR_AIVEN_PASSWORD;SslMode=Required;CharSet=utf8mb4" `
  -e "TORITANULO_Jwt__Key=legalabb-32-karakter-hosszu-production-secret" `
  -e "TORITANULO_Cors__AllowedOrigins=http://localhost:5173;http://127.0.0.1:5173" `
  toritanulo-backend
```

Fontos:
- A Dockerfile a repository gyokerebe kerult, es a kontenerben a `toritanulo.dll` indul.
- A jelszot ne ird bele commitolt config fajlba; add at kornyezeti valtozoval.
- Aivenhez a connection stringben `SslMode=Required` kell.
