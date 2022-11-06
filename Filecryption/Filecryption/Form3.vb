Imports System
Imports System.IO
Imports System.Net
Imports System.Drawing
Imports System.Security
Imports System.Security.Cryptography
Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports ClassLibrary1



Public Class Form3

    Dim connection As New SqlConnection("Server= localhost; Database = LogDetails; Integrated Security = true")


    'Identify the Global Variables
    'for lock folder
    Public status As String
    Private arr As String() = New String(5) {} 'lock folder
    'for encrypt and decrypt, file
    Dim strFileToEncrypt As String
    Dim strFileToDecrypt As String
    Dim strOutputEncrypt As String
    Dim strOutputDecrypt As String
    Dim fsInput As System.IO.FileStream
    Dim fsOutput As System.IO.FileStream

    Dim Level1Output As String
    Dim Level2Output As String
    Dim Level3Output As String



    'Create Key
    Private Function CreateKey(ByVal strPassword As String) As Byte()
        'Convert strPassword to an array and store in chrData.
        Dim chrData() As Char = strPassword.ToCharArray
        'Use intLength to get strPassword size.
        Dim intLength As Integer = chrData.GetUpperBound(0)
        'Declare bytDataToHash and make it the same size as chrData.
        Dim bytDataToHash(intLength) As Byte

        'Use For Next to convert and store chrData into bytDataToHash.
        For i As Integer = 0 To chrData.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chrData(i)))
        Next

        'Declare what hash to use.
        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        'Declare bytResult, Hash bytDataToHash and store it in bytResult.
        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)
        'Declare bytKey(31).  It will hold 256 bits.
        Dim bytKey(31) As Byte

        'Use For Next to put a specific size (256 bits) of 
        'bytResult into bytKey. The 0 To 31 will put the first 256 bits
        'of 512 bits into bytKey.
        For i As Integer = 0 To 31
            bytKey(i) = bytResult(i)
        Next

        Return bytKey 'Return the key.
    End Function

    'Create IV

    Private Function CreateIV(ByVal strPassword As String) As Byte()
        'Convert strPassword to an array and store in chrData.
        Dim chrData() As Char = strPassword.ToCharArray
        'Use intLength to get strPassword size.
        Dim intLength As Integer = chrData.GetUpperBound(0)
        'Declare bytDataToHash and make it the same size as chrData.
        Dim bytDataToHash(intLength) As Byte

        'Use For Next to convert and store chrData into bytDataToHash.
        For i As Integer = 0 To chrData.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chrData(i)))
        Next

        'Declare what hash to use.
        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        'Declare bytResult, Hash bytDataToHash and store it in bytResult.
        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)
        'Declare bytIV(15).  It will hold 128 bits.
        Dim bytIV(15) As Byte

        'Use For Next to put a specific size (128 bits) of 
        'bytResult into bytIV. The 0 To 30 for bytKey used the first 256 bits.
        'of the hashed password. The 32 To 47 will put the next 128 bits into bytIV.
        For i As Integer = 32 To 47
            bytIV(i - 32) = bytResult(i)
        Next

        Return bytIV 'return the IV
    End Function


    'Encrypt and Decrypt File

    Private Enum CryptoAct
        'Define the enumeration for CryptoAct
        ActionEncrypt = 1
        ActionDecrypt = 2



    End Enum

    Private Sub EncryptFile(ByVal strInputFile As String,
                                     ByVal strOutputFile As String,
                                     ByVal bytKey() As Byte,
                                     ByVal bytIV() As Byte,
                                     ByVal Algorithm As String)

        'Try 'In case of errors.

        'Setup file streams to handle input and output.
        fsInput = New System.IO.FileStream(strInputFile, FileMode.Open,
                                               FileAccess.Read)
        fsOutput = New System.IO.FileStream(strOutputFile, FileMode.OpenOrCreate,
                                            FileAccess.Write)
        fsOutput.SetLength(0) 'make sure fsOutput is empty

        'Declare variables for encrypt/decrypt process.
        Dim bytBuffer(4096) As Byte 'holds a block of bytes for processing
        Dim lngBytesProcessed As Long = 0 'running count of bytes processed
        Dim lngFileLength As Long = fsInput.Length 'the input file's length
        Dim intBytesInCurrentBlock As Integer 'current bytes being processed
        Dim csCryptoStream As CryptoStream
        'Declare encrypt CryptoServiceProvider.
        Dim cspAES As New System.Security.Cryptography.AesManaged
        Dim cspRidjdeal As New System.Security.Cryptography.RijndaelManaged





        'Determine encryption and setup CryptoStream.
        If Algorithm = "Method 1" Then
            csCryptoStream = New CryptoStream(fsOutput, cspAES.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
        ElseIf Algorithm = "Method 2" Then
            csCryptoStream = New CryptoStream(fsOutput, cspRidjdeal.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
        End If

        'Use While to loop until all of the file is processed.
        While lngBytesProcessed < lngFileLength
            'Read file with the input filestream.
            intBytesInCurrentBlock = fsInput.Read(bytBuffer, 0, 4096)
            'Write output file with the cryptostream.
            csCryptoStream.Write(bytBuffer, 0, intBytesInCurrentBlock)
            'Update lngBytesProcessed
            lngBytesProcessed = lngBytesProcessed + CLng(intBytesInCurrentBlock)

        End While

        'Close FileStreams and CryptoStream.
        csCryptoStream.Close()
        fsInput.Close()
        fsOutput.Close()

        'If encrypting then delete the original unencrypted file.

        Dim fileOriginal As New FileInfo(strFileToEncrypt)
        fileOriginal.Delete()



        'Update the user when the file is done.
        Dim Wrap As String = Chr(13) + Chr(10)
        'If Direction = CryptoAct.ActionEncrypt Then
        MsgBox("Encryption Complete" + Wrap + Wrap +
                    "Total bytes processed = " +
                    lngBytesProcessed.ToString,
                    MsgBoxStyle.Information, "Done")


        'Update the textboxes of encrypted file

        TextEncryptFile.Text = "Click Browse to load file."
        TextDestinatedEncrypt.Text = ""
        TextBox2.Text = ""
        ButtonChangeEncryptFile.Enabled = False
        ButtonEncryptFile.Enabled = False



    End Sub

    Private Sub DecryptFile(ByVal strInputFile As String,
                                     ByVal strOutputFile As String,
                                     ByVal bytKey() As Byte,
                                     ByVal bytIV() As Byte,
                                     ByVal Algorithm As String)

        Try 'In case of errors.

            'Setup file streams to handle input and output.
            fsInput = New System.IO.FileStream(strInputFile, FileMode.Open,
                                               FileAccess.Read)
            fsOutput = New System.IO.FileStream(strOutputFile, FileMode.OpenOrCreate,
                                                FileAccess.Write)
            fsOutput.SetLength(0) 'make sure fsOutput is empty

            'Declare variables for decrypt process.
            Dim bytBuffer(4096) As Byte 'holds a block of bytes for processing
            Dim lngBytesProcessed As Long = 0 'running count of bytes processed
            Dim lngFileLength As Long = fsInput.Length 'the input file's length
            Dim intBytesInCurrentBlock As Integer 'current bytes being processed
            Dim csCryptoStream As CryptoStream
            'Declare CryptoServiceProvider.
            Dim cspAES As New System.Security.Cryptography.AesManaged
            Dim cspRidjdeal As New System.Security.Cryptography.RijndaelManaged



            'Determine decryption and setup CryptoStream.
            If Algorithm = "Method 1" Then
                csCryptoStream = New CryptoStream(fsOutput, cspAES.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
            ElseIf Algorithm = "Method 2" Then
                csCryptoStream = New CryptoStream(fsOutput, cspRidjdeal.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
            End If

            'Use While to loop until all of the file is processed.
            While lngBytesProcessed < lngFileLength
                'Read file with the input filestream.
                intBytesInCurrentBlock = fsInput.Read(bytBuffer, 0, 4096)
                'Write output file with the cryptostream.
                csCryptoStream.Write(bytBuffer, 0, intBytesInCurrentBlock)
                'Update lngBytesProcessed
                lngBytesProcessed = lngBytesProcessed + CLng(intBytesInCurrentBlock)

            End While

            'Close FileStreams and CryptoStream.
            csCryptoStream.Close()
            fsInput.Close()
            fsOutput.Close()

            'If decrypting then delete the encrypted file.
            Dim fileEncrypted As New FileInfo(strFileToDecrypt)
            fileEncrypted.Delete()

            'Update the user when the file is done.
            Dim Wrap As String = Chr(13) + Chr(10)

            'Update the user when the file is done.
            MsgBox("Decryption Complete" + Wrap + Wrap +
                       "Total bytes processed = " +
                        lngBytesProcessed.ToString,
                        MsgBoxStyle.Information, "Done")

            'Update the progress bar and textboxes.

            TextDecryptFile.Text = "Click Browse to load file."
            TextDestinatedFile2.Text = "Click Browse to load file."
            TextBox8.Text = ""
            ButtonChangeDecryptFile.Enabled = False
            ButtonDecryptFile.Enabled = False



            'Catch file not found error.
        Catch When Err.Number = 53 'if file not found
            MsgBox("Please check to make sure the path and filename" +
                    "are correct and if the file exists.",
                     MsgBoxStyle.Exclamation, "Invalid Path or Filename")

            'Catch all other errors. And delete partial files.
        Catch
            fsInput.Close()
            fsOutput.Close()

            Dim fileDelete As New FileInfo(TextDecryptFile.Text)
            fileDelete.Delete()
            TextBox8.Text = ""


            MsgBox("Please check to make sure that you entered the correct" +
                    "password.", MsgBoxStyle.Exclamation, "Invalid Password")

        End Try
    End Sub

    Private Sub Folder_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the 'LogDetailsDataSet6.DecDetails' table. You can move, or remove it, as needed.
        Me.DecDetailsTableAdapter3.Fill(Me.LogDetailsDataSet6.DecDetails)
        'TODO: This line of code loads data into the 'LogDetailsDataSet5.EncDetails' table. You can move, or remove it, as needed.
        Me.EncDetailsTableAdapter2.Fill(Me.LogDetailsDataSet5.EncDetails)
        status = ""

        arr(0) = ".{2559a1f2-21d7-11d4-bdaf-00c04f60b9f0}"

        arr(1) = ".{21EC2020-3AEA-1069-A2DD-08002B30309D}"

        arr(2) = ".{2559a1f4-21d7-11d4-bdaf-00c04f60b9f0}"

        arr(3) = ".{645FF040-5081-101B-9F08-00AA002F954E}"

        arr(4) = ".{2559a1f1-21d7-11d4-bdaf-00c04f60b9f0}"

        arr(5) = ".{7007ACC7-3202-11D1-AAD2-00805FC1270E}"


    End Sub



    Private Sub HelpToolStripMenuItem_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub


    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        'Hide Key/Pass

        If TextBox2.UseSystemPasswordChar = True Then

            TextBox2.UseSystemPasswordChar = False

        Else

            TextBox2.UseSystemPasswordChar = True

        End If

    End Sub


    Private Sub Button4_Click(ByVal sender As System.Object,
                                           ByVal e As System.EventArgs) _
                                           Handles Button4.Click
        'Setup open dialog
        OpenFileDialog1.FileName = ""
        OpenFileDialog1.Title = "Choose a file to encrypt"
        OpenFileDialog1.InitialDirectory = "C:\"
        OpenFileDialog1.Filter = "All Files (*.*) | *.*"

        'Find out on whether the user has chosen a file.
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            strFileToEncrypt = OpenFileDialog1.FileName
            TextEncryptFile.Text = strFileToEncrypt

            Dim iPosition As Integer = 0
            Dim i As Integer = 0


            While strFileToEncrypt.IndexOf("\"c, i) <> -1
                iPosition = strFileToEncrypt.IndexOf("\"c, i)
                i = iPosition + 1
            End While


            strOutputEncrypt = strFileToEncrypt.Substring(iPosition + 1)

            Dim S As String = strFileToEncrypt.Substring(0, iPosition + 1)

            strOutputEncrypt = strOutputEncrypt.Replace("."c, "_"c)
            'The final file name that will appear after encrypted filename.encrypt
            TextDestinatedEncrypt.Text = S + strOutputEncrypt + ".encrypt"
            Level1Output = S + strOutputEncrypt + ".encrypt1"
            Level2Output = S + strOutputEncrypt + ".encrypt2"
            Level3Output = S + strOutputEncrypt + ".encrypt3"


            'Update buttons where when user has already chosen a file, it automatically update the destinated file similar to the original path file.
            ButtonEncryptFile.Enabled = True
            ButtonChangeEncryptFile.Enabled = True

        End If

    End Sub


    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        If TextBox8.UseSystemPasswordChar = True Then

            TextBox8.UseSystemPasswordChar = False

        Else

            TextBox8.UseSystemPasswordChar = True

        End If
    End Sub

    Private Sub Button17_Click(sender As Object, e As EventArgs)
        If TextBox9.UseSystemPasswordChar = True Then

            TextBox9.UseSystemPasswordChar = False

        Else

            TextBox9.UseSystemPasswordChar = True

        End If
    End Sub


    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        TextEncryptFile.Text = ""
        TextDestinatedEncrypt.Text = ""
        TextBox2.Text = ""
        ComboBox1.Text = ""
        ComboBox2.Text = ""
        ComboBox3.Text = ""


    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        TextDecryptFile.Text = ""
        TextDestinatedFile2.Text = ""
        TextBox8.Text = ""
        ComboBox7.Text = ""
        ComboBox8.Text = ""
        ComboBox9.Text = ""
    End Sub

    Private Sub FeaturesToolStripMenuItem_Click(sender As Object, e As EventArgs)
        MsgBox("Encrypt File: A simple file that is required to be unseen by outsiders", MsgBoxStyle.Information, "About Filecryption")

    End Sub



    Private Sub ButtonChangeEncrypt_Click(ByVal sender As System.Object,
                                       ByVal e As System.EventArgs) _
                                       Handles ButtonChangeEncryptFile.Click
        'Setup up folder browser.
        FolderBrowserDialog1.Description = "Select a folder to place the encrypted file in."
        'If the user selected a folder assign the path to txtDestinationEncrypt.
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            TextDestinatedEncrypt.Text = FolderBrowserDialog1.SelectedPath +
                                         "\" + strOutputEncrypt + ".encrypt"
        End If
    End Sub


    'Click on one drive image which will link to its official website
    Private Sub PictureBox2_Click(sender As Object, e As EventArgs) Handles OneDrivePic.Click
        Process.Start("https://onedrive.live.com/about/en-us/")
    End Sub

    'Click on Google drive Image which will link to its official website
    Private Sub PictureBox4_Click(sender As Object, e As EventArgs) Handles GoogleDrivePic.Click
        Process.Start("https://www.google.com/drive/")
    End Sub

    'Click on Drop Box image which will link to its official website
    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles DropboxPic.Click
        Process.Start("https://www.dropbox.com/login")
    End Sub


    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        Dim key As String
        key = TextKey.Text

        TextBoxDecrypt.Text = Crypto.Encrypt(TextBoxEncrypt.Text, key)
        TextBoxEncrypt.Clear()


    End Sub

    Private Sub Label31_Click(sender As Object, e As EventArgs) Handles Label31.Click

    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click

        Dim key As String
        key = TextKey.Text

        TextBoxEncrypt.Text = Crypto.Decrypt(TextBoxDecrypt.Text, key)
        TextBoxDecrypt.Clear()

    End Sub

    Private Sub ButtonDecryptFile_Click(ByVal sender As System.Object,
                                       ByVal e As System.EventArgs) _
                                       Handles ButtonDecryptFile.Click

        'Setup the open dialog.
        OpenFileDialog1.FileName = ""
        OpenFileDialog1.Title = "Choose a file to decrypt"
        OpenFileDialog1.InitialDirectory = "C:\"
        OpenFileDialog1.Filter = "Encrypted Files (*.encrypt, *.encrypt1, *.encrypt2, *.encrypt3) | *.encrypt;*.encrypt1;*.encrypt2;*.encrypt3"


        'Find out if the user chose a file.
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            strFileToDecrypt = OpenFileDialog1.FileName
            TextDecryptFile.Text = strFileToDecrypt
            Dim iPosition As Integer = 0
            Dim i As Integer = 0
            'Get the position of the last "\" in the OpenFileDialog.FileName path.
            '-1 is when the character your searching for is not there.
            'IndexOf searches from left to right.

            While strFileToDecrypt.IndexOf("\"c, i) <> -1
                iPosition = strFileToDecrypt.IndexOf("\"c, i)
                i = iPosition + 1
            End While

            'strOutputFile = the file path minus the last 8 characters (.encrypt)
            strOutputDecrypt = strFileToDecrypt.Substring(0, strFileToDecrypt.Length - 8)
            'Assign S the entire path, ending at the last "\".
            Dim S As String = strFileToDecrypt.Substring(0, iPosition + 1)
            'Assign strOutputFile to the position after the last "\" in the path.
            strOutputDecrypt = strOutputDecrypt.Substring((iPosition + 1))
            'Replace "_" with "."
            TextDestinatedFile2.Text = S + strOutputDecrypt.Replace("_"c, "."c)
            Level1Output = S + strOutputDecrypt.Replace("_"c, "."c)
            Level2Output = S + strOutputDecrypt.Replace("_"c, "."c)
            Level3Output = S + strOutputDecrypt.Replace("_"c, "."c)
            'Update buttons
            Button14.Enabled = True
            ButtonChangeDecryptFile.Enabled = True

        End If
    End Sub


    Private Sub ButtonChangeDecryptFile_Click(ByVal sender As System.Object,
                                       ByVal e As System.EventArgs) _
                                       Handles ButtonChangeDecryptFile.Click
        'Setup up folder browser.
        FolderBrowserDialog1.Description = "Select a folder for to place the decrypted file in."
        'If the user selected a folder assign the path to txtDestinationDecrypt.
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            TextDestinatedFile2.Text = FolderBrowserDialog1.SelectedPath +
                                         "\" + strOutputDecrypt.Replace("_"c, "."c)
        End If
    End Sub



    Private Sub ButtonEncryptFile_Click(ByVal sender As System.Object,
                                 ByVal e As System.EventArgs) _
                                 Handles ButtonEncryptFile.Click

        'allow the values of encrypt details that user inserted in the system will be stored in the database
        Dim insertQuery As String = "INSERT INTO EncDetails(fileEncName,DestinatedEncFile,Encryption1,Encryption2,Encryption3,DateCreated) VALUES ('" & TextEncryptFile.Text & "','" & TextDestinatedEncrypt.Text & "','" & ComboBox1.Text & "','" & ComboBox2.Text & "','" & ComboBox3.Text & "', '" + DateTimePicker1.Value.Date + "')"
        ExecuteQuery(insertQuery)

        'Make sure the password is correct.
        If String.IsNullOrEmpty(TextBox2.Text) Then
            MsgBox("Please re-enter your password.", MsgBoxStyle.Exclamation)
            TextBox2.Text = ""
        Else
            'Declare variables for the key and iv.
            'The key needs to hold 256 bits and the iv 128 bits.
            Dim bytKey As Byte()
            Dim bytIV As Byte()
            'Send the password to the CreateKey function.
            bytKey = CreateKey(TextBox2.Text)
            'Send the password to the CreateIV function.
            bytIV = CreateIV(TextBox2.Text)
            'Start the encryption.
            EncryptOrDecryptFile(strFileToEncrypt, TextDestinatedEncrypt.Text, _
                                 bytKey, bytIV, CryptoAction.ActionEncrypt)

            'Send the password to the CreateKey function.
            bytKey = CreateKey(ComboBox1.Text + "_" + ComboBox2.Text + "_" + ComboBox3.Text)
            'Send the password to the CreateIV function.
            bytIV = CreateIV(ComboBox1.Text + "_" + ComboBox2.Text + "_" + ComboBox3.Text)
            EncryptOrDecryptFile(TextDestinatedEncrypt.Text, TextDestinatedEncrypt.Text + "3", _
                                 bytKey, bytIV, CryptoAction.ActionEncrypt)
            File.Delete(TextDestinatedEncrypt.Text)
        End If


        'Declare variables for the key and iv.
        'The key needs to hold 256 bits and the iv 128 bits.
        'Dim bytKey As Byte()
        'Dim bytIV As Byte()

        ''Send the password to the CreateKey function.
        'bytKey = CreateKey(TextBox2.Text)
        ''Send the password to the CreateIV function.
        'bytIV = CreateIV(TextBox2.Text)


        ''Start the encryption.




        'If ComboBox1.Text = "Method 1" Then 'Level 1 encryption Method 1
        '    EncryptFile(strFileToEncrypt, Level1Output, bytKey, bytIV, "Method 1")

        'ElseIf ComboBox1.Text = "Method 2" Then 'Level 1 encryption Method 2

        '    EncryptFile(strFileToEncrypt, Level1Output, bytKey, bytIV, "Method 2")

        'End If

        ''if statement encryption level 2
        'If ComboBox2.Text = "Method 1" Then 'Level 2 Encryption Method 1
        '    EncryptFile(Level1Output, Level2Output, bytKey, bytIV, "Method 1")

        'ElseIf ComboBox2.Text = "Method 2" Then 'Level 2 Encryption Method 2
        '    EncryptFile(Level1Output, Level2Output, bytKey, bytIV, "Method 2")

        'End If

        ''if statement encryption level 3
        'If ComboBox3.Text = "Method 1" Then  'Level 3 encryption Method 1
        '    EncryptFile(Level2Output, Level3Output, bytKey, bytIV, "Method 1")

        'ElseIf ComboBox3.Text = "Method 2" Then 'Level 3 encryption Method 2
        '    EncryptFile(Level2Output, Level3Output, bytKey, bytIV, "Method 2")

        'End If

    End Sub

#Region "4. Encrypt / Decrypt File "

    '****************************
    '** Encrypt/Decrypt File
    '****************************

    Private Enum CryptoAction
        'Define the enumeration for CryptoAction.
        ActionEncrypt = 1
        ActionDecrypt = 2
    End Enum

    Private Sub EncryptOrDecryptFile(ByVal strInputFile As String, _
                                     ByVal strOutputFile As String, _
                                     ByVal bytKey() As Byte, _
                                     ByVal bytIV() As Byte, _
                                     ByVal Direction As CryptoAction)
        Try 'In case of errors.

            'Setup file streams to handle input and output.
            fsInput = New System.IO.FileStream(strInputFile, FileMode.Open, _
                                               FileAccess.Read)
            fsOutput = New System.IO.FileStream(strOutputFile, FileMode.OpenOrCreate, _
                                                FileAccess.Write)
            fsOutput.SetLength(0) 'make sure fsOutput is empty

            'Declare variables for encrypt/decrypt process.
            Dim bytBuffer(4096) As Byte 'holds a block of bytes for processing
            Dim lngBytesProcessed As Long = 0 'running count of bytes processed
            Dim lngFileLength As Long = fsInput.Length 'the input file's length
            Dim intBytesInCurrentBlock As Integer 'current bytes being processed
            Dim csCryptoStream As CryptoStream
            'Declare your CryptoServiceProvider.
            Dim cspRijndael As New System.Security.Cryptography.RijndaelManaged

            'Determine if ecryption or decryption and setup CryptoStream.
            Select Case Direction
                Case CryptoAction.ActionEncrypt
                    csCryptoStream = New CryptoStream(fsOutput, _
                    cspRijndael.CreateEncryptor(bytKey, bytIV), _
                    CryptoStreamMode.Write)

                Case CryptoAction.ActionDecrypt
                    csCryptoStream = New CryptoStream(fsOutput, _
                    cspRijndael.CreateDecryptor(bytKey, bytIV), _
                    CryptoStreamMode.Write)
            End Select

            'Use While to loop until all of the file is processed.
            While lngBytesProcessed < lngFileLength
                'Read file with the input filestream.
                intBytesInCurrentBlock = fsInput.Read(bytBuffer, 0, 4096)
                'Write output file with the cryptostream.
                csCryptoStream.Write(bytBuffer, 0, intBytesInCurrentBlock)
                'Update lngBytesProcessed
                lngBytesProcessed = lngBytesProcessed + CLng(intBytesInCurrentBlock)

            End While

            'Close FileStreams and CryptoStream.
            csCryptoStream.Close()
            fsInput.Close()
            fsOutput.Close()

            'If encrypting then delete the original unencrypted file.
            If Direction = CryptoAction.ActionEncrypt Then
                Dim fileOriginal As New FileInfo(strFileToEncrypt)
                fileOriginal.Delete()
            End If

            'If decrypting then delete the encrypted file.
            If Direction = CryptoAction.ActionDecrypt Then
                Dim fileEncrypted As New FileInfo(strFileToDecrypt)
                fileEncrypted.Delete()
            End If

            'Update the user when the file is done.
            Dim Wrap As String = Chr(13) + Chr(10)
            If Direction = CryptoAction.ActionEncrypt Then
                MsgBox("Encryption Complete" + Wrap + Wrap + _
                        "Total bytes processed = " + _
                        lngBytesProcessed.ToString, _
                        MsgBoxStyle.Information, "Done")


            Else
                'Update the user when the file is done.
                MsgBox("Decryption Complete" + Wrap + Wrap + _
                       "Total bytes processed = " + _
                        lngBytesProcessed.ToString, _
                        MsgBoxStyle.Information, "Done")


            End If


            'Catch file not found error.
        Catch When Err.Number = 53 'if file not found
            MsgBox("Please check to make sure the path and filename" + _
                    "are correct and if the file exists.", _
                     MsgBoxStyle.Exclamation, "Invalid Path or Filename")

            'Catch all other errors. And delete partial files.
        Catch
            fsInput.Close()
            fsOutput.Close()

            'If Direction = CryptoAction.ActionDecrypt Then
            '    Dim fileDelete As New FileInfo(TextDestinatedFile2.Text)
            '    fileDelete.Delete()

            '    MsgBox("Please check to make sure that you entered the correct" + _
            '            "password.", MsgBoxStyle.Exclamation, "Invalid Password")
            'Else
            '    Dim fileDelete As New FileInfo(txtDestinationEncrypt.Text)
            '    fileDelete.Delete()


            'MsgBox("This file cannot be encrypted.", _
            '        MsgBoxStyle.Exclamation, "Invalid File")

            'End If

        End Try
    End Sub

#End Region

    Public Sub ExecuteQuery(query As String)
        Dim command As New SqlCommand(query, connection)

        connection.Open()
        command.ExecuteNonQuery()
        connection.Close()


    End Sub



    Private Sub Button14_Click(ByVal sender As System.Object,
                                 ByVal e As System.EventArgs) _
                                 Handles Button14.Click

        'insert decryption details into SQL Server
        Dim insertQuery As String = "INSERT INTO DecDetails(FileDecryptName,DestinatedDecFile,Decryption1,Decryption2,Decryption3,DateCreated) VALUES ('" & TextDecryptFile.Text & "','" & TextDestinatedFile2.Text & "','" & ComboBox7.Text & "','" & ComboBox8.Text & "','" & ComboBox9.Text & "', '" + DateTimePicker2.Value.Date + "')"


        ExecuteQuery(insertQuery)

        'Make sure the password is correct.
        If String.IsNullOrEmpty(TextBox8.Text) Then
            MsgBox("Please re-enter your password.", MsgBoxStyle.Exclamation)
            TextBox8.Text = ""
            'txtConPassDecrypt.Text = ""
        Else
            'Declare variables for the key and iv.
            'The key needs to hold 256 bits and the iv 128 bits.
            Dim bytKey As Byte()
            Dim bytIV As Byte()

            'Send the password to the CreateKey function.
            bytKey = CreateKey(ComboBox7.Text + "_" + ComboBox8.Text + "_" + ComboBox9.Text)
            'Send the password to the CreateIV function.
            bytIV = CreateIV(ComboBox7.Text + "_" + ComboBox8.Text + "_" + ComboBox9.Text)
            EncryptOrDecryptFile(TextDecryptFile.Text, TextDecryptFile.Text + "2", _
                                 bytKey, bytIV, CryptoAction.ActionDecrypt)


            'Send the password to the CreateKey function.
            bytKey = CreateKey(TextBox8.Text)
            'Send the password to the CreateIV function.
            bytIV = CreateIV(TextBox8.Text)
            'Start the decryption.
            EncryptOrDecryptFile(TextDecryptFile.Text + "2", TextDestinatedFile2.Text, _
                                 bytKey, bytIV, CryptoAction.ActionDecrypt)
            File.Delete(TextDecryptFile.Text + "2")

        End If

        'Declare variables for the key and iv.
        'The key needs to hold 256 bits and the iv 128 bits.
        'Dim bytKey As Byte()
        'Dim bytIV As Byte()
        ''Send the password to the CreateKey function.
        'bytKey = CreateKey(TextBox8.Text)
        ''Send the password to the CreateIV function.
        'bytIV = CreateIV(TextBox8.Text)


        ''Start the decryption.

        'Dim Level1Output As String
        'Dim Level2Output As String
        'Dim Level3Output As String

        ''if statement decryption level 1
        'If ComboBox1.Text = "Method 1" Then 'Level 1 decrypt Method 1
        '    DecryptFile(strFileToDecrypt, TextDestinatedFile2.Text, bytKey, bytIV, "Method 1")

        'ElseIf ComboBox1.Text = "Method 2" Then 'Level 1 decrypt Method 2

        '    DecryptFile(strFileToDecrypt, TextDestinatedFile2.Text, bytKey, bytIV, "Method 2")


        'End If

        ''if statement decryption level 2
        'If ComboBox2.Text = "Method 1" Then 'Level 2 decrypt Method 1
        '    DecryptFile(Level1Output, Level2Output, bytKey, bytIV, "Method 1")

        'ElseIf ComboBox2.Text = "Method 2" Then 'Level 2 decrypt Method 2
        '    DecryptFile(Level1Output, Level2Output, bytKey, bytIV, "Method 2")

        'End If

        ''if statement decryption level 3
        'If ComboBox3.Text = "Method 1" Then  'Level 3 decrypt Method 1
        '    DecryptFile(Level2Output, Level3Output, bytKey, bytIV, "Method 1")

        'ElseIf ComboBox3.Text = "Method 2" Then 'Level 3 decrypt Method 2
        '    DecryptFile(Level2Output, Level3Output, bytKey, bytIV, "Method 2")

        'End If


    End Sub

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the 'LogDetailsDataSet4.DecDetails' table. 
        'Me.DecDetailsTableAdapter2.Fill(Me.LogDetailsDataSet4.DecDetails)
        'TODO: This line of code loads data into the 'LogDetailsDataSet3.EncDetails' table. 
        'Me.EncDetailsTableAdapter1.Fill(Me.LogDetailsDataSet3.EncDetails)
        'TODO: This line of code loads data into the 'LogDetailsDataSet2.DecDetails' table. 
        'Me.DecDetailsTableAdapter1.Fill(Me.LogDetailsDataSet2.DecDetails)
        'TODO: This line of code loads data into the 'LogDetailsDataSet1.DecDetails' table. 
        'Me.DecDetailsTableAdapter.Fill(Me.LogDetailsDataSet1.DecDetails)


    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs)

    End Sub

    'for the file to be locked
    Private Sub FolderLock_Click(sender As Object, e As EventArgs) Handles FolderLock.Click


        status = arr(0)

        If FolderBrowserDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

            Dim d As DirectoryInfo = New DirectoryInfo(FolderBrowserDialog1.SelectedPath)

            Dim selectedpath As String = d.Parent.FullName + d.Name

            ProgressBar1.Value = 35

            If FolderBrowserDialog1.SelectedPath.LastIndexOf(".{") = -1 Then



                If (Not d.Root.Equals(d.Parent.FullName)) Then

                    d.MoveTo(d.Parent.FullName & "\" & d.Name & status)

                Else

                    d.MoveTo(d.Parent.FullName + d.Name & status)

                End If

                FoldertxtFile.Text = FolderBrowserDialog1.SelectedPath
                ProgressBar1.Value = 70

                ' PictureBox1.Image = Image.FromFile(Application.StartupPath & "\lock.jpg")
                ProgressBar1.Value = 100
            Else

                status = getstatus(status)



                d.MoveTo(FolderBrowserDialog1.SelectedPath.Substring(0, FolderBrowserDialog1.SelectedPath.LastIndexOf(".")))

                FoldertxtFile.Text = FolderBrowserDialog1.SelectedPath.Substring(0, FolderBrowserDialog1.SelectedPath.LastIndexOf("."))

                ' PictureBox1.Image = Image.FromFile(Application.StartupPath & "\unlock.jpg")



            End If

        End If



    End Sub

    'for the file to be unlock
    Private Sub FolderUnlock_Click(sender As Object, e As EventArgs) Handles FolderUnlock.Click


        status = arr(1)



        If FolderBrowserDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then



            Dim d As DirectoryInfo = New DirectoryInfo(FolderBrowserDialog1.SelectedPath)

            Dim selectedpath As String = d.Parent.FullName + d.Name



            If FolderBrowserDialog1.SelectedPath.LastIndexOf(".{") = -1 Then



                If (Not d.Root.Equals(d.Parent.FullName)) Then

                    d.MoveTo(d.Parent.FullName & "\" & d.Name & status)

                Else

                    d.MoveTo(d.Parent.FullName + d.Name & status)

                End If

                FoldertxtFile.Text = FolderBrowserDialog1.SelectedPath

                'PictureBox1.Image = Image.FromFile(Application.StartupPath & "\lock.jpg")

            Else

                status = getstatus(status)

                d.MoveTo(FolderBrowserDialog1.SelectedPath.Substring(0, FolderBrowserDialog1.SelectedPath.LastIndexOf(".")))

                FoldertxtFile.Text = FolderBrowserDialog1.SelectedPath.Substring(0, FolderBrowserDialog1.SelectedPath.LastIndexOf("."))

                ' PictureBox1.Image = Image.FromFile(Application.StartupPath & "\unlock.jpg")


            End If

        End If



    End Sub

    Private Function getstatus(ByVal stat As String) As String

        For i As Integer = 0 To 5

            If stat.LastIndexOf(arr(i)) <> -1 Then

                stat = stat.Substring(stat.LastIndexOf("."))

            End If

        Next i

        Return stat

    End Function



    Private Sub Button6_Click(sender As Object, e As EventArgs)
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            FoldertxtFile.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub BrowseFolderLocker_Click(sender As Object, e As EventArgs)
        With FolderBrowserDialog1

            If .ShowDialog() = DialogResult.OK Then

                FoldertxtFile.Text = .SelectedPath

            End If

        End With

    End Sub

    Private Sub FoldertxtFile_TextChanged(sender As Object, e As EventArgs) Handles FoldertxtFile.TextChanged

    End Sub



    Private Sub LockPassword_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged

    End Sub
End Class
