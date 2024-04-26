namespace Tgs.Payment.HaloPay;
public record HaloPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);