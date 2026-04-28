Run the following after installing the EF Core CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The project is code-first; migrations are intentionally generated from the current model so they match the selected provider and local connection string.
