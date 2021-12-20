/*
Humans must learn to apply their intelligence correctly and evolve beyond their current state.
People must change. Otherwise, even if humanity expands into space, it will only create new
conflicts, and that would be a very sad thing. - Aeolia Schenberg, 2091 A.D.
　　　　 ,r‐､　　　　 　, -､
　 　 　 !　 ヽ　　 　 /　　}
　　　　 ヽ､ ,! -─‐- ､{　　ﾉ
　　　 　 ／｡　｡　　　 r`'､´
　　　　/ ,.-─- ､　　 ヽ､.ヽ　　　Haro
　　 　 !/　　　　ヽ､.＿, ﾆ|　　　　　Haro!
 　　　 {　　　 　  　 　 ,'
　　 　 ヽ　 　     　 ／,ｿ
　　　　　ヽ､.＿＿__r',／
*/

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace PSO2JP_Token_Generator
{
    internal class Program
    {
        public static string username;
        public static string password;

        //Pretty sure the OTP is *always* numbers but let's just play it safe because it -is- SEGA.
        public static string otp;

        public static string token;
        public static string userid;
        public static int ResponseCode;
        public static int OTPCodeTries = 5;

        private static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This application takes your username and password and generates a token for the Tweaker to start the game with.");
            Console.WriteLine("This will allow you to switch characters without needing to re-enter your username or password.");
            Console.Write("This program is open source and available at ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("http://github.com/Aida-Enna/PSO2JP_Token_Generator");
            Console.ForegroundColor = ConsoleColor.Yellow;
            //Aesthetic!
            Console.WriteLine(".");
            Console.Write("As with all sensitive data, ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("you should never share your username, password, OTP code, or token with ANYONE");
            Console.ForegroundColor = ConsoleColor.Yellow;
            //Aesthetic!
            Console.WriteLine(".");
            Console.ResetColor();
            TokenResponse SEGAResponse;
            do
            {
                //Get the user's info
                GetUserInfo();
                //Get the response from SEGA
                SEGAResponse = GetSEGAInfo();

                //Parse the response
                ResponseCode = SEGAResponse.Result;
                if (ResponseCode == 5002)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Incorrect username. Please enter your information again.");
                    Console.ResetColor();
                }
                else if (ResponseCode == 5001)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Incorrect password. Please enter your information again.");
                    Console.ResetColor();
                }
                else if (ResponseCode != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown error. Please try logging in again.");
                    Console.ResetColor();
                }
            } while (ResponseCode != 0);
            token = SEGAResponse.Token;
            userid = SEGAResponse.UserID;
            bool otpRequired = SEGAResponse.OTPRequired;
            if (otpRequired)
            {
                do
                {
                    if (OTPCodeTries == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("You have failed the OTP code 5 times. For your safety, this program will not allow you to try a 6th time.");
                        Console.WriteLine("Please wait 5 minutes and then try again. If you do not wait 5 minutes and you fail the OTP again, you will get locked out for up to an hour.");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to exit this program.");
                        Console.ReadLine();
                        return;
                    }
                    GetOTP();
                    //Get the OTP response from SEGA
                    SEGAResponse = GetSEGAInfo(true);
                    //Parse the response
                    ResponseCode = SEGAResponse.Result;
                    switch(ResponseCode){
						case 0: break;
						case 5002:
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Incorrect username. Please enter your information again.");
							Console.ResetColor();
							break;
						case 5001:
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Incorrect password. Please enter your information again.");
							Console.ResetColor();
							break;
						default:
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Unknown error ({0}). Please try logging in again.", ResponseCode);
							Console.ResetColor();
							break;
					}
                } while (ResponseCode != 0);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Logged in - Token generation complete. Passing info back to Tweaker...");
            Console.ResetColor();
            for (int countdown = 3; countdown > 0; countdown--)
            {
                Console.Write("\rContinuing in {0} seconds...   ", countdown);
                Thread.Sleep(1000);
            }
            Console.WriteLine("\rContinuing in 0 seconds...   ");
            Console.ResetColor();
            File.WriteAllText("temp.tmp", token + "|" + userid);
        }

        private static void GetUserInfo()
        {
            do
            {
                Console.WriteLine("Enter your SEGAID username:");
                username = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(username))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You must enter a valid username.");
                    Console.ResetColor();
                }
            } while (String.IsNullOrWhiteSpace(username));
            do
            {
                Console.WriteLine("Enter your SEGAID password:");
                //Set the password to nothing
				password = null; // Setting the password to an empty string before writing to it is redundant
				var passBuilder = new StringBuilder(); // Using a builder is more efficient than creating a new string object for each character
                ConsoleKey key;
                //This puts asteriks (*) on the console instead of your text, for security.
                do
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && passBuilder.Length > 0)
                    {
                        Console.Write("\b \b");
						passBuilder.Remove(passBuilder.Length - 1, 1);
					}
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
						passBuilder.Append(keyInfo.KeyChar);
                    }
                } while (key != ConsoleKey.Enter);

				password = passBuilder.ToString();
                if (String.IsNullOrWhiteSpace(password))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine("You must enter a valid password.");
                    Console.ResetColor();
                }
            } while (String.IsNullOrWhiteSpace(password));
            Console.WriteLine();
        }

        private static void GetOTP()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Your OTP (One Time Password) code will be valid for approximately 1 minute.");
            Console.WriteLine("If it is close to expiring, it's recommended to wait until a new one is generated.");
            Console.WriteLine("Your account will be locked if you fail to enter the correct code 6 times.");
            Console.WriteLine("Therefore, for your own safety, you will only be allowed to enter your OTP through this program 5 times.");
            Console.WriteLine("You have " + OTPCodeTries + " more attempt(s) to login via OTP.");
            Console.ResetColor();
            do
            {
                Console.WriteLine("Enter your SEGAID OTP (" + OTPCodeTries + " attempt(s) remaining):");
                otp = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(otp) || otp.Length != 6)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You must enter a valid OTP. (6 characters)");
                    Console.ResetColor();
                }
            } while (String.IsNullOrWhiteSpace(otp) || otp.Length != 6);
            Console.WriteLine();
            OTPCodeTries--;
        }

        private static TokenResponse GetSEGAInfo(bool DoingOTP = false)
        {
            string payload;
            HttpWebRequest TokenRequest = (HttpWebRequest)WebRequest.Create(DoingOTP ? "https://auth.pso2.jp/auth/v1/otpAuth" : "https://auth.pso2.jp/auth/v1/auth");
            if (DoingOTP)
            {
                CredentialsWithOTP otp_credentials = new CredentialsWithOTP
                {
                    userId = userid,
                    token = token,
                    otp = otp
                };
                payload = JsonConvert.SerializeObject(otp_credentials);
            }
            else
            {
                Credentials simple_credentials = new Credentials
                {
                    id = username,
                    password = password
                };
                payload = JsonConvert.SerializeObject(simple_credentials);
            }
            TokenRequest.ContentType = "application/json; charset=utf-8";
            TokenRequest.Host = "auth.pso2.jp";
            TokenRequest.UserAgent = "PSO2 Launcher";
            TokenRequest.Method = "POST";
            //Send SEGA your credentials
            using (var streamWriter = new StreamWriter(TokenRequest.GetRequestStream()))
            {
                streamWriter.Write(payload);
            }

            //Get the response from them
            var response = TokenRequest.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            using (var reader = new JsonTextReader(streamReader))
            {
                //Return the response back to the main code
                return JsonSerializer.CreateDefault().Deserialize<TokenResponse>(reader);
            }
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        private struct Credentials
        {
            [JsonProperty("id")]
            public string id;
            [JsonProperty("password")]
            public string password;
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        private struct CredentialsWithOTP
        {
            [JsonProperty("userId")]
            public string userId;
            [JsonProperty("token")]
            public string token;
            [JsonProperty("otp")]
            public string otp;
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        private struct TokenResponse
        {
            //The result of the login attempt
            [JsonProperty("result")]
            public int Result { get; set; }

            //Unique user ID
            [JsonProperty("userId")]
            public string UserID { get; set; }

            //Temporary token
            [JsonProperty("token")]
            public string Token { get; set; }

            //Whether we need to ask for OTP
            [JsonProperty("otpRequired")]
            public bool OTPRequired { get; set; }
        }
    }
}
