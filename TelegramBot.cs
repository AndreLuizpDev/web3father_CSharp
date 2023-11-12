using CryptoPay.Responses;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using web3father_CSharp.Models;

public class TelegramBot
{
    private readonly TelegramBotClient botClient;
    private readonly CancellationTokenSource cancellationToken;
    private readonly Database database;
    private readonly CryptoPayApi cryptoPayApi;
    private readonly string cryptoPayToken;

    public TelegramBot(string token, Database db, string cryptoPayToken)
    {
        botClient = new TelegramBotClient(token);
        database = db;
        cancellationToken = new CancellationTokenSource();

        this.cryptoPayToken = cryptoPayToken;
        cryptoPayApi = new CryptoPayApi(cryptoPayToken);

        botClient.StartReceiving(
            updateHandler: updateHandlerAsync,
            pollingErrorHandler: pollingErrorHandlerAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            },
            cancellationToken: cancellationToken.Token
        );
    }

    public async Task StartBotAsync()
    {
        var me = await botClient.GetMeAsync(cancellationToken.Token);
        Console.WriteLine($"Escutando {me.Username}");
        Console.WriteLine("Bot is running. Press Ctrl+C to stop.");
        Console.ReadLine();
        cancellationToken.Cancel();
    }
    public List<string> GetAvailableCommands()
    {
        List<string> commands = new List<string>
    {
        "```/start``` - Start the bot",
        "```/addproduct``` <Name> <Price> - Add a new product",
        "```/addseller``` <Name> - Add a new seller",
        "```/listproducts``` - List all products",
        "```/listsellers``` - List all sellers",
        "```/getbalance``` - Get your balance"
        // Add more commands here as needed
    };

        return commands;
    }


    private Task pollingErrorHandlerAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ctoken)
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }

    private async Task updateHandlerAsync(ITelegramBotClient botClient, Update update, CancellationToken ctoken)
    {
        if (update.Message is not { } message)
            return;

        var chatId = message.Chat.Id;
        var messageText = message.Text;

        Console.WriteLine($"mensagem recebida: '{messageText}' do tipo '{message.Type}' no chat '{chatId}'");

        if (message.Text != null)
        {
            string dueDateString = database.GetDueDateByUserId(message.From.Id);

            DateTime dueDate = DateTime.MinValue;

            if (dueDateString != null)
            {
                dueDate = DateTime.ParseExact(dueDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            }

            // Handle user commands and messages
            if (message.Text.StartsWith("/start"))
            {
                List<string> availableCommands = GetAvailableCommands();
                string response = "Available commands:\n" + string.Join("\n", availableCommands);
                await botClient.SendTextMessageAsync(message.Chat.Id, response, parseMode: ParseMode.Markdown);
            }
            else if (dueDate < DateTime.Now && !message.Text.StartsWith("/pay"))
            {
                string response = $"User not registered or payment expired. dueDate: {dueDate}";
                await botClient.SendTextMessageAsync(message.Chat.Id, response);

            }
            else if (message.Text.StartsWith("/pay"))
            {
                // Criar uma instância da classe Payment e preencher os campos
                Payment newPayment = new Payment
                {
                    UserId = message.From.Id, // Substitua pelo ID do usuário associado ao pagamento
                    PaymentId = message.From.Id.ToString(), // Substitua pelo identificador único do pagamento
                    PaymentDate = DateTime.Now.ToString("yyyy-MM-dd"), // Substitua pela data de pagamento formatada como string
                    DueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"), // Substitua pela data de vencimento formatada como string
                };

                // Chamar o método InsertPayment para inserir o pagamento no banco de dados
                if (database.InsertPayment(newPayment.UserId, newPayment.PaymentId, newPayment.PaymentDate, newPayment.DueDate))
                {
                    string response = $"User '{newPayment.UserId}' has been inserted successfully! DueDate {newPayment.DueDate}";
                    await botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    string response = $"Failed to insert.";
                    await botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
            }
            else if (message.Text.StartsWith("/addproduct"))
            {
                // Handle the command to add a product
                var parameters = message.Text.Split(' ');
                if (parameters.Length == 3)
                {
                    string productName = parameters[1];
                    if (double.TryParse(parameters[2], out double productPrice))
                    {
                        // Adicione o produto ao banco de dados
                        // Use o objeto 'database' para inserir os dados no SQLite
                        if (database.AddProduct(productName, productPrice))
                        {
                            string response = $"Product '{productName}' has been inserted successfully!";
                            await botClient.SendTextMessageAsync(message.Chat.Id, response);
                        }
                        else
                        {
                            string response = $"Failed to insert product '{productName}'";
                            await botClient.SendTextMessageAsync(message.Chat.Id, response);
                        }
                    }
                }
            }
            else if (message.Text.StartsWith("/addseller"))
            {
                var sellerName = message.Text.Substring("/addseller".Length).Trim();
                if (!string.IsNullOrWhiteSpace(sellerName))
                {
                    database.AddSeller(sellerName);

                    // Adicione o vendedor ao banco de dados
                    // Use o objeto 'database' para inserir os dados na tabela de vendedores
                    // Certifique-se de associar vendedores aos produtos, se necessário
                }
            }
            else if (message.Text.StartsWith("/listproducts"))
            {
                // Recupere a lista de produtos do banco de dados
                List<Product> products = database.GetAllProducts();

                if (products.Count > 0)
                {
                    string response = "List of Products:\n";
                    foreach (var product in products)
                    {
                        response += $"ID: {product.Id}, Name: {product.Name}, Price: {product.Price}\n";
                    }

                    // Envie a lista de produtos como resposta
                    await botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "No products found.");
                }
            }
            else if (message.Text.StartsWith("/listsellers"))
            {
                // Recupere a lista de vendedores do banco de dados
                List<Seller> sellers = database.GetAllSellers();

                if (sellers.Count > 0)
                {
                    string response = "List of Sellers:\n";
                    foreach (var seller in sellers)
                    {
                        response += $"ID: {seller.Id}, Name: {seller.Name}\n";
                    }

                    // Envie a lista de vendedores como resposta
                    await botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "No sellers found.");
                }
            }
            else if (message.Text.StartsWith("/getbalance"))
            {
                // Chame o método GetBalanceAsync da classe CryptoPayApi
                double balance = await cryptoPayApi.GetBalanceAsync(cryptoPayToken);

                // Envie a resposta com o saldo para o usuário no Telegram
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Your balance is: {balance}");
            }
            else if (message.Text.StartsWith("/pay"))
            {
                // Criar uma instância da classe Payment e preencher os campos
                Payment newPayment = new Payment
                {
                    UserId = message.From.Id, // Substitua pelo ID do usuário associado ao pagamento
                    PaymentId = message.From.Id.ToString(), // Substitua pelo identificador único do pagamento
                    PaymentDate = DateTime.Now.ToString("yyyy-MM-dd"), // Substitua pela data de pagamento formatada como string
                    DueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"), // Substitua pela data de vencimento formatada como string
                };

                // Chamar o método InsertPayment para inserir o pagamento no banco de dados
                if (database.InsertPayment(newPayment.UserId, newPayment.PaymentId, newPayment.PaymentDate, newPayment.DueDate))
                {
                    string response = $"User '{newPayment.UserId}' has been inserted successfully! DueDate {newPayment.DueDate}";
                    await botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    string response = $"Failed to insert.";
                    await botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
            }
            else
            {
                List<string> availableCommands = GetAvailableCommands();
                string response = "Available commands:\n" + string.Join("\n", availableCommands);
                await botClient.SendTextMessageAsync(message.Chat.Id, response, parseMode: ParseMode.Markdown);
            }


            // Add more commands and logic as needed
        }
    }
}
