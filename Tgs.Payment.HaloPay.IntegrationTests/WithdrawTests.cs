using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.HaloPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;


namespace Tgs.Payment.HaloPay.IntegrationTests;

public class WithdrawTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代付回調_訂單狀態失敗()
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

        var formId = Guid.Parse("a89241e88c7546a7bb00a928f485b1fd");
        var brandId = Guid.NewGuid();
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        var settings = new HaloPayWithdrawSettings(
            merchantId,
            apiKey,
            formId,
            "687354546545",
            "test",
            "1");
        var withdrawResponse = new HaloPayWithdrawResponse(formId, "1991783685007318458368772");

        var applyInfo = new HaloPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, HaloPayUtility.SerializerOptions)
            });

        var request = """
            {
                "amount": "100.00",
                "payOrderNo": "1991783685007318458368772",
                "merchantId": "H0131389964",
                "sign": "B5B35BDEBCF918449AF8480B7C90E09B",
                "completionDate": "2024-04-26 10:46:23",
                "merchantOrderNo": "a89241e88c7546a7bb00a928f485b1fd",
                "status": "2"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new HaloPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse("100.00"),
            Sign = "B5B35BDEBCF918449AF8480B7C90E09B",
            Status = "2",
            CompletionDate = "2024-04-26 10:46:23",
            MerchantId = "H0131389964",
            ThirdPartyOrderNo = "1991783685007318458368772"
        };

        Assert.Equal("SUCCESS", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, HaloPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態成功()
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

        var formId = Guid.Parse("73c808b6756f427e981c5613d2ec4a06");
        var brandId = Guid.NewGuid();
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        var settings = new HaloPayWithdrawSettings(
            merchantId,
            apiKey,
            formId,
            "687354546545",
            "test",
            "1");
        var withdrawResponse = new HaloPayWithdrawResponse(formId, "7631783684224019603456238");

        var applyInfo = new HaloPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, HaloPayUtility.SerializerOptions)
            });

        var request = """
            {
                "amount": "100.00",
                "payOrderNo": "7631783684224019603456238",
                "merchantId": "H0131389964",
                "sign": "0E5B1DC6843C25D0DB3082D9CFC77BA3",
                "completionDate": "2024-04-26 10:42:24",
                "merchantOrderNo": "73c808b6756f427e981c5613d2ec4a06",
                "status": "0"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new HaloPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse("100.00"),
            Sign = "0E5B1DC6843C25D0DB3082D9CFC77BA3",
            Status = "0",
            CompletionDate = "2024-04-26 10:42:24",
            MerchantId = "H0131389964",
            ThirdPartyOrderNo = "7631783684224019603456238"
        };

        Assert.Equal("SUCCESS", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, HaloPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}