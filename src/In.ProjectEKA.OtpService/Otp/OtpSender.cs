namespace In.ProjectEKA.OtpService.Otp
{
    using System;
    using System.Threading.Tasks;
    using Clients;
    using Common;
    using Microsoft.Extensions.Configuration;

    public class OtpSender : IOtpSender
    {
        private readonly IOtpRepository otpRepository;
        private readonly IOtpGenerator otpGenerator;
        private readonly ISmsClient smsClient;
        private readonly OtpProperties otpProperties;

        public OtpSender(IOtpRepository otpRepository, IOtpGenerator otpGenerator, ISmsClient smsClient, OtpProperties otpProperties)
        {
            this.otpRepository = otpRepository;
            this.otpGenerator = otpGenerator;
            this.smsClient = smsClient;
            this.otpProperties = otpProperties;
        }

        public async Task<Response> GenerateOtp(OtpGenerationRequest otpGenerationRequest)
        {
            var generateOtp = otpGenerator.GenerateOtp();
            var generateMessage = GenerateMessage(otpGenerationRequest.CreationDetail, generateOtp);
            
            var sendOtp = await smsClient.Send(otpGenerationRequest.Communication.Value, generateMessage);
            if (sendOtp.ResponseType == ResponseType.Success)
            {
                return await otpRepository.Save(generateOtp, otpGenerationRequest.SessionId);
            }

            return sendOtp;
        }

        public string GenerateMessage(OtpCreationDetail creationDetail, string value)
        {
            return creationDetail.Action switch
            {
                Action.LOGIN => $"The OTP is {value} to login, This one time password is valid for {otpProperties.ExpiryInMinutes}  minutes." +
                                $" Message sent by {creationDetail.SystemName}",
                Action.FORGOT_CM_ID => $"The OTP is {value} to recover the username, This one time password is valid" +
                                       $" for {otpProperties.ExpiryInMinutes} minutes. Message sent by {creationDetail.SystemName}",
                Action.REGISTRATION => $"The OTP is {value} to verify the mobile number, This one time password is valid " +
                                       $"for {otpProperties.ExpiryInMinutes} minutes. Message sent by {creationDetail.SystemName}",
                Action.LINK_PATIENT_CARECONTEXT => $"The OTP is {value} to link your care context, This one time password is valid " +
                                                   $"for {otpProperties.ExpiryInMinutes} minutes. Message sent by {creationDetail.SystemName}",
                Action.RECOVER_PASSWORD => $"The OTP is {value} to recover password, This one time password is valid " +
                                                   $"for {otpProperties.ExpiryInMinutes} minutes. Message sent by {creationDetail.SystemName}", 
                _ => throw new Exception("Unknown action")
            };
        }
    }
}