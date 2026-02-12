public class Program
{
    public decimal Balance { get; private set; }

    public Program(decimal initialBalance)
    {
        Balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        if (amount < 0)
            throw new Exception("Deposit amount cannot be negative");

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount > Balance)
            throw new Exception("Insufficient funds.");

        Balance -= amount;
    }
}
