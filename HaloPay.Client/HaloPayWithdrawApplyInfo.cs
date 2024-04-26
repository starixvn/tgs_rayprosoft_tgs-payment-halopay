namespace Tgs.Payment.HaloPay.Client;
public record HaloPayWithdrawApplyInfo(
    HaloPayWithdrawSettings Settings,
    HaloPayWithdrawResponse Response);