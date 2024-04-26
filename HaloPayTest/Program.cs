using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var merchantId = "H0131389964";
var apiKey = "PRAN12179161511U";

//代收
//var orderNo = Guid.NewGuid();
//var amount = 500.00m;

//var depositsettings = new HaloPayDepositSettings(
//    merchantId,
//    apiKey,
//    orderNo,
//    "4ucmqplxb029",
//    "",
//    "08123765216");

//var depositreturn = await paymentClient.DepositAsync(
//    "tgs.payment.halopay.deposit",
//    amount,
//    JsonSerializer.Serialize(depositsettings));
//logger.LogInformation($"deposit return: {depositreturn}");

//代付
var orderNo = Guid.NewGuid();
var withdrawSettings = new HaloPayWithdrawSettings(
    merchantId,
    apiKey,
    orderNo,
    "687354546545",
    "test",
    "1");

var withdrawReturn = await paymentClient.WithdrawAsync(
    "tgs.payment.halopay.withdraw",
    100M,
    JsonSerializer.Serialize(withdrawSettings));
logger.LogInformation($"Withdraw Return: {withdrawReturn}");

//代收訂單查詢
//var queryFormId = Guid.Parse("a8c94112-fb23-4a6b-a61e-c5ca7cf44021");

//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("https://api.halopayipi.com")
//};
//var client = new HaloPayClient(httpClient, new HaloPayHasher(NullLogger<HaloPayHasher>.Instance), NullLogger<HaloPayClient>.Instance);
//var depositQuery = await client.QueryDepositFormAsync(merchantId, apiKey, queryFormId);

//logger.LogInformation("{depositQuery}", depositQuery);