Projekt neve / namespace: toritanulo

Ez a verzió már a teljes alap userkezelést tartalmazza:
- ASP.NET 8 Web API
- Entity Framework Core
- MySQL / XAMPP
- JWT login
- Swagger JWT teszteléssel
- seedelt balazs admin user
- regisztráció
- admin userkezelés

BELÉPÉS:
felhasználónév: balazs
jelszó: balazs123

FONTOS ENDPOINTOK:
POST /api/Auth/register
POST /api/Auth/login
GET /api/Auth/me
GET /api/Auth/admin-test

ADMIN ENDPOINTOK:
GET /api/Users
GET /api/Users/{id}
POST /api/Users
PUT /api/Users/{id}
PUT /api/Users/{id}/password
DELETE /api/Users/{id}

LÉPÉSEK:
1. Visual Studio-ban hozz létre egy új ASP.NET Core Web API projektet toritanulo néven.
2. Cseréld le a projekt fájljait a zipben lévő fájlokra.
3. XAMPP-ban indítsd el a MySQL-t.
4. phpMyAdminban futtasd le a Scripts/create_database.sql fájlt.
5. Ha a root jelszavas, akkor az appsettings.json kapcsolatot írd át.
6. NuGet restore.
7. Indítás.

Megjegyzés:
- Az induláskor az alkalmazás létrehozza a users táblát, ha még nincs meg.
- Az induláskor létrejön a balazs admin felhasználó is, ha még nem létezik.
- Swaggerben először POST /api/Auth/login, utána a kapott tokent be lehet írni az Authorize gombnál.
- A register végpont alapból Student szerepkörű felhasználót hoz létre.
- Admin szerepkörrel a /api/Users végpontokon teljes felhasználókezelés van.
