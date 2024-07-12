# Code style

## C# code style

Code should be formatted with `dotnet format` or Rider's built-in formatter.  
Variables should *always* be declared using `var`, unless the correct type
can't be inferred from the declaration (i.e. if the variable needs to be an
`IEnumerable<T>` instead of a `List<T>`, or if a variable is initialized as `null`).

## TypeScript code style

Use `prettier` for formatting the frontend code.