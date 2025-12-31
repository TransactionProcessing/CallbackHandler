using CallbackHandler.DataTransferObjects;
using Reqnroll;
using SecurityService.DataTransferObjects.Responses;
using Shared.Logger;
using Shouldly;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.IntegrationTesting.Helpers;

namespace CallbackHandler.IntegrationTests.Common;

public class TestingContext
{
    
    public DockerHelper DockerHelper { get; set; }
    public NlogLogger Logger { get; set; }

    public List<Deposit> Deposits { get; set; }

    public Dictionary<Guid, String> SentCallbacks { get; set; }

    public TestingContext()
    {
        this.SentCallbacks = new Dictionary<Guid, string>();
        this.Estates = new List<EstateDetails>();
        this.Clients = new List<ClientDetails>();
    }

    public String AccessToken { get; set; }
    internal readonly List<ClientDetails> Clients;
    internal readonly List<EstateDetails> Estates;
    public void AddClientDetails(String clientId,
                                 String clientSecret,
                                 String grantType)
    {
        this.Clients.Add(ClientDetails.Create(clientId, clientSecret, grantType));
    }

    public ClientDetails GetClientDetails(String clientId)
    {
        ClientDetails clientDetails = this.Clients.SingleOrDefault(c => c.ClientId == clientId);

        clientDetails.ShouldNotBeNull();

        return clientDetails;
    }
    public List<Guid> GetAllEstateIds()
    {
        return this.Estates.Select(e => e.EstateId).ToList();
    }

    public EstateDetails GetEstateDetails(DataTableRow tableRow)
    {
        String estateName = ReqnrollTableHelper.GetStringRowValue(tableRow, "EstateName");
        EstateDetails estateDetails = null;

        estateDetails = this.Estates.SingleOrDefault(e => e.EstateName == estateName);

        if (estateDetails == null && estateName == "InvalidEstate")
        {
            estateDetails = EstateDetails.Create(Guid.Parse("79902550-64DF-4491-B0C1-4E78943928A3"), estateName, "estateRef1");
            MerchantResponse merchantResponse = new MerchantResponse
            {
                MerchantId = Guid.Parse("36AA0109-E2E3-4049-9575-F507A887BB1F"),
                MerchantName = "Test Merchant 1"
            };
            estateDetails.AddMerchant(merchantResponse);
            this.Estates.Add(estateDetails);
        }

        estateDetails.ShouldNotBeNull();

        return estateDetails;
    }

    /// <summary>
    /// Gets the estate details.
    /// </summary>
    /// <param name="estateName">Name of the estate.</param>
    /// <returns></returns>
    public EstateDetails GetEstateDetails(String estateName)
    {
        EstateDetails estateDetails = this.Estates.SingleOrDefault(e => e.EstateName == estateName);

        estateDetails.ShouldNotBeNull();

        return estateDetails;
    }

    /// <summary>
    /// Gets the estate details.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <returns></returns>
    public EstateDetails GetEstateDetails(Guid estateId)
    {
        EstateDetails estateDetails = this.Estates.SingleOrDefault(e => e.EstateId == estateId);

        estateDetails.ShouldNotBeNull();

        return estateDetails;
    }

    public void AddEstateDetails(Guid estateId,
                                 String estateName,
                                 String estateReference)
    {
        this.Estates.Add(EstateDetails.Create(estateId, estateName, estateReference));
    }
}

public class ClientDetails
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDetails"/> class.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="grantType">Type of the grant.</param>
    private ClientDetails(String clientId,
                          String clientSecret,
                          String grantType)
    {
        this.ClientId = clientId;
        this.ClientSecret = clientSecret;
        this.GrantType = grantType;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public String ClientId { get; }

    /// <summary>
    /// Gets the client secret.
    /// </summary>
    /// <value>
    /// The client secret.
    /// </value>
    public String ClientSecret { get; }

    /// <summary>
    /// Gets the type of the grant.
    /// </summary>
    /// <value>
    /// The type of the grant.
    /// </value>
    public String GrantType { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the specified client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="grantType">Type of the grant.</param>
    /// <returns></returns>
    public static ClientDetails Create(String clientId,
                                       String clientSecret,
                                       String grantType)
    {
        return new ClientDetails(clientId, clientSecret, grantType);
    }

    #endregion
}