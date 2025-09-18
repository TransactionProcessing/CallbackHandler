using CallbackHandler.IntegrationTests.Common;
using Reqnroll;
using Shared.IntegrationTesting;
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
using Shouldly;

namespace CallbackHandler.IntegrationTests.Shared
{
    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public SharedSteps(ScenarioContext scenarioContext,
            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [Given(@"I have the following Bank Deposit Callbacks")]
        public async Task GivenIHaveTheFollowingBankDepositCallbacks(DataTable table)
        {
            List<Deposit> requests = table.Rows.ToDepositRequests();
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
                ResponseData<Guid> responseData =
                    JsonConvert.DeserializeObject<ResponseData<Guid>>(content);

                this.TestingContext.SentCallbacks.Add(responseData.Data, payload);
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
                ResponseData<CallbackMessage> responseData =
                    JsonConvert.DeserializeObject<ResponseData<CallbackMessage>>(content);


                responseData.Data.Reference.ShouldBe(originalDeposit.Reference);
                var callbackDeposit = JsonConvert.DeserializeObject<Deposit>(responseData.Data.Message);
                callbackDeposit.AccountNumber.ShouldBe(originalDeposit.AccountNumber);
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
