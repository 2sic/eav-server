using System;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.Jwt
{
    [TestClass]
    public class ValidateToken2sxcTest
    {

        [TestMethod]
        public void TokenValidation()
        {
            string SelfSigned2048_SHA256 = @"MIIK9wIBAzCCCrMGCSqGSIb3DQEHAaCCCqQEggqgMIIKnDCCBhUGCSqGSIb3DQEHAaCCBgYEggYCMIIF/jCCBfoGCyqGSIb3DQEMCgECoIIE/jCCBPowHAYKKoZIhvcNAQwBAzAOBAiQvAQS6npsiQICB9AEggTY4EAsz2OOaOVNLjLXxIRGCxxH21qUoOVzuJSKlifFEK3j9PSjgOib28gCf6wVSTuK66a+MRdzL00CpCxUmwGPxX6fMYpzamSdlgojIlOAxktf8hy0GFtWt0bNA900PY/+uTlS7YjU4oH9uHXART2Om6unECEiNuS9MLzNZc6sRBlO+aFfFmLF3PasAbQBZK1NnPJRgK3fZYIyOInEMUMh6drhoM1GjCqMIUBcALZhg0U5I5kkBwHec/2EW5gARk+pUk2eG5bo/rduquvRuhrq2f5U+s7Am3HNJXN05K4uUvzpgc10USdM/JFjVeLUmUyNkMEwS2iNi8DmleYsajCrJj9CpF7mn/eiIXlfbpUaPqLXDFlBMHZ0eSjlALqOMAWEW7fq3nWt/h+MBvBulBXAG5Rc8pFU75xiICE1wFtzfR0geBaFXDzo3yci38RQdPhMMrb4hGlEDaPL9OikkQ9hxnULL7DqJClpdC2Oo6bI/Ku7x75/HI14hyycfb0vOESM88PKVDdr+o7xisci9iw/Xf1EPa9AgX/e3I28LolIW1e9LJJyP1QxD2MW7nphKkKrum8Gj0qORFxHbbceZJBo4l/BUMUubIiJlxRgTxNL1s4UvyrAU2ie+kVWVmJHnCmZa1Y9RN4obtgoq2ZoOt4Jh3qkHy18J97nIrgzMrXmXbiyrcgIPB3u74lJOVzFrGQ6SrwDnOznCOomlC0Lk5Z8C6Oko8IAVUDePg3GrjFVXULXXXSWmSJrKawnpg67lBopMzWXXvS3e7usnhF2XBzpva7ST/2BJJL7JMBcVUx/L/z7PTCtjBg6mEqdndWxVj9Jd22mIwQcchgDgVbVDtEayB9G7hTyP+G5bNw8+E7jGn0mANRbH6n9jdlXURcnSKh6ZH7y2Tf5xk6giJBVF9i6WVFFKRLMmE4HAie26+WFUmvdxyMCDU7NK3i3IjYhOdnU4NtQfb4/kRg5DpPx0dnj3PlwI7Dkizm7viwP6OJAaRRRwAyxnl8KGmT7o9RkJl+95VI6DlR0nuV8gri7NLbKdua8svY+zAp+LxseF+VR0jJDcfn2EFjQuFDnffrhoXUed79AloR410vGfSCP/p77QJEURim/I8QawTGW7qNaFT9k85dV7Hizo0jNMngceuxFs1fHDTt9s5NRY1k1SCWQiRXVdRDFdpFS5jdhgQJHhc3VObhs85U07bQa4+cukyPTYxBTNf2wIxA79Kxn14FqGGXYW2TfkHsLJn+cPLcEfYy/nboa7oVyLkpHhbqfkG6vV2h6wnJxG9dgjiVMNtMfwR6LfiJy/V4bm2UDi0K2bifv2FLpe6jXgANlACAKsQs7wpyNnSqyGtJguaK7/4kx8jQM2hII8mrDRo/7nK5yEtdha/gEXPbFK3zTzj+7pOoK9qCMqguxg1Kaxqa16ei+LqKm5sHrVkpaCw1VmDXKe4CzYOBDWF8+v8Z0IrvdbLkW8dNN7jYBCgXXoiD3Jb6cLtqkoX74bHWkEAVjCebgzxCOORRYNB9edbBT4juQUx3i39eAZxrv3TM3P42DqSWH0lj822ufYwzEkFRYxUNOmU0lDK5Ws5149NftVDnjYnWGMJy+cQ583GdRXdAo05Uy003oBIKpQu3/qhQ7voK6ZxcpElmkgPp0PDGB6DANBgkrBgEEAYI3EQIxADATBgkqhkiG9w0BCRUxBgQEAQAAADBdBgkqhkiG9w0BCRQxUB5OAHQAZQAtADMANwBkAGEANQBiAGQAMAAtADgAOQA3ADYALQA0AGIAMABmAC0AYQA4AGEAZAAtADgAOAAzADYANABmADQAMABhAGEANgAxMGMGCSsGAQQBgjcRATFWHlQATQBpAGMAcgBvAHMAbwBmAHQAIABCAGEAcwBlACAAQwByAHkAcAB0AG8AZwByAGEAcABoAGkAYwAgAFAAcgBvAHYAaQBkAGUAcgAgAHYAMQAuADAwggR/BgkqhkiG9w0BBwagggRwMIIEbAIBADCCBGUGCSqGSIb3DQEHATAcBgoqhkiG9w0BDAEDMA4ECFe2/vWj9QveAgIH0ICCBDiYxFltU6RmUtITvsaM4uh56h4GnW2XMUspsCQgXl6uoQanDRfCHWEad+9NmyrxXtRxGeDTZkw11Asm2FmcAmCXuIYIogqVvpEK5LLSu7o01mwO+EHWFnEtrJ95HWBs3qJ9RKgoOlAOeEKdGFX5DUx5G+igoHMxjfeEModQgpxYyWfj2v+xvKeOzBwK+8RpshgYq9Ptt12WpIbrQhy7C7rCem/tf1uT3YTXRSKT9hsBb8h10mggqRFSJIV8x15n6LB53zWBLrACbsFXK5zXJ9NJNUYJIH9WKrv8tOASOQFjU4Gn247xPnv2ile4FHsoAHe2nmh63TiFz58YUUrWu3j4Kbz7pd8ouRYe8pDnOatB25KaFU2W+tzklYJ/Ob2LB4pqpXrVvc6riF/AKVh/nvlWGXMBAdiMErNbkPswv9eIPg5EcOtOQwtWzYI+AnM615BXzj5UaL+H0TVKipLvAoh5r1dUeQOiqQNBOep57Xy9ww784iEALL5KuN3U5NzcrbWa6sZoG0ZhwpSi0oPPIqFRgxN/vuVAWwY5MpIRn4063F/kwX/UaaXZ/3VTH99+mbj6Di3M8t/n77E/jUo6cY/7hCUxhoJ9ZiT/NVMgHJoeu6vScOmAGQSUJkHw9wvTG4vjOUx6EFbkRfnE3t4/2XwJmJTFkb/gUnRI/g71THWanhKgLBZmVVV6oKWxIZSF/gcwUzNebherfy0IWk4wIveYa9ABWcWD8H+zXzrZHTEm8yEVVnfHKiLKuimlv/Ul5UPy8Wd86Ka3lc3MelWohLh8mZYE/pRLyEWlK/fElHbUh+S631I5zaOi26nUa2gnpU/C2TcvhNAErCZ3CDyQkxZBJ9CjUK0YYZZVFAIf7+6UJmd7OpJHtZovsNn++3haEkrAEJWpWNQuEwCfpHZTQmURJQVJ2K8Mht55g8JYpzMZNGR8GTMVALtds6n/YppEEH9i5CFh+Jzc261E7u1lJpC76M9SFP5rYRZjjSnFC0l0jXQ9r/m/xwwrRQFbSezjS6hcHAHKMpDlU0XH2A255I95ljPLyT7ARg9eauTceA/kWajqxUIk+ywaqst5usiVE3Xyjk1BMTkDWYWUfASJpYneR6hZnIu6Qu2lCNzI9GabbD7RQoE6H4fTEgflhw92G52w581O3hkBM+kiZVr5utzr9Wp1rXIzBOlF3hvyY2ATk7kF/RulpnO/abS1oOWuoTorVGqLofxYnXJrts0A65nGUMSHzn8r3/RUjRj4w6rng9QKq1gDAOliNB1QZx3qw6QiNij8GHx0mrNdXCAcBDIEX/1y239Dn4kebbMWdw5YiiK4f/bB2QVi4rIO1Y6fxS1LBcBbZFuNfLF9ybVRZJl2GkaTq+VS9AKSOTlcFu3efXi3ThQUKq7N1xqvMemCfjGEC1wlKyHhEFjrjBeF/Y8J1kYNYQpj6UQwOzAfMAcGBSsOAwIaBBQxCy1Od89j0wydVRhX7mr00yKKuQQUrR9gPvPNxrWChwVF8krCCyRJ3pkCAgfQ";
            string SelfSigned2048_SHA256_Public = @"MIIDxjCCAq6gAwIBAgIQdPcZzh1mIbVHaqFLCasfHzANBgkqhkiG9w0BAQsFADBnMQ4wDAYDVQQHDAVCdWNoczERMA8GA1UECAwIU3RHYWxsZW4xCzAJBgNVBAYTAkNIMRQwEgYDVQQLDAtEZXZlbG9wbWVudDENMAsGA1UECgwEMnNpYzEQMA4GA1UEAwwHMnN4YyBDQTAeFw0xODA0MjAxNTA5MTNaFw0zOTEyMzEyMjAwMDBaMGcxDjAMBgNVBAcMBUJ1Y2hzMREwDwYDVQQIDAhTdEdhbGxlbjELMAkGA1UEBhMCQ0gxFDASBgNVBAsMC0RldmVsb3BtZW50MQ0wCwYDVQQKDAQyc2ljMRAwDgYDVQQDDAcyc3hjIENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAm1UGoVRxzd8juZzY1em03TiQL2zPHrmRci9/FGz/x2K4H+3LzlB9i5Z3C8N+9uzMiHlU0OTLBFAoFKWxqz908qq9HBVg6fyy35QtUHf4RfW0bkz8RiamkFuZKU9dFPudHlegAg6oy24O6/6ghVHsEtg6yy/n+1kiuq5gbCClQFxD4ps/oDNdQ989NsyZf+eLe1jKsiJeHm+KnCih2fvNaKQnHiJ5GHydKTiWN8UsOwIb78K8aHiSAeoyEkX7rvjKCIVn5mWrCOsRQ4IiTbfX830KrCrtsfvU35IVj/AGLCBCx0QVvND6RapsqaFx3v23kXL4CaX5aGoswfbP3RHcsQIDAQABo24wbDAOBgNVHQ8BAf8EBAMCAgQwEwYDVR0lBAwwCgYIKwYBBQUHAwMwEgYDVR0RBAswCYIHMnN4YyBDQTASBgNVHRMBAf8ECDAGAQH/AgEAMB0GA1UdDgQWBBT7kGSPYdOVUmzhp9vp1nJdCcfZGjANBgkqhkiG9w0BAQsFAAOCAQEAMSEVGEARn+2kFbLa9dOglgKhQ39ApjNeiV2AB2gwzXOABAOzS/DN5wOcfkPKt/rUgcnSx/sha8F5Jt8uDWE5fBgujndX1UwgOd3s6tHDo/V+Aze9hsh5ojOapaW7nMNyUe7dFZYgWz9lnM68Dm0YL356tVj+7mZdGIPmUR+hRzPWRlzwnYB3P9raTg1pTv0TwNantQSDvYr2PooMCL4pKFXQabRM/AscLfbgJLm3rmgdiXvstWUYDOdJUaN5Kati3kiFjpz6+xsIxnDS8DWbvHz7yqH01AprkjENIspyDBQ9OdhGricQnDQ03ndd5hHtevOfJrATdaSeuzAVyd0GFQ==";
            X509Certificate2 CertSelfSigned2048_SHA256 = new X509Certificate2(Convert.FromBase64String(SelfSigned2048_SHA256), "", X509KeyStorageFlags.PersistKeySet);
            X509SecurityKey X509SecurityKeySelfSigned2048_SHA256 = new X509SecurityKey(CertSelfSigned2048_SHA256);
            X509Certificate2 CertSelfSigned2048_SHA256_Public = new X509Certificate2(Convert.FromBase64String(SelfSigned2048_SHA256_Public), "");
            X509SecurityKey X509SecurityKeySelfSigned2048_SHA256_Public = new X509SecurityKey(CertSelfSigned2048_SHA256_Public);

            var signingCredentials = new SigningCredentials(X509SecurityKeySelfSigned2048_SHA256, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);
            var header = new JwtHeader(signingCredentials);

            var payload = new JwtPayload
            {
                { "some", "hello"},
                { "scope", "http://dummy.com/"},
            };

            var jwtToken = new JwtSecurityToken(header, payload);

            var handler = new JwtSecurityTokenHandler();

            // Token to String so you can use it in your client
            var jwt = handler.WriteToken(jwtToken);
            Trace.WriteLine($"server: jwt token {jwt}");

            var jwtParts = jwt.Split('.');
            var jwtHeader = Encoding.UTF8.GetString(Base64UrlDecode(jwtParts[0]));
            Trace.WriteLine($"server: jwt token DecodeBase64 header is not encrypted {jwtHeader}");

            var jwtPayload = Encoding.UTF8.GetString(Base64UrlDecode(jwtParts[1]));
            Trace.WriteLine($"server: jwt token DecodeBase64 payload is not encrypted {jwtPayload}");

            var jwtSignatureBase64UrlEncoded = jwtParts[2];
            Trace.WriteLine($"server: jwt token DecodeBase64 signature {jwtSignatureBase64UrlEncoded}");

            var jwtSignature = Encoding.UTF8.GetString(Base64UrlDecode(jwtSignatureBase64UrlEncoded));
            Trace.WriteLine($"server: jwt token DecodeBase64 signature {jwtSignature}");

            Trace.WriteLine("Consume Token on client...");

            var validationParameters =
                new TokenValidationParameters
                {
                    IssuerSigningKey = X509SecurityKeySelfSigned2048_SHA256_Public,
                    RequireSignedTokens = true,
                    RequireExpirationTime = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                };

            SecurityToken validatedSecurityToken = null;
            ClaimsPrincipal principal = handler.ValidateToken(jwt, validationParameters, out validatedSecurityToken);

            var token = handler.ReadToken(jwt);
            Trace.WriteLine(token);
            Trace.WriteLine($"client: read token {token}");
            Trace.WriteLine($"client: validated token {validatedSecurityToken}");

            //Assert.AreEqual("hello", principal.FindFirst("some").Value);
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding  
            output = output.Replace('_', '/'); // 63rd char of encoding  
            switch (output.Length % 4) // Pad with trailing '='s  
            {
                case 0: break; // No pad chars in this case  
                case 2: output += "=="; break; // Two pad chars  
                case 3: output += "="; break; // One pad char  
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder  
            return converted;
        }
    }
}
