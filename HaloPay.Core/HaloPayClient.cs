using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.HaloPay;

public class HaloPayClient(
    HttpClient httpClient,
    HaloPayHasher hasher,
    ILogger<HaloPayClient> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly HaloPayHasher _hasher = hasher;
    private readonly ILogger<HaloPayClient> _logger = logger;

    public async ValueTask<HaloPayClientDepositResponse> DepositAsync(
        string merchantId,
        string apiKey,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = merchantId,
            ["merchantOrderNo"] = order.FormId.ToString("N"),
            ["amount"] = order.Amount.ToString(),
            ["accountName"] = order.AccountName,
            ["desc"] = "",
            ["payType"] = order.PayType,
            ["mobile"] = order.Mobile,
            ["notificationUrl"] = order.CallbackUrl.AbsoluteUri,
        };

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await _httpClient
            .PostAsJsonAsync("/halo/indonesia/payIn", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("HaloPayClient Deposit response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(node["message"]!.ToString());

        return new HaloPayClientDepositResponse(
            Amount: decimal.Parse(node["amount"]!.GetValue<string>()),
            FormId: order.FormId,
            ThirdPartyOrderNo: node["payOrderNo"]!.GetValue<string>(),
            RedirectUrl: new Uri(node["payUrl"]!.GetValue<string>()));
    }

    public async ValueTask<string> QueryDepositFormAsync(
        string merchantId,
        string apiKey,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = merchantId,
            ["merchantOrderNo"] = formId.ToString("N"),
        };

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await _httpClient
            .PostAsJsonAsync($"/halo/pay/getPayinOrder", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("HaloPayClient QueryDepositForm response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException();

        //订单状态:0：成功，1：处理中，2：失败
        //4代表待支付状态，模拟回调不改变订单状态
        return node["status"]!.GetValue<string>();
    }

    public async ValueTask<HaloPayClientWithdrawResponse> WithdrawAsync(
        string merchantId,
        string apiKey,
        WithdrawOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = merchantId,
            ["desc"] = "",
            ["merchantOrderNo"] = order.FormId.ToString("N"),
            ["amount"] = order.Amount.ToString("#.##"),
            ["accountNumber"] = order.AccountNumber,
            ["accountName"] = order.AccountName,
            ["notificationUrl"] = order.CallbackUrl.AbsoluteUri,
            ["bankId"] = order.BankId,            
        };        

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        _logger.LogInformation("HaloPay Withdraw request Send sign:{sign}", sign);

        var response = await _httpClient
            .PostAsJsonAsync("/halo/indonesia/payOut", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("HaloPayClient Withdraw response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Withdraw response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(node["message"]!.ToString());

        return new HaloPayClientWithdrawResponse(
            FormId: Guid.Parse(node["merchantOrderNo"]!.GetValue<string>()),
            ThirdPartyOrderNo: node["payOrderNo"]!.GetValue<string>(),
            Amount: decimal.Parse(node["amount"]!.GetValue<string>()));
    }
}