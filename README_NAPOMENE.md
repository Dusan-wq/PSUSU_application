# PSUSU SCADA - napomene

## Šta je urađeno
Sve osnovne funkcionalnosti iz projektnog zadatka + dodatna funkcionalnost **3. Lokalizacija**
(jezik sr/en, vremenska zona, format datuma, tooltip na svakoj kontroli).

- **DataConcentrator**: `Tag` (abstract) → `InputTag`/`OutputTag` → `AnalogInput`, `AnalogOutput`,
  `DigitalInput`, `DigitalOutput` (EF6 TPH nasleđivanje, jedna "Tags" tabela). `Alarm` i `ActivatedAlarm`
  entiteti. `DataConcentratorManager` je centralna klasa - drži tagove, pokreće/gasi scan niti,
  proverava alarme (uključujući hysteresis), piše u izlazne tagove, generiše Report.txt, i loguje
  akcije u `system.log`.
- **PLCSimulator**: popravljene nedostajuće adrese (bio je bug - simulaton je pucao na ADDR002-004,
  ADDR011-013 jer originalni konstruktor nije inicijalizovao te ključeve), dodato zaključavanje
  (`lock`) i na čitanje i na pisanje, `Thread.Abort()` zamenjen kooperativnim gašenjem (`running` flag).
- **ScadaGUI**: `MainWindow` (tabela svih tagova, signalizacija bojom - crveno/žuto, sken checkbox,
  upis u izlaze, Add/Remove/Report dugmad, traka za jezik/vremensku zonu/format datuma),
  `AddWindow` (jedan prozor za AI/AO/DI/DO/Alarm sa dinamičkim poljima), `AlarmDetailsWindow`
  (alarmi vezani za AI, acknowledge/remove).
- Lokalizacija je urađena runtime-podešavanjem (bez restarta) preko `LocalizationManager`
  (rečnik sr/en + custom `Loc` markup extension), a ne preko .resx satelitskih sklopova - jer
  .resx designer fajlove generiše Visual Studio, a to nisam mogao ovde da pokrenem/testiram.

## Bitna napomena o testiranju
Ovo je pisano u Linux sandbox okruženju bez Windows-a/Visual Studio-a/.NET Framework 4.7.2 runtime-a,
pa **kod nije mogao da se kompajlira niti pokrene ovde**. Pažljivo sam pregledao sintaksu i tipove,
ali prvi build u Visual Studio-u obavezno uradi ti - normalno je da nešto zafali (tipičnu grešku,
namespace, XAML typo). Javi mi grešku iz Error List-a i brzo je rešavamo.

## Pre prvog pokretanja
1. Otvori `PSUSUproject.sln` u Visual Studio-u, Restore NuGet Packages (EF6 već je u `packages/`).
2. Proveri da imaš instaliran **SQL Server LocalDB** (deo je SQL Server Express/Developer edition
   ili "SQL Server Express LocalDB" instalera). Connection string je u `ScadaGUI/App.config`:
   `(localdb)\MSSQLLocalDB`, baza `PSUSU_SCADA_DB` - kreira se automatski pri prvom pokretanju.
3. Postavi `ScadaGUI` kao Startup Project (desni klik → Set as Startup Project) i pokreni (F5).
4. Test scenario: Add → AI (npr. Name=AI1, IOAddress=ADDR001, LowLimit=-100, HighLimit=100,
   ScanTime=500, čekiraj OnScan) → zatim Add → Alarm (bira se AI1, LimitValue npr. 50, Above,
   poruka) → posle par sekundi vrednost prelazi 50 i red postaje crven → Details → Acknowledge
   → red postaje žut dok se vrednost ne vrati ispod (LimitValue - Hysteresis).

## Stvari koje vredi dvaput proveriti u VS-u
- XAML `x:Static` reference na `dc:TagType.AI` / `dc:AlarmState.Active` - ako VS ne prepozna
  namespace, proveri da li se `assembly=DataConcentrator` u `xmlns:dc` poklapa sa stvarnim
  AssemblyName (jeste, `DataConcentrator`).
- `DataGridTemplateColumn` binding `Type` (enum) protiv `x:Static` - retko, ali ako XAML parser
  zakuka oko poređenja enum vrednosti, alternativa je `IValueConverter` (mogu dodati ako zatreba).
- `DropCreateDatabaseIfModelChanges` znači da će baza da se obriše i napravi iznova ako promeniš
  model (npr. dodaš property) - to je OK za razvoj, ali obrati pažnju pred odbranu da ne izgubiš
  test podatke nepotrebno.
