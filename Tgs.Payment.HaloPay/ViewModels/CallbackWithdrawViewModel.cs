using System.Text.Json.Serialization;

namespace Tgs.Payment.HaloPay.ViewModels;

public class CallbackWithdrawViewModel
{
    [JsonPropertyName("merchantId")]
    public string MerchantId { get; init; } = default!;

    /// <summary>
    /// Gets the status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = default!;

    [JsonPropertyName("merchantOrderNo")]
    public string MerchantOrderNo { get; init; } = default!;

    [JsonPropertyName("payOrderNo")]
    public string PayOrderNo { get; init; } = default!;

    [JsonPropertyName("completionDate")]
    public string CompletionDate { get; init; } = default!;

    [JsonPropertyName("amount")]
    public string Amount { get; init; } = default!;

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("sign")]
    public string Sign { get; init; } = default!;
}