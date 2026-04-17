Backend progress patch - max 5 legutóbbi tétel mentése felhasználónként

Új / módosított fájlok:
- Models/TetelOlvasasiAllapot.cs
- DTOs/TetelProgressDto.cs
- DTOs/SaveTetelProgressRequestDto.cs
- Controllers/TetelProgressController.cs
- Data/ApplicationDbContext.cs

Új endpointok:
- GET /api/TetelProgress/{tetelId}
- PUT /api/TetelProgress/{tetelId}
- GET /api/TetelProgress/recent

Működés:
- Egy felhasználónak legfeljebb 5 db mentett tételhaladása marad meg.
- Ha ugyanarra a tételre mentesz újra, a meglévő sor frissül.
- Ha új tételt mentesz és már van 5 mentésed, a legrégebbi rekord felülíródik.
