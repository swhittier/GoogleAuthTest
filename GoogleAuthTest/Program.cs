﻿using System;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace GoogleAuthTest
{
    class AuthenticatorTest
    {
        #region Private Fields

        /// <summary>
        /// The shared secret used by the HOTP and TOTP algorithms
        /// when validating a verification code.
        /// </summary>
        private readonly string _sharedSecret = null;

        /// <summary>
        /// The Label used by the Google Authenticator App when setting 
        /// up an account using a QR Code.
        /// </summary>
        private const string APP_LABEL = "MAX Exchange";

        #endregion

        #region Main Entry Point

        static void Main(string[] args)
        {
            AuthenticatorTest at = new AuthenticatorTest();
            at.Start();
        }

        #endregion

        #region Constructor

        public AuthenticatorTest()
        {            
            // Initialize the shared secret.

            // This demonstrates how to generate a shared secret.
            // Typically, once one is generated, it will be stored in 
            // a database for future use.            
            _sharedSecret = GenerateSharedSecret(); //"pI7uJmILHf";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Where it all begins.
        /// </summary>
        public void Start()
        {
            string timeBasedKey = DisplaySecretAndTimeBasedKey();

            GenerateQRCode(timeBasedKey);

            string verificationCode = PromptForVerificationCode();

            PerformAuthentication(verificationCode);
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Generates a 10 character string of A-Z, a-z, 0-9
        /// Randomized using the RandomNumberGenerator.
        /// </summary>
        /// <returns></returns>
        private string GenerateSharedSecret()
        {
            byte[] buffer = new byte[9];

            using (RandomNumberGenerator rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetBytes(buffer);
            }

            // Don't need to worry about any = padding from the
            // Base64 encoding, since our input buffer is divisible by 3
            string sharedSecret = Convert.ToBase64String(buffer).Substring(0, 10).Replace('/', '0').Replace('+', '1');

            return sharedSecret;
        }

        /// <summary>
        /// Displays the shared secret to the user using the Base32Encoder.
        /// </summary>
        /// <remarks>This is what is to be entered into the Google Authenticator app as the KEY 
        /// under Manual Account Entry.  Alternatively, a QR code can be generated.</remarks>
        private string DisplaySecretAndTimeBasedKey()
        {
            Console.WriteLine("This is the raw, unencoded shared secret:");
            
            //
            // Display Shared Secret.
            //
            Console.WriteLine();
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_sharedSecret);
            Console.ForegroundColor = currentColor;

            Console.WriteLine();

            Base32Encoder enc = new Base32Encoder();

            string timeBasedKey = enc.Encode(Encoding.ASCII.GetBytes(_sharedSecret));

            Console.WriteLine("Create a new account in the Google Authenticator app,");
            Console.WriteLine("then enter the following as the time based key:");

            //
            // Display Time Based Key (Base32 encoded version of the Shared Secret)
            //
            Console.WriteLine();
            currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(timeBasedKey);
            Console.ForegroundColor = currentColor;

            Console.WriteLine();
            
            return timeBasedKey;            
        }

        private void GenerateQRCode(string timeBasedKey)
        {
            var qrcode = new QRCodeWriter();
            var qrData = string.Format("otpauth://totp/{0}?secret={1}", APP_LABEL, timeBasedKey);

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1
                }
            };

            using (var bitmap = barcodeWriter.Write(qrData))            
            {
                bitmap.Save("QRCode.png", ImageFormat.Png);                
            }
        }

        /// <summary>
        /// Prompts the user to enter the time based code displayed in the Google Authenticator app.
        /// </summary>
        /// <returns></returns>
        private string PromptForVerificationCode()
        {
            Console.WriteLine("Enter your verification code: ");

            string verificationCode = Console.ReadLine();

            return verificationCode;
        }

        /// <summary>
        /// Uses the authenticator to check validity of the entered verification code
        /// using the current shared secret.
        /// </summary>
        /// <param name="verificationCode"></param>
        private void PerformAuthentication(string verificationCode)
        {
            var authenticator = new Authenticator();

            if (authenticator.IsValid(_sharedSecret, verificationCode))
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("ERROR!");
            }

            Console.ReadLine();
        }

        #endregion
    }
}
