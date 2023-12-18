# Introduction
The following coding guidelines are inspired from MonoGame and Microsoft guidelines. The naming conventions are very similar to the default Visual Studio's C# editor conventions, this makes a more grateful integration with the IDE.
# Coding Guidelines
## File Encoding
All source files must be encoded in **UTF-8 with BOM**.
## Tabs & Indenting
* All indentation should be done with space characters. 
* Default indentation level is 4 spaces.
* Tabs are prohibited.
## Bracing
Open braces should always be at the beginning of the line after the statement that begins the block. Contents of the brace should be indented by 4 spaces. Single statements do not have braces. For example:
```
if (someExpression)
{
   DoSomething();
   DoAnotherThing();
}
else
   DoSomethingElse();
```
`case` statements should be indented from the switch statement like this:
```
switch (someExpression) 
{
   case 0:
      DoSomething();
      break;
   case 1:
      DoSomethingElse();
      break;
   case 2: 
      int n = 1;
      DoAnotherThing(n);
      break;
}
```
Single `if` statements can be optionally collapsed into a single line like this, specially `return`:
```
if (someExpression) return someValue;
```
Braces are not used for single statement blocks immediately following a `for`, `foreach`, `if`, `do`, etc. The single statement block should always be on the following line in any kind of loop and indented by four spaces. This increases code readability and maintainability.
```
for (int i = 0; i < 100; ++i)
    DoSomething(i);
```

## Single line property statements
It's recommended to use one liner Property statements without braces. Add a single space before and after the braces.
```
public class Foo
{
   int _bar;

   public int Bar { get => _bar; set => _bar = value; }
}
```
Single line property statements can have braces that begin and end on the same line. This should only be used for simple property statements.  
```
public class Foo
{
   int _bar;

   public int Bar
   {
      get { return _bar; }
      set { _bar = value; }
   }
}
```

## Commenting
Comments should be used to describe intention, algorithmic overview, and/or logical flow.  It would be ideal if, from reading the comments alone, someone other than the author could understand a function's intended behavior and general operation. While there are no minimum comment requirements (and certainly some very small routines need no commenting at all), it is best that most routines have comments reflecting the programmer's intent and approach.

Comments must provide added value or explanation to the code. Simply describing the code is not helpful or useful.
```
    // Wrong
    // Set count to 1
    count = 1;

    // Right
    // Set the initial reference count so it isn't cleaned up next frame
    count = 1;
```

### Copyright/License notice
Each file should start with a copyright notice. This is a short statement declaring the project name and copyright notice, and directing the reader to the license document elsewhere in the project. To avoid errors in doc comment builds, avoid using triple-slash doc comments.
```
/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * Copyright (c) 2023 Daniel Molinero Lucas
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 * 
 * Scene.cs
 * Represents an environment containing a collection of active behaviours, exposes
 * multiple methods to manipulate the scene. This class is mainly used in the
 * Sceme Management system.
 */
```

### Documentation Comments
All methods should use XML doc comments. For internal dev comments, the `<devdoc>` tag should be used.
It's recommended to derive this comments to external xml files and include the xml comments to the source code.
```
public class Foo 
{
    /// <summary>Public stuff about the method</summary>
    /// <param name="bar">What a neat parameter!</param>
    /// <devdoc>Cool internal stuff!</devdoc>
    public void MyMethod(int bar)
    {
        ...
    }
}
```

### Comment Style
The // (two slashes) style of comment tags should be used in most situations. Wherever possible, place comments above the code instead of beside it.  Here are some examples:
```
    // This is required for WebClient to work through the proxy
    GlobalProxySelection.Select = new WebProxy("http://itgproxy");

    // Create object to access Internet resources
    WebClient myClient = new WebClient();
```

## Spacing
Spaces improve readability by decreasing code density. Here are some guidelines for the use of space characters within code:

Do use a single space after a comma between function arguments.
```
Console.In.Read(myChar, 0, 1);  // Right
Console.In.Read(myChar,0,1);    // Wrong
```
Do not use a space after the parenthesis and function arguments.
```
CreateFoo(myChar, 0, 1)         // Right
CreateFoo( myChar, 0, 1 )       // Wrong
```
Do not use spaces between a function name and parentheses.
```
CreateFoo()                     // Right
CreateFoo ()                    // Wrong
```
Do not use spaces inside brackets.
```
x = dataArray[index];           // Right
x = dataArray[ index ];         // Wrong
```
Do use a single space before flow control statements.
```
while (x == y)                  // Right
while(x==y)                     // Wrong
```
Do use a single space before and after binary operators.
```
if (x == y)                     // Right
if (x==y)                       // Wrong
```
Do not use a space between a unary operator and the operand.
```
++i;                            // Right
++ i;                           // Wrong
```
Do not use a space before a semi-colon. Do use a space after a semi-colon if there is more on the same line.
```
for (int i = 0; i < 100; ++i)   // Right
for (int i=0 ; i<100 ; ++i)     // Wrong
```

## Naming
Follow all .NET Framework Design Guidelines for both internal and external members. Highlights of these include:
* Do not use Hungarian notation
* Do use an underscore prefix for member variables, e.g. "_foo"
* Do use camelCasing for member variables (first word all lowercase, subsequent words initial uppercase)
* Do use camelCasing for parameters
* Do use camelCasing for local variables
* Do use PascalCasing for function, property, event, and class names (all words initial uppercase)
* Do use PascalCasing for public static member variables.
* Do prefix interfaces names with "I"
* Do not prefix enums, classes, or delegates with any letter

The reasons to extend the public rules (no Hungarian, underscore prefix for member variables, etc.) is to produce a consistent source code appearance. In addition, the goal is to have clean, readable source. Code legibility should be a primary goal.

## File Organization
* Source files should contain only one public type, although multiple internal types are permitted if required
* Source files should be given the name of the public type in the file
* Directory names should follow the namespace for the class after `Core`. For example, one would expect to find the public class `ScapeCore.Core.Batching.Resources.ResourceWrapper` in **Core\Batching\Resources\ResourceWrapper.cs**
* Class members should be grouped logically, and encapsulated into regions (Fields, Constructors, Properties, Events, Methods, Private interface implementations, Nested types)
* Using statements should be before the namespace declaration.
```
using System;

namespace MyNamespace 
{
    public class MyClass : IFoo 
    {
        #region Fields
        int _foo;
        #endregion

        #region Properties
        public int Foo { get { ... } set { ... } }
        #endregion

        #region Constructors
        public MyClass()
        {
            ...
        }
        #endregion

        #region Events
        public event EventHandler FooChanged { add { ... } remove { ... } }
        #endregion

        #region Methods
        void DoSomething()
        {
            ...
        }

        void FindSomething()
        {
            ...
        }
        #endregion

        #region Private interface implementations
        void IFoo.DoSomething()
        {
            DoSomething();
        }
        #endregion

        #region Nested types
        class NestedType
        {
            ...
        }
        #endregion
    }
}
```
## Method conventions
You should always try to split your methods into more concrete, and meaningfull methods. This enhances code maintainability.
When you are working with inheritance if a local value used by a method could be modified in a override implementation of the method, instead of making the method overrideable, create a virtual property that holds the value and can be overrided, this improves code maintainability and cyclomatic complexity.

```
public class Foo
{
    // Fields representing the foo's properties
    private double _fooWidth;
    private double _fooHeight;

    // Public properties for accessing the foo's dimensions
    public virtual double FooWidth { get => _fooWidth; set => _fooWidth = value; }
    public virtual double FooHeight { get => _fooHeight; set => _fooHeight = value; }

    // Constructor to initialize the foo
    public Foo(double width, double height)
    {
        _fooWidth = width;
        _fooHeight = height;
    }

    // Method to calculate and return the area of the foo
    public double CalculateFooArea() => _fooWidth * _fooHeight;

    // Method to display information about the foo
    public void DisplayFooInfo() => Log.Debug($"Foo: Width = {_fooWidth}, Height = {_fooHeight}, Area = {CalculateFooArea()}");
}

// Inherited class representing a specific type of foo (e.g., Bar)
public class Bar : Foo
{
    // Additional property specific to Bar
    public double BarDiagonal => Math.Sqrt(FooWidth * FooWidth + FooHeight * FooHeight);

    // Constructor calling the base class constructor
    public Bar(double width, double height) : base(width, height)
    {
    }

    // Override the FooWidth property to enforce constraints
    public override double FooWidth { get => base.FooWidth; set => base.FooWidth = Math.Max(value, 0); }

    // Override the FooHeight property to enforce constraints
    public override double FooHeight { get => base.FooHeight; set => base.FooHeight = Math.Max(value, 0); }

    // Override the CalculateFooArea method to provide a specialized implementation
    public override double CalculateFooArea() => base.CalculateFooArea() * BarDiagonal; // A custom formula for Bar's area
}
```

# Useful Links
[C# Coding Conventions (MSDN)](http://msdn.microsoft.com/en-us/library/ff926074.aspx)
