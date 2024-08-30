using System;
using UnityEngine;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace com.playbux.api
{
    public class TokenUtility
    {
        // Start is called before the first frame update
        public static string accessTokenKey = "accessTokenKey";
        public static string refreshTokenKey = "refreshTokenKey";
        public static string expiryAccessTokenDateKey = "expiryAccessTokenDateKey";
        public static string expiryRefeshTokenDateKey = "expiryRefeshTokenDateKey";
        public static double dateToExpire = 3;
        public static string _id = "_id";

        public static void SetToken(string accessToken, string refreshToken, DateTime expireAccessTokenDate, DateTime expireRefeshTokenDate)
        {
            PlayerPrefs.SetString(refreshTokenKey, refreshToken);
            PlayerPrefs.SetString(expiryRefeshTokenDateKey, expireRefeshTokenDate.ToString("yyyy-MM-dd HH:mm:ss"));
            SetAccessToken(accessToken, expireAccessTokenDate);
        }
        public static void SetToken(string accessToken, string refreshToken)
        {

            AccessTokenData accessTokenData = new AccessTokenData();
            accessTokenData = JsonConvert.DeserializeObject<AccessTokenData>(Decode(accessToken));

            RefreshTokenData refreshTokenData = new RefreshTokenData();
            refreshTokenData = JsonConvert.DeserializeObject<RefreshTokenData>(Decode(refreshToken));

            DateTime dateTimeAccessToken = SetDateTime(accessTokenData.exp);
            DateTime dateTimeRefreshToken = SetDateTime(refreshTokenData.exp);

            SetToken(accessToken, refreshToken, dateTimeAccessToken, dateTimeRefreshToken);

        }


        public static void SetAccessToken(string accessToken, DateTime expireAccessTokenDate)
        {
            PlayerPrefs.SetString(accessTokenKey, accessToken);
            PlayerPrefs.SetString(expiryAccessTokenDateKey, expireAccessTokenDate.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static void SetNewAccessTokenData(string accessToken)
        {
            AccessTokenData accessTokenData = new AccessTokenData();
            accessTokenData = JsonConvert.DeserializeObject<AccessTokenData>(Decode(accessToken));
            var newAccessTokenDate = SetDateTime(accessTokenData.exp);
            SetAccessToken(accessToken, newAccessTokenDate);
        }

        public static (string access, string refresh) GetToken()
        {
            if (!PlayerPrefs.HasKey(expiryAccessTokenDateKey))
            {
                if (DateTime.UtcNow > GetExpiryDate(expiryRefeshTokenDateKey))
                {
                    return (null, PlayerPrefs.GetString(refreshTokenKey));
                }
                return (null, null);
            }



            if (DateTime.UtcNow > GetExpiryDate(expiryAccessTokenDateKey))
            {
                if (!PlayerPrefs.HasKey(expiryRefeshTokenDateKey))
                {
                    return (null, null);
                }
                if (DateTime.UtcNow > GetExpiryDate(expiryRefeshTokenDateKey))
                {
                    return (null, null);
                }
                return (null, PlayerPrefs.GetString(refreshTokenKey));
            }
            return (PlayerPrefs.GetString(accessTokenKey), PlayerPrefs.GetString(refreshTokenKey));
        }

        public static async UniTask<(string access, string refresh)> GetToken(string username, string password)
        {

            AuthResponse authResponse = new AuthResponse();

            authResponse = await APIServerConnector.LogIn(username, password);

            if (authResponse == null)
            {

                return (null, null);
            }

            string accessToken = authResponse.accessToken;
            string refreshToken = authResponse.refreshToken;

            AccessTokenData accessTokenData = new AccessTokenData();
            accessTokenData = JsonConvert.DeserializeObject<AccessTokenData>(Decode(accessToken));

            RefreshTokenData refreshTokenData = new RefreshTokenData();
            refreshTokenData = JsonConvert.DeserializeObject<RefreshTokenData>(Decode(refreshToken));

            DateTime dateTimeAccessToken = SetDateTime(accessTokenData.exp);
            DateTime dateTimeRefreshToken = SetDateTime(refreshTokenData.exp);

            SetToken(accessToken, refreshToken, dateTimeAccessToken, dateTimeRefreshToken);
            return (accessToken, refreshToken);
        }




        public static async UniTask<(string access, string refresh)> GetToken(string refreshToken)
        {
            var accessTokenResponse = await APIServerConnector.RefreshTokenAPI(refreshToken);

            if (accessTokenResponse == null)
            {

                return (null, null);
            }

            return (accessTokenResponse.accessToken, refreshToken);
        }

        public static DateTime GetExpiryDate(string expireDateToken)
        {
            string expiryDateString = PlayerPrefs.GetString(expireDateToken);
            DateTime expiryDate;

            if (DateTime.TryParseExact(expiryDateString, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out expiryDate))
            {
                return expiryDate;
            }
            else
            {

                return DateTime.MinValue;
            }
        }

        public static DateTime SetDateTime(long exp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(exp);
            DateTime dateTimeToken = dateTimeOffset.DateTime;
            return dateTimeToken;
        }

        public static string Decode(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var payload = jsonToken?.Payload.SerializeToJson();
            return payload;
        }

    }

    public class AccessTokenData //exp 30min
    {
        public string uid { get; set; }
        public string session { get; set; }
        public long iat { get; set; }
        public long exp { get; set; }
    }


    public class RefreshTokenData //exp 7day
    {
        public string uid { get; set; }
        public long iat { get; set; }
        public long exp { get; set; }
    }
}
