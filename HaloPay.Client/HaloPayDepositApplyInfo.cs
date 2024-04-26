namespace Tgs.Payment.HaloPay.Client;
public record HaloPayDepositApplyInfo(
    HaloPayDepositSettings Settings,
    HaloPayDepositResponse Response);