using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public override string ToString()
    {
        return $"{Name} - {Price} SEK";
    }
}

public class Customer
{
    public string Name { get; private set; }
    private string Password { get; set; }
    private List<Product> _cart;
    public List<Product> Cart { get { return _cart; } }

    public Customer(string name, string password)
    {
        Name = name;
        Password = password;
        _cart = new List<Product>();
    }

    public virtual decimal GetTotalCartPrice()
    {
        return _cart.Sum(p => p.Price);
    }

    public bool VerifyPassword(string password)
    {
        return Password == password;
    }

    public void AddToCart(Product product)
    {
        _cart.Add(product);
    }

    public string GetPassword()
    {
        return Password;
    }

    public override string ToString()
    {
        string cartContent = _cart.Count > 0
            ? string.Join(", ", _cart.Select(p => $"{p.Name} ({p.Price} SEK)"))
            : "Kundvagnen är tom.";

        return $"Namn: {Name}, Lösenord: {new string('*', Password.Length)}, Kundvagn: [{cartContent}], Total: {GetTotalCartPrice()} SEK";
    }
}

public class GoldCustomer : Customer
{
    public GoldCustomer(string name, string password) : base(name, password) { }

    public override decimal GetTotalCartPrice()
    {
        return base.GetTotalCartPrice() * 0.85m; // 15% rabatt
    }
}

public class SilverCustomer : Customer
{
    public SilverCustomer(string name, string password) : base(name, password) { }

    public override decimal GetTotalCartPrice()
    {
        return base.GetTotalCartPrice() * 0.90m; // 10% rabatt
    }
}

public class BronzeCustomer : Customer
{
    public BronzeCustomer(string name, string password) : base(name, password) { }

    public override decimal GetTotalCartPrice()
    {
        return base.GetTotalCartPrice() * 0.95m; // 5% rabatt
    }
}

class Program
{
    static List<Customer> customers = new List<Customer>();

    static void Main(string[] args)
    {
        LoadCustomersFromFile(); // Ladda kunder vid start

        // Fördefinierade kunder
        customers.Add(new GoldCustomer("Knatte", "123"));
        customers.Add(new SilverCustomer("Fnatte", "321"));
        customers.Add(new BronzeCustomer("Tjatte", "213"));

        while (true)
        {
            Console.WriteLine("Välkommen till min smink butik!");
            Console.WriteLine("1. Registrera ny kund");
            Console.WriteLine("2. Logga in");
            Console.WriteLine("3. Avsluta");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    RegisterCustomer();
                    break;
                case "2":
                    Customer loggedInCustomer = Login();
                    if (loggedInCustomer != null)
                    {
                        CustomerMenu(loggedInCustomer);
                    }
                    break;
                case "3":
                    SaveCustomersToFile(); // Spara kunder vid avslutning
                    return;
            }
        }
    }

    static void RegisterCustomer()
    {
        Console.Write("Ange namn: ");
        string name = Console.ReadLine();
        Console.Write("Ange lösenord: ");
        string password = Console.ReadLine();

        // Välj kundnivå
        Console.WriteLine("Välj kundnivå: 1. Gold, 2. Silver, 3. Bronze");
        string levelChoice = Console.ReadLine();
        Customer newCustomer;

        switch (levelChoice)
        {
            case "1":
                newCustomer = new GoldCustomer(name, password);
                break;
            case "2":
                newCustomer = new SilverCustomer(name, password);
                break;
            case "3":
                newCustomer = new BronzeCustomer(name, password);
                break;
            default:
                Console.WriteLine("Felaktigt val, sätter kundnivå till Bronze.");
                newCustomer = new BronzeCustomer(name, password);
                break;
        }

        customers.Add(newCustomer);
        Console.WriteLine($"Kund {name} registrerad som {newCustomer.GetType().Name}!");
    }

    static Customer Login()
    {
        Console.Write("Ange namn: ");
        string name = Console.ReadLine();
        Console.Write("Ange lösenord: ");
        string password = Console.ReadLine();

        Customer customer = customers.FirstOrDefault(c => c.Name == name);
        if (customer == null)
        {
            Console.WriteLine("Kunden finns inte. Vill du registrera en ny kund? (j/n)");
            if (Console.ReadLine().ToLower() == "j")
            {
                RegisterCustomer();
            }
            return null;
        }

        if (!customer.VerifyPassword(password))
        {
            Console.WriteLine("Fel lösenord, försök igen.");
            return null;
        }

        Console.WriteLine($"Välkommen {customer.Name}!");
        return customer;
    }

    static void CustomerMenu(Customer customer)
    {
        while (true)
        {
            Console.WriteLine("1. Handla");
            Console.WriteLine("2. Se kundvagn");
            Console.WriteLine("3. Gå till kassan");
            Console.WriteLine("4. Logga ut");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Shop(customer);
                    break;
                case "2":
                    ViewCart(customer);
                    break;
                case "3":
                    Console.WriteLine("Välj valuta: 1. SEK, 2. USD, 3. EUR");
                    string currencyChoice = Console.ReadLine();
                    string currency = currencyChoice == "1" ? "SEK" : currencyChoice == "2" ? "USD" : "EUR";
                    DisplayCartInCurrency(customer, currency);
                    return;
                case "4":
                    return;
            }
        }
    }

    static void Shop(Customer customer)
    {
        List<Product> products = new List<Product>
        {
            new Product("Mascara", 200m),
            new Product("Läppstift", 150m),
            new Product("Fondation", 100m)
        };

        Console.WriteLine("Tillgängliga produkter:");
        for (int i = 0; i < products.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {products[i].Name} - {products[i].Price} SEK");
        }

        Console.Write("Välj en produkt att lägga till i kundvagnen: ");
        int productChoice = int.Parse(Console.ReadLine()) - 1;

        if (productChoice >= 0 && productChoice < products.Count)
        {
            customer.AddToCart(products[productChoice]);
            Console.WriteLine($"{products[productChoice].Name} tillagd i kundvagnen.");
        }
        else
        {
            Console.WriteLine("Felaktigt val.");
        }
    }

    static void ViewCart(Customer customer)
    {
        Console.WriteLine(customer.ToString());
    }

    static void DisplayCartInCurrency(Customer customer, string currency)
    {
        decimal totalPrice = ConvertCurrency(customer.GetTotalCartPrice(), currency);
        Console.WriteLine($"Totalpris i {currency}: {totalPrice} {currency}");
    }

    static decimal ConvertCurrency(decimal amount, string currency)
    {
        switch (currency)
        {
            case "USD":
                return amount * 0.11m;  
            case "EUR":
                return amount * 0.10m; 
            default:
                return amount; // SEK
        }
    }

    static void Checkout(Customer customer)
    {
        Console.WriteLine($"Totalt pris: {customer.GetTotalCartPrice()} SEK");
        customer.Cart.Clear();
        Console.WriteLine("Tack för ditt köp!");
    }

    static void SaveCustomersToFile()
    {
        using (StreamWriter writer = new StreamWriter("customers.txt"))
        {
            foreach (var customer in customers)
            {
                // Spara kundtyp (Gold, Silver, Bronze), namn och lösenord
                string customerType = customer.GetType().Name;
                writer.WriteLine($"{customerType},{customer.Name},{customer.GetPassword()}");
            }
        }
    }

    static void LoadCustomersFromFile()
    {
        if (!File.Exists("customers.txt"))
            return;

        using (StreamReader reader = new StreamReader("customers.txt"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',');
                string type = parts[0];
                string name = parts[1];
                string password = parts[2];

                switch (type)
                {
                    case "GoldCustomer":
                        customers.Add(new GoldCustomer(name, password));
                        break;
                    case "SilverCustomer":
                        customers.Add(new SilverCustomer(name, password));
                        break;
                    case "BronzeCustomer":
                        customers.Add(new BronzeCustomer(name, password));
                        break;
                }
            }
        }
    }
}
