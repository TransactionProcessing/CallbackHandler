@base @shared
Feature: BankDepositCallback

A short summary of the feature

Scenario: Process a Bank Deposit Callback
	Given I have the following Bank Deposit Callbacks
	| Amount | DateTime | DepositId                            | HostIdentifier                       | Reference | SortCode | AccountNumber |
	| 100.00 | Today    | 6AE04AFC-D7F8-4936-A3A2-DCA177CAA106 | DC4A7DDA-45A1-4D5B-8D46-21FA99A3868E | Deposit1  | 11-22-33 | 12345678      |
	When I send the requests to the callback handler for deposits
	Then the deposit records are recorded
