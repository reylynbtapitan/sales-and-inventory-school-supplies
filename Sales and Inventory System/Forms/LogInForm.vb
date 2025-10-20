Imports System.Security.Cryptography
Imports System.Text

Public Class LogInForm

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click

        ' --- VALIDATION ---
        If String.IsNullOrWhiteSpace(Guna2TextBox1.Text) Then
            MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Guna2TextBox1.Focus()
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(Guna2TextBox2.Text) Then
            MessageBox.Show("Password is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Guna2TextBox2.Focus()
            Exit Sub
        End If

        If Guna2ComboBox1.SelectedIndex = -1 Then
            MessageBox.Show("Please select a role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Guna2ComboBox1.Focus()
            Exit Sub
        End If

        ' --- USE PLAIN PASSWORD (no hashing) ---
        Dim plainPassword As String = Guna2TextBox2.Text

        AddParam("@Username", Guna2TextBox1.Text)
        AddParam("@Password", plainPassword)
        AddParam("@Role", Guna2ComboBox1.SelectedItem.ToString())

        Dim sql As String = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password AND Role = @Role"

        If Query(sql) Then
            CurrentUser = Data.Tables(0).Rows(0)("Username").ToString()
            Dim role As String = Data.Tables(0).Rows(0)("Role").ToString()

            MessageBox.Show("Log In Successfully!" & Environment.NewLine &
                            "Hello, " & CurrentUser, "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' --- SAVE LOGIN HISTORY ---
            Dim logSql As String = "INSERT INTO LoginHistory (Username, Role, LoginDate) VALUES (@Username, @Role, @Date)"
            AddParam("@Username", CurrentUser)
            AddParam("@Role", role)
            AddParam("@Date", DateTime.Now)
            ExecuteNonQuery(logSql)

      
                If role = "Cashier" Then
                    Dim f As New CashierDashboard
                    f.Show()
                ElseIf role = "Inventory" Then
                    Dim f As New InventoryDashboard
                    f.Show()
                ElseIf role = "Manager" Then
                    Dim f As New ManagerDashboard
                    f.Show()
                End If

                Me.Hide()
            Else
                MessageBox.Show("Invalid username, password, or role.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

    End Sub

    Private Sub LogInForm_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Guna2ComboBox1.Items.Clear()
        Guna2ComboBox1.Items.Add("Cashier")
        Guna2ComboBox1.Items.Add("Inventory")
        Guna2ComboBox1.Items.Add("Manager")
    End Sub

    Private Sub Guna2ComboBox1_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles Guna2ComboBox1.SelectedIndexChanged

    End Sub

    Private Sub Guna2PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2PictureBox1.Click

    End Sub
End Class
