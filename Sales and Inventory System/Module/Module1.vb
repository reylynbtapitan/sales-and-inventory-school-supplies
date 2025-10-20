Imports System.Data.SqlClient

Module Module1
    Public connString As String = "Server=.\SQLEXPRESS; Database=ESI; Integrated Security=True;"

    Public Connect As New SqlConnection(connString)
    Public Parameters As New List(Of SqlParameter)
    Public Data As DataSet
    Public Datacount As Integer
    Public CurrentUser As String = ""

    Public Sub Open()
        Try
            If Connect.State = ConnectionState.Closed Then
                Connect.Open()
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to connect to database: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Sub Close()
        Try
            If Connect.State = ConnectionState.Open Then
                Connect.Close()
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to close database connection: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Sub AddParam(ByVal Key As String, ByVal value As Object)
        Parameters.Add(New SqlParameter(Key, value))
    End Sub
    Public Function Query(ByVal command_query As String) As Boolean
        Try
            Open()
            Using Command As New SqlCommand(command_query, Connect)
                If Parameters.Count > 0 Then
                    For Each param As SqlParameter In Parameters
                        Command.Parameters.Add(param)
                    Next
                    Parameters.Clear()
                End If

                Using Adapter As New SqlDataAdapter(Command)
                    Data = New DataSet()
                    Datacount = Adapter.Fill(Data)
                End Using
            End Using
            Return Datacount > 0
        Catch ex As Exception
            MessageBox.Show("Query failed: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            Close()
        End Try
    End Function
    Public Function ExecuteNonQuery(ByVal command_query As String) As Boolean
        Try
            Open()
            Using Command As New SqlCommand(command_query, Connect)
                If Parameters.Count > 0 Then
                    For Each param As SqlParameter In Parameters
                        Command.Parameters.Add(param)
                    Next
                    Parameters.Clear()
                End If
                Dim rowsAffected As Integer = Command.ExecuteNonQuery()
                Return rowsAffected > 0
            End Using
        Catch ex As Exception
            MessageBox.Show("Execution failed: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            Close()
        End Try
    End Function
    Public Function ExecuteQuery(ByVal command_query As String) As DataTable
        Try
            Open()
            Using Command As New SqlCommand(command_query, Connect)
                If Parameters.Count > 0 Then
                    For Each param As SqlParameter In Parameters
                        Command.Parameters.Add(param)
                    Next
                    Parameters.Clear()
                End If

                Using Adapter As New SqlDataAdapter(Command)
                    Dim dt As New DataTable()
                    Adapter.Fill(dt)
                    Return dt
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Query failed: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        Finally
            Close()
        End Try
    End Function
    Public Sub RefreshProductsGrid(ByVal dgv As DataGridView)
        Try
            Dim sql As String = "SELECT ProductID, ProductName, Category, Quantity, UnitPrice, SellingPrice FROM Products"

            Open()
            Using cmd As New SqlCommand(sql, Connect)
                Using adapter As New SqlDataAdapter(cmd)
                    Dim dt As New DataTable()
                    adapter.Fill(dt)
                    dgv.DataSource = dt
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Failed to load products: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Close()
        End Try
    End Sub
    Public Sub GlobalLogout()
        Try
            Dim confirm As DialogResult = MessageBox.Show("Are you sure you want to log out?",
                                                         "Confirm Logout",
                                                         MessageBoxButtons.YesNo,
                                                         MessageBoxIcon.Question)

            If confirm = DialogResult.Yes Then

                LogInForm.Show()

                For Each frm As Form In Application.OpenForms.Cast(Of Form).ToList()
                    If frm IsNot LogInForm Then
                        frm.Hide()
                    End If
                Next

                LogInForm.Guna2TextBox2.Clear()
                LogInForm.Guna2TextBox1.Clear()
                LogInForm.Guna2ComboBox1.SelectedIndex = -1

            End If
        Catch ex As Exception
            MessageBox.Show("Error during logout: " & ex.Message,
                            "Logout Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
        End Try
    End Sub

    ' Scanner
    Public Function ExecuteScalar(ByVal command_query As String) As Object
        Try
            Open()
            Using Command As New SqlCommand(command_query, Connect)
                ' Add any parameters
                If Parameters.Count > 0 Then
                    For Each param As SqlParameter In Parameters
                        Command.Parameters.Add(param)
                    Next
                    Parameters.Clear()
                End If

                ' ExecuteScalar returns only the first column of the first row
                Dim result As Object = Command.ExecuteScalar()
                Return result
            End Using
        Catch ex As Exception
            MessageBox.Show("Execution failed: " & ex.Message,
                            "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        Finally
            Close()
        End Try
    End Function
    Public Function GetSalesSummary() As Dictionary(Of String, Decimal)
        Dim summary As New Dictionary(Of String, Decimal) From {
            {"Today", 0D},
            {"Yesterday", 0D},
            {"Accumulated", 0D}
        }

        Try
            ' Today
            Dim sqlToday As String = "SELECT ISNULL(SUM(Gross),0) FROM Transactions WHERE CONVERT(date, DateAndTime) = @today"
            Parameters.Clear()
            AddParam("@today", DateTime.Now.Date)
            Dim todayTotal As Object = ExecuteScalar(sqlToday)
            If todayTotal IsNot Nothing AndAlso IsNumeric(todayTotal) Then
                summary("Today") = Convert.ToDecimal(todayTotal)
            End If

            ' Yesterday
            Dim sqlYesterday As String = "SELECT ISNULL(SUM(Gross),0) FROM Transactions WHERE CONVERT(date, DateAndTime) = @yesterday"
            Parameters.Clear()
            AddParam("@yesterday", DateTime.Now.Date.AddDays(-1))
            Dim yesterdayTotal As Object = ExecuteScalar(sqlYesterday)
            If yesterdayTotal IsNot Nothing AndAlso IsNumeric(yesterdayTotal) Then
                summary("Yesterday") = Convert.ToDecimal(yesterdayTotal)
            End If

            ' Accumulated
            Dim sqlAccumulated As String = "SELECT ISNULL(SUM(Gross),0) FROM Transactions"
            Parameters.Clear()
            Dim accumulatedTotal As Object = ExecuteScalar(sqlAccumulated)
            If accumulatedTotal IsNot Nothing AndAlso IsNumeric(accumulatedTotal) Then
                summary("Accumulated") = Convert.ToDecimal(accumulatedTotal)
            End If

        Catch ex As Exception
            MessageBox.Show("Failed to load sales summary: " & ex.Message,
                            "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Return summary
    End Function

    Public Sub LoadTotalItemsToLabel(ByVal lblTarget As Label)
        Try
            Dim sql As String = "SELECT COUNT(*) FROM Products"
            Dim dt As DataTable = ExecuteQuery(sql)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                lblTarget.Text = "Total Items: " & dt.Rows(0)(0).ToString()
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading total items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub LoadTotalItemsToLabel(ByVal lblTarget As Object)
        Try
            Dim sql As String = "SELECT COUNT(*) FROM Products"
            Dim dt As DataTable = ExecuteQuery(sql)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                lblTarget.Text = "Total Items: " & dt.Rows(0)(0).ToString()
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading total items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub LoadLowAndOutOfStockToGrid(ByVal dgv As DataGridView, ByVal lblLow As Object, ByVal lblOut As Object, Optional ByVal search As String = "")
        Try
            Dim sql As String = "" &
                "SELECT ProductID, ProductName, Category, Quantity, UnitPrice, SellingPrice, Supplier " & vbCrLf &
                "FROM Products " & vbCrLf &
                "WHERE Quantity <= 50 "

            If Not String.IsNullOrWhiteSpace(search) Then
                sql &= " AND ProductName LIKE @Search"
                AddParam("@Search", "%" & search.Trim() & "%")
            End If

            sql &= " ORDER BY Quantity ASC"

            Dim dt As DataTable = ExecuteQuery(sql)
            dgv.DataSource = dt

            ' Apply color coding
            For Each row As DataGridViewRow In dgv.Rows
                Dim qty As Integer = Convert.ToInt32(row.Cells("Quantity").Value)
                If qty = 0 Then
                    row.DefaultCellStyle.BackColor = Color.Red
                    row.DefaultCellStyle.ForeColor = Color.White
                ElseIf qty <= 50 Then
                    row.DefaultCellStyle.BackColor = Color.Yellow
                    row.DefaultCellStyle.ForeColor = Color.Black
                End If
            Next

            ' Update labels
            Dim lowCount As Integer = dt.Select("Quantity > 0 AND Quantity <= 50").Length
            Dim outCount As Integer = dt.Select("Quantity = 0").Length

            If lblLow IsNot Nothing Then lblLow.Text = "Low Stock: " & lowCount.ToString()
            If lblOut IsNot Nothing Then lblOut.Text = "Out of Stock: " & outCount.ToString()

        Catch ex As Exception
            MessageBox.Show("Error loading low/out-of-stock products: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Sub CountAllUsersByRole(
        ByVal lblTotalUsers As Object,
        ByVal lblAdmin As Object,
        ByVal lblStaff As Object,
        ByVal lblManager As Object
    )
        Try
            Dim sql As String = "SELECT Role, COUNT(*) AS RoleCount FROM Users GROUP BY Role"
            Dim dt As DataTable = ExecuteQuery(sql)

            ' Reset labels
            lblTotalUsers.Text = "Total Users: 0"
            lblAdmin.Text = "Admin: 0"
            lblStaff.Text = "Staff: 0"
            lblStaff.Text = "Inventory: 0"
            lblManager.Text = "Manager: 0"

            Dim totalUsers As Integer = 0

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For Each row As DataRow In dt.Rows
                    Dim role As String = row("Role").ToString()
                    Dim count As Integer = Convert.ToInt32(row("RoleCount"))
                    totalUsers += count

                    Select Case role
                        Case "Admin"
                            lblAdmin.Text = "Admin: " & count
                        Case "Staff"
                            lblStaff.Text = "Staff: " & count
                        Case "Staff"
                            lblStaff.Text = "Staff: " & count
                        Case "Manager"
                            lblManager.Text = "Manager: " & count
                    End Select
                Next
            End If

            lblTotalUsers.Text = "Total Users: " & totalUsers

        Catch ex As Exception
            MessageBox.Show("Error counting users: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


End Module

