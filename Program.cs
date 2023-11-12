using Newtonsoft.Json.Linq;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string baseDirectory = Directory.GetCurrentDirectory();
        for (int i = 0; i < 3; i++)
        {
            baseDirectory = Directory.GetParent(baseDirectory).FullName;
        }
        string jsonPath = Path.Combine(baseDirectory, "config_master.json");
        string json = System.IO.File.ReadAllText(jsonPath);

        JObject jsonObj = JObject.Parse(json);

        string botToken = jsonObj["bot"]["bottoken"].ToString();
        string cryptoPayApiKey = jsonObj["bot"]["CryptoPaytokenTest"].ToString();
        string dbPath = $"{baseDirectory}/data/base.db"; // Nome do arquivo do banco de dados;

        var database = new Database(dbPath);
        database.InitializeDatabase();

        var bot = new TelegramBot(botToken, database, cryptoPayApiKey);
        bot.StartBotAsync().Wait();
    }
}
