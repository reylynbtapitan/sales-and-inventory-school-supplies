Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.SqlClient
Imports System.Drawing
Public Class MAccounts

    Private Sub MAccounts_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        Guna2HtmlLabel19.Text = CurrentUser

        LoadRolesToComboBox()
        LoadUsers()
        CountAllUsers()
    End Sub
    Private Sub LoadUsers(Optional ByVal searchText As String = "", Optional ByVal selectedRole As String = "All")
        Try
            Dim sql As String = "SELECT UserID, FullName, Username, Contact, Gmail, Role, Password, PhotoPath, BarcodePath FROM Users WHERE 1=1"

            If Not String.IsNullOrEmpty(searchText) Then
                sql &= " AND (FullName LIKE @Search OR Username LIKE @Search)"
                AddParam("@Search", "%" & searchText & "%")
            End If

            If selectedRole <> "All" Then
                sql &= " AND Role = @Role"
                AddParam("@Role", selectedRole)
            End If

            Dim dt As DataTable = ExecuteQuery(sql)
            If dt Is Nothing Then Return

            DGVUsers.DataSource = dt

            If DGVUsers.Columns.Contains("UserID") Then DGVUsers.Columns("UserID").HeaderText = "ID"
            If DGVUsers.Columns.Contains("FullName") Then DGVUsers.Columns("FullName").HeaderText = "Full Name"
            If DGVUsers.Columns.Contains("Username") Then DGVUsers.Columns("Username").HeaderText = "Username"
            If DGVUsers.Columns.Contains("Role") Then DGVUsers.Columns("Role").HeaderText = "Role"

            DGVUsers.Refresh()
            CountAllUsers()

        Catch ex As Exception
            MessageBox.Show("Error loading users: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LoadRolesToComboBox()
        Try
            Dim sql As String = "SELECT DISTINCT Role FROM Users ORDER BY Role"
            Dim dt As DataTable = ExecuteQuery(sql)
            If dt Is Nothing Then Return

            CmbFilterRole.Items.Clear()
            CmbFilterRole.Items.Add("All")

            For Each row As DataRow In dt.Rows
                Dim role As String = row("Role").ToString()
                If Not String.IsNullOrEmpty(role) Then CmbFilterRole.Items.Add(role)
            Next

            CmbFilterRole.SelectedIndex = 0
        Catch ex As Exception
            MessageBox.Show("Error loading roles: " & ex.Message)
        End Try
    End Sub
    Private Sub CountAllUsers()
        Try
            Dim sql As String = "SELECT COUNT(*) FROM Users"
            Dim dt As DataTable = ExecuteQuery(sql)
            If dt.Rows.Count > 0 Then
                LblUserCount.Text = "0" & dt.Rows(0)(0).ToString()
            Else
                LblUserCount.Text = "00"
            End If
        Catch ex As Exception
            MessageBox.Show("Error counting users: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub DGVUsers_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGVUsers.CellContentClick
        Try
            If e.RowIndex < 0 Then Exit Sub

            Dim row As DataGridViewRow = DGVUsers.Rows(e.RowIndex)

            TxtUserID.Text = If(row.Cells("UserID").Value IsNot Nothing, row.Cells("UserID").Value.ToString(), "")
            TxtFullName.Text = If(row.Cells("FullName").Value IsNot Nothing, row.Cells("FullName").Value.ToString(), "")
            TxtUsername.Text = If(row.Cells("Username").Value IsNot Nothing, row.Cells("Username").Value.ToString(), "")
            TxtContact.Text = If(row.Cells("Contact").Value IsNot Nothing, row.Cells("Contact").Value.ToString(), "")
            TxtGmail.Text = If(row.Cells("Gmail").Value IsNot Nothing, row.Cells("Gmail").Value.ToString(), "")
            CmbRole.Text = If(row.Cells("Role").Value IsNot Nothing, row.Cells("Role").Value.ToString(), "")
            TxtPassword.Text = If(row.Cells("Password").Value IsNot Nothing, row.Cells("Password").Value.ToString(), "")

            ' --- Load Photo ---
            Dim photoPath As String = ""
            If row.Cells("PhotoPath").Value IsNot Nothing Then photoPath = row.Cells("PhotoPath").Value.ToString()
            If Not String.IsNullOrWhiteSpace(photoPath) AndAlso IO.File.Exists(photoPath) Then
                PictureBoxPhoto.Image = Image.FromFile(photoPath)
                PictureBoxPhoto.Tag = photoPath
            Else
                PictureBoxPhoto.Image = Nothing
                PictureBoxPhoto.Tag = Nothing
            End If

            ' --- Load QR / Barcode ---
            Dim qrPath As String = ""
            If row.Cells("BarcodePath").Value IsNot Nothing Then qrPath = row.Cells("BarcodePath").Value.ToString()
            If Not String.IsNullOrWhiteSpace(qrPath) AndAlso IO.File.Exists(qrPath) Then
                QrPictureBox.Image = Image.FromFile(qrPath)
                QrPictureBox.Tag = qrPath
            Else
                QrPictureBox.Image = Nothing
                QrPictureBox.Tag = Nothing
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading user details: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LoadUsersFiltered()
        Try
            Dim searchText As String = TxtSearch.Text.Trim()
            Dim selectedRole As String = If(CmbFilterRole.SelectedItem IsNot Nothing, CmbFilterRole.SelectedItem.ToString(), "All")

            Dim sql As String = "SELECT UserID, FullName, Username, Role FROM Users WHERE 1=1"

            If Not String.IsNullOrEmpty(searchText) Then
                sql &= " AND (FullName LIKE @Search OR Username LIKE @Search)"
                AddParam("@Search", "%" & searchText & "%")
            End If

            If selectedRole <> "All" Then
                sql &= " AND Role = @Role"
                AddParam("@Role", selectedRole)
            End If

            Dim dt As DataTable = ExecuteQuery(sql)
            If dt Is Nothing Then Return

            DGVUsers.DataSource = dt
        Catch ex As Exception
            MessageBox.Show("Error filtering users: " & ex.Message)
        End Try
    End Sub
    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtSearch.TextChanged
        LoadUsers(TxtSearch.Text.Trim(), If(CmbFilterRole.SelectedIndex >= 0, CmbFilterRole.Text, "All"))
    End Sub
    Private Sub CmbFilterRole_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmbFilterRole.SelectedIndexChanged
        LoadUsers(TxtSearch.Text.Trim(), CmbFilterRole.Text)
    End Sub

    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub

    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton6.Click
        ManagerDashboard.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton4.Click
        MInventory.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton9.Click
  
    End Sub

    Private Sub Guna2GradientButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton8.Click
  
    End Sub

    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton3.Click
        MSales.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton7.Click

    End Sub

    Private Sub Guna2HtmlLabel17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2HtmlLabel17.Click

    End Sub

    Private Sub Guna2HtmlLabel1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2HtmlLabel1.Click

    End Sub

    Private Sub Guna2PictureBox3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2PictureBox3.Click

    End Sub

    Private Sub Guna2HtmlLabel2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2HtmlLabel2.Click

    End Sub
End Class