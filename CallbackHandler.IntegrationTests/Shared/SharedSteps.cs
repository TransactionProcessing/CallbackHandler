using CallbackHandler.IntegrationTests.Common;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CallbackHandler.DataTransferObjects;
using Newtonsoft.Json;
using SecurityService.DataTransferObjects.Requests;
using SecurityService.DataTransferObjects.Responses;
using SecurityService.IntegrationTesting.Helpers;
using Shouldly;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.IntegrationTesting.Helpers;
using ClientDetails = CallbackHandler.IntegrationTests.Common.ClientDetails;
using ReqnrollTableHelper = Shared.IntegrationTesting.ReqnrollTableHelper;

namespace CallbackHandler.IntegrationTests.Shared
{
    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        private readonly SecurityServiceSteps SecurityServiceSteps;

        private readonly TransactionProcessorSteps TransactionProcessorSteps;

        public SharedSteps(ScenarioContext scenarioContext,
            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(testingContext.DockerHelper.SecurityServiceClient);
            this.TransactionProcessorSteps = new TransactionProcessorSteps(testingContext.DockerHelper.TransactionProcessorClient, testingContext.DockerHelper.TestHostHttpClient,
                testingContext.DockerHelper.ProjectionManagementClient);

        }

        [Given(@"I have the following Bank Deposit Callbacks")]
        public async Task GivenIHaveTheFollowingBankDepositCallbacks(DataTable table)
        {
            List<Deposit> requests = table.Rows.ToDepositRequests();

            List<Guid> allEstateIds = this.TestingContext.GetAllEstateIds();
            var estateId = allEstateIds.First();
            var estateDetails = this.TestingContext.GetEstateDetails(estateId);
            var merchantList = estateDetails.GetMerchants();
            var merchantId = merchantList.First().MerchantId;
            foreach (Deposit request in requests) {
                request.Reference = $"{estateId:N}-{merchantId:N}";
            }

            this.TestingContext.Deposits = requests;
        }

        [When(@"I send the requests to the callback handler for deposits")]
        public async Task WhenISendTheRequestsToTheCallbackHandlerForDeposits()
        {
            using HttpClient client = new();
            foreach (var testingContextDeposit in this.TestingContext.Deposits)
            {
                String requestUri =
                    $"http://localhost:{this.TestingContext.DockerHelper.GetCallbackHandlerPort()}/api/callbacks";
                HttpRequestMessage msg = new(HttpMethod.Post, requestUri);
                var payload = JsonConvert.SerializeObject(testingContextDeposit);

                msg.Content = new StringContent(payload, Encoding.UTF8,
                    MediaTypeHeaderValue.Parse("application/json"));
                var response = await client.SendAsync(msg);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
                var content = await response.Content.ReadAsStringAsync();
                CallbackResponse responseData =
                    JsonConvert.DeserializeObject<CallbackResponse>(content);

                this.TestingContext.SentCallbacks.Add(responseData.CallbackId, payload);
            }
        }

        [Then("the deposit records are recorded")]
        public async Task ThenTheDepositRecordsAreRecorded()
        {
            using HttpClient client = new();
            foreach (var sentCallback in this.TestingContext.SentCallbacks)
            {
                String requestUri =
                    $"http://localhost:{this.TestingContext.DockerHelper.GetCallbackHandlerPort()}/api/callbacks/{sentCallback.Key}";
                HttpRequestMessage msg = new(HttpMethod.Get, requestUri);
                var response = await client.SendAsync(msg);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                var originalDeposit = JsonConvert.DeserializeObject<Deposit>(sentCallback.Value);

                var content = await response.Content.ReadAsStringAsync();
                CallbackMessage responseData =
                    JsonConvert.DeserializeObject<CallbackMessage>(content);


                responseData.Reference.ShouldBe(originalDeposit.Reference);
                var callbackDeposit = JsonConvert.DeserializeObject<Deposit>(responseData.Message);
                callbackDeposit.AccountNumber.ShouldBe(originalDeposit.AccountNumber);
            }
        }

        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(DataTable table)
        {
            List<CreateApiScopeRequest> requests = table.Rows.ToCreateApiScopeRequests();
            await this.SecurityServiceSteps.GivenICreateTheFollowingApiScopes(requests);
        }

        [Given(@"the following api resources exist")]
        public async Task GivenTheFollowingApiResourcesExist(DataTable table)
        {
            List<CreateApiResourceRequest> requests = table.Rows.ToCreateApiResourceRequests();
            await this.SecurityServiceSteps.GivenTheFollowingApiResourcesExist(requests);
        }

        [Given(@"the following clients exist")]
        public async Task GivenTheFollowingClientsExist(DataTable table)
        {
            List<CreateClientRequest> requests = table.Rows.ToCreateClientRequests();
            List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = await this.SecurityServiceSteps.GivenTheFollowingClientsExist(requests);
            foreach ((String clientId, String secret, List<String> allowedGrantTypes) client in clients)
            {
                this.TestingContext.AddClientDetails(client.clientId, client.secret, String.Join(",", client.allowedGrantTypes));
            }
        }

        [Given(@"I have a token to access the estate management resource")]
        [Given(@"I have a token to access the estate management and transaction processor resources")]
        public async Task GivenIHaveATokenToAccessTheEstateManagementResource(DataTable table)
        {
            DataTableRow firstRow = table.Rows.First();
            String clientId = ReqnrollTableHelper.GetStringRowValue(firstRow, "ClientId");
            ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

            //this.TestingContext.AccessToken = await this.SecurityServiceSteps.GetClientToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
            var token = await this.TestingContext.DockerHelper.SecurityServiceClient.GetToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
            token.IsSuccess.ShouldBeTrue();
            this.TestingContext.AccessToken = token.Data.AccessToken;

        }
        
        [Given(@"I have created the following estates")]
        [When(@"I create the following estates")]
        public async Task WhenICreateTheFollowingEstates(DataTable table)
        {
            List<CreateEstateRequest> requests = table.Rows.ToCreateEstateRequests();

            var verifiedEstates = await this.TransactionProcessorSteps.WhenICreateTheFollowingEstatesX(this.TestingContext.AccessToken, requests);

            foreach (EstateResponse verifiedEstate in verifiedEstates)
            {
                this.TestingContext.AddEstateDetails(verifiedEstate.EstateId, verifiedEstate.EstateName, verifiedEstate.EstateReference);
                this.TestingContext.Logger.LogInformation($"Estate {verifiedEstate.EstateName} created with Id {verifiedEstate.EstateId}");
            }
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(DataTable table)
        {
            List<(EstateDetails estate, CreateMerchantRequest)> requests = table.Rows.ToCreateMerchantRequests(this.TestingContext.Estates);

            List<MerchantResponse> verifiedMerchants = await this.TransactionProcessorSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (MerchantResponse verifiedMerchant in verifiedMerchants)
            {
                //await this.TransactionProcessorSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, verifiedMerchant.EstateId, verifiedMerchant.MerchantId);

                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(verifiedMerchant.EstateId);
                estateDetails.AddMerchant(verifiedMerchant);
                this.TestingContext.Logger.LogInformation($"Merchant {verifiedMerchant.MerchantName} created with Id {verifiedMerchant.MerchantId} for Estate {estateDetails.EstateName}");
            }
        }
    }

    public static class ReqnrollExtensions
    {
        public static List<Deposit> ToDepositRequests(this DataTableRows tableRows)
        {
            List<Deposit> requests = new();
            foreach (DataTableRow tableRow in tableRows)
            {
                Deposit depositCallback = new()
                {
                    AccountNumber = ReqnrollTableHelper.GetStringRowValue(tableRow, "AccountNumber"),
                    Amount = ReqnrollTableHelper.GetDecimalValue(tableRow, "Amount"),
                    DateTime = ReqnrollTableHelper.GetDateForDateString(ReqnrollTableHelper.GetStringRowValue(tableRow, "DateTime"), DateTime.Now),
                    DepositId = Guid.Parse(ReqnrollTableHelper.GetStringRowValue(tableRow, "DepositId")),
                    HostIdentifier = Guid.Parse(ReqnrollTableHelper.GetStringRowValue(tableRow, "HostIdentifier")),
                    Reference = ReqnrollTableHelper.GetStringRowValue(tableRow, "Reference"),
                    SortCode = ReqnrollTableHelper.GetStringRowValue(tableRow, "SortCode")
                };
                requests.Add(depositCallback);
            }

            return requests;
        }
    }

    internal class ResponseData<T>
    {
        public T Data { get; set; }
    }
}
