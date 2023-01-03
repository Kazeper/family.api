# family.api

## Requirements

+ [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
+ SQL server/[SQL server express](https://download.microsoft.com/download/7/f/8/7f8a9c43-8c8a-4f7c-9f92-83c18d96b681/SQL2019-SSEI-Expr.exe)

## Initial run

### Create database

Install entity framework:
```
dotnet tool install --global dotnet-ef
```


Connection string should be provided in *appsettings.Development.json*. To create/update database specified in connection string run in cmd/powershell:
```
dotnet ef database update
```

### Run from cmd/powershell

To run api with default url execute command:
```
dotnet run
```

or specify port using:
```
dotnet run --urls="https://localhost:9203"
```

## Swagger documentation
To open Swagger doc run the api and append '/swagger' to the default url: `https://localhost:7011/swagger`
