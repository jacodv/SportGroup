using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GolfGroup.Api
{
  public class Sample
  {
    public Sample()
    {
      // New Statement
      var statement = new Statement();

      // Read Header
      statement.Header = new Header()
      {
        BankName = "",
        GenerationDate = DateTime.ParseExact("the date value", "the input format", null),
        // Rest of the fields
      };

      // Read Recipient
      statement.CsiRecipient = new CsiRecipient()
      {
        ToBeSpecified1 = "",
        Email = ""
        // Rest of the fields
      };

      // TODO: E

      // Addresses
      statement.AddressLines.Add("Address line");
      // OR
      statement.AddressLines.AddRange(new []{"",""}); // Add an array of address lines

      // START WITH ACCOUNTS
      // DO THIS FOR EACH ACCOUNT
      var account = new Account()
      {
        Group = Enum.Parse<AccountGroup>("the account type", true),
        Name = ""
        // Rest of the fields
      };

      // START WITH TRANSACTIONS FOR THIS ACCOUNT 
      // DO THIS FOR EACH TRANSACTION
      account.Transactions.Add(new Transaction()
      {
        Date = DateTime.ParseExact("the date", "the input format", null),
        Balance = Double.Parse("the balance")
        // Rest of the fields
      });

      // ADD CREATED ACCOUNT TO THE STATEMENT
      statement.Accounts.Add(account);


      //=====================
      // TO GET the JSON for the current statement
      var statmentJson = Newtonsoft.Json.JsonConvert.SerializeObject(statement, Formatting.None);
      // PRETTY VERSION FOR TESTING
      //var statmentJson = Newtonsoft.Json.JsonConvert.SerializeObject(statement, Formatting.Indented);



    }
  }



  public class Statement
  {
    public Statement()
    {
      // Make sure the list is init
      AddressLines = new List<string>();
      Accounts = new List<Account>();
    }

    [JsonProperty("head")]
    public Header Header { get; set; }
    [JsonProperty("rec")]
    public CsiRecipient CsiRecipient { get; set; }
    [JsonProperty("date")]
    public DateTime Date { get; set; }
    public string  E_ToBeSpecified { get; set; }
    /// <summary>
    /// POSSIBLE DUPLICATE
    /// </summary>
    [JsonProperty("accno")]
    public string  AccountNumber { get; set; }
    [JsonProperty("address")]
    public List<string> AddressLines { get; set; }
    [JsonProperty("accs")]
    public List<Account> Accounts { get; set; }
  }

  public class Account
  {
    public Account()
    {
      // Make sure the list is init
      Transactions = new List<Transaction>();
    }
    #region G
    [JsonProperty("grp")]
    public AccountGroup Group { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    #endregion

    #region J
    /// <summary>
    /// POSSIBLE DUPLICATE
    /// </summary>
    [JsonProperty("accno")]
    public string  AccountNumber { get; set; }
    [JsonProperty("start")]
    public DateTime StartDate { get; set; }
    [JsonProperty("end")]
    public DateTime EndDate { get; set; }
    [JsonProperty("obal")]
    public double  OpeningBalance { get; set; }
    [JsonProperty("cycle")]
    public int  DaysInCycle { get; set; }
    [JsonProperty("depunit")]
    public int  DepositCreditsUnits { get; set; }
    [JsonProperty("depval")]
    public double DepositCreditsValue { get; set; }
    [JsonProperty("depbal")]
    public double DepositsLedgerBalance { get; set; }
    [JsonProperty("chkunit")]
    public int ChecksDebitsUnits { get; set; }
    [JsonProperty("chkval")]
    public double ChecksDebitsValue { get; set; }
    [JsonProperty("chkbal")]
    public double ChecksLedgerBalance { get; set; }
    [JsonProperty("svc")]
    public double ServiceCharge { get; set; }
    [JsonProperty("int")]
    public double InterestPaid { get; set; }
    [JsonProperty("stmtbal")]
    public double StatementBalance { get; set; }    
    #endregion


    [JsonProperty("tran")]
    public List<Transaction> Transactions { get; set; }
  }

  public class Transaction
  {
    [JsonProperty("dte")]
    public DateTime Date { get; set; }
    [JsonProperty("desc")]
    public string Description { get; set; }
    [JsonProperty("val")]
    public double Value { get; set; }
    [JsonProperty("bal")]
    public double Balance { get; set; }
    [JsonProperty("note")]
    public string Note { get; set; }
  }

  public class CsiRecipient
  {
    public string ToBeSpecified1 { get; set; }
    public string ToBeSpecified2 { get; set; }
    public string ToBeSpecified3 { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }
  }

  public class Header
  {
    [JsonProperty("type")]
    public HeaderType HeaderType { get; set; }
    [JsonProperty("gendate")]
    public DateTime GenerationDate { get; set; }
    [JsonProperty("value_1")]
    public string Value_1 { get; set; }
    [JsonProperty("bank")]
    public string BankName { get; set; }
    [JsonProperty("seq")]
    public string Sequence { get; set; }
  }

  public enum HeaderType
  {
    Statement
  }
  public enum AccountGroup
  {
    CHECKING_ACCOUNTS,
    SAVING_ACCOUNTS
  }
}
