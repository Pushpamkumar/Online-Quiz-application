using System;
using System.Collections.Generic;

class BankAccount{
    private string _accountNumber;
    private decimal _balance;
    public string AccountNumber{
        get { return _accountNumber;}
    }
    public decimal Balance{
        get { return _balance; }
    }
    public BankAccount(string accountNumber, decimal openingBalance){
        _accountNumber=accountNumber;
        _balance= openingBalance;
    }
}