# Code style

## C# code style

- Code should be formatted with `dotnet format` or Rider's built-in formatter.
- Variables should always be declared with their type name, unless the type is obvious from the declaration.
  (For example, `var stream = new Stream()` or `var db = services.GetRequiredService<DatabaseContext>()`)

### Naming

- Service values should be named the same as the type, but camel case, if the name of the service does *not*
  in a verb (i.e. a variable of type `KeyCacheService` should be named `keyCacheService`).
  If the name of the service *does* end in a verb, the final "service" should be omitted
  (i.e. a variable of type `UserRendererService` should be named `userRenderer`).
- Interface values should be named the same as the type, but camel case, without the leading `I`
  (i.e. a variable of type `ISnowflakeGenerator` should be named `snowflakeGenerator`).
- Values of type `DatabaseContext` should always be named `db`.

There are some exceptions to this. For example Sentry's `IHub` should be named `sentry` as the name `hub` isn't clear.

## TypeScript code style

Use `prettier` for formatting the frontend code.