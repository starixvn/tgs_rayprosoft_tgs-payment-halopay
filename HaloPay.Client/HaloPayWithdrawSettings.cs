namespace Tgs.Payment.HaloPay.Client;
public record HaloPayWithdrawSettings(
    string MerchantId,
    string ApiKey,
    Guid FormId,
    string AccountNumber,
    string AccountName,
    string BankId);