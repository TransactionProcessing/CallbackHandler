FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CallbackHandler/NuGet.Config", "."]
COPY ["CallbackHandler/CallbackHandler.csproj", "CallbackHandler/"]
COPY ["CallbackHandler.BusinessLogic/CallbackHandler.BusinessLogic.csproj", "CallbackHandler.BusinessLogic/"]
COPY ["CallbackHandler.CallbackMessageAggregate/CallbackHandler.CallbackMessageAggregate.csproj", "CallbackHandler.CallbackMessageAggregate/"]
COPY ["CallbackHandlers.Models/CallbackHandlers.Models.csproj", "CallbackHandlers.Models/"]
COPY ["CallbackHandler.CallbackMessage.DomainEvents/CallbackHandler.CallbackMessage.DomainEvents.csproj", "CallbackHandler.CallbackMessage.DomainEvents/"]
COPY ["CallbackHandler.DataTransferObjects/CallbackHandler.DataTransferObjects.csproj", "CallbackHandler.DataTransferObjects/"]
RUN dotnet restore "CallbackHandler/CallbackHandler.csproj"
COPY . .
WORKDIR "/src/CallbackHandler"
RUN dotnet build "CallbackHandler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CallbackHandler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CallbackHandler.dll"]