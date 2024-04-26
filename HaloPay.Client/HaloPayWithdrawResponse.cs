namespace Tgs.Payment.HaloPay.Client;
public record HaloPayWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo);