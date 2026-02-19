
---

# SnapTrace 🛰️

SnapTrace is a "Black Box" flight recorder for C# applications. Utilizing C# 12 Interceptors, it automatically records method arguments and class states into a high-speed memory buffer (serialized via `System.Text.Json`) without requiring boilerplate code.

It runs silently in production. When a crash occurs, you dump the history to see the exact sequence of states and inputs that led to the failure.

---

## 🛠️ The Attributes

Use these to instruct the Source Generator on what to intercept and record.

* **`[SnapTrace]` (Class):** Opts the class into recording. Records all public methods by default `(TracePrivate = false)`.
* **`[SnapTrace]` (Method):** Opts a specific method into recording (useful for capturing private/internal logic).
* **`[SnapIgnore]` (Parameter):** Redacts sensitive data from the trace buffer.
* **`[SnapIgnore]` (Method):** Opts a specific method out of a class-wide `[SnapTrace]`.
* **`[SnapContext]` (Property/Field):** Captures the value of this class-level variable alongside every intercepted method call.

> **Note on Local Variables:** Because interceptors wrap method *calls* and cannot see inside method *bodies*, local variables cannot use attributes. To capture local state mid-method, use the manual inline call: `SnapTraceObserver.SnapLocal(new { x, y });`

---

## 🚀 Initialization & Settings

Configure the ring buffer and your preferred output sink at application startup.

```csharp
SnapTraceObserver.Initialize(new SnapOptions {
    BufferSize = 200,         // Number of method calls to retain in memory
    RecordTimestamp = true,   // Prepends timestamps to each recorded frame
    
    // Pipe the dumped buffer to your logging framework, console, or a file
    Output = message => Log.Fatal("SnapTrace Crash Dump:\n{History}", message) 
});

```

---

## 🏗️ The Build Flag (Zero Overhead)

SnapTrace is designed to be completely removable. By passing the `NoTrace` MSBuild property, the Source Generator will skip writing the interceptors. No generated code means zero performance overhead in the compiled binary.

**Standard Build (SnapTrace Active & Listening):**

```bash
dotnet build -c Release

```

**No-Trace Build (SnapTrace Completely Stripped):**

```bash
dotnet build -c Release -p:NoTrace=true

```
