# SCADA

SCADA application for acquisition, monitoring, and management of tags and alarms, developed as part of the PSUSU course.  
The project consists of three parts: **DataConcentrator**, **PLCSimulator**, and **ScadaGUI**.

## Technologies

- C# / .NET Framework 4.7.2
- WPF
- Entity Framework 6 (TPH inheritance)
- SQL Server LocalDB
- Visual Studio

## Features

All core functionalities from the project assignment are implemented, along with additional functionality 3 — **localization** (Serbian/English language, time zone, date format, tooltip on every control).

### DataConcentrator

- `Tag` (abstract) → `InputTag` / `OutputTag` → `AnalogInput`, `AnalogOutput`, `DigitalInput`, `DigitalOutput`
- EF6 TPH inheritance, a single `Tags` table
- `Alarm` and `ActivatedAlarm` entities
- `DataConcentratorManager` is the central class:
  - holds tags
  - starts and stops scan threads
  - checks alarms, including hysteresis
  - writes to output tags
  - generates `Report.txt`
  - logs actions to `system.log`

### PLCSimulator

- Fixed missing addresses `ADDR002–004` and `ADDR011–013`; the simulator previously crashed because the original constructor did not initialize those keys.
- Added locking (`lock`) for both reading and writing.
- Replaced `Thread.Abort()` with cooperative shutdown via a `running` flag.

### ScadaGUI

- `MainWindow`:
  - table of all tags
  - color indication — red/yellow
  - scan checkbox
  - writing to outputs
  - Add / Remove / Report buttons
  - language, time zone, and date format bar
- `AddWindow`:
  - a single window for AI / AO / DI / DO / Alarm with dynamic fields
- `AlarmDetailsWindow`:
  - alarms related to AI
  - acknowledge / remove

### Localization

Localization is implemented through runtime configuration, without restart, via `LocalizationManager` (Serbian/English dictionary + custom `Loc` markup extension).  
`.resx` satellite assemblies are not used.

## Project Structure

- `DataConcentrator`
- `PLCSimulator`
- `ScadaGUI`

## Prerequisites

- Visual Studio
- .NET Framework 4.7.2
- SQL Server LocalDB
- EF6 NuGet packages — already located in `packages/`

## Getting Started

1. Open `PSUSUproject.sln` in Visual Studio.
2. Run **Restore NuGet Packages**.
3. Verify that SQL Server LocalDB is installed.  
   The connection string is located in `ScadaGUI/App.config`:
   - `(localdb)\MSSQLLocalDB`
   - database: `PSUSU_SCADA_DB` — created automatically on first run.
4. Set `ScadaGUI` as the Startup Project.
5. Run the project with `F5`.

## Test Scenario

1. `Add` → `AI`  
   Example: `Name=AI1`, `IOAddress=ADDR001`, `LowLimit=-100`, `HighLimit=100`, `ScanTime=500`, check `OnScan`.
2. `Add` → `Alarm`  
   Select `AI1`, `LimitValue=50`, `Above`, enter a message.
3. After a few seconds, the value exceeds 50 and the row turns red.
4. `Details` → `Acknowledge` → the row turns yellow until the value returns below `LimitValue - Hysteresis`.

## Notes and Known Issues

- The project was developed in a Linux environment without Windows, Visual Studio, or the .NET Framework 4.7.2 runtime, so it could not be compiled or run in that environment. Syntax and types were reviewed manually, but the first build must be performed in Visual Studio. Minor errors are possible, such as namespace or XAML typo errors.
- Check the XAML `x:Static` references to `dc:TagType.AI` and `dc:AlarmState.Active`. If Visual Studio does not recognize the namespace, verify that `assembly=DataConcentrator` in `xmlns:dc` matches the actual `AssemblyName` — currently it is `DataConcentrator`.
- `DataGridTemplateColumn` binding `Type` (enum) versus `x:Static`: if the XAML parser reports an issue with enum value comparison, the alternative is an `IValueConverter`.
- `DropCreateDatabaseIfModelChanges` means the database will be deleted and recreated if the model changes. This is acceptable during development, but before the defense, care should be taken not to lose test data unnecessarily.
