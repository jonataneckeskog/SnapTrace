> [!CAUTION]
> ## ⚠️ **DISCLAIMER: UNDER ACTIVE DEVELOPMENT**
> **SnapTrace is currently in an early experimental stage.** The current version likely **will not work as intended**. Use of this library may result in **crashes, build errors, or unpredictable behavior** within your application. It is not yet recommended for any use beyond isolated experimentation.
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

    private AccountService _accountService; // 3. Private method, opted out by default.

    [SnapTraceIgnore] // 4. Opts-out: This public method will NOT be traced.
    public void Ping() { }

    // 5. Traced by default. 'amount' is recorded, 'pin' is ignored/redacted.
    public void Deposit([SnapTraceIgnore] string pin, decimal amount) 
    {
        CalculateInterest(); 
        SyncToDatabase();
    }

    // 6. NOT traced: Private methods are ignored by default to reduce noise.
    private void CalculateInterest() { }

    // 7. Explicitly traced: Opt-in required for private/internal methods.
    [SnapTrace] 
    private void SyncToDatabase() { }

    // 8. Capture the parameter AccountId without mutation.
    public AccountService getAccountService([SnapTraceDeep] AccountId accountId)
    {
        return _accountService;
    }
}

```

---

## 🛠️ Attributes

Use these to instruct the Source Generator on what to intercept and record.

* **`[SnapTrace]` (Class/Method):** Opts the class into recording. Records all public methods by default.
* **`[SnapTrace]` (Method):** Explicitly opts a method into recording (required for capturing private/internal logic).
* **`[SnapTraceIgnore]` (Method):** Opts a specific public method out of a class-wide `[SnapTrace]`.
* **`[SnapTraceIgnore]` (Parameter/Return):** Redacts sensitive data from the trace buffer, preventing PII leaks.
* **`[SnapTraceDeep]`(Method):** Forces a deep copy of all parameters and returns values to be stored in the trace buffer. Each value which is logged with SnapDeep is required to implement .Clone().
* **`[SnapTraceDeep]`(Parameter/Return):** Specific opt-ins for deep copying parameters or return values.
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
dotnet build -c Release -p:SnapTraceDisable=true

```

## Examples

### BankService

#### Program

```csharp
using SnapTrace;

// 1. Setup the recorder at the very start of the app
SnapTraceObserver.Initialize(new SnapOptions
{
    BufferSize = 10,
    RecordTimestamp = true,
    Output = message => Console.WriteLine($"--- SNAPTRACE CRASH REPORT ---\n{message}")
});

Console.WriteLine("Starting SnapTrace Playground...");

// 2. Trigger the intercepted code
var service = new BankService();
service.Deposit("1234-SECRET", 500.00m);

// 3. Simulate a crash to cause a dump
throw new ArgumentException("Something went wrong!");


[SnapTrace]
public class BankService
{
    [SnapTraceContext]
    private decimal _currentBalance = 1000.00m;

    public void Deposit([SnapTraceIgnore] string pin, decimal amount)
    {
        _currentBalance += amount;
        Console.WriteLine($"Deposited {amount}. New Balance: {_currentBalance}");
    }
}
```

#### Output

```txt
Starting SnapTrace Playground...
Deposited 500.00. New Balance: 1500.00
--- SNAPTRACE CRASH REPORT ---
{
  "Status": "Call",
  "Method": "Deposit",
  "Timestamp": "14:43:01.491",
  "Data": [
    "[REDACTED]",
    500.00
  ],
  "Context": {
    "_currentBalance": 1000.00
  }
}
{
  "Status": "Return",
  "Method": "Deposit",
  "Timestamp": "14:43:01.497",
  "Data": null,
  "Context": {
    "_currentBalance": 1500.00
  }
}
{
  "Status": "Error",
  "Method": "<Main>$",
  "Timestamp": "14:43:01.556",
  "Data": "Something went wrong!",
  "Context": "   at Program.<Main>$(String[] args) in /path/to/project/Program.cs:line 18"
}
Unhandled exception. System.ArgumentException: Something went wrong!
   at Program.<Main>$(String[] args) in /path/to/project/Program.cs:line 18
```
