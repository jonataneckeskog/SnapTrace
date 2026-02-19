
---

# SnapTrace 🛰️

SnapTrace is a "Black Box" flight recorder for C# applications. Utilizing C# 12 Interceptors, it automatically records method arguments and class states into a high-speed memory buffer (serialized via `System.Text.Json`) without requiring boilerplate code.

It runs silently in production. When a crash occurs, you dump the history to see the exact sequence of states and inputs that led to the failure.

---

## 🎯 Showcase

SnapTrace relies on a strict, intuitive opt-in/opt-out design to keep your traces clean, secure, and noise-free.

```csharp
[SnapTrace] // 1. Opts-in the class. Traces ALL public methods by default.
public class BankService
{
    [SnapTraceContext] // 2. Context inclusion: Captures this field with every method call.
    private decimal _currentBalance;

    [SnapTraceIgnore] // 3. Opts-out: This public method will NOT be traced.
    public void Ping() { }

    // 4. Traced by default. 'amount' is recorded, 'pin' is ignored/redacted.
    public void Deposit([SnapTraceIgnore] string pin, decimal amount) 
    {
        CalculateInterest(); 
        SyncToDatabase();
    }

    // 5. NOT traced: Private methods are ignored by default to reduce noise.
    private void CalculateInterest() { }

    // 6. Explicitly traced: Opt-in required for private/internal methods.
    [SnapTrace] 
    private void SyncToDatabase() { }
}

```

---

## 🛠️ Attributes

Use these to instruct the Source Generator on what to intercept and record.

* **`[SnapTrace]` (Class):** Opts the class into recording. Records all public methods by default.
* **`[SnapTrace]` (Method):** Explicitly opts a method into recording (required for capturing private/internal logic).
* **`[SnapTraceIgnore]` (Method):** Opts a specific public method out of a class-wide `[SnapTrace]`.
* **`[SnapTraceIgnore]` (Parameter):** Redacts sensitive data from the trace buffer, preventing PII leaks.
* **`[SnapTraceContext]` (Property/Field):** Captures the value of this class-level variable alongside every intercepted method call.

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

## 🏗️ Build Flag (Zero Overhead)

SnapTrace is designed to be completely removable. By passing the `NoTrace` MSBuild property, the Source Generator will skip writing the interceptors. No generated code means zero performance overhead in the compiled binary.

**Standard Build (SnapTrace Active & Listening):**

```bash
dotnet build -c Release

```

**No-Trace Build (SnapTrace Completely Stripped):**

```bash
dotnet build -c Release -p:NoTrace=true

```
