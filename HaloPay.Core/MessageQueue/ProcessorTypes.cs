namespace Tgs.Payment.HaloPay.MessageQueue;

public static class ProcessorTypes
{
    public static readonly Type DepositProcessorType = typeof(DepositProcessor);
    public static readonly Type DepositTimeoutHandlerType = typeof(DepositTimeoutHandler);
    public static readonly Type WithdrawProcessorType = typeof(WithdrawProcessor);
}