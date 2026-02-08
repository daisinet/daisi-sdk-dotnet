//using Daisi.SDK.Web.Services;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Security.Claims;
//using System.Text;
//using System.Text.Encodings.Web;

//namespace Daisi.SDK.Web.Authentication
//{
//    public class DaisiAuthenticationOptions : AuthenticationSchemeOptions { }
//    public class DaisiAuthenticationHandler : AuthenticationHandler<DaisiAuthenticationOptions>
//    {
//        public DaisiAuthenticationHandler(IOptionsMonitor<DaisiAuthenticationOptions> options, 
//            ILoggerFactory logger, 
//            UrlEncoder encoder, 
//            ISystemClock clock) : base(options, logger, encoder, clock)
//        {
//        }

//        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
//        {
//            var authService = (AuthService)this.Context.RequestServices.GetService(typeof(AuthService))!;

//            var user = new System.Security.Claims.ClaimsPrincipal();
//            var claims = new List<Claim>()
//                {
//                    new Claim(ClaimTypes.Name, validateResponse.HasUserName ? validateResponse.UserName : string.Empty),
//                    new Claim(ClaimTypes.Role, validateResponse.HasUserRole ? validateResponse.UserRole.ToString() : string.Empty),
//                    new Claim(ClaimTypes.Sid, validateResponse.HasUserId ? validateResponse.UserId : string.Empty),

//                };
//            var identity = new ClaimsIdentity(claims, "Custom");
//            user.AddIdentity(identity);

//            return AuthenticateResult.Success(new AuthenticationTicket();
//        }
//    }
//}
