---
name: entity-framework
description: Koristi ovaj skill pri radu s EF Core entitetima u FantasyFootball projektu — dodavanje/izmjena modela, generiranje migracija, osvježavanje baze podataka, konfiguracija relacija u DbContext-u.
---

# Entity Framework skill — FantasyFootball

Aktiviraj kad korisnik:
- dodaje novi entitet ili DbSet u [FantasyFootball.Core/DAL/FantasyFootballDbContext.cs](../../../FantasyFootball.Core/DAL/FantasyFootballDbContext.cs)
- mijenja postojeći model u [FantasyFootball.Core/Models/](../../../FantasyFootball.Core/Models/) (polja, relacije, anotacije)
- treba generirati ili primijeniti EF migraciju
- treba dodati seed podatke

## Konvencije projekta

- **DbContext**: [FantasyFootball.Core/DAL/FantasyFootballDbContext.cs](../../../FantasyFootball.Core/DAL/FantasyFootballDbContext.cs) — jedina instanca, klasa `FantasyFootballDbContext`.
- **Connection string**: `ConnectionStrings:FantasyFootballDbContext` u [FantasyFootball.Web/appsettings.json](../../../FantasyFootball.Web/appsettings.json). Baza je MSSQL u Dockeru (vidi [docker-compose.yml](../../../docker-compose.yml)).
- **Modeli** žive u namespace-u `FantasyFootball.Models`, svaki u svojoj datoteci.
- **Seed** se izvodi programatski u [FantasyFootball.Core/DAL/DbSeeder.cs](../../../FantasyFootball.Core/DAL/DbSeeder.cs) pri startu aplikacije (vidi [FantasyFootball.Web/Program.cs](../../../FantasyFootball.Web/Program.cs)).
- **Repozitoriji** su tanki wrapper-i oko DbContext-a u [FantasyFootball.Core/Repositories/](../../../FantasyFootball.Core/Repositories/), registrirani kao Scoped u DI.

## Anotacije koje MORAJU biti na svakom modelu

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MyEntity
{
    [Key]
    public int Id { get; set; }

    // Strani ključ (1-N veza)
    [ForeignKey(nameof(ParentEntity))]
    public int? ParentEntityId { get; set; }
    public virtual ParentEntity? ParentEntity { get; set; }

    // 1-N kolekcija (na "1" strani)
    public virtual ICollection<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}
```

Pravila:
1. `Id` uvijek `[Key]` — EF to i sam detektira, ali eksplicitno je jasnije.
2. Svaki FK **eksplicitno** deklariran kao `int` (ili `int?` ako nullable) + `[ForeignKey(nameof(NavProp))]`.
3. Navigacijska svojstva su `virtual` — omogućava lazy-loading ako bude uključen i čisti Moq override u testovima.
4. Kolekcije su `virtual ICollection<T>`, **nikad** `List<T>` ili `IEnumerable<T>` (EF mora moći track-ati dodavanja/brisanja).
5. N-N veze: dodaj `ICollection` na **obje strane**; EF automatski kreira join tablicu.

## Workflow za dodavanje novog entiteta

1. **Kreiraj model** u `FantasyFootball.Core/Models/NovaKlasa.cs` s anotacijama iz gornjeg pravila.
2. **Dodaj DbSet** u `FantasyFootballDbContext`:
   ```csharp
   public DbSet<NovaKlasa> NoveKlase { get; set; }
   ```
3. **Konfiguriraj relacije** u `OnModelCreating` (ako entitet ima FK prema više tablica ili N-N):
   ```csharp
   modelBuilder.Entity<NovaKlasa>()
       .HasOne(x => x.Parent)
       .WithMany(p => p.Djeca)
       .HasForeignKey(x => x.ParentId)
       .OnDelete(DeleteBehavior.Restrict);
   ```
   **Bitno**: u ovoj aplikaciji na MSSQL-u koristi `DeleteBehavior.Restrict` za sve FK-ove koji čine "multi-path cascade" — MSSQL to ne dozvoljava i migracija će puknuti.
4. **Generiraj migraciju**:
   ```powershell
   dotnet ef migrations add <OpisPromjene> -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext
   ```
5. **Primijeni migraciju** (opcionalno — app sama poziva `Migrate()` pri startu):
   ```powershell
   dotnet ef database update -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext
   ```
6. Ako entitet treba seed: dodaj u [FantasyFootball.Core/DAL/DbSeeder.cs](../../../FantasyFootball.Core/DAL/DbSeeder.cs) u granu koja se izvodi kad je baza prazna.

## Workflow za izmjenu postojećeg modela

1. Promijeni polje/relaciju u odgovarajućoj `Models/*.cs` datoteci.
2. Ako relacija: provjeri da li treba dodatna konfiguracija u `OnModelCreating`.
3. Generiraj migraciju s opisnim imenom (`AddXxxColumn`, `RenameYyyToZzz`, `AddIndexOnAbc`).
4. Pokreni `dotnet ef database update`.
5. Ako mijenjaš tip kolone ili uvodiš NOT NULL: provjeri hoće li migracija proći na postojećim podacima — dodaj `UPDATE` korak u migraciju po potrebi.

## Česte greške i kako ih izbjeći

- **"Introducing FOREIGN KEY constraint ... may cause cycles or multiple cascade paths"** — postavi `OnDelete(DeleteBehavior.Restrict)` ili `SetNull` na sumnjivom FK-u.
- **"Unable to determine the relationship"** — dodaj eksplicitni `HasOne/HasMany` u `OnModelCreating`.
- **"AddRange does not exist on ICollection<T>"** — u FantasyFootball projektu već postoji extension u [FantasyFootball.Core/Repositories/CollectionExtensions.cs](../../../FantasyFootball.Core/Repositories/CollectionExtensions.cs). Koristi ga ili `foreach + Add`.
- **Seed ne puca ali nema podataka** — provjeri `if (ctx.Players.Any()) return;` guard u DbSeeder-u; ako baza već ima bilo što, seed neće pokrenuti.

## Korisne naredbe

```powershell
# Popis migracija
dotnet ef migrations list -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext

# Ukloni zadnju (još neprimijenjenu) migraciju
dotnet ef migrations remove -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext

# Rollback na konkretnu migraciju
dotnet ef database update <NazivMigracije> -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext

# Generiraj SQL skriptu umjesto direktne primjene (za produkcijske deployove)
dotnet ef migrations script <FROM> <TO> -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext -o migration.sql
```

## Repozitoriji

Nakon dodavanja novog entiteta, kreiraj `FantasyFootball.Core/Repositories/NovaKlasaRepository.cs` po uzoru na postojeće:

```csharp
public class NovaKlasaRepository
{
    private readonly FantasyFootballDbContext _ctx;
    public NovaKlasaRepository(FantasyFootballDbContext ctx) { _ctx = ctx; }

    public List<NovaKlasa> GetAll() =>
        _ctx.NoveKlase.Include(x => x.Parent).AsNoTracking().ToList();

    public NovaKlasa? GetById(int id) =>
        _ctx.NoveKlase.Include(x => x.Parent).AsNoTracking().FirstOrDefault(x => x.Id == id);
}
```

Registriraj u [FantasyFootball.Web/Program.cs](../../../FantasyFootball.Web/Program.cs):
```csharp
builder.Services.AddScoped<NovaKlasaRepository>();
```

Uvijek `AsNoTracking()` za read-only scenarije (performanse) i `Include` za sve relacije koje view koristi.
