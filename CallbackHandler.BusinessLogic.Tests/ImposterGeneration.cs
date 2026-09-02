using Imposter.Abstractions;
using Microsoft.AspNetCore.Hosting;
using SecurityService.Client;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using TransactionProcessor.Client;

[assembly: GenerateImposter(typeof(CallbackHandler.BusinessLogic.Services.ICallbackDomainService))]
[assembly: GenerateImposter(typeof(Shared.EventStore.Aggregate.IAggregateRepository<,>))]
[assembly: GenerateImposter(typeof(ISecurityServiceClient))]
[assembly: GenerateImposter(typeof(ITransactionProcessorClient))]
[assembly: GenerateImposter(typeof(IWebHostEnvironment))]
