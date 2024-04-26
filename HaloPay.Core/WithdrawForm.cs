using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.Client;

namespace Tgs.Payment.HaloPay;

public record WithdrawForm(
      Guid Id,
      string? UserId,
      UserType UserType,
      Guid BrandId,
      Guid? BrandPaymentId,
      decimal Amount,
      PaymentFlowState Status,
      Guid BankInfoId,
      string? BankInfo,
      HaloPayWithdrawApplyInfo? ApplyInfo,
      string? PaymentReturn = null);