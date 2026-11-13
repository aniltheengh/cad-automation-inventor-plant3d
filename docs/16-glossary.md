# 16 — Glossary (English / 日本語)

## Add-in vocabulary by learning level

Use [chapter 19](19-addin-learning-from-zero.md) for examples and exercises. You do not need to memorize the whole glossary first.

### Beginner — words used in the first lesson

| Word | Meaning in this course |
|---|---|
| Variable | A named place for a value, such as tag text |
| Type | The kind of value, such as text, whole number or true/false |
| Condition | A test that decides what the code does next |
| Loop | Instructions repeated for several values |
| Function / method | A named operation; a method belongs to a type |
| Build | Translate/check source code to produce program output |
| Host | The application that loads and runs your add-in |

**Checkpoint:** identify a variable, condition and loop in workbook B4.

### Transition — words used when entering CAD

| Word | Meaning in this course |
|---|---|
| Class | A type grouping related data and operations |
| Instance / object | A particular value created from or represented by a type |
| Attribute | Metadata in square brackets, such as CommandMethod |
| Interface | A contract of members a class implements |
| Null | No object/value reference available |
| Exception | A reported failure during execution |
| Transaction | A controlled lifetime for drawing database access |

**Checkpoint:** explain why a missing document must be checked before using its editor.

### Intermediate — words used when sharing a tool

| Word | Meaning in this course |
|---|---|
| Scope | Exactly which objects, drawing or project a job covers |
| Schema | The structure of data: classes, columns and relationships |
| Validation | Checking data against a stated rule |
| Dependency | Another library or component your tool needs |
| Manifest | A file describing how a host finds/loads an extension |
| Regression | Previously working behavior broken by a change |
| Modeless UI | A window/palette that stays open while the user works elsewhere |

**Checkpoint:** describe a report's scope and validation rule without referring to code.


Japanese terms are the usual ones in the Japanese UI / Japanese Autodesk documentation; check your UI
where they differ.

## Programming and platform

| Term | 日本語 | Meaning |
|---|---|---|
| API | API | the set of objects/functions a program exposes for automation |
| COM | COM | Microsoft's component technology; Inventor's API and AutoCAD's ActiveX use it |
| Interop assembly | 相互運用アセンブリ | .NET DLL describing a COM API (`Autodesk.Inventor.Interop.dll`) |
| In-process / out-of-process | インプロセス / アウトプロセス | code inside the CAD program vs. a separate program |
| Add-in | アドイン | Extension loaded by a host application; Inventor and Plant/AutoCAD use different entry points and loading mechanisms |
| Plug-in | プラグイン | AutoCAD term for a loaded DLL/LISP; DA term for a cloud add-in |
| Registry-free | レジストリフリー | add-in found via `.addin` XML, no registry entries (mandatory since Inventor 2024) |
| `.addin` manifest | .addin マニフェスト | XML telling Inventor which DLL/class to load |
| `StandardAddInServer` | | the class Inventor creates for your add-in (`Activate`/`Deactivate`) |
| ClientId / ClassId | | GUIDs identifying the add-in |
| GUID | GUID | globally unique ID like `{0AC6FD96-...}` |
| ProgID | ProgID | COM name like `Inventor.Application` |
| Running Object Table (ROT) | | Windows list of running COM servers; `GetActiveObject` reads it |
| HRESULT | HRESULT | 32-bit COM error code (`0x80004005`) |
| .NET Framework 4.8 / .NET 8 / .NET 10 | | runtimes; Inventor 2025–26 = .NET 8, 2027 = .NET 10 |
| TFM | ターゲットフレームワーク | `net8.0-windows` in the project file |
| NuGet | NuGet | .NET package manager (ClosedXML, AutoCAD.NET) |
| SDK | SDK | developer kit (Inventor SDK, Plant 3D SDK, .NET SDK) |
| `Directory.Build.props` | | MSBuild file auto-imported by all projects below it |
| Late binding / Option Strict | 遅延バインディング | member resolved at run time — typos found only when executed |
| Transaction | トランザクション | group of changes = one undo step (Inventor) / required DB access (AutoCAD) |
| STA | シングルスレッドアパートメント | COM threading model for UI objects (`[STAThread]`) |
| Elevation | 管理者として実行 | "run as administrator"; mismatch breaks COM connection |
| Mark of the Web | | Windows flag on downloaded files; `Unblock-File` removes it |
| APS | Autodesk Platform Services | Autodesk's cloud APIs (formerly Forge) |
| Design Automation (DA) | デザイン オートメーション | CAD engines in the cloud (Inventor, AutoCAD, Revit, 3ds Max) |
| AppBundle / Activity / WorkItem | | DA: plug-in zip / how to run / one job |
| OAuth 2-legged | | app-only authentication for APS |
| OSS / signed URL | | APS object storage / time-limited upload/download link |

## Inventor

| Term | 日本語 | Meaning |
|---|---|---|
| Part / Assembly / Drawing / Presentation | 部品 / アセンブリ / 図面 / プレゼンテーション | `.ipt` / `.iam` / `.idw`,`.dwg` / `.ipn` |
| Sheet metal | 板金 | part subtype with flat pattern |
| Flat pattern | 展開図 / 展開パターン | unfolded sheet metal |
| Parameter (model / user / reference) | パラメータ（モデル / ユーザ / 参照） | named values driving the model |
| iProperties | iProperty | file properties (Part Number = 部品番号, Description = 説明, Revision = 改訂番号, Stock Number = 在庫番号) |
| Custom property | ユーザ定義プロパティ | `Inventor User Defined Properties` set |
| Component definition | コンポーネント定義 | the model content of a document |
| Occurrence | オカレンス / コンポーネント | placed instance of a part/sub-assembly |
| Proxy | プロキシ | geometry seen through an occurrence |
| Constraint / Joint | 拘束 / ジョイント | assembly relations |
| Model State | モデル状態 | variants inside one file (2022+; replaced Level of Detail) |
| iPart / iAssembly | iPart / iAssembly | table-driven families with member files |
| iLogic | iLogic | VB.NET rules engine inside Inventor |
| Internal / external rule | 内部ルール / 外部ルール | stored in document / in a folder |
| Event trigger | イベント トリガ | runs a rule on an event (before save…) |
| iLogic form | iLogic フォーム | no-code dialog for parameters |
| BOM | 部品表 | bill of materials; views: Structured (構成), Parts Only (部品のみ) |
| BOM structure | 部品表構造 | Normal, Phantom (ファントム), Reference (参照), Purchased (購入), Inseparable (分離不可) |
| Content Center | コンテンツ センター | standard parts library (JIS/ISO bolts, profiles) |
| Frame Generator | フレーム ジェネレータ | structural frame tool |
| Design Accelerator | デザイン アクセラレータ | gear/shaft/bolt calculators |
| Translator add-in | トランスレータ アドイン | export/import module (PDF, DXF, STEP) |
| Apprentice Server | Apprentice Server | free read-mostly Inventor API, no UI |
| Vault | Vault | Autodesk PDM; Job Processor = server-side jobs |
| Project file (.ipj) | プロジェクト ファイル | search paths for references/libraries |
| Template | テンプレート | starting file (`.ipt/.iam/.idw`) |
| Sheet format | シート フォーマット | drawing sheet with predefined views |
| Title block | 表題欄 | drawing title block |
| Parts list / balloon | 部品表（図面）/ バルーン | drawing BOM table / item callouts |

## AutoCAD / Plant 3D

| Term | 日本語 | Meaning |
|---|---|---|
| Database / Document / Editor | データベース / ドキュメント / エディタ | DWG content / open window / command line |
| ObjectId / Handle | オブジェクト ID / ハンドル | session id / permanent id in DWG |
| Block / block reference / attribute | ブロック / ブロック参照 / 属性 | definition / insert / tagged text |
| Dynamic block | ダイナミック ブロック | block with parameters; real name = EffectiveName |
| Layout / model space / paper space | レイアウト / モデル空間 / ペーパー空間 | |
| AutoLISP / Visual LISP | AutoLISP | built-in LISP language; `vl-`/`vla-` = ActiveX |
| Script (.scr) | スクリプト | keystroke file |
| Core Console (accoreconsole) | コア コンソール | AutoCAD without UI for batch |
| Bundle / PackageContents.xml | バンドル | autoloader package format |
| IExtensionApplication | | entry-point interface of an AutoCAD .NET DLL: `Initialize` / `Terminate` |
| Demand loading | デマンド ロード | loading a DLL from registry values (`LOADER`, `LOADCTRLS`) |
| SECURELOAD / TRUSTEDPATHS | | code-loading security / trusted folders |
| Object enabler | オブジェクト イネーブラ | DBX that lets other apps show custom objects |
| Proxy object | プロキシ オブジェクト | custom object shown without its enabler |
| Plant project / Project.xml | プラント プロジェクト | project definition |
| Project part (Piping, PnId, Ortho, Iso) | プロジェクト パート | sub-areas of a Plant project, each with a database |
| `.dcf` | | project database file (SQLite by default) |
| Data Manager | データ マネージャ | table view of Plant data |
| DataLinksManager | | API link between DWG objects and database rows |
| PnPDatabase / PnPTable / PnPRow | | API database, table, row |
| PnPID / row id | | permanent row number of a Plant object |
| Project class | クラス | class in Project Setup = table in the project database (`Pipe`, `Elbow`…); parent via `BaseTableName` |
| Relationship | リレーションシップ | link between two rows with a type and two role names (`P3dLineGroupPartRelationship`) |
| Port / Pair / Connector | ポート / — / コネクタ | connection point of a part / object + port / the joint between two ports |
| Line group / line number | ライン グループ / ライン番号 | `P3dLineGroup` |
| Spec (`.pspx`) / Catalog (`.pcat`) | 仕様 / カタログ | allowed parts per service class / all parts |
| Spec Editor / Catalog Builder | 仕様エディタ / カタログ ビルダー | UI tools for specs/catalogs |
| Service class | | e.g. CS150 — material + rating |
| P&ID | P&ID（配管計装図） | piping and instrumentation diagram |
| Isometric (iso) / Isogen | アイソメ図 / Isogen | pipe isometric drawings / engine producing them |
| PCF | PCF | Piping Component File (Isogen text input) |
| Ortho | オルソ図 | orthographic drawings from the 3D model |
| MTO | 材料集計 | material take-off |
| Line list | ラインリスト | table of all lines with process data |
| Nozzle / flange / elbow / tee / reducer / olet | ノズル / フランジ / エルボ / ティー / レデューサ / オレット | piping parts |
| Custom part script | カスタム スクリプト | Python geometry script for catalogue parts |
| Report Creator | レポート クリエータ | Plant 3D report tool |

## Engineering terms used in samples

| Term | 日本語 | Where |
|---|---|---|
| Module m | モジュール | gears (P1) |
| Pressure angle α | 圧力角 | gears |
| Involute | インボリュート | gears |
| Profile shift x | 転位係数 | gears |
| Undercut | 切下げ | gears |
| Pitch / base / tip / root diameter | 基準円 / 基礎円 / 歯先円 / 歯底円直径 | gears |
| Design pressure / allowable stress / joint efficiency | 設計圧力 / 許容応力 / 継手効率 | tank (P6) |
| Corrosion allowance | 腐れ代 | tank |
| Ellipsoidal / torispherical / hemispherical head | 半楕円鏡板 / 皿形鏡板 / 半球鏡板 | tank |
| Square hollow section (SHS) | 角形鋼管 | frame (P5) |
| K-factor | K係数 | sheet metal flat length |

Next: [17 — references](17-references.md).
