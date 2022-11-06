Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Windows.Forms




Public Class Crypto
    'declare service provider
    Private Shared DES As New TripleDESCryptoServiceProvider
    Private Shared MD5 As New MD5CryptoServiceProvider

    Public Shared Function MD5Hash(ByVal value As String) As Byte()
        Return MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(value))
    End Function

    'encrypt text
    Public Shared Function Encrypt(ByVal stringToEncrypt As String, ByVal key As String) As String

        DES.Key = Crypto.MD5Hash(key)
        DES.Mode = CipherMode.ECB
        Dim Buffer As Byte() = ASCIIEncoding.ASCII.GetBytes(stringToEncrypt)
        Return Convert.ToBase64String(DES.CreateEncryptor().TransformFinalBlock(Buffer, 0, Buffer.Length))


    End Function

    'decrypt text
    Public Shared Function Decrypt(ByVal encryptedString As String, ByVal key As String) As String

        Try

            DES.Key = Crypto.MD5Hash(key)
            DES.Mode = CipherMode.ECB
            Dim buffer As Byte() = Convert.FromBase64String(encryptedString)
            Return ASCIIEncoding.ASCII.GetString(DES.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length))

        Catch ex As Exception
            MessageBox.Show("Invalid Key Entered!", "Impossible to decrypt!", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try




    End Function





End Class



