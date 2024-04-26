namespace Tgs.Payment.HaloPay;
public record HaloPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);