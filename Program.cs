using System;
using System.Collections.Generic;

public abstract class Transaction
{
    protected decimal _amount;
    protected bool _success;
    protected bool _executed;
    protected bool _reversed;
    protected DateTime _dateStamp;

    public bool Success => _success;
    public bool Executed => _executed;
    public bool Reversed
    {
        get { return _reversed; }
        set { _reversed = value; }
    }
    public DateTime DateStamp => _dateStamp;

    public Transaction(decimal amount)
    {
        _amount = amount;
    }

    public abstract void Execute();
    public abstract void Rollback();

    public void Print()
    {
        Console.WriteLine($"Transaction Amount: {_amount:C}");
        Console.WriteLine($"Transaction Success: {_success}");
        Console.WriteLine($"Transaction Executed: {_executed}");
        Console.WriteLine($"Transaction Reversed: {_reversed}");
        Console.WriteLine($"Transaction DateStamp: {_dateStamp}");
    }
}


public class Account
{
    public string Name { get; }
    public decimal Balance { get; set; }

    public Account(string name, decimal balance)
    {
        Name = name;
        Balance = balance;
    }
}

public class DepositTransaction : Transaction
{
    private Account _account;

    public DepositTransaction(Account account, decimal amount) : base(amount)
    {
        _account = account ?? throw new ArgumentNullException(nameof(account));
    }

    public override void Execute()
    {
        _executed = true;
        _dateStamp = DateTime.Now;

        _account.Balance += _amount;
        _success = true;
    }

    public override void Rollback()
    {
        if (_executed && !_reversed)
        {
            _account.Balance -= _amount;
            _success = false;
            _reversed = true;
            _dateStamp = DateTime.Now;
        }
    }
}

public class WithdrawTransaction : Transaction
{
    private Account _account;

    public WithdrawTransaction(Account account, decimal amount) : base(amount)
    {
        _account = account ?? throw new ArgumentNullException(nameof(account));
    }

    public override void Execute()
    {
        _executed = true;
        _dateStamp = DateTime.Now;

        if (_account.Balance >= _amount)
        {
            _account.Balance -= _amount;
            _success = true;
        }
        else
        {
            _success = false;
        }
    }

    public override void Rollback()
    {
        if (_executed && !_reversed)
        {
            _account.Balance += _amount;
            _success = false;
            _reversed = true;
            _dateStamp = DateTime.Now;
        }
    }
}

public class TransferTransaction : Transaction
{
    private Account _fromAccount;
    private Account _toAccount;

    public TransferTransaction(Account fromAccount, Account toAccount, decimal amount) : base(amount)
    {
        _fromAccount = fromAccount ?? throw new ArgumentNullException(nameof(fromAccount));
        _toAccount = toAccount ?? throw new ArgumentNullException(nameof(toAccount));
    }

    public override void Execute()
    {
        _executed = true;
        _dateStamp = DateTime.Now;

        if (_fromAccount.Balance >= _amount)
        {
            _fromAccount.Balance -= _amount;
            _toAccount.Balance += _amount;
            _success = true;
        }
        else
        {
            _success = false;
        }
    }

    public override void Rollback()
    {
        if (_executed && !_reversed)
        {
            _fromAccount.Balance += _amount;
            _toAccount.Balance -= _amount;
            _success = false;
            _reversed = true;
            _dateStamp = DateTime.Now;
        }
    }
}

class Bank
{
    private List<Account> _accounts;
    private List<Transaction> _transactions;

    public Bank()
    {
        _accounts = new List<Account>();
        _transactions = new List<Transaction>();
    }

    public void AddAccount(Account account)
    {
        if (account != null)
        {
            _accounts.Add(account);
        }
    }

    public Account? GetAccount(string name)
    {
        return _accounts.Find(account => account.Name == name);
    }

    public void ExecuteTransaction(Transaction transaction)
    {
        transaction.Execute();
        _transactions.Add(transaction);
    }

    public void RollbackTransaction(Transaction transaction)
    {
        if (transaction.Executed && !transaction.Reversed)
        {
            transaction.Rollback();
            transaction.Reversed = true;
        }
        else
        {
            Console.WriteLine("Unable to rollback the transaction. Either it was not executed or it has already been reversed.");
        }
    }

    public void PrintAllTransactions()
    {
        Console.WriteLine("List of Transactions:");
        foreach (Transaction transaction in _transactions)
        {
            transaction.Print();
        }
    }

    public int GetTransactionsCount()
    {
        return _transactions.Count;
    }

    public Transaction GetTransaction(int index)
    {
        if (index >= 0 && index < _transactions.Count)
        {
            return _transactions[index];
        }
        return null;
    }

    public void PrintTranscationHistory()
    {
        Console.WriteLine("Transaction History:");
        for (int i = 0; i < _transactions.Count; i++)
        {
            Transaction transaction = _transactions[i];
            Console.WriteLine($"Transaction {i}:");
            transaction.Print();
            Console.WriteLine();
        }
    }



    
}

class BankSystem
{
    private static Account FindAccount(Bank bank)
    {
        Console.Write("Enter account name: ");
        string accountName = Console.ReadLine();

        Account foundAccount = bank.GetAccount(accountName);

        if (foundAccount == null)
        {
            Console.WriteLine("Account not found.");
        }

        return foundAccount;
    }

    private static void DoDeposit(Bank bank)
    {
        Account account = FindAccount(bank);

        if (account != null)
        {
            Console.Write("Enter the deposit amount: ");
            decimal amount = Convert.ToDecimal(Console.ReadLine());

            DepositTransaction depositTransaction = new DepositTransaction(account, amount);
            bank.ExecuteTransaction(depositTransaction);
            Console.WriteLine("Deposit successful.");
        }
    }

    private static void DoWithdraw(Bank bank)
    {
        Account account = FindAccount(bank);

        if (account != null)
        {
            Console.Write("Enter the withdrawal amount: ");
            decimal amount = Convert.ToDecimal(Console.ReadLine());

            WithdrawTransaction withdrawTransaction = new WithdrawTransaction(account, amount);
            bank.ExecuteTransaction(withdrawTransaction);
            Console.WriteLine("Withdrawal successful.");
        }
    }

    private static void DoTransfer(Bank bank)
    {
        Account fromAccount = FindAccount(bank);

        if (fromAccount != null)
        {
            Account toAccount = FindAccount(bank);

            if (toAccount != null)
            {
                Console.Write("Enter the transfer amount: ");
                decimal amount = Convert.ToDecimal(Console.ReadLine());

                TransferTransaction transferTransaction = new TransferTransaction(fromAccount, toAccount, amount);
                bank.ExecuteTransaction(transferTransaction);
                Console.WriteLine("Transfer successful.");
            }
        }
    }

    private static void DoPrint(Bank bank)
    {
        Account account = FindAccount(bank);

        if (account != null)
        {
            Console.WriteLine($"Account Name: {account.Name}");
            Console.WriteLine($"Balance: {account.Balance:C}");
        }
    }

    private static void DoRollback(Bank bank)
    {
        bank.PrintAllTransactions();
        Console.Write("Enter the index of the transaction to rollback: ");
        int transactionIndex = Convert.ToInt32(Console.ReadLine());

        if (transactionIndex >= 0 && transactionIndex < bank.GetTransactionsCount())
        {
            Transaction transaction = bank.GetTransaction(transactionIndex);
            bank.RollbackTransaction(transaction);
            Console.WriteLine("Transaction rolled back successfully.");
        }
        else
        {
            Console.WriteLine("Invalid transaction index. Please try again.");
        }
    }


    static void Main(string[] args)
    {
        Bank bank = new Bank();
        int choice;

        do
        {
            Console.WriteLine("1. Add new account");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4. Transfer");
            Console.WriteLine("5. Print account details");
            Console.WriteLine("6. Print transaction history"); // New option
            Console.WriteLine("7. Rollback transaction");
            Console.WriteLine("0. Exit");
            Console.Write("Enter your choice: ");
            choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.Write("Enter account name: ");
                    string accountName = Console.ReadLine();
                    Console.Write("Enter starting balance: ");
                    decimal startingBalance = Convert.ToDecimal(Console.ReadLine());

                    Account newAccount = new Account(accountName, startingBalance);
                    bank.AddAccount(newAccount);
                    Console.WriteLine("Account added successfully.");
                    break;

                case 2:
                    DoDeposit(bank);
                    break;

                case 3:
                    DoWithdraw(bank);
                    break;

                case 4:
                    DoTransfer(bank);
                    break;

                case 5:
                    DoPrint(bank);
                    break;

                 case 6:
                    bank.PrintTranscationHistory(); // Print transaction history
                    break;
                    
                case 7:
                    DoRollback(bank); // Rollback a transaction
                    break;

                case 0:
                    Console.WriteLine("Exiting the program.");
                    break;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }

        } while (choice != 0);
    }
}
