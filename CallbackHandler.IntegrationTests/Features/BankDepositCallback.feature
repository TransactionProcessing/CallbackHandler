@base @shared
Feature: BankDepositCallback

Background: 
Given I create the following api scopes
	| Name                 | DisplayName                       | Description                            |
	| estateManagement     | Estate Managememt REST Scope      | A scope for Estate Managememt REST     |
	| transactionProcessor | Transaction Processor REST  Scope | A scope for Transaction Processor REST |
	| voucherManagement    | Voucher Management REST  Scope    | A scope for Voucher Management REST    |
	| messagingService     | Scope for Messaging REST          | Scope for Messaging REST               |

	Given the following api resources exist
	| Name         | DisplayName                | Secret  | Scopes               | UserClaims                 |
	| estateManagement     | Estate Managememt REST     | Secret1 | estateManagement     | MerchantId, EstateId, role |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor |                            |
	| voucherManagement    | Voucher Management REST    | Secret1 | voucherManagement    |                            |
	| messagingService     | Messaging REST             | Secret  | messagingService     |                            |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,transactionProcessor,voucherManagement,messagingService | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
		
	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |

Scenario: Process a Bank Deposit Callback
	Given I have the following Bank Deposit Callbacks
	| Amount | DateTime | DepositId                            | HostIdentifier                       |  SortCode | AccountNumber |
	| 100.00 | Today    | 6AE04AFC-D7F8-4936-A3A2-DCA177CAA106 | DC4A7DDA-45A1-4D5B-8D46-21FA99A3868E |  11-22-33 | 12345678      |
	When I send the requests to the callback handler for deposits
	Then the deposit records are recorded
