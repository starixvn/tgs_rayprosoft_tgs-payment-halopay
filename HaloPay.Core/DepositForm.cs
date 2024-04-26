using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.Client;

namespace Tgs.Payment.HaloPay;

public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    HaloPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);