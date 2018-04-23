using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.Signature
{
    [TestClass]
    public class SignatureHashSha1
    {

        [TestMethod]
        public void SignatureSha1ValidationTest()
        {
            string Json = @"[
  {
    '_id': '5ade0ab128c25c19e3e09774',
    'index': 0,
    'guid': '61b11a93-6d7f-4ca8-bc53-7cb755d62b32',
    'isActive': false,
    'balance': '$2,388.35',
    'picture': 'http://placehold.it/32x32',
    'age': 40,
    'eyeColor': 'brown',
    'name': 'Alfreda Bryan',
    'gender': 'female',
    'company': 'PURIA',
    'email': 'alfredabryan@puria.com',
    'phone': '+1 (905) 546-2693',
    'address': '332 Noel Avenue, Kenmar, Delaware, 3814',
    'about': 'Id est dolore proident exercitation excepteur aute consequat dolore voluptate eiusmod. Minim duis amet duis ut dolore commodo eu eiusmod commodo enim sit. Fugiat ea sunt et quis reprehenderit ex occaecat veniam cupidatat est proident labore. Veniam ea voluptate sint labore incididunt ea quis adipisicing duis occaecat reprehenderit. Tempor magna ea magna excepteur eu consequat veniam do minim excepteur non dolore labore adipisicing.\r\n',
    'registered': '2014-03-18T09:44:06 -01:00',
    'latitude': 43.555408,
    'longitude': -153.185991,
    'tags': [
      'est',
      'reprehenderit',
      'deserunt',
      'adipisicing',
      'voluptate',
      'consectetur',
      'sit'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Desiree Young'
      },
      {
        'id': 1,
        'name': 'Lourdes Mcbride'
      },
      {
        'id': 2,
        'name': 'Floyd Alexander'
      }
    ],
    'greeting': 'Hello, Alfreda Bryan! You have 10 unread messages.',
    'favoriteFruit': 'banana'
  },
  {
    '_id': '5ade0ab1865f0f24537111e9',
    'index': 1,
    'guid': 'c348879d-221b-442d-8f8d-3f0886fec28f',
    'isActive': false,
    'balance': '$2,674.96',
    'picture': 'http://placehold.it/32x32',
    'age': 39,
    'eyeColor': 'green',
    'name': 'Hawkins Nielsen',
    'gender': 'male',
    'company': 'FUTURIS',
    'email': 'hawkinsnielsen@futuris.com',
    'phone': '+1 (995) 420-3972',
    'address': '664 Ralph Avenue, Cobbtown, Guam, 768',
    'about': 'Magna duis qui voluptate esse pariatur do pariatur anim in et sit irure. Dolore cillum qui et voluptate laborum reprehenderit ea labore eu nisi est nulla. Proident sunt duis aliqua in eu quis.\r\n',
    'registered': '2015-01-21T11:50:24 -01:00',
    'latitude': -39.009212,
    'longitude': 122.757482,
    'tags': [
      'fugiat',
      'ullamco',
      'et',
      'enim',
      'velit',
      'deserunt',
      'pariatur'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Ines Talley'
      },
      {
        'id': 1,
        'name': 'Ortiz Bartlett'
      },
      {
        'id': 2,
        'name': 'Tammy Petty'
      }
    ],
    'greeting': 'Hello, Hawkins Nielsen! You have 2 unread messages.',
    'favoriteFruit': 'strawberry'
  },
  {
    '_id': '5ade0ab1f778314895a0896e',
    'index': 2,
    'guid': 'b94e8f0a-71b7-49fa-8ffa-47ede9e685f7',
    'isActive': true,
    'balance': '$2,276.30',
    'picture': 'http://placehold.it/32x32',
    'age': 38,
    'eyeColor': 'blue',
    'name': 'Mcgowan Chandler',
    'gender': 'male',
    'company': 'BIFLEX',
    'email': 'mcgowanchandler@biflex.com',
    'phone': '+1 (949) 522-3048',
    'address': '239 Dahlgreen Place, Adamstown, Rhode Island, 7826',
    'about': 'Ad cupidatat dolor et pariatur veniam esse enim ea quis exercitation veniam duis. Magna aliqua minim laborum consectetur voluptate quis do consequat cillum amet dolore magna non. Veniam aliquip reprehenderit reprehenderit cillum sit culpa non nostrud. Cillum Lorem voluptate non est culpa ullamco labore est eu aute mollit. Proident ad officia deserunt sint laboris. Duis do qui qui deserunt. Laborum do culpa excepteur anim magna nisi.\r\n',
    'registered': '2015-05-13T01:00:53 -02:00',
    'latitude': -5.682748,
    'longitude': 80.792187,
    'tags': [
      'sunt',
      'reprehenderit',
      'ut',
      'enim',
      'laboris',
      'minim',
      'elit'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Gilbert Silva'
      },
      {
        'id': 1,
        'name': 'Harvey Sherman'
      },
      {
        'id': 2,
        'name': 'Herminia Weaver'
      }
    ],
    'greeting': 'Hello, Mcgowan Chandler! You have 2 unread messages.',
    'favoriteFruit': 'banana'
  },
  {
    '_id': '5ade0ab126a5920a399228bd',
    'index': 3,
    'guid': 'ab270c5a-51fd-4820-a335-70019ceffa45',
    'isActive': false,
    'balance': '$3,952.52',
    'picture': 'http://placehold.it/32x32',
    'age': 21,
    'eyeColor': 'blue',
    'name': 'Dejesus King',
    'gender': 'male',
    'company': 'MOMENTIA',
    'email': 'dejesusking@momentia.com',
    'phone': '+1 (802) 450-2953',
    'address': '579 Crystal Street, Shrewsbury, Illinois, 6419',
    'about': 'Pariatur anim aliquip ut est est velit. Laboris culpa esse voluptate exercitation culpa magna laboris dolore. Magna proident dolor fugiat fugiat.\r\n',
    'registered': '2015-10-20T10:34:14 -02:00',
    'latitude': -9.553297,
    'longitude': -98.920955,
    'tags': [
      'magna',
      'cillum',
      'dolor',
      'aliquip',
      'ad',
      'consectetur',
      'sunt'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Patti Bennett'
      },
      {
        'id': 1,
        'name': 'Dona Boone'
      },
      {
        'id': 2,
        'name': 'Stuart Kelley'
      }
    ],
    'greeting': 'Hello, Dejesus King! You have 5 unread messages.',
    'favoriteFruit': 'banana'
  },
  {
    '_id': '5ade0ab1b26bc33110dd9605',
    'index': 4,
    'guid': 'a1d3255a-c0b6-4718-8afc-c3ddac3d510e',
    'isActive': false,
    'balance': '$2,666.60',
    'picture': 'http://placehold.it/32x32',
    'age': 36,
    'eyeColor': 'brown',
    'name': 'Beulah Landry',
    'gender': 'female',
    'company': 'CIRCUM',
    'email': 'beulahlandry@circum.com',
    'phone': '+1 (888) 542-2197',
    'address': '973 Portal Street, Hemlock, Tennessee, 9886',
    'about': 'Reprehenderit eiusmod deserunt labore irure eu exercitation voluptate Lorem quis occaecat. Consectetur eu nostrud velit non dolor voluptate ipsum aliqua reprehenderit excepteur incididunt ipsum adipisicing. Nostrud deserunt minim consectetur eu quis. Cillum in mollit dolore aliquip dolor ut commodo dolore quis ea non ad sint magna.\r\n',
    'registered': '2017-11-01T03:06:48 -01:00',
    'latitude': 45.720448,
    'longitude': -133.4329,
    'tags': [
      'aliquip',
      'velit',
      'ea',
      'consequat',
      'culpa',
      'ea',
      'esse'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Ina Moss'
      },
      {
        'id': 1,
        'name': 'Rodriguez Melendez'
      },
      {
        'id': 2,
        'name': 'Kline Monroe'
      }
    ],
    'greeting': 'Hello, Beulah Landry! You have 8 unread messages.',
    'favoriteFruit': 'banana'
  },
  {
    '_id': '5ade0ab10a1ec5ad72e07367',
    'index': 5,
    'guid': '5dfd73af-5419-4c9a-b392-35384572b513',
    'isActive': true,
    'balance': '$3,667.04',
    'picture': 'http://placehold.it/32x32',
    'age': 24,
    'eyeColor': 'brown',
    'name': 'Ashley Burnett',
    'gender': 'female',
    'company': 'GEEKETRON',
    'email': 'ashleyburnett@geeketron.com',
    'phone': '+1 (880) 561-3928',
    'address': '559 Ferry Place, Blairstown, Utah, 3981',
    'about': 'Est nisi excepteur et est cupidatat ipsum. Commodo eiusmod labore cillum incididunt nisi labore dolor consequat do Lorem. Mollit sit pariatur incididunt aliquip dolor officia id pariatur duis et et duis fugiat. Culpa sint dolore amet minim duis proident aliqua laborum. Reprehenderit velit occaecat adipisicing officia.\r\n',
    'registered': '2017-04-23T11:40:03 -02:00',
    'latitude': 58.217111,
    'longitude': 131.279895,
    'tags': [
      'aliquip',
      'excepteur',
      'minim',
      'eiusmod',
      'sit',
      'exercitation',
      'excepteur'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Moon Osborne'
      },
      {
        'id': 1,
        'name': 'Ellison Saunders'
      },
      {
        'id': 2,
        'name': 'Kim Trujillo'
      }
    ],
    'greeting': 'Hello, Ashley Burnett! You have 8 unread messages.',
    'favoriteFruit': 'banana'
  },
  {
    '_id': '5ade0ab1cb24834128a67c1e',
    'index': 6,
    'guid': '0bb49326-3087-4f66-aa34-783c6cec5a35',
    'isActive': true,
    'balance': '$2,025.27',
    'picture': 'http://placehold.it/32x32',
    'age': 22,
    'eyeColor': 'brown',
    'name': 'Kerri Powers',
    'gender': 'female',
    'company': 'EVIDENDS',
    'email': 'kerripowers@evidends.com',
    'phone': '+1 (909) 597-3762',
    'address': '843 Danforth Street, Soham, Florida, 6737',
    'about': 'Enim esse minim non adipisicing amet. Esse sit voluptate esse exercitation amet incididunt enim. Excepteur cupidatat consectetur fugiat enim id aute duis velit sunt ut.\r\n',
    'registered': '2015-08-06T01:03:06 -02:00',
    'latitude': 77.902205,
    'longitude': 155.447677,
    'tags': [
      'sit',
      'duis',
      'sunt',
      'sint',
      'labore',
      'nulla',
      'velit'
    ],
    'friends': [
      {
        'id': 0,
        'name': 'Josephine Frederick'
      },
      {
        'id': 1,
        'name': 'Best Pittman'
      },
      {
        'id': 2,
        'name': 'Tiffany Hooper'
      }
    ],
    'greeting': 'Hello, Kerri Powers! You have 4 unread messages.',
    'favoriteFruit': 'strawberry'
  }
]";


            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] data = encoding.GetBytes(Json);

            // get hash (SHA1 IS LESS SECURE)
            var sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(data);

            // sign hash
            string SelfSigned2048_SHA256 = @"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";
            X509Certificate2 CertSelfSigned2048_SHA256 = new X509Certificate2(Convert.FromBase64String(SelfSigned2048_SHA256), "", X509KeyStorageFlags.PersistKeySet);
            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)CertSelfSigned2048_SHA256.PrivateKey;
            byte[] signature = csp.SignHash(hash, "SHA1");
            string signatureBase64 = Convert.ToBase64String(signature);
            Trace.WriteLine($"server: signature Base64 {signatureBase64}");


            Trace.WriteLine("client:  Verify signature... SHA1 IS LESS SECURE");


            UnicodeEncoding encodingClient = new UnicodeEncoding();
            byte[] dataClient = encodingClient.GetBytes(Json);
            byte[] signatureClient = Convert.FromBase64String(signatureBase64);

            // get hash
            var sha1Client = new SHA1Managed();
            byte[] hashClient = sha1Client.ComputeHash(dataClient);

            // verify the signature with the hash
            string SelfSigned2048_SHA256_Public = @"MIIDxjCCAq6gAwIBAgIQdPcZzh1mIbVHaqFLCasfHzANBgkqhkiG9w0BAQsFADBnMQ4wDAYDVQQHDAVCdWNoczERMA8GA1UECAwIU3RHYWxsZW4xCzAJBgNVBAYTAkNIMRQwEgYDVQQLDAtEZXZlbG9wbWVudDENMAsGA1UECgwEMnNpYzEQMA4GA1UEAwwHMnN4YyBDQTAeFw0xODA0MjAxNTA5MTNaFw0zOTEyMzEyMjAwMDBaMGcxDjAMBgNVBAcMBUJ1Y2hzMREwDwYDVQQIDAhTdEdhbGxlbjELMAkGA1UEBhMCQ0gxFDASBgNVBAsMC0RldmVsb3BtZW50MQ0wCwYDVQQKDAQyc2ljMRAwDgYDVQQDDAcyc3hjIENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAm1UGoVRxzd8juZzY1em03TiQL2zPHrmRci9/FGz/x2K4H+3LzlB9i5Z3C8N+9uzMiHlU0OTLBFAoFKWxqz908qq9HBVg6fyy35QtUHf4RfW0bkz8RiamkFuZKU9dFPudHlegAg6oy24O6/6ghVHsEtg6yy/n+1kiuq5gbCClQFxD4ps/oDNdQ989NsyZf+eLe1jKsiJeHm+KnCih2fvNaKQnHiJ5GHydKTiWN8UsOwIb78K8aHiSAeoyEkX7rvjKCIVn5mWrCOsRQ4IiTbfX830KrCrtsfvU35IVj/AGLCBCx0QVvND6RapsqaFx3v23kXL4CaX5aGoswfbP3RHcsQIDAQABo24wbDAOBgNVHQ8BAf8EBAMCAgQwEwYDVR0lBAwwCgYIKwYBBQUHAwMwEgYDVR0RBAswCYIHMnN4YyBDQTASBgNVHRMBAf8ECDAGAQH/AgEAMB0GA1UdDgQWBBT7kGSPYdOVUmzhp9vp1nJdCcfZGjANBgkqhkiG9w0BAQsFAAOCAQEAMSEVGEARn+2kFbLa9dOglgKhQ39ApjNeiV2AB2gwzXOABAOzS/DN5wOcfkPKt/rUgcnSx/sha8F5Jt8uDWE5fBgujndX1UwgOd3s6tHDo/V+Aze9hsh5ojOapaW7nMNyUe7dFZYgWz9lnM68Dm0YL356tVj+7mZdGIPmUR+hRzPWRlzwnYB3P9raTg1pTv0TwNantQSDvYr2PooMCL4pKFXQabRM/AscLfbgJLm3rmgdiXvstWUYDOdJUaN5Kati3kiFjpz6+xsIxnDS8DWbvHz7yqH01AprkjENIspyDBQ9OdhGricQnDQ03ndd5hHtevOfJrATdaSeuzAVyd0GFQ==";
            X509Certificate2 CertSelfSigned2048_SHA256_Public = new X509Certificate2(Convert.FromBase64String(SelfSigned2048_SHA256_Public), "");
            RSACryptoServiceProvider cspPublic = (RSACryptoServiceProvider)CertSelfSigned2048_SHA256_Public.PublicKey.Key;
            bool isValidSignature = cspPublic.VerifyHash(hashClient, "SHA1", signatureClient);
            Trace.WriteLine($"client:  Signature valid {isValidSignature}");

            Assert.AreEqual(true, isValidSignature);
        }
    }
}
