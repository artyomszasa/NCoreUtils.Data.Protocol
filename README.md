## Using explicit type descriptors for complex property types

Entities may have some properties with a complex type that cannot be handled neither by the generator nor by the
protocol itself. Yet thay can be used as opaque types or explicit logic can be created to handle some of thier
functionality.

To achieve this explicit type descriptor must be used and must be passed to the context generator. Explicit type
descriptor must be a class that has a default constructor and that implements the `ITypeDescriptor<T>` interface
(where `T` is a described complex type itself). e.g.:

```
using NCoreUtils.Data.Protocol.Internal;

namespace MyNamespace;

public MyComplexType { /* ... */ }

public MyDescriptorOfTheComplexType : ITypeDescriptor<MyComplexType>
{
    /* ... interface implementation ... */
}

```

Passing the explicit type descriptor can be done using `ProtocolDescriptorAttribute`:

```

[ProtocolEntity(typeof(SomeEntityWithMyComplexType))]
[ProtocolDescriptor(typeof(MyDescriptorOfTheComplexType))]
public partial class MyDataContext { }

```
