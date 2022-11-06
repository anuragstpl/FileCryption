Imports System.Data.SqlClient

Public Class SignUp
    Dim cn As SqlConnection
    Dim cmd As SqlCommand
    Dim rdr As SqlDataReader
    Dim cs As String = "Data Source = MIZAHHAMZAH;Initial Catalog=filecryption;Integrated Security =true"


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub

    Private Sub CheckedListBox1_SelectedIndexChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            cn.Open()
            cmd.CommandText = "insert into login values('" + TextBox2.Text + "','" + TextBox1.Text + "','" + TextBox3.Text + "')"
            cmd.ExecuteScalar()
            MessageBox.Show("User Created")
            cn.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString())
        Finally
            cn.Close()
        End Try
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            cn = New SqlConnection(cs)
            cmd = New SqlCommand(cs, cn)
            cn.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString())
        Finally
            cn.Close()

        End Try
    End Sub
End Class