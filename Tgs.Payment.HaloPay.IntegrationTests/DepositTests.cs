using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.HaloPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = HaloPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("82ee6e78f54e4585acd78b11e22bc57f");
        var brandId = Guid.NewGuid();
        var apiKey = "PRAN12179161511U";

        var request = """
            {
                "amount": "500.00",
                "payOrderNo": "075582657330",
                "merchantId": "H0131389964",
                "sign": "E244C176E84B0997DC53248C6748764A",
                "completionDate": "2024-04-25 16:12:26",
                "merchantOrderNo": "82ee6e78f54e4585acd78b11e22bc57f",
                "status": "0"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new HaloPayDepositApplyInfo(
            new HaloPayDepositSettings(
                "H0131389964",
                apiKey,
                formId,
                "4ucmqplxb029",
                "",
                "08123765216",
                1m),
             new HaloPayDepositResponse(
                500m,
                formId,
                new Uri("https://api.halopayipi.com/cash/?order_code=82ee6e78f54e4585acd78b11e22bc57f"),
                "HA7351783402199253524480684"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 500m,
                ThirdPartyOrderNo = "HA7351783402199253524480684",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, HaloPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("SUCCESS", actual);

        var paymentReturn = new HaloPayDepositReturn()
        {
            Status = "0",
            FormId = formId,
            OrderAmount = decimal.Parse("500.00"),
            Sign = "E244C176E84B0997DC53248C6748764A",
            MerchantId = "H0131389964",
            ThirdPartyOrderNo = "075582657330",
            CompletionDate = "2024-04-25 16:12:26",
            AfterExchangeOrderAmount = decimal.Parse("500.00"),
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, HaloPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.OrderAmount),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代收回調_訂單狀態失敗()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = HaloPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("93126c000488435b9606190c7a49a0f5");
        var brandId = Guid.NewGuid();
        var apiKey = "PRAN12179161511U";

        var request = """
            {
                "amount": "500.00",
                "payOrderNo": "830165468183",
                "merchantId": "H0131389964",
                "sign": "AC702EC0BF51B2756B4EA67F86137F39",
                "completionDate": "2024-04-26 09:41:29",
                "merchantOrderNo": "93126c000488435b9606190c7a49a0f5",
                "status": "2"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new HaloPayDepositApplyInfo(
            new HaloPayDepositSettings(
                "H0131389964",
                apiKey,
                formId,
                "4ucmqplxb029",
                "",
                "08123765216",
                1m),
             new HaloPayDepositResponse(
                500m,
                formId,
                new Uri("https://api.halopayipi.com/cash/?order_code=93126c000488435b9606190c7a49a0f5"),
                "HA8651783669973385224192649"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 500m,
                ThirdPartyOrderNo = "HA8651783669973385224192649",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, HaloPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("SUCCESS", actual);

        var paymentReturn = new HaloPayDepositReturn()
        {
            Status = "2",
            FormId = formId,
            OrderAmount = decimal.Parse("500.00"),
            Sign = "AC702EC0BF51B2756B4EA67F86137F39",
            MerchantId = "H0131389964",
            ThirdPartyOrderNo = "830165468183",
            CompletionDate = "2024-04-26 09:41:29",
            AfterExchangeOrderAmount = decimal.Parse("500.00"),
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, HaloPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}