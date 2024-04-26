namespace Tgs.Payment.HaloPay.Client;
public record HaloPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);