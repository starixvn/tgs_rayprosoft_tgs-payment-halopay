using System.ComponentModel;

namespace Tgs.Payment.HaloPay.Client;
public record HaloPayDepositSettings(
    string MerchantId,
    string ApiKey,
    Guid FormId,
    string AccountName,
    string PayType,
    string Mobile,
    [property: Description("匯率轉換")]
    decimal ExchangeRate);