namespace Tgs.Payment.HaloPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string AccountNumber,
    string AccountName,
    string BankId);