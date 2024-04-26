namespace Tgs.Payment.HaloPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string AccountName,
    string PayType,
    string Mobile);