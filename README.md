# CallbackHandler

## Development setup

Install the .NET 11 SDK. The repository pins SDK `11.0.100` in `global.json` and rolls
forward to later feature bands in that release line.

From the repository root, restore and build the solution with:

```powershell
dotnet restore CallbackHandler.sln
dotnet build CallbackHandler.sln --configuration Release
```

Run the unit tests with:

```powershell
dotnet test CallbackHandler.BusinessLogic.Tests/CallbackHandler.BusinessLogic.Tests.csproj --configuration Release
dotnet test CallbackHandler.CallbackMessageAggregate.Tests/CallbackHandler.CallbackMessageAggregate.Tests.csproj --configuration Release
dotnet test CallbackHandler.Tests/CallbackHandler.Tests.csproj --configuration Release
```

Integration tests use Docker containers and require Docker to be available.

## Build Status

[![Last Nightly Build Status Badge](https://github.com/TransactionProcessing/CallbackHandler/actions/workflows/nightlybuild.yml/badge.svg)](https://github.com/TransactionProcessing/CallbackHandler/actions/workflows/nightlybuild.yml)
[![Last Release Build Status Badge](https://github.com/TransactionProcessing/CallbackHandler/actions/workflows/createrelease.yml/badge.svg)](https://github.com/TransactionProcessing/CallbackHandler/actions/workflows/createrelease.yml)

## Code Coverage Results

## Code Quality Results

[![CodeFactor](https://www.codefactor.io/repository/github/transactionprocessing/callbackhandler/badge)](https://www.codefactor.io/repository/github/transactionprocessing/callbackhandler)

![Alt](https://repobeats.axiom.co/api/embed/32b48c2ad661e507573444af606488540d6d681d.svg "Repobeats analytics image")
