# 🧱 C# Class and Method Components Cheat Sheet

A structured reference for understanding what can be declared inside a **C# class**, and how **methods**, **modifiers**, and **return types** work.

---

## 🧩 Class Members Overview

| **Member Type** | **Purpose / Description** | **Common Modifiers** | **Example** |
|------------------|----------------------------|----------------------|--------------|
| **Field** | Stores data or state directly inside the class. | `public`, `private`, `protected`, `static`, `readonly`, `const` | `private int speed;`<br>`public const double PI = 3.14;` |
| **Property** | Controlled access to fields (uses `get`/`set`). | `public`, `private`, `protected`, `static`, `virtual`, `override` | `public int Speed { get; set; }` |
| **Method** | Defines behavior or actions the class can perform. | `public`, `private`, `protected`, `internal`, `static`, `virtual`, `override`, `async` | `public void Drive() { ... }` |
| **Constructor** | Initializes new objects of the class. | `public`, `private`, `protected`, `static` | `public Car(string color) { Color = color; }` |
| **Destructor (finalizer)** | Called by GC before an object is destroyed (rarely used). | *(always protected)* | `~Car() { /* cleanup */ }` |
| **Event** | Used for signaling and subscribing to actions (observer pattern). | `public`, `private`, `protected`, `static` | `public event Action OnStart;` |
| **Delegate** | Defines a type for methods with a specific signature. | `public`, `private`, `protected`, `static` | `public delegate void Notify(string msg);` |
| **Indexer** | Allows class instances to be accessed like arrays. | `public`, `private`, `protected` | `public int this[int i] { get => data[i]; set => data[i] = value; }` |
| **Operator** | Overloads an operator like `+`, `-`, `==`, etc. | `public static` | `public static Car operator +(Car a, Car b) { ... }` |
| **Nested Type** | Declares another class/struct/enum/interface inside. | `public`, `private`, `protected`, `internal`, `static` | `private class Engine { ... }` |
| **Constant** | Compile-time constant. | `public`, `private`, `protected`, `internal`, `const` | `public const int Wheels = 4;` |
| **Read-only field** | Assignable only in constructor or declaration. | `public`, `private`, `protected`, `readonly` | `private readonly string vin;` |
| **Static constructor** | Runs once before any static member is accessed. | `static` | `static Car() { Console.WriteLine("Static init"); }` |

---

## ⚙️ Access Modifiers

| **Modifier** | **Accessible From** | **Typical Use** |
|---------------|----------------------|-----------------|
| `public` | Anywhere | Public API or shared logic |
| `private` | Only inside the same class | Hidden internal logic |
| `protected` | Inside the class or subclasses | Base class inheritance |
| `internal` | Within the same assembly | Internal components |
| `protected internal` | Subclasses or same assembly | Mixed visibility |
| `private protected` | Subclasses in same assembly only | Restricted hybrid |

---

## 🔧 Additional Modifiers

| **Modifier** | **Applies To** | **Meaning** |
|---------------|----------------|--------------|
| `static` | Class, method, field, property | Belongs to the type, not instances |
| `readonly` | Field | Assignable only once |
| `const` | Field | Compile-time constant |
| `abstract` | Class, method, property | Must be implemented by a subclass |
| `virtual` | Method, property | Can be overridden in a subclass |
| `override` | Method, property | Replaces a base version |
| `sealed` | Class, method | Prevents inheritance or further overrides |
| `partial` | Class, struct, interface, method | Code split across files |
| `async` | Method | Enables asynchronous operations |
| `extern` | Method | Implemented externally (e.g., native DLL) |
| `unsafe` | Method, block | Allows pointer use (`/unsafe` needed) |
| `new` | Method, property | Hides a base class member |

---

## 💡 Common Return Types in C#

| **Return Type** | **Category** | **Description / Example** |
|------------------|--------------|----------------------------|
| `void` | None | Does *not* return a value |
| `int`, `float`, `double`, `decimal` | Numeric | `int Add(int a, int b) => a + b;` |
| `bool` | Logical | `bool IsEven(int n) => n % 2 == 0;` |
| `string` | Text | `string GetName() => "ChatGPT";` |
| `char` | Character | `char GetSymbol() => 'A';` |
| `object` | General | `object GetValue() => 42;` |
| `T` | Generic | `T Echo<T>(T value) => value;` |
| `ClassName` | Custom type | `Car CreateCar() => new Car("Red");` |
| `StructName` | Custom value type | `Point GetPoint() => new(3, 5);` |
| `EnumName` | Enum type | `DayOfWeek GetDay() => DayOfWeek.Monday;` |
| `(int, string)` | Tuple | `(int, string) GetPair() => (1, "One");` |
| `List<T>`, `Dictionary<K,V>` | Collections | `List<string> GetNames() => new() { "A", "B" };` |
| `Task`, `Task<T>` | Async | `async Task<int> GetDataAsync() => 5;` |
| `IEnumerable<T>` | Iterator | `IEnumerable<int> GetNumbers() { yield return 1; }` |

---

## 🧮 Return Type Rules

| **Rule** | **Explanation** |
|-----------|----------------|
| Must match declared type | The returned value must match the declared return type. |
| Only one return type | A method can return only one type (but can pack multiple values). |
| `void` means no return | No `return` value is required. |
| Async must return `Task` or `Task<T>` | Required by the async model. |
| Constructors have no return type | They return their class instance implicitly. |

---

## 🧠 Examples

```csharp
public class Car
{
    private readonly string vin;
    public static int TotalCars;
    private int speed;

    public const int Wheels = 4;
    public int Speed { get => speed; set => speed = value; }

    public Car(string vin) { this.vin = vin; TotalCars++; }
    static Car() { TotalCars = 0; }

    public virtual void Drive() => Console.WriteLine($"Car {vin} driving at {Speed} km/h");
    public event Action OnStart;

    private class Engine { public void Start() => Console.WriteLine("Engine started."); }

    public static bool operator ==(Car a, Car b) => a.vin == b.vin;
    public static bool operator !=(Car a, Car b) => !(a == b);
}
