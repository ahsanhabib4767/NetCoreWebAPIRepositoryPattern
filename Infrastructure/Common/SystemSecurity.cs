using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RTGSWebApi.Infrastructure.Common
{
    public class SystemSecurity 
    {
        public static string MakeReadable(byte[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            string str = string.Empty;
            bool flag = true;
            int num1 = 0;
            foreach (short num2 in Enumerable.Select<byte, short>((IEnumerable<byte>)values, new Func<byte, short>(Convert.ToInt16)))
            {
                if (flag)
                {
                    //str = (string)(object)(int)num2 + (object)",";
                    str = num2.ToString() + (object)",";
                    flag = false;
                }
                else
                {
                    int num3 = checked(num1 - (int)num2);
                    str = str + (object)num3 + ",";
                }
                num1 = (int)num2;
            }
            if (str.Length > 1)
                str = str.Substring(0, checked(str.Length - 1));
            return str;
        }

        public static byte[] MakeReadable(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            string[] strArray = value.Split(',');
            byte[] numArray = new byte[strArray.Length];
            bool flag = true;
            int num1 = 0;
            int index = 0;
            foreach (string s in strArray)
            {
                int result;
                if (!int.TryParse(s, out result))
                    result = 0;
                int num2 = result;
                if (flag)
                {
                    numArray[index] = Convert.ToByte(result);
                    flag = false;
                }
                else
                {
                    num2 = checked(num1 - result);
                    numArray[index] = Convert.ToByte(num2);
                }
                num1 = num2;
                checked { ++index; }
            }
            return numArray;
        }

        public static string Encrypt(string password)
        {
            try
            {
                password = "Secret" + password;
                DESCryptoServiceProvider cryptoServiceProvider1 = new DESCryptoServiceProvider();
                cryptoServiceProvider1.Key = new byte[8]
                {
          (byte) 203,
          (byte) 139,
          (byte) 248,
          (byte) 99,
          (byte) 17,
          (byte) 143,
          (byte) 101,
          (byte) 94
                };
                cryptoServiceProvider1.IV = new byte[8]
                {
          (byte) 203,
          (byte) 139,
          (byte) 248,
          (byte) 99,
          (byte) 17,
          (byte) 143,
          (byte) 101,
          (byte) 94
                };
                DESCryptoServiceProvider cryptoServiceProvider2 = cryptoServiceProvider1;
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, cryptoServiceProvider2.CreateEncryptor(), CryptoStreamMode.Write);
                StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream);
                streamWriter.WriteLine(password);
                streamWriter.Close();
                cryptoStream.Close();
                byte[] values = memoryStream.ToArray();
                memoryStream.Close();
                return SystemSecurity.MakeReadable(values);
            }
            catch (Exception)
            {
                return password;
            }
        }

        public static string DecryptValue(string cipher)
        {
            return SystemSecurity.Decrypt(cipher).Substring(6);
        }

        public static string Decrypt(string cipher)
        {
            string str;
            try
            {
                byte[] buffer = SystemSecurity.MakeReadable(cipher);
                if (buffer.Length < 2)
                    return cipher;
                DESCryptoServiceProvider cryptoServiceProvider1 = new DESCryptoServiceProvider();
                cryptoServiceProvider1.Key = new byte[8]
                {
          (byte) 203,
          (byte) 139,
          (byte) 248,
          (byte) 99,
          (byte) 17,
          (byte) 143,
          (byte) 101,
          (byte) 94
                };
                cryptoServiceProvider1.IV = new byte[8]
                {
          (byte) 203,
          (byte) 139,
          (byte) 248,
          (byte) 99,
          (byte) 17,
          (byte) 143,
          (byte) 101,
          (byte) 94
                };
                DESCryptoServiceProvider cryptoServiceProvider2 = cryptoServiceProvider1;
                MemoryStream memoryStream = new MemoryStream(buffer);
                CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, cryptoServiceProvider2.CreateDecryptor(), CryptoStreamMode.Read);
                StreamReader streamReader = new StreamReader((Stream)cryptoStream);
                str = streamReader.ReadLine();
                streamReader.Close();
                cryptoStream.Close();
                memoryStream.Close();
            }
            catch (Exception)
            {
                str = cipher;
            }
            return str;
        }

        public static bool IsEncrypted(string cipher)
        {
            bool flag = false;
            try
            {
                string str = SystemSecurity.Decrypt(cipher);
                if (str.Length >= 6)
                {
                    if (str.Substring(0, 6) == "Secret")
                        flag = true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return flag;
        }
        
        public static string DesktopEncrypt(string password)
        {
            string seed = string.Empty;
            try
            {
                password = "Encrypted" + password;
                byte[] bytes = Encoding.Default.GetBytes(password);
                int index = 0;
                while (index < bytes.Length)
                {
                    int num = checked(Convert.ToInt32(bytes[index]) + 80);
                    if (num < 256)
                        bytes[index] = Convert.ToByte(num);
                    checked { ++index; }
                }
                return Enumerable.Aggregate<char, string>((IEnumerable<char>)Encoding.Unicode.GetChars(Encoding.Convert(Encoding.Default, Encoding.Unicode, bytes)), seed, (Func<string, char, string>)((current, loopChar) => current + loopChar.ToString()));
            }
            catch (Exception)
            {
                return password;
            }
        }

        public static string DesktopDecrypt(string cipher)
        {
            string seed = string.Empty;
            try
            {
                byte[] bytes = Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(cipher));
                int index = 0;
                while (index < bytes.Length)
                {
                    int num = Convert.ToInt32(bytes[index]);
                    if (num > 80)
                        bytes[index] = Convert.ToByte(checked(num - 80));
                    checked { ++index; }
                }
                return Enumerable.Aggregate<char, string>((IEnumerable<char>)Encoding.Default.GetChars(bytes), seed, (Func<string, char, string>)((current, loopChar) => current + loopChar.ToString()));
            }
            catch (Exception)
            {
                return cipher;
            }
        }
        
    }
}
