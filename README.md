# gimzo

Stock Market Data Collection and Analysis

## Getting Started

### FinancialData.Net

For data collection, gimzo uses the [FinancialData.Net](https://www.financialdata.net/) (FDN) API.

The current version of gimzo is built on the `Standard` subscription, but not all endpoints have been implemented yet.

You will need your own key to make gimzo function.

### Databases

Please review the README.md in the project's `/database` folder.
It will walk you through setting up your databases.

### Connection Strings

Gimzo expects a `secrets.json` file in the root of the Infrastructure tests directory.
As in: `src/dotnet/tests/Gimzo.Infrastructure.Tests/secrets.json`
The same (or similar) file is expected in the root of the CLI application, as in:
`src/dotnet/apps/Gimzo.Cli/secrets.json`.

The `secrets.json` file contains two connection strings and your financialdata.net API key.
Here is an example for the test application.

```json
{
  "ConnectionStrings": {
    "Gimzo": "User ID=gimzo_admin;Password=YOUR PASSWORD;Host=127.0.0.1;Port=5432;Database=gimzo_test;",
    "Gimzo-Read": "User ID=gimzo_reader;Password=YOUR PASSWORD;Host=127.0.0.1;Port=5432;Database=gimzo_test;"
  },
  "ApiKeys": {
    "financialdata.net": "YOUR KEY HERE"
  }
}
```

### Running Tests

Some of the tests are INTEGRATION tests, meaning that they actively use both the FDN API and the database.
Once you have the databases created, an FDN API key, and a valid `secrets.json` file within your test app, you should be able to run all tests without error.