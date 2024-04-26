namespace Tgs.Payment.HaloPay.Client;
public record HaloPayDepositSettings(
    string MerchantId,
    string ApiKey,
    Guid FormId,
    string AccountName,
    string PayType,
    string Mobile);