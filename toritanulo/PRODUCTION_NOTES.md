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
