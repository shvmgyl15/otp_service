namespace In.ProjectEKA.OtpServiceTest.Otp
{
	using Builder;
	using FluentAssertions;
	using Moq;
	using OtpService.Clients;
	using OtpService.Common;
	using OtpService.Otp;
	using Xunit;

	[Collection("Otp Service Tests")]
    public class OtpSenderTest
    {
        private readonly OtpSender otpSender;
        private readonly Mock<IOtpRepository> otpRepository = new Mock<IOtpRepository>();
        private readonly Mock<IOtpGenerator> otpGenerator = new Mock<IOtpGenerator>();
        private readonly Mock<ISmsClient> otpWebHandler = new Mock<ISmsClient>();
        private readonly OtpProperties otpProperties = new OtpProperties(5);
        public OtpSenderTest()
        {
            otpSender = new OtpSender(otpRepository.Object, otpGenerator.Object, otpWebHandler.Object, otpProperties);
        }

        [Fact]
        private async void ReturnSuccessResponse()
        {
            var sessionId = TestBuilder.Faker().Random.Hash();
            const string otpToken = "123456";
            var systemName = TestBuilder.Faker().Random.Word();
            var phoneNumber = TestBuilder.Faker().Phone.PhoneNumber();
            var testOtpResponse = new Response(ResponseType.Success, "Otp Created");
            var otpCreationDetail = new OtpGenerationDetail(systemName, Action.REGISTRATION.ToString());
            var otpRequest = new OtpGenerationRequest(sessionId, new Communication("MOBILE"
                , phoneNumber), otpCreationDetail);
            var generatedMessage = otpSender.GenerateMessage(otpCreationDetail, otpToken);
            otpGenerator.Setup(e => e.GenerateOtp()).Returns(otpToken);
            otpWebHandler.Setup(e => e.Send(otpRequest.Communication.Value, generatedMessage))
                .ReturnsAsync(testOtpResponse);
            otpRepository.Setup(e => e.Save(otpToken, sessionId))
                .ReturnsAsync(testOtpResponse);

            var otpResponse = await otpSender.GenerateOtp(otpRequest);

            otpGenerator.Verify();
            otpWebHandler.Verify();
            otpRepository.Verify();
            otpResponse.Should().BeEquivalentTo(testOtpResponse);
        }
    }
}