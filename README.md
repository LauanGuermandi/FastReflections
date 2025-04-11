# FastReflections

**FastReflections** is a lightweight and high-performance utility library for .NET that provides fast, cached reflection-based access to methods, properties, using expressions, and types with clean and expressive syntax.

---

## Installation

```bash
dotnet add package FastReflections
```

---

## Usage

### Invoke Method

```csharp
var instance = new MyClass();
var method = typeof(MyClass).GetMethod("SayHello");

var result = FastReflections.Invoke<MyClass, string>(method, instance, "John");
// result => "Hello, John"
```

### Get Property

```csharp
var instance = new MyClass { Name = "FastReflections" };

var value = FastReflections.GetPropertyValue(instance, x => x.Name);
// value => "FastReflections"
```

### Set Property

```csharp
var instance = new MyClass();

FastReflections.SetPropertyValue(instance, x => x.Name, "Updated");
// instance.Name => "Updated"
```

### Find Type by Name

```csharp
var type = FastReflections.GetTypeByName("MyNamespace.MyClass");
// type => typeof(MyClass)
```

---

## When to Use

Use `FastReflections` when:

- You need dynamic access to methods and properties
- You want performance without writing boilerplate reflection code
- You’re building frameworks, tools, or extensible plugins

---

## Internals

- Uses `Expression Trees` for performance
- Thread-safe `ConcurrentDictionary` for caching
- Automatic type cache loading via `AssemblyLoad` event

---

##  License

MIT License © LauanGuermandi