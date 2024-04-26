using System.Text.Json.Serialization;

namespace Tgs.Payment.HaloPay.ViewModels;

public class CallbackDepositViewModel
{
    [JsonPropertyName("merchantId")]
    public string MerchantId { get; init; } = default!;
    /// <summary>
    /// 订单状态:0：成功，1：处理中，2：失败
    /// </summary>    
    [JsonPropertyName("status")]
    public string Status { get; init; } = default!;
    /// <summary>
    /// 商户订单号
    /// </summary>
    [JsonPropertyName("merchantOrderNo")]
    public string MerchantOrderNo { get; init; } = default!;
    /// <summary>
    /// 平台订单号
    /// </summary>    
    [JsonPropertyName("payOrderNo")]
    public string PayOrderNo { get; init; } = default!;

    [JsonPropertyName("amount")]
    public string Amount { get; init; } = default!;
    /// <summary>
    /// 交易时间
    /// </summary>
    [JsonPropertyName("completionDate")]
    public string CompletionDate { get; init; } = default!;

    [JsonPropertyName("sign")]
    public string Sign { get; init; } = default!;
}