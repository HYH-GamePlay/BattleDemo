---
name: luban
description: "Use this skill any time the user wants to add, edit, or manage Luban configuration tables for the BattleDemo Unity project. Trigger when the user wants to: add new fields or rows to Excel data tables in Luban/DataTables/Datas/; create new table types (beans, enums, tables); regenerate config code and binary data by running gen.bat; or fix Luban schema/data errors. Also trigger when the user says '配表', '导表', '建表', '加字段', or references specific table files like 角色.xlsx, 敌人.xlsx, etc. Do NOT trigger for general Unity scripting tasks that don't involve Luban tables."
---

# Luban Table Workflow for BattleDemo

## Project Paths

- **Excel data files**: `D:\Unity\Project\BattleDemo\Luban\DataTables\Datas\`
  - Combat tables: `战斗\` (角色.xlsx, 敌人.xlsx, 武器.xlsx, 遗物.xlsx, Buff.xlsx, BuffEffect.xlsx, 道具.xlsx, 掉落规则.xlsx)
  - Common tables: `公共\` (全局常量.xlsx)
  - Schema files: `__tables__.xlsx`, `__beans__.xlsx`, `__enums__.xlsx`
- **Generated C# code**: `D:\Unity\Project\BattleDemo\Assets\Scripts\Config\Gen\`
- **Generated binary data**: `D:\Unity\Project\BattleDemo\Assets\Res\Config\GenerateDatas\Byte\`
- **Generated JSON data**: `D:\Unity\Project\BattleDemo\Assets\Res\Config\GenerateDatas\Json\`

## Excel File Structure (Luban Format)

Every data table Excel file uses this row layout:

| Row | Column A | Column B+ |
|-----|----------|-----------|
| 1 | `##var` | field names |
| 2 | `##type` | field types |
| 3 | `##group` | group tags (usually empty) |
| 4 | `##` | Chinese comments/descriptions |
| 5+ | **empty** (export marker) | actual data values |

**CRITICAL**: Column A is the Luban marker column — NEVER put data in column A.
- Header rows use `##var`, `##type`, `##group`, `##` in column A
- Data rows must have column A = empty (None)
- Data starts at column B

## Generating Tables

Run Luban with .NET roll-forward (system has .NET 9, Luban requires .NET 8):

```bash
cd "D:/Unity/Project/BattleDemo/Luban/DataTables"
dotnet --roll-forward Major "../Tools/Luban/Luban.dll" \
  -t client -c cs-bin -d json -d bin \
  --conf luban.conf \
  -x cs-bin.outputCodeDir=../../Assets/Scripts/Config/Gen \
  -x json.outputDataDir=../../Assets/Res/Config/GenerateDatas/Json \
  -x bin.outputDataDir=../../Assets/Res/Config/GenerateDatas/Byte
```

Do NOT use `gen.bat` directly (it has a `pause` command). Do NOT modify `Luban.runtimeconfig.json`.

## Adding Fields to Existing Tables

1. Open the Excel file with openpyxl
2. Add the new column header in row 1 (##var row), type in row 2, comment in row 4
3. Add values to existing data rows (column A stays empty)
4. Save and run gen.bat equivalent command above
5. Refresh Unity: `manage_asset` or `refresh_unity`

## Adding New Enum Values

Edit `__enums__.xlsx`. Format:
- Column A: enum type name (first row of each enum), empty for subsequent values
- Column B: value name
- Column C: int value
- Column D: alias (Chinese name)
- Column E: comment

## Adding New Bean Types

Edit `__beans__.xlsx`. Format:
- Column A: bean type name (first row), empty for fields
- Column B: field name
- Column C: field type
- Column D: comment

## Adding New Tables

Edit `__tables__.xlsx` to register the new table, then create the corresponding Excel file.

## Reading Generated Data in C#

Tables are accessed via `cfg.Tables` (loaded by `ConfigComp`):
```csharp
Game.Config.Tables.TbActor.GetOrDefault(id)
Game.Config.Tables.TbEnemy.DataList
```

Generated classes are in namespace `cfg.Config` (e.g., `cfg.Config.ActorCfg`).
Enums are in namespace `cfg` (e.g., `cfg.StatType`, `cfg.RelicTrigger`).

## Verification

After generating:
1. Check Luban output for errors (no `ERROR` lines)
2. Verify JSON output in `GenerateDatas/Json/` has correct data
3. Refresh Unity and check console for compilation errors
