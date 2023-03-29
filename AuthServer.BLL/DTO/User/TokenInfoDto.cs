using AuthServer.BLL.AppConst;
using System.Security.Claims;

namespace AuthServer.BLL.DTO.User
{
    public class TokenInfoDto : UserBaseDto
    {
        public AuthSystemEnumDto AuthSystemEnumId;
        public ConsumeServiceEnumDto ConsumeServiceEnumId;
        public ulong? ServiceProfileId;
        public ulong UserId;

        public void SetFromClaims(IEnumerable<Claim>? claims)
        {
            if ((claims != null) && claims.Count() > 0)
            {
                Email = claims.FirstOrDefault(x => x.Type == "email")?.Value;
                Login = claims.FirstOrDefault(x => x.Type == "login")?.Value;
                Enum.TryParse(claims.First(x => x.Type == "authSystemEnumId").Value, out AuthSystemEnumDto outAuthSystemEnumId);
                Enum.TryParse(claims.First(x => x.Type == "consumeServiceEnumId").Value, out ConsumeServiceEnumDto outConsumeServiceEnumId);
                AuthSystemEnumId = outAuthSystemEnumId;
                ConsumeServiceEnumId = outConsumeServiceEnumId;
                ulong.TryParse(claims.FirstOrDefault(x => x.Type == "userid")?.Value, out ulong userid);
                UserId = userid;
            }
        }

        public IEnumerable<Claim> ClaimsFromInfo(IList<Claim> claims)
        {
            if (claims == null) claims = new List<Claim>();
            if (Login != null)
            {
                claims.Add(new Claim("login", Login));
            }
            if (Email != null)
            {
                claims.Add(new Claim("email", Email));
            }
            claims.Add(new Claim("authSystemEnumId", AuthSystemEnumId.ToString()));
            claims.Add(new Claim("consumeServiceEnumId", ConsumeServiceEnumId.ToString()));
            claims.Add(new Claim("userid", UserId.ToString()));

            return claims;
        }
    }
}
