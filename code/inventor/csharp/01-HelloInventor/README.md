# 01-HelloInventor — your first Inventor EXE (C#)

**Goal:** prove the toolchain works: Visual Studio → .NET 8 → COM → Inventor.

**What it does:** connects to the *running* Inventor, prints the version and open documents, then the
parameters (name, expression, value, units) and a few iProperties of the active document.

## Run
1. Start Inventor 2026, open any part (`.ipt`).
2. Open `HelloInventor.csproj` in Visual Studio 2022 → **F5**. (Or: `dotnet run` in this folder.)
3. Expected output (example):
   ```
   Inventor 2026 (Build 300...)  documents open: 1
   Active: Bracket.ipt  type=kPartDocumentObject
     d0          = 50 mm      -> 5 (cm internal)
     Length      = 120 mm     -> 12
   Part Number : Bracket
   ```

## Learn from it
- `InventorConnector.Connect(false)` — the .NET 8 replacement for `Marshal.GetActiveObject` ([docs/05](../../../../docs/05-csharp-vbnet-exe.md#connecting-to-inventor-from-net-8)).
- `Parameter.Value` is **internal units** (cm, rad); `Parameter.Expression` is what the user typed.
- `[STAThread]` on `Main` — COM UI objects expect a single-threaded apartment.

## If it fails
| Message | Fix |
|---|---|
| `Inventor is not running` | start Inventor; do not run VS **or** Inventor "as administrator" (both or neither) |
| `Could not load file Autodesk.Inventor.Interop` | `InventorBin` in `code/inventor/Directory.Build.props` must point at your `Inventor 2026\Bin` |
| `0x80010001 RPC_E_CALL_REJECTED` | Inventor is busy (dialog open) — close dialogs, retry ([docs/13](../../../../docs/13-errors-and-fixes.md)) |
