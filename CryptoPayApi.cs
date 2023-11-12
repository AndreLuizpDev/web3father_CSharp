using CryptoPay.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Telegram.Bot.Types;

public class CryptoPayApi
{
    private readonly string apiKey; // Sua chave de API CryptoPay
    private readonly HttpClient httpClient;

    public CryptoPayApi(string cryptoPayToken)
    {
        this.apiKey = cryptoPayToken;
        this.httpClient = new HttpClient();
        this.httpClient.BaseAddress = new Uri("https://testnet-pay.crypt.bot/"); // URL base da API CryptoPay
    }
    public async Task<double> GetBalanceAsync(string cryptoPayToken)
    {
        // Chame a API CryptoPay com o token
        var request = new HttpRequestMessage(HttpMethod.Get, "account/balance");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cryptoPayToken); // Use o token do CryptoPay aqui

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Analise a resposta para obter o saldo (suponhamos que a resposta seja um JSON)
            var json = JObject.Parse(content);
            double balance = json["balance"].Value<double>();

            return balance;
        }
        else
        {
            // Tratamento de erro se a chamada não for bem-sucedida (por exemplo, lançar uma exceção)
            throw new Exception("Exception: " + response.ReasonPhrase);
        }
    }


    public async Task<bool> VerifyPaymentAsync(string transactionId, double expectedAmount)
    {
        // Faça uma chamada à API CryptoPay para verificar o pagamento com base no ID da transação
        // Certifique-se de implementar a lógica real para verificar a transação e seu valor
        // Consulte a documentação do CryptoPay para detalhes específicos sobre como verificar transações
        // Você pode precisar de cabeçalhos de autenticação e parâmetros específicos

        // Exemplo de chamada à API:
        // var response = await httpClient.GetAsync($"transactions/{transactionId}");
        // Se a transação for válida e o valor corresponder, retorne true, senão retorne false.

        // Substitua o código acima com a lógica real de verificação do CryptoPay
        return true; // Retorne true se a transação for válida, senão retorne false.
    }
}
