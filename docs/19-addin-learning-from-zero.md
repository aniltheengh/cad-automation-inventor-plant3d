# 19 — Add-in development from zero: beginner, transition and intermediate

[Home](../README.md) · [Plant add-in guide](18-plant3d-addin-development.md) · [Plain-language glossary](16-glossary.md)

You do not need to be a software engineer to start here. You need a small task you understand, patience to try one change at a time, and a way to check the result. This workbook teaches programming through a Plant 3D add-in. Knowledge of piping or drawings is useful; programming knowledge is not assumed.

**How to use this workbook:** complete one numbered action, check the expected result, then continue. Keep a notebook with: what I changed, what I expected, what happened, and what I learned. The levels are milestones, not deadlines.

## Choose your starting level

| Level | Starting point | What you will be able to do |
|---|---|---|
| Beginner | You have never written or built a program | Explain simple C#, run a tiny program, find your files and build an add-in |
| Transition | You can follow a small example | Explain how CAD calls your command, trace a selection and adapt one read-only operation |
| Intermediate | You can build/load/debug a simple command | Develop a small report or validation tool, handle failures and package a tested version |

**Recommended order:** Beginner → Transition → Intermediate. Transition is the bridge between copying an example and changing it with understanding. Do not jump to ribbon design, bulk editing or automatic pipe routing before you can explain a small command.

## Beginner — learn the language and the build cycle

### B1. Understand the tools before installing more

| Word | Plain-language meaning | Engineering analogy |
|---|---|---|
| Source code | Instructions you write as text | A written procedure |
| C# | The language of those instructions; pronounced “C sharp” | The notation used in the procedure |
| Visual Studio | The application where you edit, build and debug | A workbench |
| Compiler / build | Checks and translates the instructions into program output | A manufacturing/checking step |
| .NET SDK | Tools needed to build .NET programs | A tool kit |
| Runtime | Software that executes the built program | The operating machinery |
| API | The operations a program allows your code to request | A documented control panel |
| Plant SDK | Autodesk references, documentation and examples for Plant development | The equipment-specific manual and interfaces |
| DLL | A compiled library loaded by another program | A module fitted into a larger machine |
| Host | The application running the add-in; here Plant 3D | The larger machine |
| Reference | A connection from your project to another code library | A referenced standard or component |
| Debugger | A tool to pause code and inspect its values | Instruments for fault-finding |

An analogy helps you start; it is not a technical definition. In particular, “build succeeded” means the compiler accepted the code, not that the engineering result is correct.

**Action:** write one sentence explaining each of C#, Visual Studio, .NET and Plant 3D.  
**Expected:** they have different jobs.  
**Check answer:** C# is the language; Visual Studio is the development application; .NET supplies build/runtime technology; Plant 3D is the host and source of Plant functionality.

### B2. Recognize the files

| Item | What to do with it |
|---|---|
| `.cs` | Read and edit C# instructions |
| `.csproj` | Open the project; it records build settings and references |
| `.sln` | Optional container that opens several projects together |
| `bin` | Look here for build output such as your DLL |
| `obj` | Temporary build information; do not edit it as lesson source |
| `.pdb` | Debug information matched to a particular built DLL |
| `README.md` | Read the project's instructions |

**Action:** find [Plant3DDataTools.csproj](../code/plant3d/csharp/Plant3DDataTools/Plant3DDataTools.csproj) and [PipingCommands.cs](../code/plant3d/csharp/Plant3DDataTools/PipingCommands.cs) in your downloaded repository.  
**Expected:** you can distinguish the build recipe from the source code without editing either.

### B3. Run your first C# program without CAD

Prerequisite: Visual Studio with C# development tools and a supported .NET SDK. This practice program has no Autodesk dependencies, so it can use an installed supported target offered by your development tools. The later Plant add-in must use a target matched to its host.

1. In Visual Studio, choose **Create a new project → Console App → C#**.
2. Name it `PlantLearningBasics` and save it in a new practice folder outside the repository's existing add-in projects.
3. Use the top-level-statements template. If a “Do not use top-level statements” option appears, leave it unchecked.
4. Open `Program.cs` and replace its contents with the following.
5. Run it with **Start Without Debugging**.

```csharp
string tag = "P-101";
int componentCount = 3;
bool hasTag = !string.IsNullOrWhiteSpace(tag);

Console.WriteLine($"Tag: {tag}");
Console.WriteLine($"Components: {componentCount}");

if (hasTag)
{
    Console.WriteLine("Tag is present.");
}
else
{
    Console.WriteLine("Tag is missing.");
}
```

Expected output:

```text
Tag: P-101
Components: 3
Tag is present.
```

Read the code slowly:

- `string` stores text. `tag` is the variable's name, and `"P-101"` is its value.
- `int` stores a whole number. A component count is a suitable example.
- `bool` stores true or false. Here it records whether text was supplied.
- `=` assigns a value. It does not test whether two things are equal.
- `IsNullOrWhiteSpace` checks for absent, empty or whitespace-only text. `!` reverses its true/false answer.
- `Console.WriteLine` writes a line of output. The parentheses contain what to write.
- The `$` before a quoted string lets `{tag}` insert the variable's value.
- `if` chooses a branch; braces group its instructions. A semicolon ends a statement.
- C# is case-sensitive: `tag` and `Tag` are different names.

**Practice:** change the tag to `"   "` and predict the last output line before running.  
**Answer:** `Tag is missing.` Spaces alone do not count as a tag.

This checks presence only. It does not prove a tag follows your project's rules or is unique.

Microsoft's [top-level statements tutorial](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/top-level-statements) explains this console format. You will use a class and a command method for the add-in instead.

### B4. Repeat a check and give it a name

Replace the practice program with:

```csharp
string[] tags = { "P-101", "", "V-201" };
int missingCount = 0;

foreach (string tag in tags)
{
    if (IsMissingTag(tag))
    {
        missingCount++;
    }
}

Console.WriteLine($"Missing tags: {missingCount}");

static bool IsMissingTag(string value)
{
    return string.IsNullOrWhiteSpace(value);
}
```

Expected output: `Missing tags: 1`.

An array holds several values. `foreach` processes one value at a time. `missingCount++` adds one to the count. The named function `IsMissingTag` receives text and returns a bool. `return` sends that answer back to the caller. `static` means this helper does not require creating an instance of a class.

**Practice:** add another empty string to the array.  
**Answer:** the output becomes `Missing tags: 2`.

**Why this matters for an add-in:** reading Plant data and deciding whether a tag is missing are separate tasks. The rule can be understood and checked before connecting it to a CAD object.

### B5. Learn to recover from a simple error

1. Remove the semicolon after `int missingCount = 0;`.
2. Build the practice program.
3. Read the first compiler error and double-click it.
4. Restore the semicolon and build again.

**Expected:** the first build fails and the corrected build succeeds. Error wording can vary. A compiler error is feedback about instructions; it is not evidence that you cannot learn programming. Later errors can be consequences of the first one, so start there.

Set a breakpoint by clicking the margin beside `missingCount++`. Start with debugging. When execution pauses, hover over `tag` and `missingCount`; step over once and inspect the count again. A breakpoint pauses **before** the marked instruction executes.

### B6. Build and load the existing add-in

Use [chapter 18, environment and loading instructions](18-plant3d-addin-development.md). First record your installed product/update and resolve the correct SDK references. Open the existing project, build it, and find its output DLL.

**Expected:** the real-reference build succeeds, and NETLOAD accepts that DLL inside the matching Plant 3D installation. Record the full DLL path to avoid loading an older copy. If SDK setup is not ready, continue the console lessons; installing more unrelated software will not fix a missing Autodesk reference.

### Beginner checkpoint

Before moving on, demonstrate all four:

- Explain what a variable, condition, loop and function do in B3/B4.
- Make one small change and predict the new output.
- Fix the deliberate compiler error.
- Distinguish source code, build output and loading the DLL into Plant 3D.

The CAD part can wait until you have access to the required host and SDK.

## Transition — connect C# to the CAD host

### T1. Understand who starts your code

In the console exercise, running the program starts its instructions. In an add-in, Plant 3D loads the DLL and waits for the user to invoke a registered command.

```text
You edit .cs -> build -> DLL
Plant 3D loads DLL -> user types command -> command method runs
Command asks the host for drawing/project data -> result appears in the editor
```

A method is a named operation inside a class. A class groups related operations and data. A namespace groups names. The `using` declarations at the top let you use shorter type names; they do not install or load the SDK.

**Action:** read the P3DHELLO exercise in [chapter 18](18-plant3d-addin-development.md) before adding it. Include its assembly attributes.  
**Expected:** you can point to the command name, the method body, the document check and the output statement.

### T2. Read the first command line by line

| Code idea | Meaning | Why it is present |
|---|---|---|
| `public sealed class AddinEntry` | A class the host can discover; sealed prevents inheritance | Groups the introductory command and lifecycle hooks |
| `: IExtensionApplication` | The class promises to provide the interface's methods | Gives AutoCAD load/shutdown hooks |
| `Initialize()` | Runs when the extension initializes | Keep setup separate from user commands |
| `Terminate()` | Shutdown hook | Clean up subscriptions/resources when applicable |
| `[CommandMethod("P3DHELLO")]` | Registers the typed command name | Tells the host how to call your method |
| `public void Hello()` | Public method with no returned value | Implements the command |
| `var doc = ...` | Compiler infers the variable's type from its value | Gets the currently active document |
| `doc == null` | Tests whether no document was returned | Avoids using a missing object |
| `return;` | Exits this method now | Stops when its prerequisites are absent |
| `doc.Editor.WriteMessage(...)` | Writes in the CAD command area | Uses the host's output instead of a console window |

**Practice:** change only the displayed greeting, rebuild, restart the host, NETLOAD the new DLL and run the command.  
**Expected:** the new greeting appears. If the old one appears, check the DLL path and host restart before editing more code.

### T3. Trace one Plant property read before changing it

Open [PidCommands.cs](../code/plant3d/csharp/Plant3DDataTools/PidCommands.cs) and locate `PIDPICK`. Read the implementation rather than inferring behavior from the name.

1. Find `GetEntity`: it asks the user to choose an entity, meaning a drawing object.
2. Find the selection-status check: Escape should leave this command.
3. Find `PlantHelpers.Dlm(part)`: it retrieves the manager for a project part.
4. Find `FindAcPpRowId`: it maps a selected drawing object to a Plant data row.
5. Find `GetAllProperties` and the loop that prints each name/value pair.

The existing sample tries `PnId` and then `Piping`; this is not a guarantee that fallback succeeds if the first lookup throws an exception. It is a teaching sample to inspect, not a fully guarded inspector.

**Action:** in a disposable P&ID project drawing, use a known linked symbol and compare returned values with Data Manager after a real SDK build.  
**Expected:** the class and properties refer to that selected object. If the command fails, record the exception and project part instead of assuming the object has no data.

**Check answer:** an ObjectId identifies a drawing object in its current context; a Plant row ID identifies a record in a project-part database. They are not interchangeable.

### T4. Turn reading into a small modification

On a practice branch or copy, adapt a property-reading command to print only a property name you discovered in T3.

Write the behavior in ordinary language first:

```text
If the user cancels, stop.
If the project or needed project part is unavailable, explain and stop.
If the selected object has no usable linked row, explain and stop.
Read the properties.
Look for the chosen property.
If found, print its value; otherwise print "Property not found".
```

This is **pseudocode**: a plan, not C# to paste. Implement one condition at a time. Keep a list of cases you have actually tested. A missing property and a present property with an empty value are different outcomes.

**Practice question:** why not assume every object contains a property named Tag?  
**Answer:** classes and project schemas differ; absence must be handled explicitly.

### T5. Understand a drawing transaction

A transaction manages access to drawing database objects. In the existing `PIDTAGS` method, locate StartTransaction, GetObject, OpenMode.ForRead and the using block. Read-only access expresses the intent to inspect.

The `using (...)` block here disposes a resource when the block finishes; it has a different job from a `using Namespace;` declaration at the top of a file. Do not keep transaction-owned drawing objects for later use after the transaction ends. Learn the pattern from Autodesk's [transaction guide](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-DevGuide-Managed/files/GUID-12ADA0F2-C44D-4D88-B248-1803D39DF3AA.htm).

A drawing transaction is not a universal “undo everything” wrapper around Plant project database changes. Learn data-writing behavior separately.

### Transition checkpoint

You are ready for the intermediate exercises when you can explain the command's entry point, follow the data from selection to row to property, use a breakpoint to inspect a value, and handle cancel/missing-data cases in your practice command.

Do not judge progress by how many lines you have copied.

## Intermediate — build one useful, checkable tool

### I1. Define a small read-only deliverable

Build a **missing-tag report** for one explicitly chosen scope, such as the current P&ID drawing. Do not accidentally combine a current-drawing selection with a whole-project database query.

Write the specification first:

- Inputs: current project/drawing, target classes and the discovered tag property.
- Output: rows identifying objects with absent or empty tags.
- Exclusions: ordinary AutoCAD objects and out-of-scope classes.
- No result: distinguish “no problems found” from “the inspection could not run.”
- Evidence: compare against a small drawing whose expected results you recorded manually.

A useful test drawing might have three relevant objects: one with a tag, one with an empty tag, and one outside the chosen class filter. Record expected inclusion before running your tool.

### I2. Separate the responsibilities

| Responsibility | What it does | Can it be checked without Plant? |
|---|---|---|
| Command | Prompts, selects and reports messages | Host behavior needs Plant |
| Data reader | Retrieves the intended Plant properties | Needs the real API and project |
| Validation rule | Decides whether copied text is missing/invalid | Yes, using ordinary C# values |
| Report formatter | Converts findings into columns/text | Yes, with sample records |

Start with small methods in one project. More projects and complicated design patterns are not a requirement. Keep exceptions from API access distinct from ordinary findings such as “tag is empty.”

### I3. Add export after the report is correct

First print findings in the editor. Then write a CSV to an explicitly chosen path and report its location. CSV means comma-separated values; commas, quotes and line breaks inside a value need proper handling.

Inspect the existing `PlantHelpers.Csv` formatter. Before accepting your export, try a description containing a comma and a quotation mark, then check that it still occupies the correct column when opened. Decide what happens if the destination exists or is not writable.

The existing `P3DMTO` counts pieces; it is not a pipe-length takeoff. Likewise, `P3DDUMP` exports a whole table and `P3DSETPROPS` imports updates from CSV. Do not run a writer as a first inspection exercise.

### I4. Learn editing as a separate milestone

Before implementing edits, add a preview of object identity, old value and proposed value. Validate that the target property exists, is appropriate to write, and belongs to the intended project part. Apply a single test change first, then save/reopen and inspect the result.

The existing P3DSETPROPS example is a bulk CSV writer with simple comma splitting. It does not implement this preview workflow, and quoted commas require a proper CSV reader. Treat it as intermediate source to improve; do not infer those protections from the command name.

**Exercise:** list what should happen if the first object updates and the second fails.  
**Suggested answer:** report the successful and failed objects separately, do not claim the whole job succeeded or rolled back, and verify the actual state before retrying.

### I5. Add a user interface after the command works

A ribbon button is a convenient way to start an already-tested command. A palette is a panel that can stay open while the user works. That convenience introduces a question: has the active drawing or project changed since the panel opened?

Recheck context when the button is clicked. Learn host command context and document locking before using modeless UI to change drawings. Avoid background CAD API calls. See [chapter 18](18-plant3d-addin-development.md) and [chapter 14](14-best-practices.md).

### I6. Package and explain your tool

Use the deployment section of chapter 18. Keep a short release note containing: tool purpose, exact tested host/update, installation steps, command names, output location, known limitations and removal steps.

Test a fresh host session. A successful NETLOAD is one check; correct command behavior, report contents and bundle loading are separate checks.

### Intermediate checkpoint

Demonstrate your tool on a small known project, including a valid case, an empty result, cancellation and an unavailable prerequisite. Explain each failure message. Show the saved output and reconcile it with the source drawing. Only then expand scope or add writing.

## Related chapters by learning stage

Each linked chapter now includes its own beginner, transition and intermediate add-in section.

| Topic | Read |
|---|---|
| Tools, files and setup | [00 — Getting started](00-getting-started.md) |
| Why C# and an add-in | [01 — Choosing a language](01-choosing-a-language.md) |
| C# practice and standalone programs | [05 — C# / VB.NET](05-csharp-vbnet-exe.md) |
| Inventor add-ins, if you also use Inventor | [06 — Inventor add-ins](06-inventor-addins.md) |
| Commands, documents and transactions | [10 — AutoCAD .NET](10-autocad-dotnet-basics.md) |
| Plant project data and properties | [11 — Plant SDK](11-plant3d-sdk.md) |
| Understanding and investigating errors | [13 — Errors and fixes](13-errors-and-fixes.md) |
| Testing, organization, packaging and sharing | [14 — Best practices](14-best-practices.md) |
| Choosing an exercise at the right level | [15 — Projects](15-projects.md) |
| Vocabulary, in learning order | [16 — Glossary](16-glossary.md) |
| Putting the Plant add-in together | [18 — Plant add-in development](18-plant3d-addin-development.md) |

For the Plant track, you do not need to complete Inventor, VBA, iLogic, Python or cloud automation first.

## A learning record you can reuse

```text
Lesson:
My one small change:
Why I think it will work:
Expected result:
Actual result:
Exact error, if any:
What I checked:
What I can now explain:
Next small step:
```

All exercises here are learning instructions. They are not a claim that the repository examples have been executed on your workstation.
